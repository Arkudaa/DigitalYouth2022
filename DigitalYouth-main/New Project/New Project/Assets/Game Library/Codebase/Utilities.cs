using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;


/// <summary>
/// This is a bunch of helper tasks that we can use throughout the game.
/// </summary>
namespace Helper {
	public class Utilities : MonoBehaviour {
		// Sets the time scale of Unity alowing us to pause/resume or slow the game down
		public static void SetTimeScale(float value) {
			Time.timeScale = value;
		}

		// Reloads current level - note this will have to be revisited and probably put in it's own class with other scene management tasks
		public static void ReloadLevel() {
			Scene scene = SceneManager.GetActiveScene();
			//Debug.Log("Active scene is '" + scene.name + "'.");
			// Reload Active Scene
			SceneManager.LoadScene(scene.name);
		}

		public static IEnumerator BlendUIColour(Image whichImage, Color targetColour, float duration, float pauseBeforeStart = 0.0f) {
			Color originalColour = whichImage.color;
			float timer = 0;
			yield return new WaitForSeconds(pauseBeforeStart);

			while (timer < duration) {
				whichImage.color = Color.Lerp (originalColour, targetColour, timer / duration);
				timer += Time.deltaTime;
				yield return null;
			}
			// Debug.Log ("<b>BlendUIColour Finished </b>");
		}

		// Simple wait coroutine
		public static IEnumerator Wait(float time, Action onComplete) {
			yield return new WaitForSeconds(time);
			onComplete();
		}
	}
}