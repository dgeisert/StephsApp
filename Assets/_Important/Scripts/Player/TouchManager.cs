﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour {

	public enum Mode
	{
		Build,
		Move,
		Command,
		InBuilding
	}

	public static TouchManager instance;

	public Mode mode = Mode.Move;
	public Camera camera;
	public Vector2 startTouch;
	float cameraMaxHeight = 40, cameraMinHeight = 10;
	public bool touchHeld = false, positionChange = false, zooming = false, allowTap = true;
	public float speed = 0.5f, touchTime;
	Building draggingObject;
	public GameObject particleObject;
	CreateIsland.Node focusNode;
	Vector3 focusPoint;
	public Vector3 cameraCenter;

	void Start(){
		instance = this;
		speed = transform.position.y / Screen.dpi / 1.5f;
		camera = gameObject.GetComponent<Camera> ();
		Input.multiTouchEnabled = true;
		RaycastHit hit = new RaycastHit ();
		Physics.Raycast (new Ray (transform.position, transform.forward), out hit, 200);
		cameraCenter = hit.point;
	}

	void Update(){
		if (MenuManager.instance.backgroundClickBlocker.activeSelf) {
			return;
		}
		positionChange = false;
		if (touchHeld) {
			touchTime += Time.deltaTime;
		}
		if (Input.touchSupported) {
			if (Input.touches.Length >= 2) {
				Zoom ((Vector3.Distance (Input.GetTouch (0).position - Input.GetTouch (0).deltaPosition, Input.GetTouch (1).position - Input.GetTouch (1).deltaPosition)
				- Vector3.Distance (Input.GetTouch (0).position, Input.GetTouch (1).position)) / Screen.dpi * -15);
			} else if (Input.touches.Length == 1 && !zooming) {
				if (touchHeld) {
					SingleTouch (Input.GetTouch (0).position);
				} else {
					SingleTouchStart (Input.GetTouch (0).position);
				}
			} else if (Input.touches.Length == 0) {
				zooming = false;
				if (touchHeld) {
					if (!CheckTap ()) {
						CheckDrop ();
					}
				}
				touchHeld = false;
			}
		} else {
			if (Input.GetMouseButtonDown (0)) {
				SingleTouchStart (new Vector2 (Input.mousePosition.x, Input.mousePosition.y));
			}
			if (Input.GetMouseButtonUp (0)) {
				if (!CheckTap ()) {
					CheckDrop ();
				}
				touchHeld = false;
			}
			if (Input.GetMouseButton (0)) {
				SingleTouch (new Vector2 (Input.mousePosition.x, Input.mousePosition.y));
			}
			Zoom (Input.mouseScrollDelta.y / 4);
		}
		if (positionChange) {
			CheckForAreaLoad ();
		}
	}

	void CheckForAreaLoad(){
		RaycastHit hit = new RaycastHit ();
		Physics.Raycast (new Ray (transform.position, transform.forward), out hit, 200);
		cameraCenter = hit.point;
		particleObject.transform.position = hit.point;
		CreateLevel.instance.CheckForAreaLoad (hit.point);
	}

	void SingleTouch (Vector2 point){
		RaycastHit hit = new RaycastHit ();
		Physics.Raycast (camera.ScreenPointToRay (new Vector3 (point.x, point.y, 0)), out hit, 200);
		if (point != startTouch) {
			if (hit.transform != null) {
				if (hit.transform.name != "Ground") {
					return;
				}
			}
			switch (mode) {
			case Mode.Build:
				CreateIsland.Node node = CreateLevel.instance.GetNode (hit.point + new Vector3(0.5f, 0, 0.5f));
				if (node != focusNode) {
					focusPoint = hit.point;
					focusNode = node;
					draggingObject.transform.parent = focusNode.island.transform;
					draggingObject.transform.localPosition = focusNode.GetPoint ();
					draggingObject.gameObject.SetActive (draggingObject.CheckPlacement(focusPoint));
					CreateLevel.instance.ResetHighlights ();
					if (draggingObject.gameObject.activeSelf) {
						ResourceManager.instance.HighlightResource (draggingObject.consumedResource
							, hit.point + new Vector3((draggingObject.size.x - 1)/2, 0, (draggingObject.size.y - 1)/2)
							, draggingObject.radius);
					}
				}
				break;
			case Mode.Move:
				Vector3 change = new Vector3 ((startTouch.x - point.x) * speed, 0, (startTouch.y - point.y) * speed);
				startTouch = point;
				if (change.magnitude > 1) {
					allowTap = false;
				}
				transform.position += change;
				positionChange = true;
				break;
			default:
				break;
			}
		}
	}
	void SingleTouchStart (Vector2 point){
		touchTime = 0;
		allowTap = true;
		touchHeld = true;
		startTouch = point;
		RaycastHit hit = new RaycastHit ();
		Physics.Raycast (camera.ScreenPointToRay(new Vector3(startTouch.x, startTouch.y, 0)), out hit, 200);
		if (hit.transform != null) {
			if (hit.transform.name != "Ground") {
				return;
			}
		}
		focusPoint = hit.point;
		focusNode = CreateLevel.instance.GetNode (hit.point + new Vector3(0.5f, 0, 0.5f));
		switch (mode) {
		case Mode.Build:
			draggingObject = dgUtil.Instantiate (ResourceManager.instance.currentBuilding
				, focusNode.GetPoint ()
				, Quaternion.identity
				, true
				, focusNode.island.transform).GetComponent<Building>();
			draggingObject.gameObject.SetActive (draggingObject.CheckPlacement(focusPoint));
			if (draggingObject.gameObject.activeSelf) {
				ResourceManager.instance.HighlightResource (draggingObject.consumedResource
					, hit.point + new Vector3((draggingObject.size.x - 1)/2, 0, (draggingObject.size.y - 1)/2)
					, draggingObject.radius);
			}
			break;
		case Mode.Move:
			break;
		default:
			break;
		}
	}
	void Zoom(float scroll){
		zooming = true;
		if ((transform.position.y + transform.forward.y * scroll < cameraMaxHeight) 
			&& (transform.position.y + transform.forward.y * scroll > cameraMinHeight)) {
			transform.position += transform.forward * scroll;
			transform.eulerAngles = new Vector3 (50 - (cameraMaxHeight - transform.position.y) / 3, 0, 0);
			speed = transform.position.y / Screen.dpi / 1.5f;
			positionChange = true;
		}
	}

	bool CheckTap(){
		if (touchTime < 0.2f && allowTap) {
			switch (mode) {
			case Mode.Build:
				if (draggingObject != null) {
					if (!focusNode.occupied) {
						draggingObject.SetNodes(
							ResourceManager.instance.ClaimResource (draggingObject.consumedResource
								, focusPoint + new Vector3((draggingObject.size.x - 1)/2, 0, (draggingObject.size.y - 1)/2)
								, draggingObject.radius, draggingObject),
							focusNode);
						draggingObject = null;
						CreateLevel.instance.ResetHighlights ();
					}
				}
				break;
			case Mode.Move:
				if (focusNode != null) {
					if (focusNode.item != null) {
						Debug.Log (focusNode.item.name);
						focusNode.Highlight ();
					}
				}
				//select building
				break;
			default:
				break;
			}
		}
		return touchTime < 0.2f && allowTap;
	}

	bool CheckDrop(){
		switch (mode) {
		case Mode.Build:
			if (draggingObject != null) {
				if (!focusNode.occupied) {
					draggingObject.SetNodes(
						ResourceManager.instance.ClaimResource (draggingObject.consumedResource
							, focusPoint + new Vector3((draggingObject.size.x - 1)/2, 0, (draggingObject.size.y - 1)/2)
							, draggingObject.radius, draggingObject),
						focusNode);
					draggingObject = null;
					CreateLevel.instance.ResetHighlights ();
					return true;
				}
			}
			break;
		default:
			break;
		}
		return false;
	}

	public void BuildModeToggle(){
		mode = Mode.Build;
	}

	public void MoveModeToggle(){
		mode = Mode.Move;
	}

	public void CommandModeToggle(){
		mode = Mode.Command;
	}
}
