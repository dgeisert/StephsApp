using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttack : MonoBehaviour {

	public float telegraphTime = 3f, damageTime = 2f, elapsedTime = 0f;
	public GameObject damageTrigger, damageParticles;
	
	// Update is called once per frame
	void Update () {
		elapsedTime += Time.deltaTime;
		if (elapsedTime > telegraphTime) {
			damageTrigger.SetActive (true);
			damageParticles.SetActive (true);
		}
		if (elapsedTime > telegraphTime + damageTime) {
			damageTrigger.SetActive (false);
		}
		if (elapsedTime > telegraphTime + damageTime + 1) {
			Destroy (gameObject);
		}
	}
}
