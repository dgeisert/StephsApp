using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spark : MonoBehaviour {

	public void Init(Color col){
		ParticleSystem ps = GetComponent<ParticleSystem> ();
		var main = ps.main;
		main.startColor = new ParticleSystem.MinMaxGradient(col);
		Invoke ("DestroyThis", ps.main.startLifetime.constant);
	}

	void DestroyThis(){
		Destroy (gameObject);
	}
}
