using UnityEngine;
using UnityEditor;
 
[CustomEditor(typeof(Debugger))]
public class GameManagerEditor : Editor
{

	public override void OnInspectorGUI()
	{
		Debugger Debugger = (Debugger)target;

		serializedObject.Update ();
		DrawDefaultInspector ();

		if(GUILayout.Button("Destroy all Enemies")){
			Debugger.DestroyAllEnemies();
		}

		if(GUILayout.Button("Collect all Coins")){
			Debugger.CollectAllCoins();
		}

		if(GUILayout.Button("Destroy Player")){
			Debugger.DestroyPlayer();
		}

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Enable Powerups")){
			Debugger.EnablePowerups();
		}

		if (GUILayout.Button("Disable Powerups")){
			Debugger.DisablePowerups();
		}
		GUILayout.EndHorizontal();

		serializedObject.ApplyModifiedProperties();
	}

}
 