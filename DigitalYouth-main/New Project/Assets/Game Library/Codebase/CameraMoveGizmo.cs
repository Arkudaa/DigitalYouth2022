using UnityEngine;
using System.Collections;

public class CameraMoveGizmo : MonoBehaviour {

	// object to draw line to
	public Transform target;

	void OnDrawGizmos() {
		// Draw camera gizmo on this game object and draw line to target transform
		Gizmos.color = Color.red;
		Gizmos.DrawIcon ( transform.position, "camera.png", true );
		Gizmos.DrawWireCube (transform.position, Vector3.one);
	 	Gizmos.DrawLine (transform.position, target.position);
	}
}