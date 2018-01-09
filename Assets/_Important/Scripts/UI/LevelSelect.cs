using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK.UnityEventHelper;
using VRTK;
using Steamworks;

public class LevelSelect : MonoBehaviour
{

	public static LevelSelect instance;

	public Text levelText, levelNumberText, infiniteRank, infiniteName, infiniteScore;

	public CustomInputField customSeed, customDifficulty, customCount;
    public LevelManager levelManager;
	public GameObject customLevelContainer, levelSelectScreen, otherScreen, infiniteScreen;
	int value, currentLevel, screen;
	public Button next, next10, previous, previous10;

	Dictionary<int, GameObject> screens;

	//settings from ship
	private VRTK_Wheel wheel;

	public void Init()
    {
		instance = this;
        currentLevel = PlayerPrefs.GetInt ("currentLevel");
        value = Mathf.Min(currentLevel, levelManager.levelSettings.Count - 1);
        currentLevel = value;
		customDifficulty.placeholder.GetComponent<Text>().text = "Difficulty (1 to " + (currentLevel).ToString() + ")";
		screens = new Dictionary<int, GameObject> ();
		screens.Add (0, infiniteScreen);
		screens.Add (1, levelSelectScreen);
		screens.Add (2, otherScreen);
		if (currentLevel >= 10) {
			screens.Add (3, customLevelContainer);
		} else {
			customLevelContainer.SetActive (false);
		}
		SetScreen (0);
        if (GameManager.GetScene() == "islandgen")
        {
            infiniteScreen.SetActive(false);
        }
        SteamworksManager.instance.GetInfiniteWaveScore();
    }
	public void NextScreen(){
		screen++;
		if (screen >= screens.Count) {
			screen = 0;
		}
		SetScreen (screen);
	}
	public void PreviousScreen(){
		screen--;
		if (screen < 0) {
			screen = screens.Count - 1;
		}
		SetScreen (screen);
	}
	public void SetScreen(int setScreen){
		foreach (KeyValuePair<int, GameObject> kvp in screens) {
			kvp.Value.SetActive (false);
		}
		if (setScreen >= 0) {
			screens [setScreen].SetActive (true);
		}
	}
	public void ShipInit(Ship ship){
		wheel = ship.wheel.GetComponent<VRTK_Wheel>();
		wheel.ValueChanged += HandleChange;
		wheel.ValueChanged += HandleGrabbed;
		wheel.minimumValue = -18;
		wheel.maximumValue = 0;
		wheel.OnValueChanged(new Control3DEventArgs());
        if (GameManager.GetScene() == "islandgen")
        {
            infiniteScreen.SetActive(false);
        }
	}
    public void HandleGrabbed(object sender, InteractableObjectEventArgs e)
    {
        VRTK.VRTK_InteractGrab vrtkG = e.interactingObject.GetComponentInParent<VRTK.VRTK_InteractGrab>();
        if (vrtkG != null)
        {
            GameObject destroyGo = vrtkG.grabbedObject;
            if (destroyGo != null)
            {
                if (destroyGo.GetComponent<BaseWeapon>() != null)
                {
                    Destroy(destroyGo);
                }
            }
		}
    }

    public void HandleChange(object sender, Control3DEventArgs e)
	{
        if(wheel.wheelRigidbody == null)
        {
            return;
        }
        if(wheel.wheelRigidbody.angularVelocity.x > 0)
        {
			ChangeLevel (1);
        }
        else
        {
			ChangeLevel (-1);
        }
    }

