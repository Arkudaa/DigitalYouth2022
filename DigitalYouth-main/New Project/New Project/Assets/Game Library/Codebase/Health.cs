using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

//attach to any object in the game which takes damage (player, enemies, breakable crates, smashable windows..)
// Note that damaage can be given via on collision using physics, or by passing a message via DealDamage. The ticks
// in the script (takeImpactDmg etc) determine this
[RequireComponent(typeof(AudioSource))]
public class Health : MonoBehaviour 
{
	private int defHealth, h, hitForce;

	//Add Health to the Character
	public int currentHealth = 1;					//health of the object
	[Space(20)]

	//Sounds
	public AudioClip impactSound;					//play when object imacts with something else
	public AudioClip hurtSound;						//play when this object recieves damage
	public AudioClip deadSound;						//play when this object dies
	public bool takeImpactDmg;						//does this object take damage from impacts?
	public bool onlyRigidbodyImpact;				//if yes to the above, does it only take impact damage from other rigidbodies?
	public bool respawn;							//should this object respawn when killed?
	public string[] impactFilterTag;				//if we take impact damage, don't take impact damage from these objects (tags)
	//Flashing
	private bool Can_Flash = true;
	public float hitFlashDelay = 0.1f;				//how long each flash lasts (smaller number = more rapid flashing)
	public float flashDuration = 0.9f;				//how long flash lasts (object is invulnerable to damage during this time)
	public Color hitFlashColor = Color.red;			//color object should flash when it takes damage
	public Transform flashObject;					//object to flash upon receiving damage (ie: a child mesh). If left blank it defaults to this object.
	public GameObject[] spawnOnDeath;				//objects to spawn upon death of this object (ie: a particle effect or a coin)

	[HideInInspector]
	public bool dead, flashing,Can_KO = false;
	[HideInInspector]
	public Vector3 respawnPos;

	//Flashing
	private Color originalColor;

	private bool hitColor = false;
	private float nextFlash, stopFlashTime;

	private Throwing throwing;
	private PlayerMove p_movement;
	private Rigidbody myRigidbody;
	private Animator AnimatorControl;
	private Renderer flashRender;
	private AudioSource aSource;

	[HideInInspector]
	public bool InstantRespawn;

	//setup
	public virtual void Awake() {
		aSource = GetComponent<AudioSource>();
		if (currentHealth <= 0)
			Debug.LogWarning(transform.name + " has 'currentHealth' set to 0 or less in 'Health' script: it has been destroyed upon scene start");
		aSource.playOnAwake = false;

		if (flashObject == null) {
			flashObject = transform;
		}

		flashRender = flashObject.GetComponent<Renderer>();

		if (flashRender) {
			originalColor = flashRender.material.color;	
		}

		defHealth = currentHealth;
		respawnPos = transform.position;

		if (gameObject.CompareTag ("Player")) {
			int a = 5 + 4;
		}

		//Get reference to controller
		if (tag == "Player") {
			//Q - Get Animator Controller
			AnimatorControl = GetComponent<Throwing> ().animator;
			throwing = GetComponent<Throwing> ();
		}
		AnimatorControl = GetComponentInChildren<Animator>();
		myRigidbody = GetComponent<Rigidbody> ();
	}
		
	// detecting damage
	public virtual void Update() {		
		//flash if we took damage
		if (currentHealth < h) {
			if (Can_Flash) {
				if (flashObject != null) {
					if (currentHealth > 1)
					flashing = true;
				} else {
					Can_Flash = false;
					Debug.Log ("No Flash Object selected.  Setting Can Flash False.");
				}
				stopFlashTime = Time.time + flashDuration;
				if (hurtSound)
					AudioSource.PlayClipAtPoint (hurtSound, transform.position);

				if (AnimatorControl != null) {
					if (currentHealth > 1) {
						AnimatorControl.SetFloat ("Hit_Anim", Random.Range (1, 2));
						AnimatorControl.SetTrigger ("Hit");
					}
				} else {
					Debug.LogWarning ("Animatorcontroller null for " + gameObject.name);
				}
			}
		}

		h = currentHealth;

		//flashing
		if (Can_Flash) {
			Flash ();
			if (Time.time > stopFlashTime && flashRender != null) {
				flashRender.material.color = originalColor;
				flashing = false;
			}
		}
		if (InstantRespawn) {
			AnimatorControl.SetBool("KO",false);
			Debug.Log ("death 4");
			Death();
			RespawnPlayer ();
		}
		// has it been destroyed?
		if (!dead) {	
			if (tag == "Player") {
				AnimatorControl.SetBool ("KO", false);
			}
		}
		dead = (currentHealth <= 0) ? true : false;

		if (dead) {
			//Debug Player Lost all health.
			if (Can_KO) {
				//Knock Out Player.
				if (tag == "Player"){
					//Attempt to freeze Player in position
					AnimatorControl.SetBool ("KO", true);	//Do KO animation
					myRigidbody.isKinematic = true;	
					//Wait for Input...
					if (Input.GetButtonDown ("Jump")) { // If we press Space bar respawn normally.
						AnimatorControl.SetBool ("KO", false);
						//Debug.Log ("destroy 3");
						Death();
					}
				} else {
					if (tag != "Player") {
						Death();
						//Debug.Log ("destroy 1");
					}
				}
			}
			//Debug.Log (this+" "+tag+"destroy 1");
			Death();
		}
	}

