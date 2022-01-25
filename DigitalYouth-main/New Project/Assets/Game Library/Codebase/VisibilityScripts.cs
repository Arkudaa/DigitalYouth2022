using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This allows us to hide/mark components as non editable and is password protected
[ExecuteInEditMode]
public class VisibilityScripts : MonoBehaviour {
  
	public bool m_notEditable;
	public bool m_hideInInspector;

	[Header("Enter Password to Unlock Visibility Options")]
	public string m_password;

	[SerializeField]
	private List<Object> m_componentList = new List<Object> ();
  
	void Update() {
		// If there's no password, just return
		if (m_password == null)
			return;
		
		if (m_password.Equals ("YouthDigital")) {
			SelectedComponents (m_componentList);
		}
	}
 
	// This automatically acts upon all objects in list
	void SelectedComponents (List<Object> objectList) {
		if (objectList == null)
			return;
		foreach (Object component in objectList) {
			if (component == null)
				continue;
			// Hide flag
			if (m_hideInInspector) {
				component.hideFlags |= HideFlags.HideInInspector;
			} else {
				// Remove flag
				component.hideFlags &= ~HideFlags.HideInInspector;
			}
			// Not Editable flag
			if (m_notEditable) {
				component.hideFlags |= HideFlags.NotEditable;
			} else {
				// Remove flag
				component.hideFlags &= ~HideFlags.NotEditable;
			}
		}
	}
}