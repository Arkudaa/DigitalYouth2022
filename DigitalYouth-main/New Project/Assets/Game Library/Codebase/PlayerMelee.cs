using UnityEngine;
using System.Collections;
using System.Collections.Generic;
	
public class PlayerMelee : MonoBehaviour {

	//We'll use those 3 to communicate with the rest of the kit.
	private PlayerMove playerMove;
	private CharacterMotor characterMotor;
	private DealDamage DoDamage;
	
	//This will be used to cycle through 2 different punch animations
	private bool punchleft;
	//This will be used to check if we should ge giving damage.
	[HideInInspector]
	public bool Punching;
	private GameObject spawnedParticle;

	private List<GameObject> BeingPunched = new List<GameObject>();
	
	//These are our public vars, PunchHitBox should be like the GrabBox,
	//and should encompass the area you want your punch to cover
	public Collider PunchHitBox;	
	public GameObject SpinParticle;
	
	public int PunchDamage = 1;
	public float PushHeight = 4;
	public float PushForce =10;
	
	// Use this for initialization
	void Start() {
		//We're supposed to be on the same gameobject as the PlayerMove,
		//CharacterMotor etc, so lets get them as reference!
		playerMove = GetComponent<PlayerMove>();
		characterMotor = GetComponent<CharacterMotor>();
		DoDamage = GetComponent<DealDamage>();
		
		//Did you even make a PunchBox? Or you were lazy and didn't make one?
		if (!PunchHitBox) {
			GameObject GameObjectPunchHitBox = new GameObject();
			PunchHitBox = GameObjectPunchHitBox.AddComponent<BoxCollider>();
			GameObjectPunchHitBox.GetComponent<Collider>().isTrigger = true;
			GameObjectPunchHitBox.transform.parent = transform;
			//Let's place it a little but farther away from us.
			GameObjectPunchHitBox.transform.localPosition = new Vector3(0f, 0f, 1f);
			//It should Ignore Raycast so let's put it on layer 2.
			GameObjectPunchHitBox.layer = 2;
			Debug.LogWarning("player melee hitbox generated", GameObjectPunchHitBox);
		}
		
		//Also lets turn off the PunchHitBox, we'll only turn that on while punching
		//so the Grabbing script doesn't get confused with it.
		PunchHitBox.enabled=false;
	}
	
	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.Mouse0) && Abilities.spinAttackEnabled && !UIManager.Instance.pausePanelObject.active) {
			
			//see if the spin animation is playing on Layer 1 (The one with the arms).
			if (!CheckIfPlaying("Spin",0)) {
				
				//no spin animation is playing so we can spin-to-win now :D
				playerMove.animator.SetTrigger("Spin");

				StartCoroutine(WaitAndPunch());
				if (SpinParticle) {
					spawnedParticle = Instantiate(SpinParticle, this.transform.position + new Vector3(0,0.8f,0), Quaternion.Euler(Vector3.zero)) as GameObject;
					spawnedParticle.transform.parent = transform;
				}
			}
		}
	}
	
	IEnumerator WaitAndPunch() {
    	//Wait for 0.12f time and then punch them in their stupid faces!
        yield return StartCoroutine(Wait(0f));
        PunchThem();
    }
    
    IEnumerator WaitAndStopPunch() {
    	//Wait for 0.1f time before stopping the punch
    	yield return StartCoroutine(Wait(0.1f));
    	StopPunch();
    }
	
	//Coroutine for cool waiting stuff
	IEnumerator Wait(float duration) {
        for (float timer = 0; timer < duration; timer += Time.deltaTime)
            yield return 0;
    }
	
	void PunchThem() {

		//Enable our cool Punching Hitbox to check for enemies in there.
		PunchHitBox.enabled=true;
		//Turn our Punchin bool to true so our TriggerStay will check for people being punched.
		Punching=true;
		//Start the coroutine that will wait for a moment and stop the punching stuff turning bools back to false.
		StartCoroutine(WaitAndStopPunch());
	}
	
	void StopPunch() {
		//Turn stuff back to false so it'll stop checking for people on hitbox
		PunchHitBox.enabled=false;
		Punching=false;

		//Clear the List of people that got punched on this punch.
		BeingPunched.Clear();
	}
	
	//This function runs for each collider on our trigger zone, on each frame they are on our trigger zone.
	void OnTriggerStay(Collider other) {
		//If we're not punching, forget about it, just stop right here!
		if (!Punching) {
			return;
		}

		//If we are punching, and the tag on our trigger zone has a RigidBody and it's not tagged Player then...
        if (other.attachedRigidbody && other.gameObject.tag!="Player") {
        	//If this guy on our trigger zone is not on our List of people already punched with this punch
        	if(!BeingPunched.Contains(other.gameObject)) {
        		//Call the DealDamage script telling it to punch the hell out of this guy
            	DoDamage.Attack(other.gameObject,PunchDamage,PushHeight,PushForce);

            	//Add him to the list, so we won't hit him again with the same punch.
            	BeingPunched.Add(other.gameObject);
            }
        }
    }
	
	//This will return a TRUE/FALSE on the animation we want to check if is playing.
	bool CheckIfPlaying(string Anim, int Layer) {
		//Grabs the AnimatorStateInfo out of our PlayerMove animator for the desired Layer.
		AnimatorStateInfo AnimInfo = playerMove.animator.GetCurrentAnimatorStateInfo(Layer);
		//Returns the bool we want, by checking if the string ANIM given is playing.
		return AnimInfo.IsName(Anim);
	}
}