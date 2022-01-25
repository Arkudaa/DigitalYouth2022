using UnityEngine;
using System.Collections;

/* This makes the camera follow a target with an optional look target. Note that when
 * processing manual rotation, the values for how quickly the camera rotates/resets 
 * can be found in the input manager
 */

public class CameraTrack : MonoBehaviour
{

	[SerializeField]
	[Header ("The target to track.")]
	public Transform target;
	[Header ("Where the camera will look while tracking.")]
	public Transform lookTarget;

	[SerializeField]
	private float m_heightDamping = 2.0f;
	[SerializeField]
	private float m_rotationDamping = 2.0f;
	[SerializeField]
	private float m_distance = 6.0f;
	[SerializeField]
	private float m_height = 3.0f;
	[SerializeField]
	[Header ("Maximum manual rotation")]
	private float m_manualRotationScalar = 100f;

	private float m_manualRotation = 0.0f;

	void Awake() {
		// If no lookTarget is specified, then use the target that is being tracked
		if (target == null) {
			Debug.LogWarning("You need to assign the camera a target");
		}
		if (lookTarget == null) {
			Debug.LogWarning ("No look target is set, using the target instead");
			lookTarget = target;
		}
	}

	// Update is called once per frame
	void Update() {
		// No delta time required for getAxis
		m_manualRotation = Input.GetAxis ("CamHorizontal") * m_manualRotationScalar;
	}

	// We put camera movement in LateUpdate, after any values to do with the camera are processed within Update()
	void LateUpdate() {
		FollowCamera ();
	}

	void FollowCamera() {
		// set the desired rotation to that of the target
		float wantedRotationAngle = target.eulerAngles.y + m_manualRotation;
		float wantedHeight = target.position.y + m_height;

	 	//	Debug.Log ("Target Y: " + target.eulerAngles.y+ " ManualRotation: "+m_manualRotation+ " Total Rotation: "+wantedRotationAngle);

		float currentRotationAngle = transform.eulerAngles.y;
		float currentHeight = transform.position.y;

		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, m_rotationDamping * Time.deltaTime);

		// Damp the height
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, m_heightDamping * Time.deltaTime);
		// Debug.Log ("Curr Height: " + currentHeight + " Wanted: " + wantedHeight + " heightDamp * delta: " + m_heightDamping * Time.deltaTime);
		// Convert the angle into a rotation
		Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);

		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		transform.position = target.position;
		transform.position -= currentRotation * Vector3.forward * m_distance;

		// Set the height of the camera
		transform.position = new Vector3 (transform.position.x, currentHeight, transform.position.z);

		// Look at target
		transform.LookAt (lookTarget);
	}
}