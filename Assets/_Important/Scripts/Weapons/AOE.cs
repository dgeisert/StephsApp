using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOE : MonoBehaviour {

	public float damageTime = 2f;
	public GameObject damageTrigger, damageParticles;

	// Update is called once per frame
	void Start () {
		Destroy (gameObject, damageTime + 1);
	}
}
