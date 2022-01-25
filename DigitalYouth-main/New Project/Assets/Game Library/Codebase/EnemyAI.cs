using UnityEngine;
using System.Collections;
using System;

//simple "platformer enemy" AI
[RequireComponent (typeof(CharacterMotor))]
[RequireComponent (typeof(DealDamage))]
public class EnemyAI : MonoBehaviour {
	
	[Header("Basic Stats")]
	public float speedLimit = 10f;
	//how fast enemy can move
	public float acceleration = 35f;
	//acceleration of enemy movement
	public float deceleration = 8f;
	//deceleration of enemy movement
	public float rotateSpeed = 0.7f;
	//how fast enemy can rotate
	public int attackDmg = 1;
	//how much damage to deal to the player when theyre attacked by this enem
	public float pushForce = 10f;
	//how far away to push the player when they are attacked by the enemy
	public float pushHeight = 7f;
	//how high to push the player when they are attacked by the enemy

	[Header("Advanced Stats")]
	public Vector3 bounceForce = new Vector3 (0, 13, 0);
	//force to apply to player when player jumps on enemies head
	public AudioClip bounceSound;
	//sound when you bounce on enemies
	public bool chase = true;
	//should this enemy chase objects inside its sight?
	public bool ignoreY = true;
	//ignore Y axis when chasing? (this would be false for say.. a flying enemy)
	public float chaseStopDistance = 0.7f;
	//stop this far away from object when chasing it
	public float seekStopDistance = 3.0f;
	//trigger for attack bounds (player is hurt when they enter these bounds)

	[HideInInspector]
	public Animator animatorController;
	//object which holds the animator for this enem
	public MoveToPoints moveToPointsScript;
	//if you've attached this script, drag the component here

	private TriggerParent sightTrigger;
	protected TriggerParent attackTrigger;
	private PlayerMove playerMove;
	protected CharacterMotor characterMotor;
	private DealDamage dealDamage;
	private Health health;
	private PlayerMelee pMelee;

	// Flags to interrogate
	protected bool playerInRange = false;
	protected GameObject targetInRange;


	//called from EnemyRange.cs
	public bool IsPlayerInRange() {
		return playerInRange;
	}

	private Transform m_player;
	[HideInInspector]
	private GameObject projectile = null;
	private bool m_canFireProjectile = true;
	[HideInInspector]
	public float m_delayBetweenShots = 1.0f;
	[HideInInspector]
	public Transform projectileSpawnPoint;

	// These hashes allow us to determine which anims the controller has. The state names in the
	// animation controller (not the anims) must match the names in the strings below.
	int spinAttack = Animator.StringToHash ("spinAttack");
	int meleeAttack = Animator.StringToHash ("meleeAttack");
	int idle = Animator.StringToHash ("idle");
	int move = Animator.StringToHash ("move");

	private Coroutine attackCoroutine;
	// Abstract functions
	//	public abstract void OnAwake ();
	//	public abstract void OnEnableBehaviour ();
	//	public abstract void OnDisableBehaviour ();

	void Awake() {		
		GrabReferences();
		//OnAwake();
	}

	/*
	* This checks if the 'hashed' animation exists in the animationController.
	* NOTE: It refers to the name of the *state* in the controller and *not* the actual animation,
	* so the states can be renamed to match the above while the actual animation names can be left.
	*/
	private bool DoIHaveAnimation(int whichAttack) {
		// If there's no controller found, return
		if (!animatorController) {
			Debug.LogWarning ("No animator controller found");
			return false;
		}

		if (animatorController.HasState (0, whichAttack)) {
		// Debug.LogWarning (gameObject.name + " has a " + whichAttack.ToString () + " in it's animator.");
			return true;
		} else {
			Debug.LogWarning (gameObject.name + " does NOT has a " + whichAttack.ToString () + " in it's animator.");
			return false;
		}
	}

	private void GrabReferences() {
		characterMotor = GetComponent<CharacterMotor>();
		dealDamage = GetComponent<DealDamage>();
		m_player = GameObject.FindGameObjectWithTag("Player").transform;
		pMelee = m_player.GetComponentInParent<PlayerMelee>();

		//avoid setup errors
		if (tag != "Enemy") {
			tag = "Enemy";
			Debug.LogWarning("'EnemyAI' script attached to object without 'Enemy' tag, it has been assign automatically", transform);
		}

		// Grab sightbounds
		sightTrigger = transform.Find ("Sight Bounds").GetComponent<TriggerParent> ();
		if (!sightTrigger) {
			Debug.LogError("'TriggerParent' script needs attaching to enemy 'SightBounds'");
		}  

		// Grab attack bounds
		attackTrigger = transform.Find ("Attack Bounds").GetComponent<TriggerParent> ();
		if (!attackTrigger) {
			Debug.LogError ("'TriggerParent' script needs attaching to enemy 'attackBounds'");
		} 

		health = GetComponent<Health> ();
		if (!health) {
			Debug.LogError("This gameoObject needs a 'Health' script");
		}

		// Grab animator controller
		animatorController = GetComponentInChildren<Animator> ();
		if (!animatorController)
			Debug.LogError (gameObject.name + " needs an Animator controller. Attach one or enemy won't be able to animate!");
	}

 

