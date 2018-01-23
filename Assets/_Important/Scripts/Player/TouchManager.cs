using System.Collections;
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
	Vector3 startFocusPoint;
	float cameraMaxHeight = 50, cameraMinHeight = 5;
	public bool touchHeld = false, positionChange = false, zooming = false, allowTap = true, isFog = false, inMenu = false;
	public float speed = 0.5f, touchTime;
	Building draggingObject;
	public GameObject particleObject;
	Fog focusFog;
	public CreateIsland.Node focusNode;
	Vector3 focusPoint;
	public GameObject focusObject;
	public Vector3 cameraCenter;

	public void Init(){
		Application.targetFrameRate = 30;
		instance = this;
		speed = transform.position.y / Screen.dpi / 1.5f;
		camera = gameObject.GetComponent<Camera> ();
		Input.multiTouchEnabled = true;
		RaycastHit hit = new RaycastHit ();
		Physics.Raycast (new Ray (transform.position, transform.forward), out hit, 200);
		cameraCenter = hit.point;
		startFocusPoint = hit.point;
	}

	public void SetCameraToHome(){
		transform.position = startFocusPoint + (transform.position - cameraCenter);
		CheckForAreaLoad ();
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
				if (!inMenu) {
					if (touchHeld) {
						SingleTouch (Input.GetTouch (0).position);
					} else {
						SingleTouchStart (Input.GetTouch (0).position);
					}
				}
			} else if (Input.touches.Length == 0) {
				zooming = false;
				inMenu = false;
				if (touchHeld) {
					if (!CheckTap ()) {
						CheckDrop ();
					}
				}
				touchHeld = false;
			}
		} else {
			if (Input.GetMouseButtonUp (0)) {
				if (!CheckTap ()) {
					CheckDrop ();
				}
				touchHeld = false;
			}
			else if (Input.GetMouseButtonDown (0)) {
				SingleTouchStart (new Vector2 (Input.mousePosition.x, Input.mousePosition.y));
			}
			else if (Input.GetMouseButton (0)) {
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
					if (hit.transform.name == "NewLandFog" && mode == Mode.Move) {
						Vector3 change = new Vector3 ((startTouch.x - point.x) * speed, 0, (startTouch.y - point.y) * speed);
						startTouch = point;
						if (change.magnitude > 1) {
							allowTap = false;
						}
						transform.position += change;
						positionChange = true;
					}
					return;
				}
			}
			switch (mode) {
			case Mode.Build:
				CreateIsland.Node node = CreateLevel.instance.GetNode (hit.point + new Vector3 (0.5f, 0, 0.5f));
				if (node != focusNode) {
					focusPoint = hit.point;
					focusNode = node;
					CheckNodePlacement (hit.point);
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
				if (hit.transform.name == "NewLandFog" && mode == Mode.Move) {
					if (focusFog != null) {
						focusFog.RemoveHighlight ();
						focusObject = null;
					}
					focusFog = hit.transform.GetComponent<Fog> ();
				}
				return;
			}
		}
		if (focusFog != null) {
			focusFog.RemoveHighlight();
			focusFog = null;
			focusObject = null;
		}
		focusPoint = hit.point;
		if (focusNode != null) {
			if (focusNode.item != null) {
				focusNode.RemoveHighlight ();
				focusObject = null;
			}
			if (focusNode.reference != null) {
				focusNode.reference.RemoveHighlight ();
				focusObject = null;
			}
		}
		focusNode = CreateLevel.instance.GetNode (hit.point + new Vector3 (0.5f, 0, 0.5f));
		switch (mode) {
		case Mode.Build:
			draggingObject = dgUtil.Instantiate (ResourceManager.instance.currentBuilding
				, focusNode.GetPoint ()
				, Quaternion.identity
				, true
				, focusNode.island.transform).GetComponent<Building> ();
			draggingObject.Init ();
			if (draggingObject.name == "Warehouse") {
				draggingObject.SetWarehouseResource ();
			}
			CheckNodePlacement (hit.point);
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
			if (focusFog != null) {
				focusFog.Highlight ();
				focusObject = focusFog.gameObject;
				return true;
			}
			switch (mode) {
			case Mode.Build:
				return PlaceBuilding ();
				break;
			case Mode.Move:
				if (focusNode != null) {
					if (focusNode.item != null) {
						focusNode.Highlight ();
						focusObject = focusNode.island.gameObject;
					}
					if (focusNode.reference != null) {
						focusNode.reference.Highlight ();
						focusObject = focusNode.reference.island.gameObject;
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
			return PlaceBuilding ();
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

	public bool CheckNodePlacement(Vector3 point){
		draggingObject.transform.parent = focusNode.island.transform;
		draggingObject.transform.localPosition = focusNode.GetPoint ();
		if (draggingObject.CheckPlacement (focusPoint)) {
			if (ResourceManager.instance.HasResource(draggingObject.buildResource, draggingObject.buildCost) > 0) {
				draggingObject.RemoveBadHighlight ();
			}
			else{
				draggingObject.BadHighlight ();
			}
		} else {
			draggingObject.BadHighlight ();
		}
		CreateLevel.instance.ResetHighlights ();
		if (draggingObject.gameObject.activeSelf) {
			ResourceManager.instance.HighlightResource (draggingObject.consumedResource
				, point + new Vector3(-(draggingObject.size.x - 1)/2, 0, (draggingObject.size.y - 1)/2)
				, draggingObject.radius);
			ResourceManager.instance.HighlightConsumers (draggingObject.producedResource, focusNode);
			return true;
		}
		return false;
	}

	public bool PlaceBuilding(){
		if (draggingObject != null) {
			draggingObject.transform.parent = focusNode.island.transform;
			draggingObject.transform.localPosition = focusNode.GetPoint ();
			draggingObject.gameObject.SetActive (draggingObject.CheckPlacement(focusPoint));
			CreateLevel.instance.ResetHighlights ();
			if (draggingObject.CheckPlacement (focusPoint)
			    && ResourceManager.instance.HasResource (draggingObject.buildResource, draggingObject.buildCost) == 1) {
				CompleteBuild ();
				return true;
			} else if (draggingObject.CheckPlacement (focusPoint)
				&& ResourceManager.instance.HasResource (draggingObject.buildResource, draggingObject.buildCost) == 2) {
				MenuManager.instance.OpenConfirmation ("Use gold to build?"
					, Resource.Gold
					, draggingObject.buildCost
						- ResourceManager.instance.resourceCounts [draggingObject.buildResource]
					, CompleteBuild
					, false
					, DeclineBuild);
			} else {
				NoCompleteBuild ();
				return false;
			}
		}
		return true;
	}
	public void CompleteBuild(){
		ResourceManager.instance.RemoveResources (draggingObject.buildResource, draggingObject.buildCost);
		draggingObject.SetNodes (
			ResourceManager.instance.ClaimResource (draggingObject.consumedResource
				, focusPoint + new Vector3 (-(draggingObject.size.x - 1) / 2, 0, (draggingObject.size.y - 1) / 2)
				, draggingObject.radius
				, draggingObject)
			, focusNode);
		foreach (Building b in ResourceManager.instance.constructedBuildings) {
			if(b.consumedResource.Contains(draggingObject.producedResource)){
				b.SetNodes (
					ResourceManager.instance.ClaimResource (b.consumedResource
						, b.transform.position + new Vector3 (-(b.size.x - 1) / 2, 0, (b.size.y - 1) / 2) - new Vector3 (0.5f, 0, 0.5f)
						, b.radius
						, b),
					b.myNode
				);
			}
		}
		ResourceManager.instance.constructedBuildings.Add (draggingObject);
		CreateLevel.instance.ResetHighlights ();
		draggingObject.RemoveBadHighlight ();
		draggingObject = null;
		focusNode = null;
		MenuManager.instance.ToggleMoveMode ();
	}
	public void DeclineBuild(){
		NoCompleteBuild ();
		MenuManager.instance.ToggleMoveMode ();
	}
	public void NoCompleteBuild(){
		Destroy (draggingObject.gameObject);
		draggingObject = null;
		focusNode = null;
	}
}
