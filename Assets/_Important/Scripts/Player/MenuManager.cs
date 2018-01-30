using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public static MenuManager instance;
	public List<GameObject> dialogQueue = new List<GameObject>();

	//settings
	public GameObject backgroundClickBlocker, menuObject, settingsObject;

	//main ui area
	public Button menu, mode;
	public List<Button> options, tabs;
	public Text infoText;
	public Image modeImage;
	public Sprite moveSprite, buildSprite, centerSprite, questSprite, unlockSprite;

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
	public List<Button> warehouseSelectButtons;
	Dictionary<int, Resource> buttonResourceMapping;

	//Quest dialog
	public GameObject questDialog, questContainer, questIntro, questReward, levelUpContainer;
	public List<GameObject> activeQuestContainers;
	public Text questRewardAmount, questRewardText, questIntroText;
	public Image questRewardResource;

	public void Init(){
		instance = this;
		ToggleMoveMode ();
		activeQuestContainers = new List<GameObject> ();
		for (int i = 0; i < 3; i++) {
			activeQuestContainers.Add (dgUtil.Instantiate (QuestManager.instance.baseQuest
				, new Vector3 (0, 350 - 350 * i, 0)
				, Quaternion.identity
				, true
				, questContainer.transform)
			);
		}
		InvokeRepeating ("ShowFPS", 0.5f, 0.5f);
	}

	int fps = 0;
	public void Update(){
		///* fps
		fps++;
		//*/
	}
	void ShowFPS(){
		infoText.text = (fps * 2).ToString();
		fps = 0;
	}

	//Below is the outer ui management


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
			SetOption (4, questSprite);
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
			case 4:
				OpenQuestDialog ();
				break;
			default:
				break;
			}
			break;
		default:
			break;
		}
	}

	public void ResetOptions(){
		for (int i = 1; i < options.Count; i++) {
			options [i].gameObject.SetActive (false);
		}
	}
	public void SetOption(int setOption, Sprite setSprite){
		if (setSprite != null) {
			options [setOption].gameObject.SetActive (true);
			if (setSprite != null) {
				options [setOption].GetComponentInChildren<Image> ().sprite = setSprite;
			}
		} else {
			options [setOption].gameObject.SetActive (false);
		}
	}

	// Here and below is the dialog management

	public void OpenSettings(){
		TouchManager.instance.inMenu = true;
		dialogQueue.Add(settingsObject);
		GenericDialogToggle ();
	}
	public void Nuke(){
		foreach (Building b in GameObject.FindObjectsOfType<Building>()) {
			Destroy (b.gameObject);
		}
		GameManager.ClearData ();
	}
	public void UpLevel(){
		GameManager.instance.level = 50;
	}
	public void CloseSettings(){
		settingsObject.SetActive (false);
		dialogQueue.Remove (settingsObject);
		GenericDialogToggle ();
	}

	float itemOffsetY = 300, itemOffsetX = 160;
	public List<GameObject> scrollItems = new List<GameObject> ();
	string previousTab = "Purchase";
	void OpenMenu(){
		OpenMenu (previousTab);
	}
	public void OpenMenu(string tab){
		TouchManager.instance.inMenu = true;
		if (!dialogQueue.Contains (menuObject)) {
			dialogQueue.Add (menuObject);
		}
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
			count = 0;
			for (int j = 0; j < ResourceManager.instance.buildings.Count; j++) {
				if (ResourceManager.instance.buildings [j].GetComponent<Building> ().Category == tab) {
					count++;
				}
			}
			int i = 0;
			for (int j = 0; j < ResourceManager.instance.buildings.Count; j++) {
				if (ResourceManager.instance.buildings [j].GetComponent<Building> ().Category == tab) {
					scrollItems.Add (
						dgUtil.Instantiate (go
							, new Vector3 (i % 2 == 0 ? -itemOffsetX : itemOffsetX
								, 600 - 300 * Mathf.Floor (i / 2) + (count - 6) * 50
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
		scrollTransform.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, -1000);
		GenericDialogToggle ();
	}
	public void CloseMenu(){
		menuObject.SetActive (false);
		dialogQueue.Remove (menuObject);
		ToggleMoveMode ();
		GenericDialogToggle ();
	}

	public void OpenConfirmation(string setInfo, Resource setResource, int setAmount, System.Action setConfirmAction, bool includeGold = false, System.Action setCloseAction = null){
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
		closeAction = setCloseAction;
		TouchManager.instance.inMenu = true;
		dialogQueue.Add(confirmationDialog);
		GenericDialogToggle ();
	}
	public System.Action confirmAction, closeAction;
	public void CloseConfirmation(){
		confirmationDialog.SetActive (false);
		dialogQueue.Remove (confirmationDialog);
		if (closeAction != null) {
			closeAction();
		}
		GenericDialogToggle ();
	}
	public void ConfirmConfirmation(){
		confirmationDialog.SetActive (false);
		dialogQueue.Remove (confirmationDialog);
		confirmAction ();
		GenericDialogToggle ();
	}
	public void WarehouseSelect(){
		buttonResourceMapping = new Dictionary<int, Resource> ();
		for (int i = 0; i < warehouseSelectButtons.Count; i++) {
			if (ResourceManager.instance.warehouseResources.Count > i) {
				buttonResourceMapping.Add (i, ResourceManager.instance.warehouseResources [i]);
				warehouseSelectButtons [i].transform.GetChild (1).GetComponent<Image> ().sprite 
				= ResourceManager.instance.resourceSprites [ResourceManager.instance.warehouseResources [i]];
				if (ResourceManager.instance.resourceCounts.ContainsKey(ResourceManager.instance.warehouseResources [i]) ?
					ResourceManager.instance.resourceCounts [ResourceManager.instance.warehouseResources [i]] > 0
					:false ) {
					warehouseSelectButtons [i].enabled = true;
					warehouseSelectButtons [i].transform.GetChild (1).GetComponent<Image> ().color = Color.white;
				} else {
					warehouseSelectButtons [i].enabled = false;
					warehouseSelectButtons [i].transform.GetChild (1).GetComponent<Image> ().color = Color.gray;
					warehouseSelectButtons [i].transform.GetChild (0).GetComponent<Image> ().color = Color.gray;
				}
			} else {
				warehouseSelectButtons [i].gameObject.SetActive (false);
			}
		}
		TouchManager.instance.inMenu = true;
		dialogQueue.Add(warehouseSelectDialog);
		GenericDialogToggle ();
	}
	public void CloseWarehouseSelect(){
		dialogQueue.Remove (warehouseSelectDialog);
		warehouseSelectDialog.SetActive (false);
		GenericDialogToggle ();
	}
	public void SetWarehouse(int r){
		ResourceManager.instance.currentWarehouseResourceBuild = buttonResourceMapping [r];
		CloseWarehouseSelect ();
		ToggleBuildMode ();
	}

	public void OpenQuestDialog(){
		TouchManager.instance.inMenu = true;
		dialogQueue.Add (questDialog);
		for(int i = 0; i < 3; i++) {
			if (QuestManager.instance.activeQuests.Count > i) {
				activeQuestContainers [i].SetActive (true);
				activeQuestContainers [i].GetComponent<QuestCard> ().Init (QuestManager.instance.activeQuests[i]);
			} else {
				activeQuestContainers [i].SetActive (false);
			}
		}
		GenericDialogToggle ();
	}
	public void CloseQuestDialog(){
		questDialog.SetActive (false);
		dialogQueue.Remove (questDialog);
		GenericDialogToggle ();
	}
	public void IntroQuest(Quest q){
		TouchManager.instance.inMenu = true;
		dialogQueue.Add (questIntro);
		questIntroText.text = q.IntroText;
		GenericDialogToggle ();
	}
	public void CloseIntro(){
		questIntro.SetActive (false);
		dialogQueue.Remove (questIntro);
		OpenQuestDialog ();
		GenericDialogToggle ();
	}
	public void QuestRewardDialogOpen(Quest q){
		TouchManager.instance.inMenu = true;
		dialogQueue.Add (questReward);
		questRewardText.text = q.RewardText;
		questRewardAmount.text = q.reward.ToString ();
		GenericDialogToggle ();
	}
	public void CloseReward(){
		questReward.SetActive (false);
		dialogQueue.Remove (questReward);
		GenericDialogToggle ();
	}

	void GenericDialogToggle(){
		if (dialogQueue.Count > 0) {
			backgroundClickBlocker.SetActive (true);
			dialogQueue [0].SetActive (true);
		} else {
			backgroundClickBlocker.SetActive (false);
		}
	}
}