	/// <summary>
	/// Is the player within this range?
	/// </summary>
	/// <returns><c>true</c> if this instance is player within range the specified range; otherwise, <c>false</c>.</returns>
	/// <param name="range">Range.</param>
	protected bool IsPlayerWithinRange(float range) {
		// Clamp these values so no invalid ones are passed
		Mathf.Clamp (range, 0, 100);

		float distance = Vector3.Distance (transform.position, m_player.position);

		if (distance <= range) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Find whether an object is within a specific distance
	/// </summary>
	/// <returns><c>true</c> if this instance is object within range the specified objectToLookfor range; otherwise, <c>false</c>.</returns>
	/// <param name="objectToLookfor">Object to lookfor.</param>
	/// <param name="range">Range.</param>
	protected bool IsObjectWithinRange(GameObject objectToLookfor, float range) {
		// Clamp these values so no invalid ones are passed
		Mathf.Clamp (range, 0, 100);
		if (objectToLookfor) {
			float distance = Vector3.Distance (transform.position, objectToLookfor.transform.position);

			if (distance <= range) {
				return true;
			} 
		}
		return false;
	}
 
	/// <summary>
	/// Returns the player gameobject.
	/// </summary>
	/// <returns>The player.</returns>
	protected GameObject ReturnPlayer() {
		return m_player.gameObject;
	}

	protected bool CanIIdle() {
		// Can I attack checks no other anim or attack is running
		if (CanIAttack ()) {
			if (DoIHaveAnimation (idle)) {
				return true;
			}
		}
		return false;
	}

	protected void Idle() {
		Debug.Break ();
		animatorController.SetBool ("Moving", false);
	}

	// Returns false if already performing an attack or on way to perform an attack
	protected bool CanIAttack() {
		AnimatorStateInfo currentState = animatorController.GetCurrentAnimatorStateInfo (0);

		// If we are playing an attack animation, or moving towards an attack
		if (currentState.shortNameHash == spinAttack || currentState.shortNameHash == meleeAttack || currentState.shortNameHash == move ||
		    attackCoroutine != null) {
			return false;
		} else
			return true;
	}
	 
	IEnumerator MoveToTarget(Transform target, Action onComplete) {
		// Debug.Log ("Started Moving to target");
		// If no moving coroutine is currently running
		if (attackCoroutine == null) {
			animatorController.SetBool ("Moving", true);	
			while (!characterMotor.MoveTo (target.position, acceleration, seekStopDistance, true)) {
				// Debug.Log ("Moving to target. Distance: "+ Vector3.Distance ( transform.position, target.position) );
				yield return 0;
			}

			attackCoroutine = null;
			// Debug.Log ("At target distance");
			// Return to idle
			animatorController.SetBool ("Moving", false);	
			onComplete ();
		}
	}

	bool InRange() {
		if (attackTrigger && attackTrigger.collided) {
			return true;
		} else {
			return false;
		}
	}

	protected void MeleeAttack() {
		if (!DoIHaveAnimation (meleeAttack))
			return;

		// If not in range, start coroutine else, just attack
		if (!InRange ()) {
			attackCoroutine = StartCoroutine (MoveToTarget (m_player, () => {
				StartCoroutine (TriggerMeleeAttack ());
			})
			);
			return;
		} else {
			attackCoroutine = StartCoroutine (TriggerMeleeAttack ());
		}
	}

	protected void SpinAttack() {
		if (!DoIHaveAnimation (spinAttack))
			return;

		// If not in range, start coroutine else, just attack
		if (!InRange()) {
			attackCoroutine = StartCoroutine (MoveToTarget (m_player, () => {
				StartCoroutine (TriggerSpinAttack ());
			})
			);
			return;
		} else {
			attackCoroutine = StartCoroutine (TriggerSpinAttack ());
		}
	}

	IEnumerator TriggerMeleeAttack() {
		if (animatorController) {
			animatorController.SetTrigger("MeleeAttack");	
			dealDamage.Attack(attackTrigger.hitObject, attackDmg, pushHeight, pushForce);
		}  
		yield return 0;
	}

	IEnumerator TriggerSpinAttack() {
		if (animatorController) {
			animatorController.SetTrigger("SpinAttack");	
			dealDamage.Attack (attackTrigger.hitObject, attackDmg, pushHeight, pushForce);
		}  
		yield return 0;
	}
 
	private GameObject CreateProjectile() {
		GameObject newProjectile;
		newProjectile = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		newProjectile.AddComponent<SphereCollider> ();
		newProjectile.AddComponent<Rigidbody> ();
		newProjectile.AddComponent<Projectile> ();
		newProjectile.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
		return newProjectile;
	}

	protected bool CanIPerformRangedAttack() {
		// Refers to the cooldown
		if (m_canFireProjectile) {
			return true;
		} else {
			return false;
		}
	}

	protected void PerformRangedAttack() {
		if (CanIPerformRangedAttack()) {
			GameObject newProjectile;

			// Turn towards player
			characterMotor.RotateToDirection(m_player.position, rotateSpeed, true);

			// Debug.Log ("Firing Projectile");
			if (m_canFireProjectile) {
				if (projectile == null) {
					newProjectile = CreateProjectile ();
				} else {
					newProjectile = Instantiate (projectile, transform.position, Quaternion.identity) as GameObject;
				}

				// Sets spawn point of projectile
				if (projectileSpawnPoint == null) {
					Debug.LogWarning ("No projectileSpawnPoint found, spawning at front");
					newProjectile.transform.position = transform.position + new Vector3 (0f, 2f, 2f);
				} else {
					newProjectile.transform.position = projectileSpawnPoint.transform.position;
				}
				// Sets it up!
				newProjectile.GetComponent<Projectile> ().Owner = this.gameObject;
				newProjectile.GetComponent<Projectile> ().Initialize ();
				// Prevents constant spawning of projectile
				StartCoroutine (ProjectileCooldown (m_delayBetweenShots));
			}
		}
	}

	// This prevents the AI continually spawning bullets and introduces a delay between shots
	IEnumerator ProjectileCooldown(float delayBetweenShots) {
		m_canFireProjectile = false;
		yield return new WaitForSeconds (delayBetweenShots);
		m_canFireProjectile = true;
	}
		
	public virtual void Update() {
		// If within sight cache target object
		if (sightTrigger.colliding) {
			playerInRange = true;
			targetInRange = sightTrigger.hitObject;
		} else {
			playerInRange = false;
			targetInRange = null;
		}
			
		//chase
		if (sightTrigger && sightTrigger.colliding && chase && sightTrigger.hitObject != null && sightTrigger.hitObject.activeInHierarchy) {
			characterMotor.MoveTo(sightTrigger.hitObject.transform.position, acceleration, chaseStopDistance, ignoreY);

			//nofity animator controller
			if (animatorController)
				animatorController.SetBool ("Moving", true);
			//disable patrol behaviour
			if (moveToPointsScript)
				moveToPointsScript.enabled = false;
		} else {	
			//notify animator
			if (animatorController)
				animatorController.SetBool ("Moving", false);
			//enable patrol behaviour
			if (moveToPointsScript)
				moveToPointsScript.enabled = true;
		}

		//hitObject is null sometimes, it shouldn't be. But this fixes the null reference exception
		// attack
		if (attackTrigger && attackTrigger.collided && attackTrigger.hitObject!=null ) {
			if (attackTrigger.hitObject.transform.tag == "Player" && !pMelee.Punching) {
				animatorController.SetTrigger ("MeleeAttack");	
				dealDamage.Attack (attackTrigger.hitObject, attackDmg, pushHeight, pushForce);
			}
		}
	}

	void FixedUpdate() {
		characterMotor.ManageSpeed (deceleration, speedLimit, ignoreY);
		characterMotor.RotateToVelocity (rotateSpeed, ignoreY);
	}

	//bounce player when they land on this enemy
	public void BouncedOn() {	
		if (!playerMove)
			playerMove = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerMove> ();
		if (bounceSound)
			AudioSource.PlayClipAtPoint (bounceSound, transform.position);
		if (playerMove) {
			Vector3 bounceMultiplier = new Vector3 (0f, 1.5f, 0f) * playerMove.onEnemyBounce;
			playerMove.Jump (bounceForce + bounceMultiplier);
			animatorController.SetTrigger ("Hit");
		} else
			Debug.LogWarning ("'Player' tagged object landed on enemy, but without playerMove script attached, is unable to bounce");
	}
}