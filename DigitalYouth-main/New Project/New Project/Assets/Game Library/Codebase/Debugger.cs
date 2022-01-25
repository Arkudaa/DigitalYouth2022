using UnityEngine;
using System.Collections;

public class Debugger : MonoBehaviour {

	/// <summary>
	/// Destroys all enemies in the active scene
	/// </summary>
	public void DestroyAllEnemies(){
		foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")){
			Destroy(enemy);
		}
	}

	/// <summary>
	/// Collects all coins in the active scene
	/// </summary>
	public void CollectAllCoins(){
		foreach (GameObject coin in GameObject.FindGameObjectsWithTag("Coin")){
			Destroy(coin);
		}
		UIManager.UpdateCoinNumber(UIManager.Instance.m_coinsInLevel); 
	}

	/// <summary>
	/// Destroys any object with the tag "Player"
	/// </summary>
	public void DestroyPlayer(){
		GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>().currentHealth = 0;
	}

	/// <summary>
	/// Enables doublejump and spin attack
	/// </summary>
	public void EnablePowerups(){
		Abilities.doubleJumpEnabled = true;
		Abilities.spinAttackEnabled = true;
	}

	/// <summary>
	/// Disables doublejump and spin attack
	/// </summary>
	public void DisablePowerups(){
		Abilities.doubleJumpEnabled = false;
		Abilities.spinAttackEnabled = false;
	}
}
