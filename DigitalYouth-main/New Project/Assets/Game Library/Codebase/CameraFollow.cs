using UnityEngine;

public class CameraFollow : MonoBehaviour {
	
	// Objects camera will follow and focus on
	public Transform target;
	public Transform lookFrom;

	// how far back should camera be from the look target
	public Vector3 targetOffset =  new Vector3(0f, 3.5f, 7);

	// When enabled, locks rotation behind look target
	public bool lockRotation;

	// When enabled, mouse rotates camera
	public bool mouseFreelook;

	// When enabled, camera follows player
	public bool followPlayer = true;

	// Camera movement controls
	public float followSpeed = 6;				//how fast the camera moves to its intended position
	public float inputRotationSpeed = 100;		//how fast the camera rotates around lookTarget when you press the camera adjust buttons
	public float rotateDamping = 100;			//how fast camera rotates to look at target

	// Camera-Object interaction
	public GameObject waterFilter;				//object to render in front of camera when it is underwater
	public string[] avoidClippingTags;			//tags for big objects in your game, which you want to camera to try and avoid clipping with

	// private variables
	private Transform followTarget;
	private bool camColliding;
	private float CamX;
	private float followtarY;

	// Q - other variables
	public float angleMax = 30.0f;
	private Vector3 initialVector = Vector3.forward;

	void Awake() {
		//create empty gameObject as camera target, this will follow and rotate around the player
		followTarget = new GameObject().transform;
		followTarget.name = "PlayerFTarget";

		// disable water filter by default
		if(waterFilter) {
			waterFilter.GetComponent<Renderer>().enabled = false;
		}

		// Send error log if there is no target
		if(!target) {
			Debug.LogError("'CameraFollow script' has no target assigned to it", transform);
		}

		//don't smooth rotate if using mouselook
		if(mouseFreelook) {
			rotateDamping = 0f;
		}
	}
	
	//run our camera functions each frame
	void Update() {
		// if there is no target, break from update
		if (!target){
			return;
		}

		// Run the SmoothFollow function
		SmoothFollow();

		// if damping is set, look at the target smoothly
		if (rotateDamping > 0) {
			SmoothLookAt();
		} else {
			transform.LookAt(target.position);
		}
	}

	void OnTriggerEnter(Collider other) {
		// if camera enters water, render water filter
		if (other.tag == "Water" && waterFilter) {
			waterFilter.GetComponent<Renderer>().enabled = true;
		}
	}

	void OnTriggerExit(Collider other) {
		// if camera exits water, disable water filter
		if (other.tag == "Water" && waterFilter) {
			waterFilter.GetComponent<Renderer>().enabled = false;
		}
	}
	
	// rotates camera smoothly toward the target
	void SmoothLookAt() {
		Quaternion rotation = Quaternion.LookRotation (target.position - transform.position);
		transform.rotation = Quaternion.Slerp (transform.rotation, rotation, rotateDamping * Time.deltaTime);
	}

	// moves camera smoothly toward its target
	void SmoothFollow() {
		//move the followTarget object to correct position each frame
		followTarget.position = target.position;// + lookUp;
		if (followPlayer) {
			followTarget.Translate (targetOffset, Space.Self);
		} else {
			followTarget.rotation = Quaternion.Euler (new Vector3 (0, 0, 0));
			followTarget.Translate (lookFrom.position - target.position);
		}

		if (lockRotation) {
			followTarget.rotation = target.rotation;
		}
			
		if (mouseFreelook) {
			//mouse look
			float axisX = Input.GetAxis ("Mouse X") * inputRotationSpeed * Time.deltaTime;
			followTarget.RotateAround (target.position, Vector3.up, axisX);
			float axisY = Input.GetAxis ("Mouse Y") * inputRotationSpeed * Time.deltaTime;
			followTarget.RotateAround (target.position, -Vector3.right, axisY);
		} else {
			//keyboard camera rotation look
			float axis = Input.GetAxis ("CamHorizontal") * inputRotationSpeed * Time.deltaTime;
			followTarget.RotateAround (target.position, Vector3.up, axis);
		}

		Vector3 currentVector = transform.position - target.position;
		currentVector.y = 0;
		float rotateDegrees = 0f;
		float anglebetween = Vector3.Angle (initialVector, currentVector) * (Vector3.Cross (initialVector, currentVector).y > 0 ? 1 : -1);
		//float newAngle = Mathf.Clamp (anglebetween + rotateDegrees, 10, 9);

		//where should the camera be next frame?
		Vector3 nextFramePosition = Vector3.Lerp (transform.position, followTarget.position, followSpeed * Time.deltaTime);
		Vector3 direction = nextFramePosition - target.position;

		//raycast to this position
		if (followPlayer) {
			RaycastHit hit;

			float offSetY = 1.5f;
			if (nextFramePosition.y < target.position.y + offSetY) nextFramePosition.y = target.position.y + offSetY;  

			if (Physics.Raycast (target.position, direction, out hit, direction.magnitude + 0.3f)) {
				//Debug.Log ("hit something");
				transform.position = nextFramePosition;
				
				foreach (string tag in avoidClippingTags)
					if (hit.transform.tag == tag)
						transform.position = hit.point - direction.normalized * 0.3f;
			} else {
				//otherwise, move cam to intended position
				transform.position = nextFramePosition;
			}
		} else {
			transform.position = nextFramePosition;
		}
	}
}