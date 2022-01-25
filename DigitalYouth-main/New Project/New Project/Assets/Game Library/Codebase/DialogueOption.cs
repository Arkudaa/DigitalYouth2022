using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogueOption : MonoBehaviour {

	public string followOn;
	public Dialogue dialogue;

	void Start() {
		GetComponent<Button> ().onClick.AddListener(() => {ButtonClicked();});
	}

	void ButtonClicked() {
		dialogue.SetDialogue (followOn);
	}
}