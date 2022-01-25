using UnityEngine;
using System.Collections;

// Note that for a 3D Model to work in a Canvas, the HUD must be in Screen Space CAMERA
[ExecuteInEditMode]
public class UI3DModel : MonoBehaviour {

	[Header("Attach the Model here")]
	public GameObject coinModel;
	public float m_rotationSpeed = 120.0f;
	private GameObject UICoinModel;

	// Use this for initialization
	void OnEnable() {
		// If there is no coinModel instantiate it
		if (transform.childCount == 0) {
			UICoinModel = Instantiate (coinModel, Vector3.zero, Quaternion.identity, transform) as GameObject;
			UICoinModel.transform.localPosition = Vector3.zero;
		} else {
			// But if there is, we need to grab a reference to it so we can rotate it.
			UICoinModel = transform.GetChild ( 0 ).gameObject;
		}
	}
 
	void Update() {
		if (Application.isPlaying) {
			UICoinModel.transform.Rotate (Vector3.up * m_rotationSpeed * Time.deltaTime);
		}
	}
}