using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Critter : MonoBehaviour {

	float maxY = 5, minY = 0.5f, MaxXZ = 40;
	public float speed = 0.01f, wander = 0.2f;
	Vector3 direction = Vector3.zero;

	public void Start(){
		transform.position += MaxXZ * new Vector3 ((Random.value - 0.5f), 0, (Random.value - 0.5f));
	}

	void Update () {
		direction += new Vector3 ((Random.value - 0.5f), (Random.value - 0.5f), (Random.value - 0.5f)) * wander;
		CheckPosition ();
		direction.Normalize ();
		transform.LookAt (transform.position + direction);
		transform.position += direction * speed;
	}

	public void CheckPosition(){
		if(Vector3.Distance(TouchManager.instance.cameraCenter, transform.position) > MaxXZ){
			transform.position = TouchManager.instance.cameraCenter + MaxXZ / 3 * new Vector3 ((Random.value - 0.5f), 0.1f, (Random.value - 0.5f));
		}
		if (transform.position.y < minY) {
			direction += new Vector3 (0, 0.01f, 0);
		}
		else if (transform.position.y > maxY) {
			direction += new Vector3 (0, -0.01f, 0);
		}
	}
}
