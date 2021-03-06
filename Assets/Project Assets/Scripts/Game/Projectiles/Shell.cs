using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shell : Projectile
{
	private Vector3 previousPosition;
	
	private GameObject trail = null;
	private bool trailActive = false;
	
	protected override void InitializeSpecific()
	{
		// update layermask so units aren't included
		layerMask = ProjectileManager.projectileLayerMask;
		
		// offset a bit more
		gameObject.transform.position = gameObject.transform.position + (projectileData.direction * 1.0f);
		
		// add rb
		gameObject.AddComponent<BoxCollider>();
		gameObject.AddComponent<Rigidbody>().freezeRotation = true;
		
		// calculate veloctiy and fire
		Vector3 targetPosition;
		if (projectileData.target == null) targetPosition = gameObject.transform.position - new Vector3(0, 100, 0);
		else targetPosition = projectileData.target.transform.position;
		Vector3 dir = targetPosition - (gameObject.transform.position + (Random.insideUnitSphere*12.0f)); // get target direction HC!!
		float h = dir.y;  // get height difference
		dir.y = 0;  // retain only the horizontal direction
		float dist = dir.magnitude;  // get horizontal distance
		dir.y = dist;  // set elevation to 45 degrees
		dist += h;  // correct for different heights
		float vel = Mathf.Sqrt(Mathf.Abs(dist) * Physics.gravity.magnitude);
		gameObject.GetComponent<Rigidbody>().velocity = vel * dir.normalized;
		
		previousPosition = gameObject.transform.position;
		
		// trail (particle sys)
		trail = Loader.LoadGameObject("Effects/MammothTankGrenadeTrail_Prefab");
		trail.transform.parent = gameObject.transform;
		trail.transform.localPosition = Vector3.zero;
		trail.GetComponent<ParticleSystem>().enableEmission = false;
	}
	
	
	void FixedUpdate()
	{
		// update direction
		projectileData.direction = (gameObject.transform.position - previousPosition).normalized;
		if (projectileData.direction != Vector3.zero) 
		{
			Quaternion lookDirection = new Quaternion(0, 0, 0, 0);
			lookDirection = Quaternion.LookRotation(projectileData.direction);
			gameObject.transform.rotation = lookDirection;
		}
		previousPosition = gameObject.transform.position;
		
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawRay(gameObject.transform.position, projectileData.direction);
	}
	
	protected override void Update()
	{
		// WHY?
		if (Data.pause) return;
		
		// current position
		projectileData.position = gameObject.transform.position;
		
		// update speed
		projectileData.speed = gameObject.GetComponent<Rigidbody>().velocity.magnitude*Time.deltaTime;
		
		// prepare ray
		RaycastHit tHit;
		Ray tRay = new Ray(projectileData.position, projectileData.direction);
		
		bool tResult = Physics.Raycast(tRay, out tHit, projectileData.speed+1, layerMask);
		if(tResult) // Keep checking the distance
		{
			

			// Spawn explosion
			// damage
			ExplosionManager.AddExplosion("Default", gameObject.transform.position);
			AreaDamageManager.AddAreaDamage("Small", gameObject.transform.position, projectileData.sender);
			// Destroy
			Destroy();

			if (trail != null)
			{
				// disable trail and queue destruction
				trail.transform.parent = null;
				trail.GetComponent<ParticleSystem>().enableEmission = false;
				Destroy(trail, 1.0f);
			}
			
			// Destroy me
			Destroy();
			
		} 
		else if (trail != null)
		{
			if (trailActive && trail.GetComponent<ParticleSystem>().enableEmission == false) trail.GetComponent<ParticleSystem>().enableEmission = true;
			trailActive = !trailActive;
		}
		
		// Lifetime test for when we're not hitting anything anymore
		projectileData.lifetime -= Time.deltaTime;
		if (projectileData.lifetime <=0) Destroy();
	}
	
	protected override void Destroy()
	{
		//ProjectileManager.projectiles.Remove(gameObject);
		Scripts.audioManager.StopSFX(projectileData.sound);
		Destroy(this.gameObject);
	}
	
	
	
}

