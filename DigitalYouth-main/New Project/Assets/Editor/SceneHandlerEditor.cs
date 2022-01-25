using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneHandler), true )]

public class SceneHandlerEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();
		EditorGUILayout.HelpBox ("Ensure there is one of these game objects in every scene! It describes the type of scene which drives the UI to show. ",MessageType.Warning);

		EditorGUILayout.Separator ();
		// Get access to the script the editor is based on
		SceneHandler picker = (SceneHandler)target;
     
		var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset> (picker.scenePath);
 
		// Show message depending on whether there's a scene
		if (oldScene == null) {
			EditorGUILayout.HelpBox ("Drag a scene to load next!", MessageType.Error);
		} else {
			EditorGUILayout.HelpBox ("Loading scene '" + oldScene.name + "' next!",MessageType.Info);
		}

		// Sync it with the component it represents
		serializedObject.Update ();

		// Check if any controls were changed inside the following code
		EditorGUI.BeginChangeCheck ();

		// 
		var newScene = EditorGUILayout.ObjectField ("Scene", oldScene, typeof(SceneAsset), false) as SceneAsset;

		// Returns true if a control has been changed
		if (EditorGUI.EndChangeCheck ()) {
			var newPath = AssetDatabase.GetAssetPath (newScene);
			var ScenePathProperty = serializedObject.FindProperty ("scenePath");
			ScenePathProperty.stringValue = newPath;
		}

		// Commit any changes
		serializedObject.ApplyModifiedProperties ();

	}

}