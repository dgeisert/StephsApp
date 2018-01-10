using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour {

	public Camera camera;
	public Vector2 startTouch;
	float cameraMaxHeight = 40, cameraMinHeight = 10;
	public bool touchHeld = false, positionChange = false;
	public float speed = 0.5f;

	void Start(){
		camera = gameObject.GetComponent<Camera> ();
		Input.multiTouchEnabled = true;
	}

	void Update(){
		positionChange = false;
		if (Input.touchCount == 1) {
			if (touchHeld) {
				SingleTouch (Input.GetTouch(0).position);
			} else {
				SingleTouchStart (Input.GetTouch(0).position);
			}
		}
		if (Input.touchCount == 2) {
			touchHeld = true;
			Zoom (Vector3.Distance(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition, Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition)
				- Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(0).position));
		}
		if(Input.touchCount == 0){
			touchHeld = false;
		}
		if (Input.GetMouseButtonDown (0)) {
			SingleTouchStart (new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		}
		if (Input.GetMouseButtonUp (0)) {
			touchHeld = false;
		}
		if (Input.GetMouseButton (0)) {
			SingleTouch (new Vector2 (Input.mousePosition.x, Input.mousePosition.y));
		}
		Zoom (Input.mouseScrollDelta.y);
		if (positionChange) {
			CheckForAreaLoad ();
		}
	}

	void CheckForAreaLoad(){
		RaycastHit hit = new RaycastHit ();
		Physics.Raycast (new Ray (transform.position, transform.forward), out hit, 200);
		CreateLevel.instance.CheckForAreaLoad (hit.point);
	}

	void SingleTouch (Vector2 point){
		if (point != startTouch) {
			positionChange = true;
			transform.position += new Vector3 ((startTouch.x - point.x) * speed, 0, (startTouch.y - point.y) * speed);
			startTouch = point;
			positionChange = true;
		}
	}
	void SingleTouchStart (Vector2 point){
		touchHeld = true;
		startTouch = point;
	}
	void Zoom(float scroll){
		if ((transform.position.y + transform.forward.y * scroll < cameraMaxHeight) 
			&& (transform.position.y + transform.forward.y * scroll > cameraMinHeight)) {
			transform.position += transform.forward * scroll;
			transform.eulerAngles = new Vector3 (50 - (cameraMaxHeight - transform.position.y) / 3, 0, 0);
			speed = transform.position.y / 100;
			positionChange = true;
		}
	}
}
