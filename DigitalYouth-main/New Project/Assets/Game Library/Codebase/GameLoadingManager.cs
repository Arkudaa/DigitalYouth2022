using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameLoadingManager :  Singleton<GameLoadingManager>
{
	public void LoadSceneAysnc(string scene) {
		if (scene != null) {
			StartCoroutine (AsynchronousLoad (scene, () => {
				Debug.Log ("Finished Loading Scene.");
				CentralEventBroadcaster.BroadcastOnLevelLoaded ();
			})
			);
		} else {
			Debug.LogError ("No Next Scene Defined");
		}
	}
 
	// Asychronously loads a level
	IEnumerator AsynchronousLoad(string scene, Action onComplete) {
		AsyncOperation ao = SceneManager.LoadSceneAsync (scene);
		ao.allowSceneActivation = true;

		// Note that if allowSceneActivation is FALSE, ao.isDone will stall at 90%
		while (!ao.isDone) {
			float progress = ao.progress;//Mathf.Clamp01 (ao.progress); 
			Debug.Log ("Loading Progress: " + (progress * 100) + "%");
			yield return null;
		}

		Debug.Log("Done loading");
		// Call action on complete
		onComplete ();
	}

	// Load the actual game scene, from a string parameter setup on SceneHandler game Object
	public void LoadNextScene() {
		string scenePath = GameObject.Find ("SceneHandler").GetComponent<SceneHandler> ().scenePath;

		// Remove the '.Unity' string
		string unityString = ".Unity";
		if (scenePath.Length > unityString.Length)
			scenePath = scenePath.Substring (0, scenePath.Length - unityString.Length);

		// Remove the 'assets/My game/scene' from the string
		string pathString = "Assets/";
		if (scenePath.Length > pathString.Length)
			scenePath = scenePath.Substring (pathString.Length);

		GameLoadingManager.Instance.LoadSceneAysnc (scenePath);
	}
}