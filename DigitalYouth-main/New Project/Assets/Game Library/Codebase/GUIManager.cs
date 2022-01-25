using UnityEngine;
using System.Collections;

//ATTACH TO MAIN CAMERA, shows your health and coins
public class GUIManager : MonoBehaviour 
{
	public string coinName = "Power Cubes";
	public GUISkin guiSkin;					//assign the skin for GUI display
	[HideInInspector]
	public int coinsCollected;
	public Texture2D coinTex;
	public Texture2D healthTex;
	public Texture PlayerIcon;

	private int coinsInLevel;
	private Health health;


	//setup, get how many coins are in this level
	void Start() {
		coinsInLevel = GameObject.FindGameObjectsWithTag("Coin").Length;
		health = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
	}

	//show current health and how many coins you've collected
	void OnGUI() {
		GUI.skin = guiSkin;

		if (health && healthTex) {
			GUI.skin = guiSkin;
			GUILayout.BeginHorizontal();
			GUILayout.Label(PlayerIcon);
			for (int i = health.currentHealth; i > 0; i--) {
				GUILayout.Label (healthTex);
			}
			GUILayout.EndHorizontal();
		}


		if (coinsInLevel > 0) {
			GUILayout.Label (coinsCollected + " / " + coinsInLevel + " " + coinName);
		}
	}
}