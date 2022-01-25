using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Assertions;

public enum priority
{
	lowest = 1,
	low = 2,
	medium = 3,
	high = 4,
	highest = 5
}

public class UIManager : Singleton<UIManager> {
	[SerializeField]
	public GameObject pausePanelObject = null;
	[SerializeField]
	private GameObject HUD = null;
	[SerializeField]
	private HorizontalLayoutGroup horizontalLayoutGroup = null;
	[SerializeField]
	private Text messageOSD = null;
	[SerializeField]
	private Text coinNumberText = null;
	[SerializeField]
	private string CoinName;

	public int m_coinsInLevel;
	public int m_coinsCollected { get; set; }
	public static AudioSource UISoundSource;
	public AudioClip pauseSFX;
	public AudioClip resumeSFX;

	//QTag
	public bool MouseControl;
	public CameraFollow p_Cam;

	// Priority determines if a message should interrupt another. Priority 1 is highest, 5, lowest.
	private priority m_currentMessagePriority = priority.highest;

	// This holds the coroutine when we display text.
	private Coroutine messageOSDTimer = null;

	// private variables
	private Animator animator;
	private AudioListener UIAudioListener;
	private AudioLowPassFilter pauseFilter;
	private AudioSource[] pausedSources;
	private PlayerMove pMove;

	public static bool paused = false;

	public override void Awake() {
		Cursor_Off();
		// Execute logic in base singleton class
		base.Awake();

		UISoundSource = GetComponent<AudioSource>();

		Assert.IsNotNull(pausePanelObject);
		Assert.IsNotNull(HUD);
		Assert.IsNotNull(coinNumberText);
		Assert.IsNotNull(messageOSD);

		// Get reference to animator
		animator = GetComponentInChildren<Animator>();
		Assert.IsNotNull(animator);
	}

	void Start() {
		//This actually just runs in the title screen scene (if that's the scene you start in)
		// We'll call during start, so when the player is in a game level, it'll figure out what HUD elements should/shouldn't be shown
		RefreshUI();
		UpdateCoinNumberInLevel();
		UpdateCoinNumber(0);

		// We only want the player health to scale during editor time, not run time when the player is losing health
		//I'm actually not sure why we need to delay this from happening (the below delays it for 500ms)
		//but if we do it in Start or Awake, it breaks things
		Invoke("RemoveHealthScaling", 0.5f);
	}

	void RemoveHealthScaling() {
		SetStateHorizLayoutGroup (false);
	}

	// Subscribe/Unsubscribe to events
	void OnEnable() {
		CentralEventBroadcaster.OnLevelLoaded += RefreshUI;
		CentralEventBroadcaster.OnLevelLoaded += UpdateCoinNumberInLevel;
	}

	void OnDisable() {
		CentralEventBroadcaster.OnLevelLoaded -= RefreshUI;
		CentralEventBroadcaster.OnLevelLoaded -= UpdateCoinNumberInLevel;
	}

	void UpdateCoinNumberInLevel() {
		m_coinsInLevel = FindCoinsOnStart();
		//adding this here so HUD UI is updated
		UpdateCoinNumber(0);
	}

	// This looks for the SceneHandler object and Enables/ disables elements depending on screen type
	// Called from an event when a level is loaded (as it's loaded we know we're getting the instance in this scene)
	void RefreshUI() {
		switch (SceneHandler.Instance.m_currentGameScene) {
		case SceneHandler.sceneType.level:
			HUD.SetActive (true);
			SetActiveStateOnTag ("UIInGame", true);
			SetActiveStateOnTag ("UIMainMenu", false);
			if(Camera.main) p_Cam = Camera.main.GetComponent<CameraFollow>();
			Cursor_Off();
			break;

		case SceneHandler.sceneType.menu:
			Cursor_On();
			SetActiveStateOnTag ("UIMainMenu", true);
			SetActiveStateOnTag ("UIInGame", false);
			MouseControl = true;

			break;

		default:
			Debug.LogError ("Scene not supported. Please add it to SceneHandler.sceneType");
			break;
		}
	}

	// This goes through the children the UI, and enables / disables based on its tag
	void SetActiveStateOnTag (string whichTag, bool active) {
		int child = transform.childCount;

		for (int index = 0; index < child; index++) {
			GameObject UIElement = transform.GetChild (index).gameObject;
			if (UIElement.CompareTag (whichTag)) {
				UIElement.SetActive (active);
			}
		}
	}

	// Disables/Enables game HUD
	public void SetHudState(bool state) {
		HUD.SetActive(state);
	}

	void Update() {
		// 'Cancel' is defined in Project settings input. This allows us to bind it to the keyboard in addition to joypads
		if (Input.GetButtonDown ("Cancel")) {
			Pause();
		}
		if (Input.GetKeyDown(KeyCode.LeftShift)) {
			CursorToggle();
		}
	}

	void Pause() {
		// Only pause when in game scene (not in main menu)
		if (SceneHandler.Instance.ReturnCurrentScene() == SceneHandler.sceneType.level) {

			if (!pMove) {
				pMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
			}

			// If we are already paused, switch off menu and resume
			if (pausePanelObject.activeInHierarchy) {
				pausePanelObject.SetActive(false);
				Resume();
			} else {
				paused = true;
				// Otherwise pause the game and activate pause panel
				Helper.Utilities.SetTimeScale(0.0f);
				if (pMove) pMove.enabled = false;
				if (p_Cam) p_Cam.enabled = false;
				if (UISoundSource) UISoundSource.PlayOneShot(pauseSFX);
				PauseInGameAudio();
				Cursor_On();
				pausePanelObject.SetActive(true);
			}
		}
	}

