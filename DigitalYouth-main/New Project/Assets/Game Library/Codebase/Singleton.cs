using UnityEngine;
using System.Collections;

// Use abstract so we can't instantiate 
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

	private static T instance;

	// Property
	public static T Instance {
		get {
			if ( instance == null ) {
				// As it's a static, instance it doesnt need instance of it
				instance = FindObjectOfType<T>();
			}
			return instance;
		}
	}

	// This destroys the instance if one exists, or flags it as dontdestroy on load (so it's persistent
	// between scenes) if one isn't found
	public virtual void Awake() {
		// If there is another instance, and it's not me (ie I'm the newer instance) delete me 
		// as I'm not needed (i.e. one already exists)
		if (Instance != this) {
			// Debug.Log ("There's an existing instance of "+gameObject.name+" so deleting new one");
			Destroy(gameObject);
		} else {
			// Debug.Log ("No instance found, flagging as DontDestroyOnLoad");
			DontDestroyOnLoad(gameObject);
		}
	}
}