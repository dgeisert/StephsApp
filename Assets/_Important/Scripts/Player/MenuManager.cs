using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public static MenuManager instance;

	public GameObject backgroundClickBlocker;

	public Button menu, mode, next;
	public List<Button> options, tabs;
	public Text infoText;
	public Image modeImage;
	public Sprite moveSprite, buildSprite;

	void Start(){
		instance = this;
		ToggleMoveMode ();
	}

	public void ToggleMenu(){
		backgroundClickBlocker.SetActive (!backgroundClickBlocker.activeSelf);
		if (backgroundClickBlocker.activeSelf) {
			ModeButtonsOff ();
		} else {
			ToggleMoveMode ();
		}
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
		mode.onClick.AddListener(ToggleBuildMode);
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
		default:
			for (int i = 0; i < options.Count; i++) {
				options [i].gameObject.SetActive (false);
			}
			break;
		}
		SelectOption (0);
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
		default:
			break;
		}
	}
}
