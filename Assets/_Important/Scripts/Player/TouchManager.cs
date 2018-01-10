using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour {

	public Camera camera;
	public Vector2 startTouch1, startTouch2;
	public bool touchHeld = false;
	public float speed = 0.5f;

	void Start(){
		camera = gameObject.GetComponent<Camera> ();
	}

	void Update(){
		if (Input.touchCount == 1) {
			if (touchHeld) {
				SingleTouch (Input.GetTouch(0).position);
			} else {
				SingleTouchStart (Input.GetTouch(0).position);
			}
		}
		if (Input.touchCount == 2) {
			touchHeld = true;
			//Zoom ();
		}
		if(Input.touchCount == 0){
			touchHeld = false;
		}
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("down");
			SingleTouchStart (new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		}
		if (Input.GetMouseButtonUp (0)) {
			Debug.Log ("up");
			touchHeld = false;
		}
		if (Input.GetMouseButton (0)) {
			SingleTouch (new Vector2 (Input.mousePosition.x, Input.mousePosition.y));
		}
		Zoom (Input.mouseScrollDelta.y);
	}

	void SingleTouch (Vector2 point){
		transform.position += new Vector3 ((startTouch1.x - point.x) * speed, 0, (startTouch1.y - point.y) * speed);
		startTouch1 = point;
	}
	void SingleTouchStart (Vector2 point){
		touchHeld = true;
		startTouch1 = point;
	}
	void Zoom(float scroll){
		transform.position += transform.forward * scroll;
		speed = transform.position.y / 100;
	}
}
