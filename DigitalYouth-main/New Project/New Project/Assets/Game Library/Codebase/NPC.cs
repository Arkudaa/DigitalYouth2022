using UnityEngine;
using System.Collections;

public abstract class NPC : MonoBehaviour {

	private GameObject m_speech;
	private AudioSource SpeechSound;

	public Transform CameraMoveHere;
	public Transform CameraLookHere;

	public static Dialogue Speech;
	//private TriggerParent sightTrigger;

	// Abstract functions that the students will implement
	public abstract void OnSetUpDialogue();
	//public abstract void OnUpdate ();
	public abstract void OnTriggerNPC(Collider coll);

	// private variables for optimization
	private PlayerMove pMove;
	private Rigidbody pRB;
	private CameraFollow cFollow;
	private Transform oTarget;

	void Start() {
		Speech = FindObjectOfType(typeof(Dialogue)) as Dialogue;

		// find the audio source
		SpeechSound = GetComponent<AudioSource>();

		// Calls student functions
		OnSetUpDialogue();
	}

	void TriggerDialogue() {

		if (!GetComponent<Dialogue>().complete) {
			cFollow.mouseFreelook = false;

			// Set Player Values
			pRB.isKinematic = false;
			pRB.velocity *= 0f;
			pMove.ForceIdle(); 	
			pMove.enabled = false;

			// Set Camera Values
			cFollow.target = CameraLookHere;
			cFollow.lookFrom = CameraMoveHere;
			cFollow.followPlayer = false;
			//	UIManager.Instance.FadeIn();

			// play the speech sound
			if (SpeechSound) {
				SpeechSound.Play(5);
			}
		}
	}

	// Triggered when the dialogue ends
	public void EndDialogue() {
		UIManager.Instance.DisableText();

		//Restore Camera values
		cFollow.enabled = true;
		cFollow.followPlayer = true;
		cFollow.lookFrom = null;
		cFollow.mouseFreelook = true;
		cFollow.target = oTarget;

		// Restore Player Values
		pMove.enabled = true;
		pRB.isKinematic = false;
	}

	/*
	IEnumerator PollForActiveCutscene()
	{
		Cutscene[] gameCutscenes = FindObjectOfType<Cutscene> ();
		foreach (Cutscene cutscene in gameCutscenes) {
			if ( cutscene.enabled ){
			}
		}
	}
	*/

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			// Grab player and camera references
			pMove = other.gameObject.GetComponent<PlayerMove>();
			pRB = other.gameObject.GetComponent<Rigidbody>();
			cFollow = Camera.main.GetComponent<CameraFollow>();
			oTarget = cFollow.target;

			TriggerDialogue();

			// Calls individual NPC function
			OnTriggerNPC(other);
		}
	}
}