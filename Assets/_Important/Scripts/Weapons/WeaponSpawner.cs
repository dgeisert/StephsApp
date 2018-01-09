using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour {

	public List<GameObject> weaponBases = new List<GameObject>();
	public float spacing = 1f;
	public int number;
	public int[][] randoms;

	void Start(){
		randoms = new int[number][];
		for (int i = 0; i < number; i++) {
			randoms [i] = new int[number];
			for (int j = 0; j < number; j++) {
				randoms[i][j] = Mathf.FloorToInt (Random.value * 10000);
			}
		}
		for (int i = 0; i < number; i++) {
			for (int j = 0; j < number; j++) {
				GameObject go = dgUtil.Instantiate (weaponBases [Mathf.FloorToInt (Random.value * weaponBases.Count)], new Vector3 (i * spacing, 0, j * spacing), Quaternion.identity);
				go.GetComponent<BaseWeapon> ().seed = randoms [i] [j];
				go.GetComponent<BaseWeapon> ().Init();
				//dgUtil.Disable (go);

				/*
				 //this is for testing gun firing
				go.GetComponent<Gun>().lastFireTime = -1;
				go.GetComponent<Gun>().PullTrigger(gameObject);
				*/
			}
		}
	}
}
