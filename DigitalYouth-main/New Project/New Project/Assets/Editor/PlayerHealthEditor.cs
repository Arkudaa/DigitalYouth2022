using UnityEngine;
using UnityEditor;
 
[CustomEditor(typeof(PlayerHealth))]
public class PlayerHealthEditor : Editor
{

	public override void OnInspectorGUI()
	{
		PlayerHealth myTarget = (PlayerHealth)target;
		// Force it to redraw
		EditorUtility.SetDirty (myTarget);
		serializedObject.Update ();
		DrawDefaultInspector ();
  

		myTarget.currentHealth = EditorGUILayout.IntSlider ("Player Start Health", myTarget.currentHealth, 0, 10);
	 
		serializedObject.ApplyModifiedProperties();

	}

}
 