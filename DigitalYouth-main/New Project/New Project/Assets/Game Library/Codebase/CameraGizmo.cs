using UnityEngine;
using System.Collections;

public class CameraGizmo : MonoBehaviour {

	// variable fields
	public Transform target;
	public GameObject canvas;
	public Vector3 bounds;

	void Start() {
		// Calculate the size of the canvas
		bounds = RectTransformUtility.CalculateRelativeRectTransformBounds (canvas.transform).size;
		float distance = bounds.y * 0.5f / Mathf.Tan (Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
		transform.position = canvas.transform.position + (distance * -canvas.transform.forward);

		// Log the canvas bounds 
		// Debug.Log ("Bounds:" + bounds);
	}

	void Update() {
		// transform.forward = canvas.transform.forward;
		// transform.rotation = canvas.transform.localRotation;
	}

	void OnDrawGizmos() {
		// Draw the specified gizmos
		Gizmos.DrawIcon ( transform.position, "camera.png", true );
		Gizmos.DrawWireCube (transform.position, Vector3.one);
		//	Gizmos.DrawLine (transform.position, target.position);
		//	Gizmos.DrawFrustum ( transform.TransformPoint ( transform.position ), Camera.main.fieldOfView, 100, 1, Camera.main.aspect);
	}
}