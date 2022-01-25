using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour {
	
	public enum sceneType 
	{
		menu = 0,
		level = 1
	}
 
	public sceneType m_currentGameScene = sceneType.menu;

	// Caches current scene
	private Scene currentScene;

	[SerializeField, HideInInspector]
	public string scenePath;

	public static SceneHandler Instance;
  
	void Awake() {
		// Execute logic in base singleton class
		//base.Awake ();
		if ( Instance == null ) Instance = this;
		// Debug.Log ("SceneHandler Created");
		currentScene = ReturnSceneManagerCurrentScene();
	}

	public Scene ReturnSceneManagerCurrentScene() {
		return SceneManager.GetActiveScene();
	}

	// Return Current Scene
	public sceneType ReturnCurrentScene() {
		return m_currentGameScene;
	}
}