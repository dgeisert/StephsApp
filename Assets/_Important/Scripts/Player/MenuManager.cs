using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public static MenuManager instance;

	public GameObject backgroundClickBlocker;

	public Button menu, build, command, move;

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
		build.gameObject.SetActive (false);
		move.gameObject.SetActive (false);
		command.gameObject.SetActive (false);
	}

	public void ToggleBuildMode(){
		build.gameObject.SetActive (false);
		move.gameObject.SetActive (true);
		command.gameObject.SetActive (false);
		TouchManager.instance.BuildModeToggle ();
	}

	public void ToggleMoveMode(){
		build.gameObject.SetActive (true);
		move.gameObject.SetActive (false);
		command.gameObject.SetActive (false);
		TouchManager.instance.MoveModeToggle ();
	}

	public void ToggleCommandMode(){
		build.gameObject.SetActive (false);
		move.gameObject.SetActive (true);
		command.gameObject.SetActive (false);
	}

	public void GenericToggle(){
		switch(TouchManager.instance.mode){
		case TouchManager.Mode.Build:
			int index = ResourceManager.instance.buildings.IndexOf (ResourceManager.instance.currentBuilding);
			index++;
			if (index >= ResourceManager.instance.buildings.Count) {
				index = 0;
			}
			ResourceManager.instance.currentBuilding = ResourceManager.instance.buildings [index];
			break;
		default:
			break;
		}
	}
}
