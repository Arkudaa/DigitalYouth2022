using UnityEngine;
using System.Collections;

/*	The purpose of this is ONLY to broadcast events, which can be prompted from other game objects*/

public class CentralEventBroadcaster : MonoBehaviour {
 	
	// Delegates
	public delegate void emptyDelegate();
	public delegate void directionDelegate ( Vector2 direction );
	public delegate void positionDelegate ( Vector3 position );
	public delegate void floatDelegate ( float time );
	public delegate void sceneTypeDelegate ( SceneHandler.sceneType newScene );

	public static event emptyDelegate OnLevelLoaded;
	//public static event emptyDelegate OnLevelLoaded;
	//public static event emptyDelegate OnLoadGameLevel;

	public static void BroadcastOnLevelLoaded() {
		//	for some reason this doesnt work
		if (OnLevelLoaded != null){
			Debug.Log("<b>CentralEventBroadcaster: OnLevelLoaded</b>");
			OnLevelLoaded();
		}
	}
}