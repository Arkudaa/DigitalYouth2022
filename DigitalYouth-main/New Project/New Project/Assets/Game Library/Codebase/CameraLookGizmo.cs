using UnityEngine;
using System.Collections;

public class CameraLookGizmo : MonoBehaviour {
 
	void OnDrawGizmos(){
		// Draw Eye gizmo on this game object
		Gizmos.DrawIcon ( transform.position, "eye.png", true );
		Gizmos.DrawWireCube (transform.position, Vector3.one);
	}
}