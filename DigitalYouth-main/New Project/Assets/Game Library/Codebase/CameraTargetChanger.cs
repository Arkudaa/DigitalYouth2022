using UnityEngine;
using System.Collections;

public class CameraTargetChanger : MonoBehaviour {

	// public variables
	public Transform Target1;
	public Vector3 TargetOffset;
	public float TargetFollowSpeed;
	public bool OnlyTriggerOnce = true;
	public bool TargetIsPlayer = false;

	// private variables
	CameraFollow cFollow;
	PlayerMove pMove;

	// Update is called every frame
	void Update() {
		// log the value of Target1
		//Debug.Log(Target1);	
	}

	void OnTriggerEnter(Collider other) {
		//If what entered our trigger has the TAG Player...
		if(other.gameObject.tag == "Player") {
			// ask the Camera to give us the Camera Follow component
			cFollow = Camera.main.GetComponent<CameraFollow>();
			cFollow.followSpeed = TargetFollowSpeed;
			if (TargetIsPlayer) {
				cFollow.target = other.gameObject.transform;
			} else {
	    		cFollow.target = Target1;
	    	}
			cFollow.targetOffset = TargetOffset;
	    	
			//Now, if we want to trigger this again, and not only once...
			if (OnlyTriggerOnce) {
				gameObject.GetComponent<Collider>().enabled=false;
			}
		}
	}
}