	public void Resume() {
		Helper.Utilities.SetTimeScale(1.0f);
		ResumeInGameAudio();
		if (UISoundSource) UISoundSource.PlayOneShot(resumeSFX);
		Cursor_Off();
		if (p_Cam) p_Cam.enabled = true;
		if (pMove) pMove.enabled = true;
		paused = false;
	}

	// Update this
	public void Restart() {
		Helper.Utilities.ReloadLevel();
		Abilities.Instance.DisableAllPowerups();
		UpdateCoinNumber(0);
		UISoundSource = GetComponent<AudioSource>();
		p_Cam = Camera.main.GetComponent<CameraFollow>();
		pausePanelObject.SetActive(false);
		Resume();
	}

	// pauses all in-game audio except for UI and Music
	public void PauseInGameAudio() {
		pauseFilter = Camera.main.GetComponent<AudioLowPassFilter>();
		if (!pauseFilter) {
			pauseFilter = Camera.main.gameObject.AddComponent<AudioLowPassFilter>();
		}
		// loop through all audio sources
		pausedSources =  Object.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		for (int i =0; i< pausedSources.Length; i++) {
			// if audiosource is game Music, slap a low pass filter on it
			if (pausedSources[i].GetComponentInParent<AudioListener>() != null){
				pauseFilter.enabled = true;
				pauseFilter.GetComponentInParent<AudioSource>().volume *= 0.5f;
			} else if (pausedSources[i] == UISoundSource){
				// if the source is a UI sound, ignore it.
			} else {
				// otherwise, pause it
				pausedSources[i].Pause();
			}
		}
	}

	// Restores all in-game audio and restores music to full volume
	public void ResumeInGameAudio() {
		for (int i = 0; i < pausedSources.Length; i++) {
			// if audiosource is game Music, slap a low pass filter on it, otherwise, pause it.
			if (pausedSources[i].GetComponentInParent<AudioListener>()!= null) {
				pauseFilter.enabled = false;
				pauseFilter.GetComponentInParent<AudioSource>().volume *= 2f;
			} else if (pausedSources[i] == UISoundSource) {
			// if the source is a UI sound, ignore it.
			} else {
				pausedSources[i].UnPause();
			}
		}
	}

	// Quit the game. It will quit the editor (when in editor) or close the app when running standalone
	public void Quit() {
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif

		#if UNITY_WEBGL
		Application.ExternalCall("location.reload");
		#endif

		#if UNITY_STANDALONE
		Application.Quit();
		#endif
	}

	// Returns number of coins in level at start of game
	int FindCoinsOnStart() {
		//Debug.Log ("should find coins " + GameObject.FindGameObjectsWithTag ("Coin").Length);
		return (GameObject.FindGameObjectsWithTag("Coin").Length);
	}

	// Updates coin HUD when called
	public static void UpdateCoinNumber(int coins) {
		if (Instance.CoinName != null && Instance.m_coinsInLevel != null) {
			Instance.coinNumberText.text = (Instance.CoinName + coins + "/" + Instance.m_coinsInLevel).ToString();
		}
	}

	/// Displays text for an (optional) amount of time, then disables the game object.
	/// Note that if you omit 'timer' it will display forever (or until it's interruped/disabled). However,
	/// passing a timer value will make it remain on screen for that time.
	public void DisplayText (string text, priority newMessagePriority = priority.highest, float timer = 0.0f){
		// Compare priorities
		if ((int)newMessagePriority < (int)m_currentMessagePriority) {
			Debug.LogWarning ("Requested message is lower than current priority message, not displaying");
			Debug.LogWarning ("Not displaying: " + text);
			return;
		}

		// If another message was active and we are interrupting it, ensure we cancel the previous coroutine
		// that was created and would otherwise disable the message
		if (messageOSDTimer != null) {
			StopCoroutine (messageOSDTimer);
			messageOSDTimer = null;
		}

		// Update priority value
		m_currentMessagePriority = newMessagePriority;
		// Turn on the parent game object
		messageOSD.transform.parent.gameObject.SetActive (true);
		// Set the text
		messageOSD.text = text;

		// If a timer value isn't passed, we dont' trigger the coroutine so it remains active.
		// Otherwise, if a value is passed, then it'll use that in the coroutine
		if (timer > 0.0f) {
			messageOSDTimer = StartCoroutine (Helper.Utilities.Wait (timer, () => {
				// switch of the game object
				messageOSD.transform.parent.gameObject.SetActive (false);
				// We've stopped displaying the message, so set it to the lowest to allow other messages to be displayed
				m_currentMessagePriority = priority.lowest;
			})
			);
		}
	}

	public void DisableText() {
		//Debug.LogWarning ("DisableText () ");
		messageOSD.transform.parent.gameObject.SetActive(false);
	}

	public bool ReturnStateHorizontalGroup() {
		return horizontalLayoutGroup.enabled;
	}

	public void SetStateHorizLayoutGroup(bool state) {
		horizontalLayoutGroup.enabled = state;
	}

	public void FadeOut() {
		animator.SetTrigger("FadeOut");
	}

	public void FadeIn() {
		animator.SetTrigger("FadeIn");
	}

	public void CursorToggle() {
		if (MouseControl) {
			Cursor_On ();
		} else {
			Cursor_Off();
		}
		MouseControl = !MouseControl;
	}

	private void Cursor_On() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		MouseControl = true;
	}

	private void Cursor_Off() {
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		MouseControl = false;
	}
}
