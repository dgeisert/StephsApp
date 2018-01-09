using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

	public bool singleType = true;
	public int maxSpawns = 10;
	string myEnemyType = "spriteRed";
	List<Transform> locations;
	List<List<Vector3>> patrolPaths;

	public void Init () {
		locations = new List<Transform> ();
		patrolPaths = new List<List<Vector3>> ();
		foreach (Transform tr in transform.GetComponentsInChildren<Transform> ()) {
			if (tr.name.Contains ("Spawn")) {
				locations.Add (tr);
				List<Vector3> patrolPath = new List<Vector3> ();
				foreach (Transform tr2 in transform.GetComponentsInChildren<Transform> ()) {
					if (tr2.name.Contains ("Patrol")) {
						patrolPath.Add (tr2.position);
					}
				}
				patrolPaths.Add (patrolPath);
			}
		}
		if (CreateLevel.instance != null) {
			CreateLevel.instance.spawnPoints.Add (this);
		}
		myEnemyType = dgUtil.RandomEnemy ();
	}

	public List<BaseEnemy> Spawn(float spawnValue){
		List<BaseEnemy> spawnedEnemies = new List<BaseEnemy> ();
		float spawned = 0;
		int numSpawned = 0;
		while (spawnValue > spawned) {
			if (!singleType) {
				myEnemyType = dgUtil.RandomEnemy ();
			}
			if (spawnValue < spawned + dgUtil.enemyValues [myEnemyType]) {
				spawned += 0.5f;
			}
			else{
				int location = Mathf.FloorToInt (Random.value * locations.Count);
				BaseEnemy be = dgUtil.Instantiate (
					GameManager.instance.GetResourcePrefab(myEnemyType),
					locations [location].position,
					locations [location].rotation).GetComponent<BaseEnemy>();
				if (patrolPaths [location].Count > 0) {
					be.patrolPath = patrolPaths [location];
				}
				CreateLevel.instance.enemies.Add (be.gameObject);
				spawned += be.difficultyValue;
				spawnedEnemies.Add (be);
				numSpawned++;
				if (numSpawned >= maxSpawns) {
					break;
				}
			}
		}
		return spawnedEnemies;
	}
}
