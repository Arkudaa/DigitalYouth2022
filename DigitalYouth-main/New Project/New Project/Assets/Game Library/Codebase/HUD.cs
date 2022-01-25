using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
	
	/// <summary>
	/// Message the specified text for two seconds
	/// </summary>
	/// <param name="text">Text to Display</param>
	public static void Message(string text) {
		// print the text using the UIManager Instance with highest priority
		UIManager.Instance.DisplayText(text, priority.highest, 2f);
	}

	/// <summary>
	/// Message the specified text for two seconds
	/// </summary>
	/// <param name="text">Text to Display</param>
	public static void Message(int text) {
		// print the text using the UIManager Instance with highest priority
		UIManager.Instance.DisplayText(text.ToString(), priority.highest, 2f);
	}

	/// <summary>
	/// Message the specified text for the given displayTime
	/// </summary>
	/// <param name="text">Text to display</param>
	/// <param name="displayTime">How long to display</param>
	public static void Message(string text, float displayTime) {
		// print the text using the UIManager Instance with highest priority
		UIManager.Instance.DisplayText(text, priority.highest, displayTime);
	}

	/// <summary>
	/// Updates the HUD coin count text with the specified number of coins
	/// </summary>
	/// <param name="coinCount">Coin count to display on HUD</param>
	public static void UpdateCoinCount(int coinCount) {
		UIManager.UpdateCoinNumber(coinCount);
	}	
}