using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
 
	//Information about our projectile... how much damage? How much should it push
	//the victim on height? how much force should add on the push? how fast should it go?
	public int ProjDamage = 1;
	public float ProjSpeed = 20f;
	public float PushForce = 10f;
	public float PushHeight = 2f;

	//set variable for rigidbody
	private Rigidbody rb;
	
	//Let's store our owner so we don't hit him.
	[HideInInspector]
	public GameObject Owner;
	
	//This time is to make sure it won't be running forever if it doesn't hit anything.
	//If you want to limit the range of the projectile set this to a lower value.
	public float selfDestructTime = 3f;
	
	//It needs a DealDamage script, so we'll store one here.
	private DealDamage DoDamage;
	//We'll use this bool to check if we have been initiated by the player.
	private bool Initiated = false;

	void Start() {
		//Create a Deal Damage Component for us.
		DoDamage = gameObject.AddComponent<DealDamage> ();
		// get rigidbody component
		rb = GetComponent<Rigidbody> ();
	}
 
	//This function will be called from the player to start our projectile.
	public void Initialize() {
		Initiated = true;
		// Set the forward direction of it's parent (so it fires the direction it's owner is facing)
		transform.forward = Owner.transform.forward;
		//This will start our countdown to self destruction
		Destroy(this.gameObject, selfDestructTime);
		//StartCoroutine (selfDestruct (selfDestructTime));
	}

	void FixedUpdate() {
		//If it has been initiated by the player
		if (Initiated) {
			//Tell the Rigid Body to move forward at the desired speed
			rb.MovePosition (rb.transform.position + (transform.forward * ProjSpeed * Time.deltaTime));
		}
	}
	
	//This function runs for each collider on our trigger zone, on each frame they are on our trigger zone.
	void OnTriggerEnter(Collider other) {
		//If it hasn't been initiated by the player, just stop here.
		if (!Initiated) {
			return;
		}
		//Was an Owner set for us? It's probably an enemy, let's check, if it is we'll let him hit the player
		if (Owner) {
			//Am I the owner? If I am I don't want to hit myself so return
			if (Owner == other.gameObject) {
				return;
			}
			//I'm not the owner, but the owner is an enemy, so if the collider is a player we should...
			if (Owner.tag == "Enemy" && other.gameObject.tag == "Player") {
				if (other.attachedRigidbody) {
					
					//Deal Damage
					DoDamage.Attack (other.gameObject, ProjDamage, 0, 0);
					
					//push from Owner Enemy
					Vector3 pushDir = (other.transform.position - Owner.transform.position);
					pushDir.y = 0f;
					pushDir.y = PushHeight * 0.1f;
					if (other.GetComponent<Rigidbody> () && !other.GetComponent<Rigidbody> ().isKinematic) {
						other.GetComponent<Rigidbody> ().velocity = new Vector3 (0, 0, 0);
						other.GetComponent<Rigidbody> ().AddForce (pushDir.normalized * PushForce, ForceMode.VelocityChange);
						other.GetComponent<Rigidbody> ().AddForce (Vector3.up * PushHeight, ForceMode.VelocityChange);
					}		
					Destroy (this.gameObject);
					return;
				}
			} 

		// prevent the bullet spawned by the enemy from immediately destroying the enemy
		} else {

			//If the object colliding doesn't have the tag player and is not a trigger...
			if (other.gameObject.tag != "Player" && !other.isTrigger) {
				//If it's a rigid body, tell our DealDamage to attack it!
				if (other.attachedRigidbody) {
					//This was the bit of code that was letting Enemys shoot other enemys, becasue it didn't have
					//the check to make sure the owner was the player
					if (Owner.tag == "Player")
						DoDamage.Attack (other.gameObject, ProjDamage, PushHeight, PushForce);
				}
				//If it isn't we still probably want to destroy our projectile since it has a collider, so destroy it wether it is a rigid body or not.
				Destroy (this.gameObject);
			}
		}
	}

	//Coroutine to wait for the set amount of seconds and destroy itself.
	IEnumerator selfDestruct(float waitTime) {
		yield return new WaitForSeconds (waitTime);
		Destroy (this.gameObject);
	}
}