	public void ChangeLevel(int changeBy){
		value += changeBy;
		if(value > currentLevel)
		{
			value = 0;
		}
		if(value < 0 || value < currentLevel - levelManager.levelSettings.Count + 1)
		{
			value = currentLevel;
		}
		if (Tutorial.instance != null)
		{
			if (Tutorial.instance.currentStep == "Wheel" && value == currentLevel)
			{
				Tutorial.instance.NextTutorialStep();
			}
		}
		int showLevel = Mathf.Min(currentLevel - value, levelManager.levelSettings.Count - 1);
		string recordText = "";
		if (PlayerManager.instance.records.ContainsKey(showLevel)) {
			switch (levelManager.levelSettings[showLevel].type) {
			case LevelType.Survive:
				recordText = Mathf.RoundToInt(PlayerManager.instance.records [showLevel]) + " kills";
				break;
			default:
				recordText = dgUtil.FormatTime(PlayerManager.instance.records [showLevel]);
				break;
			}
		}
		levelNumberText.text = (showLevel + 1).ToString ();
		levelText.text = levelManager.levelSettings [showLevel].seedString + "\n"
			+ "Mission Type: "
			+ levelManager.levelSettings [showLevel].type.ToString () + "\n";
		if (recordText != "") {
			levelText.text += "Record: "
				+ recordText;
		}
	}

    public void HandleGrabbed(object sender, Control3DEventArgs e)
    {
        PlayerManager.instance.leftManager.Drop();
        PlayerManager.instance.rightManager.Drop();
    }

	bool pressed = false;
	public void StartLevel(){
		pressed = true;
		if (GameManager.GetScene () == "islandgen") {
			GameManager.instance.LoadMainMenu (2);
			Ship.instance.WarpOut ();
			return;
		}
		int showLevel = Mathf.Min(currentLevel - value, levelManager.levelSettings.Count - 1);
		levelManager.GetLevel(showLevel + 1).SetState();
		Ship.instance.WarpOut ();
		GameManager.instance.LoadLevel(2);
	}
	public void StartInfinite(){
		pressed = true;
		levelManager.infinite.SetState();
		Ship.instance.WarpOut ();
		GameManager.instance.LoadLevel(2);
	}
	public void PushCustomButton()
	{
		if (CheckCustom () && !pressed) {
			pressed = true;
			levelManager.custom.islandCount = int.Parse (customCount.text);
			levelManager.custom.seedString = customSeed.text;
			levelManager.custom.level = int.Parse (customDifficulty.text) - 1;
			levelManager.custom.SetState ();
			Ship.instance.WarpOut ();
			GameManager.instance.LoadLevel (2);
		} else {

		}
	}
	public bool CheckCustom(){
		int test = -1;
		if (int.TryParse (customCount.text, out test)) {
			if(test < 5 || test > 200){
				customCount.text = "";
				customCount.placeholder.GetComponent<Text> ().enabled = true;
				return false;
			}
		}
		else{
			return false;
		}
		test = -1;
		if (int.TryParse (customDifficulty.text, out test)) {
			if(test < 0 || test > currentLevel - 1){
				customDifficulty.text = "";
				customDifficulty.placeholder.GetComponent<Text> ().enabled = true;
				return false;
			}
		}
		else{
			return false;
		}
		return true;
	}
	public void LoadTutorial()
	{
		if (!pressed) {
			pressed = true;
			Ship.instance.WarpOut ();
			GameManager.instance.LoadTutorial (2);
		}
	}
	public void LoadCredits()
	{
		if (!pressed) {
			pressed = true;
			Ship.instance.WarpOut ();
			GameManager.instance.LoadCredits (2);
		}
    }

    public void PopulateInfiniteWaveLeaderboard(LeaderboardScoresDownloaded_t data)
    {
        for (int i = 0; i <= (int)data.m_hSteamLeaderboardEntries.m_SteamLeaderboardEntries; i++)
        {
            LeaderboardEntry_t leaderboardEntry;
            int detailsMax = 1;
            int[] details = new int[detailsMax];
            SteamUserStats.GetDownloadedLeaderboardEntry(data.m_hSteamLeaderboardEntries, i, out leaderboardEntry, details, detailsMax);
            if (leaderboardEntry.m_steamIDUser.IsValid())
            {
                infiniteRank.text += "\n";
                infiniteRank.text += leaderboardEntry.m_nGlobalRank;
                infiniteName.text += "\n";
                infiniteName.text += SteamFriends.GetFriendPersonaName(leaderboardEntry.m_steamIDUser);
                infiniteScore.text += "\n";
                infiniteScore.text += leaderboardEntry.m_nScore;
            }
        }
    }
}
