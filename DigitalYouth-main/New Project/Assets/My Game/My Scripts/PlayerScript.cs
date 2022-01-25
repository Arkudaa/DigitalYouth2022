using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	

	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	protected void Start() {
		
	}

	// OnTriggerEnter is called when the Collider "collided" enters the trigger.
	protected void OnTriggerEnter(Collider collided) {

		// Check for collision with coins
		if (collided.gameObject.tag == "Coin") {  
			
			Destroy(collided.gameObject);
		}
	}
}