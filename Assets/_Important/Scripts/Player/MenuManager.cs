using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public static MenuManager instance;

	public GameObject backgroundClickBlocker, menuObject, settingsObject;

	//main ui area
	public Button menu, mode;
	public List<Button> options, tabs;
	public Text infoText;
	public Image modeImage;
	public Sprite moveSprite, buildSprite;

	//scroll area
	public Transform scrollTransform;
	public ScrollRect scroll;
	public GameObject purchaseObject, buildObject;
	public PaymentPackage testP;

	//confirmation dialog area
	public GameObject confirmationDialog;
	public Image resourceTotalImage, resourceConfirmImage, goldTotalImage;
	public Text confirmInfoText, resourceTotalText, resourceConfirmText, goldTotalText;
	public Button closeConfirmDialog, confirmConfirmDialog;

	//warehouse select
	public GameObject warehouseSelectDialog;

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
		TouchManager.instance.inMenu = true;
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
		TouchManager.instance.inMenu = true;
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
			count = 6;
			for (int i = 0; i < count; i++) {
				scrollItems.Add (
					dgUtil.Instantiate (go
						, new Vector3 (i % 2 == 0 ? -itemOffsetX : itemOffsetX
							, 600 - 300 * Mathf.Floor (i / 2)
							, 0)
						, Quaternion.identity
						, true
						, scrollTransform)
				);
				Building b = ResourceManager.instance.buildings [0].GetComponent<Building> ();
				PaymentPackage p = testP;
				scrollItems [i].GetComponent<BuildingObjectCard> ().Init (p);
				scrollItems [i].GetComponent<Button> ().onClick.AddListener (p.Purchase);
			}
			break;
		case "House":
			
			break;
		case "Food":
			break;
		case "Resource":
			break;
		case "Military":
			break;
		case "Deco":
			break;
		default:
			break;
		}
		if (tab != "Purchase") {
			int i = 0;
			for (int j = 0; j < ResourceManager.instance.buildings.Count; j++) {
				if (ResourceManager.instance.buildings [j].GetComponent<Building> ().Category == tab) {
					scrollItems.Add (
						dgUtil.Instantiate (go
								, new Vector3 (i % 2 == 0 ? -itemOffsetX : itemOffsetX
								, 600 - 300 * Mathf.Floor (i / 2)
								, 0)
					, Quaternion.identity
					, true
					, scrollTransform)
					);
					Building b = ResourceManager.instance.buildings [j].GetComponent<Building> ();
					scrollItems [i].GetComponent<Button> ().onClick.AddListener (b.SetBuildMode);
					scrollItems [i].GetComponent<BuildingObjectCard> ().Init (b);
					i++;
				}
			}
		}
		scrollTransform.GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (
			RectTransform.Axis.Vertical, itemOffsetY * Mathf.Ceil(scrollItems.Count / 2 + 3));
		scrollTransform.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, scrollItems.Count * 100);
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
			break;
		case TouchManager.Mode.Move:
			ResetOptions ();
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
			
			break;
		case TouchManager.Mode.Move:
			switch (index) {
			case 0:
				TouchManager.instance.SetCameraToHome ();
				break;
			case 1:
				if (TouchManager.instance.focusObject != null) {
					CreateIsland ci = TouchManager.instance.focusObject.GetComponent<CreateIsland> ();
					if (ci != null) {
						OpenConfirmation ("Chop down tree?", Resource.Logs, ci.treeValue, ci.ChopTree);
						return;
					}
					Fog f = TouchManager.instance.focusObject.GetComponent<Fog> ();
					if (f != null) {
						if (ResourceManager.instance.HasResource (f.unlockResource, f.unlockCost) > 0) {
							OpenConfirmation ("Explore new land?", f.unlockResource, -f.unlockCost, f.Unlock);
						}
						return;
					}
				}
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

	public void ResetOptions(){
		for (int i = 1; i < options.Count; i++) {
			options [i].gameObject.SetActive (false);
		}
	}
	public void SetOption(int setOption, Sprite setSprite){
		options [setOption].gameObject.SetActive (true);
		if (setSprite != null) {
			options [setOption].GetComponentInChildren<Image> ().sprite = setSprite;
		}
	}

	public void OpenConfirmation(string setInfo, Resource setResource, int setAmount, System.Action setConfirmAction, bool includeGold = false){
		if (ResourceManager.instance.resourceCounts [setResource] < setAmount && setResource != Resource.Gold) {
			includeGold = true;
		}
		confirmInfoText.text = setInfo;
		resourceConfirmImage.sprite = ResourceManager.instance.resourceSprites [setResource];
		resourceTotalImage.sprite = ResourceManager.instance.resourceSprites [setResource];
		resourceConfirmText.text = (setAmount > 0 ? "+" : "") + setAmount.ToString();
		resourceTotalText.text = ResourceManager.instance.resourceCounts [setResource].ToString();
		if (includeGold) {
			goldTotalImage.sprite = ResourceManager.instance.resourceSprites [Resource.Gold];
			goldTotalText.text = ResourceManager.instance.resourceCounts [Resource.Gold].ToString();
		} else {
			goldTotalImage.gameObject.SetActive (false);
			goldTotalText.gameObject.SetActive (false);
		}
		confirmAction = setConfirmAction;
		TouchManager.instance.inMenu = true;
		backgroundClickBlocker.SetActive (true);
		confirmationDialog.SetActive (true);
	}
	public System.Action confirmAction;
	public void CloseConfirmation(){
		backgroundClickBlocker.SetActive (false);
		confirmationDialog.SetActive (false);
	}
	public void ConfirmConfirmation(){
		backgroundClickBlocker.SetActive (false);
		confirmationDialog.SetActive (false);
		confirmAction ();
	}
	public void WarehouseSelect(){
		TouchManager.instance.inMenu = true;
		backgroundClickBlocker.SetActive (true);
		warehouseSelectDialog.SetActive (true);
	}
	public void CloseWarehouseSelect(){
		backgroundClickBlocker.SetActive (false);
		warehouseSelectDialog.SetActive (false);
	}
	public void SetWarehouse(int r){

	}
}
