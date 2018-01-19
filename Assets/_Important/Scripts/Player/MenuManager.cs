﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public static MenuManager instance;

	public GameObject backgroundClickBlocker, menuObject, settingsObject;

	public Button menu, mode;
	public List<Button> options, tabs;
	public Text infoText;
	public Image modeImage;
	public Sprite moveSprite, buildSprite;

	public Transform scrollTransform;
	public ScrollRect scroll;
	public GameObject purchaseObject, buildObject;
	public PaymentPackage testP;

	public void Init(){
		instance = this;
		ToggleMoveMode ();
	}

	public void Update(){
		///* fps
		if(Mathf.FloorToInt(Time.time * 30) % 30 == 0){
			infoText.text = dgUtil.FormatNum(1 / Time.deltaTime, 0);
		}
		//*/
	}

	public void OpenSettings(){
		backgroundClickBlocker.SetActive (true);
		settingsObject.SetActive (true);
	}
	public void CloseSettings(){
		backgroundClickBlocker.SetActive (false);
		settingsObject.SetActive (false);
	}

	float itemOffsetY = 300, itemOffsetX = 160;
	public List<GameObject> scrollItems = new List<GameObject> ();
	string previousTab = "Purchase";
	void OpenMenu(){
		OpenMenu (previousTab);
	}
	public void OpenMenu(string tab){
		backgroundClickBlocker.SetActive (true);
		menuObject.SetActive (true);
		ModeButtonsOff ();
		foreach (GameObject item in scrollItems) {
			Destroy (item);
		}
		scrollItems = new List<GameObject> ();
		GameObject go = buildObject;
		previousTab = tab;
		int count = 6;
		switch (tab) {
		case "Purchase":
			go = purchaseObject;
			count = 12;
			break;
		case "House":
			count = 4;
			break;
		case "Food":
			count = 8;
			break;
		case "Resource":
			count = 7;
			break;
		case "Military":
			count = 16;
			break;
		case "Deco":
			count = 5;
			break;
		default:
			break;
		}
		for (int i = 0; i < count; i++) {
			scrollItems.Add (
				dgUtil.Instantiate (go
					, new Vector3 (i % 2 == 0 ? -itemOffsetX : itemOffsetX
						, (count - 4) * 100 - 300 * Mathf.Floor (i / 2)
						, 0)
					, Quaternion.identity
					, true
					, scrollTransform)
			);
			Building b = ResourceManager.instance.buildings [0].GetComponent<Building> ();
			PaymentPackage p = testP;
			if (scrollItems [i].GetComponent<BuildingObjectCard> ().Init (
				    b
					, p)) {
				scrollItems [i].GetComponent<Button> ().onClick.AddListener (p.Purchase);
			} else {
				scrollItems [i].GetComponent<Button> ().onClick.AddListener (b.SetBuildMode);
			}
		}
		scrollTransform.GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (
			RectTransform.Axis.Vertical, itemOffsetY * Mathf.Ceil(scrollItems.Count / 2 + 3));
		scrollTransform.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, 1200 - 50 * scrollItems.Count);
	}
	public void CloseMenu(){
		backgroundClickBlocker.SetActive (false);
		menuObject.SetActive (false);
		ToggleMoveMode ();
	}

	void ModeButtonsOff(){
		
	}

	public void ToggleBuildMode(){
		mode.onClick.RemoveAllListeners ();
		mode.onClick.AddListener(ToggleMoveMode);
		modeImage.sprite = moveSprite;
		infoText.text = "Build " + ResourceManager.instance.currentBuilding.name;
		TouchManager.instance.BuildModeToggle ();
		SetItemButtons ();
	}

	public void ToggleMoveMode(){
		mode.onClick.RemoveAllListeners ();
		mode.onClick.AddListener(OpenMenu);
		modeImage.sprite = buildSprite;
		infoText.text = "";
		TouchManager.instance.MoveModeToggle ();
		SetItemButtons ();
	}

	public void ToggleCommandMode(){
		mode.onClick.RemoveAllListeners ();
		mode.onClick.AddListener(ToggleMoveMode);
		modeImage.sprite = moveSprite;
		infoText.text = "";
		TouchManager.instance.CommandModeToggle ();
		SetItemButtons ();
	}

	public void SetItemButtons(){
		switch(TouchManager.instance.mode){
		case TouchManager.Mode.Build:
			for (int i = 0; i < options.Count; i++) {
				if (ResourceManager.instance.buildings.Count <= i) {
					options [i].gameObject.SetActive (false);
				}
				else{
					options [i].gameObject.SetActive (true);
					options [i].gameObject.GetComponentInChildren<Image> ().sprite = 
						ResourceManager.instance.resourceSprites [
							ResourceManager.instance.buildings [i].GetComponent<Building>().producedResource];
				}
			}
			break;
		case TouchManager.Mode.Move:
			for (int i = 1; i < options.Count; i++) {
				options [i].gameObject.SetActive (false);
			}
			options [0].gameObject.SetActive (true);
			//set icon
			break;
		default:
			for (int i = 0; i < options.Count; i++) {
				options [i].gameObject.SetActive (false);
			}
			break;
		}
	}

	public void SelectOption(int index){
		switch(TouchManager.instance.mode){
		case TouchManager.Mode.Build:
			ResourceManager.instance.currentBuilding = ResourceManager.instance.buildings [index];
			foreach (Button b in options) {
				b.GetComponentInChildren<Image> ().color = Color.white;
			}
			options [index].GetComponentInChildren<Image> ().color = Color.black;
			break;
		case TouchManager.Mode.Move:
			switch (index) {
			case 0:
				TouchManager.instance.SetCameraToHome ();
				break;
			case 1:
				break;
			default:
				break;
			}
			break;
		default:
			break;
		}
	}

	public void Nuke(){
		foreach (Building b in GameObject.FindObjectsOfType<Building>()) {
			Destroy (b.gameObject);
		}
		GameManager.ClearData ();
	}
}