	//toggle the flashObject material tint color
	void Flash() {
		if (flashRender != null) {
			flashRender.material.color = (hitColor) ? hitFlashColor : originalColor;
			if (Time.time > nextFlash) {
				hitColor = !hitColor;
				nextFlash = Time.time + hitFlashDelay;
			}
		}
	}

	//respawn object, or destroy it and create the SpawnOnDeath objects
	public virtual void Death() {
		if (respawn) {
			currentHealth = defHealth;
			dead = false;
			Rigidbody rigid = GetComponent<Rigidbody>();
			currentHealth = defHealth;
			InstantRespawn = false;
			rigid.isKinematic = false;
			if (this.gameObject.tag == "Player") {
				AnimatorControl.SetBool ("KO", false);
			}
			//rigid.velocity *= 0;
			transform.position = respawnPos;	//Original spawn location upon initilization(Start of scene).
			rigid.velocity *= 0;

			if (rigid) {
				rigid.isKinematic = false;
				rigid.velocity *= 0;
			}
		} else {
			Destroy (gameObject); //Q Consider Object pooling.
		}

		//player drop item
		if (tag == "Player") {
			// Q - Get Animator Controller
			AnimatorControl = GetComponent<Throwing>().animator;
			throwing = GetComponent<Throwing>();
			if (throwing && throwing.heldObj && throwing.heldObj.tag == "Pickup")
				throwing.ThrowPickup();
			RespawnPlayer();
		}

		if (deadSound)
			AudioSource.PlayClipAtPoint(deadSound, transform.position);

		flashing = false;
		flashObject.GetComponent<Renderer>().material.color = originalColor;

		//Respawn GameObject
		if (spawnOnDeath.Length != 0)
			foreach(GameObject obj in spawnOnDeath)
				Instantiate(obj, transform.position, Quaternion.Euler(Vector3.zero));
	}

	//calculate impact damage on collision
	void OnCollisionEnter(Collision col) {
		if (!aSource.isPlaying && impactSound) {
			aSource.clip = impactSound;
			aSource.volume = col.relativeVelocity.magnitude/30;
			aSource.Play();
		}

		//make sure we take impact damage from this object
		if (!takeImpactDmg)
			return;
		foreach(string tag in impactFilterTag)			
			if(col.transform.tag == tag)
				return;
		if(onlyRigidbodyImpact && !col.rigidbody)
			return;

		//calculate damage
		if(col.rigidbody)
			hitForce = (int)(col.rigidbody.velocity.magnitude/4 * col.rigidbody.mass);
		else
			hitForce = (int)col.relativeVelocity.magnitude/6;

		// Optional Debug logging
		// Debug.Log(transform.name + " took: " + hitForce + " dmg in collision with " + col.transform.name);
		// Debug.Log ("Collision between " + col.gameObject.name + " and " + gameObject.name + ". Damage: " + hitForce+" CurrentHealth"+currentHealth);
	}

	void RespawnPlayer() {
		currentHealth = defHealth;
		myRigidbody.velocity *= 0;
		AnimatorControl.SetBool("KO",false);
		myRigidbody.isKinematic = false;
		InstantRespawn = false;
	}
}

// NOTE: if you just want an object to play impact sounds, give it this script, but uncheck for impact damage