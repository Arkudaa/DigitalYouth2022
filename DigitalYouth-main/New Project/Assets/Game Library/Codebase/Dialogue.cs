using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Dialogue : MonoBehaviour {

	Transform camera;

	public GameObject option;

	private List<Info> conversation;
	private Info currentInfo;
	private string followOn;
 
	private List<GameObject> buttons;
	[HideInInspector]
	public bool complete;

	void Awake() {
		camera = Camera.main.transform;
		conversation = new List<Info> ();
		buttons = new List<GameObject> ();
	}

	public void SetDialogue(string id) {
		
		for (int a = 0; a < buttons.Count; a++) {
			Destroy (buttons [a]);
		}
		buttons.Clear ();

		Info info = null;
		for (int a = 0; a < conversation.Count; a++) {
			if (id == conversation [a].id) {
				// Grab the text
				info = conversation [a];
				// Uncomment the following to make the NPC speech only happen once
				// if (conversation [a].alreadyDisplayed) {
				//	info = null;
				//	return;
				// }  
				break;
			}
		}

		if (info != null) {
			info.alreadyDisplayed = true;
			currentInfo = info;
			//text.text = info.text;
			UIManager.Instance.DisplayText (info.text, priority.highest);

			followOn = info.followOn;

			if (info.responses != null) {
				for (int a = 0; a < info.responses.Count; a++) {
					GameObject dialogueOption = Instantiate (option);
					dialogueOption.transform.Find ("Text").GetComponent<Text> ().text = info.responses [a].text;
					dialogueOption.GetComponent<DialogueOption> ().followOn = info.responses [a].followOn;
					dialogueOption.GetComponent<DialogueOption> ().dialogue = this;
					dialogueOption.transform.SetParent(transform,false);
					dialogueOption.transform.localPosition = new Vector3 (0, -a*20, 0);
					buttons.Add (dialogueOption);
				}
			}
		} else {
			Debug.Log ("There is no dialogue ID " + id);
		}
	}

	// Update is called once per frame
	void Update() {
	//	transform.rotation = camera.rotation;
		if (Input.GetButtonUp ("Advance" ) && currentInfo!=null) {

			if (currentInfo.followOn != null) {
				SetDialogue (currentInfo.followOn);
			} else {
				GetComponent<NPC>().EndDialogue();
			}
		}
	}

	/// <summary>
	/// Adds the dialogue for this NPC
	/// </summary>
	/// <param name="id">Unique Identifier for this line</param>
	/// <param name="text">Text to display</param>
	public void AddDialogue(string id, string text) {
		Info info = new Info ();
		info.id = id;
		info.text = text;
		conversation.Add (info);
	}

	/// <summary>
	/// Adds the dialogue for this NPC
	/// </summary>
	/// <param name="id">Unique Identifier for this line.</param>
	/// <param name="text">Text to display</param>
	/// <param name="followOn">Which line ID should follow this line</param>
	public void AddDialogue(string id, string text, string followOn) {
		Info info = new Info ();
		info.id = id;
		info.text = text;
		info.followOn = followOn;
		conversation.Add (info);
	}

	public void AddDialogue(string id, string text, List<string> responses) {
		Info info = new Info ();
		info.id = id;
		info.text = text;
		info.responses = new List<Info> ();
		
		for (int a = 0; a < responses.Count; a += 2) {
			Info newInfo = new Info ();
			newInfo.text = responses [a];
			newInfo.followOn = responses [a + 1];
			info.responses.Add (newInfo);
		}
		conversation.Add (info);
	}
}

public class Info
{
	public string id = "notAssigned";
	public string text;
	public List<Info> responses;
	public string followOn;
	public bool alreadyDisplayed = false;
}