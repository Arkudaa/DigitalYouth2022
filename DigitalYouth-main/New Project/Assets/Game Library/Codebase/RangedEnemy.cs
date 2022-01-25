using UnityEngine;
using System.Collections;

public class RangedEnemy : MonoBehaviour {
	
    //This one will be our projectile object, it should have a ScriptProjectile in it.
    //If you don't set one, I'll just create a sphere for you and use it, it'll be ugly! D:
	public GameObject ProjectileObject;
    //You can set a Transform here for the object to spawn from, if none is set I'll pick a
    //Position in front of the player so no worries.
	public Transform SpawnPosition;
    //How much time should we wait before firing? We should wait for the animation probably.

	[HideInInspector]
	public float WaitBeforeFiring =0.2f;
    //How much cooldown should we have before firing again?
	public float CooldownTime =1f;
	
	// private variables
	private GameObject LastFiredProjectile;
	private Collider hitbox;
	private bool CanShoot;
	private CharacterMotor characterMotor;
	private EnemyAI myAI;
	private Transform m_player;
	private Transform m_transform;

    // Use this for initialization
	void Start() {
		// grab references
		m_player = GameObject.FindGameObjectWithTag ("Player").transform;
		m_transform = transform;
		myAI = GetComponent<EnemyAI>();
		characterMotor = GetComponent<CharacterMotor>();
       
		// make sure this is not on cooldown
		CanShoot=true;
	}

	void Update () {
		//If the enemy isnt' on cool down, we then check to see if it's close enough to the player to fire at him
		if (CanShoot && myAI.IsPlayerInRange()) {
			//Now start a coroutine that will wait before firing so the animation plays a bit before spawning projectiles.
			StartCoroutine (WaitAndFire (WaitBeforeFiring));
			CanShoot = false;
			StartCoroutine (CoolDown (CooldownTime));
		}
	}
	
    // wait for specified time, then fire
	IEnumerator WaitAndFire(float waitTime) {
        //It'll wait for as much time as the float waitTime inputted.
		yield return new WaitForSeconds(waitTime);
        //First lets create a new vector 3 and zero it out.
		Vector3 spawnPos = Vector3.zero;
	
        //If the player hasn't set a spawn position, let's make one for him in front of our character and kind of half his height
		if (!SpawnPosition) {
			spawnPos = gameObject.transform.position + gameObject.transform.forward*2 + Vector3.up*0.45f;
		} else {
                //If the player did set up a spawn position, just use it :D
			spawnPos = SpawnPosition.position;
		}
		
        // if there is no Projectile Object let's make one and position it.
		if (!ProjectileObject) {
			LastFiredProjectile = CreateProjectile();
			LastFiredProjectile.transform.rotation=gameObject.transform.rotation;
			LastFiredProjectile.transform.position=spawnPos;
		} else {
            //If the player did create a Projectile Object lets instantiate it on our spawn position and rotate it propertly
			LastFiredProjectile = GameObject.Instantiate(ProjectileObject,spawnPos,gameObject.transform.rotation) as GameObject;
		}
        //Lets initialize our newly spawned Projectile!
		LastFiredProjectile.GetComponent<Projectile>().Owner=this.gameObject;
		LastFiredProjectile.GetComponent<Projectile>().Initialize();
	}
	
    //We'll use this as a cooldown so we can't spam the projectiles D:
	IEnumerator CoolDown(float waitTime) {
        //It'll wait for as much time as the float waitTime inputted.
		yield return new WaitForSeconds(waitTime);
        //We can shoot now! The cooldown is over!
		CanShoot=true;
	}
	
    // This function will create a projectile for a developer that didn't add one
    // and add all the necessary stuff to it, then it'll return it to the funciton that called it.
	GameObject CreateProjectile() {
        //Creates a Sphere
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //Adds our Projectile script! Wouldn't be a projectile without it xD!
		go.AddComponent<Projectile>();
        //And a collider! It's important!
		go.AddComponent<SphereCollider>();
		//get the sphere collider component and assign it to hitbox
		hitbox = go.GetComponent<SphereCollider>();
        //Also it needs a rigid body to move D: so lets add one
		Rigidbody gobody = go.AddComponent<Rigidbody>();
        //Now we'll make the rigid body into a Kinematic one
		gobody.isKinematic=true;
        //And let's deactivate gravity on it.
		gobody.useGravity=false;
        //Also, the collider we added... it should be a trigger
		hitbox.isTrigger = true;
        //Now return the created game object
		return go;
	}
}