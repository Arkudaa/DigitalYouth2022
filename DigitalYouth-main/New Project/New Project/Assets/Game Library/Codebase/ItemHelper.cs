using UnityEngine;
using System.Collections;

public class ItemHelper : MonoBehaviour {

	// rotation
	[Header("Rotation")]
	public bool rotate;
	public Vector3 rotationAmount;

	//levitation
	[Header("Levitation")]
	public bool levitate;
	public float levitationAmount;

	// particles and sounds
	[Header("Misc")]
	public GameObject spawnParticle;
	public GameObject destroyedParticle;
	public AudioClip collectSound1;
	public AudioClip collectSound2;

	void Update() {
		// if rotate and levitate are true, then do it with the specified parameters
		if(rotate) transform.Rotate (rotationAmount * Time.deltaTime, Space.World);
		if (levitate) transform.Translate(Vector3.up * Mathf.Cos(Time.timeSinceLevelLoad)* levitationAmount * .01f, Space.World);
	}

	protected void OnDisable() {

		if (Time.timeSinceLevelLoad > 1 && !UIManager.paused) {
			// if there is a particle to spawn, spawn it
			if (destroyedParticle) Instantiate(destroyedParticle, transform.position, Quaternion.Euler(Vector3.zero));

			// Play either of the two sounds if set
			if (collectSound1) PlayDestroySoundAt(collectSound1, transform.position);
			if (collectSound2) PlayDestroySoundAt(collectSound2, transform.position);	
		}
	}

	// This function is called when the object becomes enabled and active.
	protected void OnEnable() {
		// if there is a spawn particle, spawn it when the object is enabled
		if (spawnParticle) {
			Instantiate(spawnParticle, this.transform.position, Quaternion.Euler(Vector3.zero));
		}
	}

	// plays a clip without doppler effect
	AudioSource PlayDestroySoundAt(AudioClip clip, Vector3 pos) {
		GameObject tempGO = new GameObject("TempAudio"); // create the temp object
		tempGO.transform.position = pos; // set its position
		AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
		aSource.clip = clip; // define the clip

		// remove doppler effect and lower the volume
		aSource.dopplerLevel = 0;
		aSource.volume = .5f;

		aSource.Play(); // start the sound
		Destroy(tempGO, clip.length); // destroy object after clip duration
		return aSource; // return the AudioSource reference
	}
}