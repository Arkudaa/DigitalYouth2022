using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour {

	// public variables
	public GameObject[] enemyPrefabs;
	public float spawnRange;
	public GameObject SpawnParticle;

	public static bool activated = false;

	// private variables
	private Vector3 spawnPos;

	protected void Awake (){
		this.enabled = false;
		activated = false;
	}

	protected void OnTriggerEnter(Collider collided) {
		if (collided.gameObject.tag == "Player"){
			activated = true;
		}
	}

	public void Spawn(){
		//	reset spawn position
		spawnPos = transform.position;

		//Choose a random enemy
		int chosenPrefab = Random.Range (0, enemyPrefabs.Length);

		// Randomize X + Z values for spawn location within Range
		spawnPos.x += Random.Range(-spawnRange, spawnRange);
		spawnPos.z += Random.Range(-spawnRange, spawnRange);

		// spawn an enemy + particle and set it to be a child of the object this script is attached to
		GameObject spawned = Instantiate(enemyPrefabs[chosenPrefab], spawnPos, transform.rotation) as GameObject;
		spawned.transform.parent = transform;
		GameObject spawnedParticle = Instantiate(SpawnParticle, spawnPos, transform.rotation) as GameObject;
		spawnedParticle.transform.parent = transform;

		//Debug.Log("Enemy Spawned");
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, spawnRange);
	}
}