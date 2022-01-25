using UnityEngine;
using System.Collections;

public class ObjectDestructor : MonoBehaviour {

	public float timeOut = 2.0f;
	public bool detachChildren = false;

	// Use this for initialization
	void Awake() {
		// invoke the Destroy method after the timeOut
		Invoke ("DestroyObject", timeOut);
	}

	void DestroyObject() {
		if (detachChildren) { // detach the children before destroying if specified
			transform.DetachChildren ();
		}
		// destroy the gameObject
		Destroy(gameObject);
	}
}