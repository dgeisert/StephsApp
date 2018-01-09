using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidePoint : MonoBehaviour {

	Vector3 origin;
	public float distance = 90;

	// Use this for initialization
	void Start () {
		origin = transform.position;
		if (Compass.instance != null) {
			Compass.instance.guidePoints.Add (this);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (true){//CreateLevel.instance.timeInLevel > CreateLevel.instance.level * 10) {
			if (Vector3.Distance (PlayerManager.instance.playerHips.position, origin) < distance) {
				transform.position = origin;
			} else {
				Vector3 direction = PlayerManager.instance.playerHips.position + (origin - PlayerManager.instance.playerHips.position).normalized * distance;
				transform.position = new Vector3 (direction.x, origin.y, direction.z);
			}
		}
	}
}
