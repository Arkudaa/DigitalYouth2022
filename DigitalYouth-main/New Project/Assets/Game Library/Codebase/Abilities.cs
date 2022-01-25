using UnityEngine;
using System.Collections;
 
public class Abilities : Singleton<Abilities> {
 
	// static booleans used to enable Abilities
	public static bool doubleJumpEnabled = false;
	public static bool spinAttackEnabled = false;
	public static bool speedBoostEnabled = false;
	public static bool invincibilityEnabled = false;

	// customizable properties
	public Vector3 doubleJumpForce;
	public GameObject jumpParticle;
	public AudioClip doubleJumpSound;

	// boost particle and sound
	[HideInInspector]
	public GameObject boostParticle;
	[HideInInspector]
	public AudioClip boostSound;

	// customizable speedboost value
	public float boostAmount= 30f;
	
	// speed boost values
	float OriginalSpeed;
	float OriginalAccel;
	float OriginalAirAccel;

	// used to reference audiosource
	AudioSource _audioSource;
 
    // This will be used to only allow the double jump if it's before highest jump point. Or not!
	private bool CanAfterHighestJumpPoint = true;
	
    // We'll use this to communicate with the playerMove.
	private PlayerMove playerMove;
 
    // This is to keep track if we have double jumped already or if we can double jump      
	private bool CanDoubleJump = true;
   
	// In case of us wanting to only double jump if we're still going up on our jump, we need to store the last Y position.
	private float LastY;
    
	// We'll pick the grounded bool from the Animator and store here to know if we're grounded.
	private bool GroundedBool;
	
    // Use this for initialization
	void Start() {
        // Grab the Player Move component
		playerMove = GetComponent<PlayerMove>();
		
		//Set the original speeds
		OriginalSpeed = playerMove.maxSpeed;
		OriginalAccel = playerMove.accel;
		OriginalAirAccel = playerMove.airAccel;
		
		// Grab the sound component
		_audioSource = GetComponent<AudioSource>();
	}

	// this function will set all static booleans for powerups to false
	public void DisableAllPowerups() {
		doubleJumpEnabled = false;
		spinAttackEnabled = false;
		speedBoostEnabled = false;
		invincibilityEnabled = false;
	}

    // Update is called once per frame
	void Update() {
		
    	//If jump button pressed, and player is able -> perform a double jump
		if (Input.GetButtonDown ("Jump") && !GroundedBool && CanDoubleJump && doubleJumpEnabled) {
            //Do a jump with the specified jump force
			playerMove.Jump (doubleJumpForce);
			playerMove.animator.SetTrigger ("DoubleJump");

			// Create a particle effect & play a sound
			if(jumpParticle) Instantiate(jumpParticle, transform.position, Quaternion.Euler(Vector3.zero));
			if(_audioSource) _audioSource.PlayOneShot(doubleJumpSound, 0.7F);
			
            // disable doublejump until player lands
			CanDoubleJump = false;
		}

		//boost the player
		if (speedBoostEnabled) {
			// if the middle mouse button is down, give the player a speedboost
			if (Input.GetKeyDown(KeyCode.Mouse2)) {
				
				//apply the speedboost
				if (playerMove.maxSpeed != (playerMove.maxSpeed + boostAmount)) playerMove.maxSpeed += boostAmount;
				if (playerMove.accel != (playerMove.accel + boostAmount)) playerMove.accel += boostAmount;				
				if (playerMove.airAccel != (playerMove.airAccel + boostAmount)) playerMove.airAccel += (boostAmount/4);
				
				// Create a particle effect and play a sound if they exist
				if (boostParticle) Instantiate(boostParticle, transform.position, Quaternion.Euler(Vector3.zero));
				if (boostSound) _audioSource.PlayOneShot(boostSound, 0.7F);

				// set the animation boolean to true
				playerMove.animator.SetBool ("SpeedUp", true);
			}
			// remove the speedboost when the button is released
			if (Input.GetKeyUp(KeyCode.Mouse2)) {
				 
				// remove the boost
				if (playerMove.maxSpeed!=OriginalSpeed) playerMove.maxSpeed = OriginalSpeed;
				if (playerMove.accel!=OriginalAccel) playerMove.accel = OriginalAccel;
				if (playerMove.airAccel!=OriginalAirAccel) playerMove.airAccel = OriginalAirAccel;

				// Set an animation boolean to false
				playerMove.animator.SetBool ("SpeedUp", false);
			}
		}
	}
	
    // This is an udpate that is called less frequently
	void FixedUpdate () {
        
		// Grab the Grounded Bool from the animator
		GroundedBool = playerMove.animator.GetBool("Grounded");

		//If I can't double jump, but I am grounded
		if(!CanDoubleJump && GroundedBool) {
            // Then... the player should be able to double jump again
			CanDoubleJump = true;
		}

        // If shouldn't be able to double jump after reaching the highest jump point, and player isnt grounded
		if(!CanAfterHighestJumpPoint && !GroundedBool) {
            //If my current Y position is less than my Previously recorded Y position, then I'm going down
			if(gameObject.transform.position.y < LastY) {
                //Since player is going down, they shouldn't be able to double jump anymore.
				CanDoubleJump = false;
			}
            //Then, record the LastY position for a check later.
			LastY = gameObject.transform.position.y;
		}
	}
}