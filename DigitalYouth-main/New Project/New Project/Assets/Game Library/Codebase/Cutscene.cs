using UnityEngine;
using System.Collections;

public class Cutscene : MonoBehaviour {

	CameraFollow cFollow;
	CameraFollow cTrack;

	PlayerMove pMove;
	Rigidbody pRB;
	public static Collider collider;

	public float DelayStart = 0;

	[Header("Camera 1")]
	public Transform CameraMoveHere1;
	public Transform CameraLookHere1;
	public float CameraMoveSpeed1;
	public float ShowTarget1ThisLong;
	public AudioClip audioToPlay1;
	public string optionalMessage1;
	//public bool shakeCamera1;

	[Header("Camera 2")]
	public Transform CameraMoveHere2;
	public Transform CameraLookHere2;
	public float CameraMoveSpeed2;
	public float ShowTarget2ThisLong;
	public AudioClip audioToPlay2;
	public string optionalMessage2;
	//public bool shakeCamera2;

	[Header("After Cutscene")]
	public bool fadeOut;
	public bool loadNextScene;

	// private variables
	private Transform OriginalTarget;
	private Vector3 OriginalTargetOffset;
	private float OriginalTargetFollowSpeed;
	private AudioSource _audio;
	private Material fader;
	private float alphaAdjust;
	private bool OnlyTriggerOnce = true;
	[Header("Set this if the Cutscene Trigger should be active by default")]
	private bool m_cutsceneTriggerActiveByDefault = true;

	// Awake is called when the script instance is being loaded.
	protected void Awake() {
		
		/*if (m_cutsceneTriggerActiveByDefault) {
			collider.enabled = true;
		} else {
			collider.enabled = false;
		}*/
	}

	// Cinematic routine that changes values of camerafollow one at a time
	IEnumerator StartCinematic() {
		// Grab a reference to the camera and deactivate key features
		cFollow = Camera.main.GetComponent<CameraFollow>();
		cFollow.mouseFreelook = false;

		// This forces the animator controller into an idle state (by forcing isGrounded flag
		// and setting Distance to Target to 0 )
		pRB.velocity *= 0f;
		pMove.ForceIdle();
		pMove.enabled = false;

		if (audioToPlay1 || audioToPlay2 && _audio == null) {
			_audio = gameObject.AddComponent<AudioSource>() as AudioSource;
			_audio.volume = .5f;
		}
			
		// Now we wait as character is in animation and state we want to keep him in
		yield return new WaitForSeconds(DelayStart);

		//Set values for camera 1
		cFollow.followSpeed = CameraMoveSpeed1;
		cFollow.target = CameraLookHere1;
		cFollow.lookFrom = CameraMoveHere1;
		cFollow.followPlayer = false;
		// Now that the cinematic camera has values, enable it
		cFollow.enabled = true;

		if (audioToPlay1) {
			_audio.PlayOneShot(audioToPlay1);
		}

		// If message is set, display it
		if (optionalMessage1 != null && optionalMessage1.Length > 0) {
			HUD.Message(optionalMessage1, ShowTarget1ThisLong);
		}

		yield return new WaitForSeconds(ShowTarget1ThisLong);

		//		if (shakeCamera1) Camera.main.GetComponent<CameraShake>().enabled = false;
		if (audioToPlay2) {
			_audio.PlayOneShot(audioToPlay2);
		}

		if (fadeOut) {
			StartCoroutine(fadeToBlack());
		}

		//Set to our second target
		cFollow.followSpeed = CameraMoveSpeed2;
		cFollow.target = CameraLookHere2;
		cFollow.lookFrom = CameraMoveHere2;

		// If message is set, display it
		if (optionalMessage2 != null && optionalMessage2.Length > 0) {
			HUD.Message(optionalMessage2, ShowTarget2ThisLong);
		}

		//wait a bit more
		yield return new WaitForSeconds(ShowTarget2ThisLong);

		if (loadNextScene) {
			GameLoadingManager.Instance.LoadNextScene();
		}

		//Reset to the values we had before starting the cinematic.
		cFollow.followSpeed = OriginalTargetFollowSpeed;
		cFollow.target = OriginalTarget;
		cFollow.targetOffset = OriginalTargetOffset;
		cFollow.followPlayer = true;
		cFollow.mouseFreelook = true;

		//Enable the player movement back, and set the rigid body iskinematic to false so the rigid body will work again.
		pMove.enabled = true;

		/*Now, if we want to trigger this again, and not only once...
		if (!OnlyTriggerOnce){
			//We'll wait 5f seconds
			yield return new WaitForSeconds(5f);

			// re-enable the collider for this object
			collider.enabled = true;
		}*/
	}

	//This is a fancy wait coroutine for the others to use.
	IEnumerator Wait (float duration) {
		for (float timer = 0; timer < duration; timer += Time.deltaTime)
			yield return 0;
	}

	// use to fade the camera to black
	IEnumerator fadeToBlack() {
		fader = GameObject.Find("FadeFilter").GetComponent<MeshRenderer>().material;
		Color color = fader.color;
		color.a = 0f;
		fader.color = color;

		for (float i = 0f; i < ShowTarget2ThisLong; i += Time.deltaTime) {
			color.a += 2.0f / (ShowTarget2ThisLong * (1f/Time.deltaTime));
			fader.color = color;
			//Debug.Log(fader.color.a);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		color.a = 0f;
		fader.color = color;
	}


	//We should trigger if the player enters our trigger
	void OnTriggerEnter (Collider other) {
		//If what entered our trigger has the TAG Player...
		if (other.CompareTag("Player")) {
			// grab and disable the box collider
			collider = gameObject.GetComponent<BoxCollider>();
			collider.enabled = false;

			// Grab references for the player
			pMove = other.gameObject.GetComponent<PlayerMove>();
			pRB = other.gameObject.GetComponent<Rigidbody>();
			cFollow = Camera.main.GetComponent<CameraFollow>();

			// store the original values
			OriginalTarget = cFollow.target;
			OriginalTargetOffset = cFollow.targetOffset;
			OriginalTargetFollowSpeed = cFollow.followSpeed;

			//And finally... start the Cinematic!
			StartCoroutine(StartCinematic());
		}
	}
}