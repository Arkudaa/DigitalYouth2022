using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Helper;

/* This class inherits from Health (as that what the game uses when anything is hit)
 * However, we have also written a customer inspector to allow the user to select their starting health
 * and this writes to the 'currentHealth' variable in base class Health
 */

//[ExecuteInEditMode]
public class PlayerHealth : Health
{
 	
	public GameObject playerHealthBar;

	// List of health pips that we will enable/disable with the slider
	private List<GameObject> playerHealthPips = new List<GameObject>();

	public override void Awake(){

		if (Application.isPlaying) {
			base.Awake ();
		}

		playerHealthBar = GameObject.Find("PlayerHealthBar");
		
	}

	// Grab the health pips. We have 10 in the scene, and we enable/disable them based on the above slider.
	// A UI element called Horizontal Layout Group then automatically resizes them to fit
	void GrabChildrenHealthPips() {
		foreach (Transform healthPip in playerHealthBar.transform) {
			playerHealthPips.Add(healthPip.gameObject);
		}
	}

	// Here we enable/disable the amount of health pips, based on the slider
	public override void Update() {
		if (SceneHandler.Instance.m_currentGameScene == SceneHandler.sceneType.level){
			if (playerHealthBar == null) {
				playerHealthBar = GameObject.Find("PlayerHealthBar");
			}

			if (playerHealthBar != null && playerHealthPips.Count == 0) {
				GrabChildrenHealthPips();
			}

			if (Application.isPlaying && playerHealthBar != null &&  playerHealthPips.Count > 0) {
				base.Update();
				UpdateHealthPipsHUD();
			}
		}
	}

	// Update the HUD drawn on screen showing player health
	void UpdateHealthPipsHUD() {
		if (gameObject.name == "Player") {
			for (int index = 0; index < playerHealthPips.Count; index++) {
				if (index < currentHealth) {
					playerHealthPips[index].SetActive(true);
				} else {
					playerHealthPips[index].SetActive(false);
				}
			}
		}
	}
}