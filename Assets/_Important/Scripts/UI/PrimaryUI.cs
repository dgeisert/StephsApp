using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using Photon;
using UnityEngine.SceneManagement;
using Holoville.HOTween;

public class PrimaryUI : UnityEngine.MonoBehaviour
{
	public GameObject inventoryUI, networkingUI, levelSelectUI, customLevelUI;
    public static PrimaryUI instance;
    public GameObject hips, canvas;
    Vector3 startPosition;
    Quaternion startRotation;
    public bool isWorldLocked = false;
    public Text healthText, coinsText, gemsText, youAreDead;
    bool initialized = false;


    public Camera interactionCamera;



    //Tabs
    public Canvas tabs;
    public Button settingsTab, adminTab;
    public GameObject settingsPage, adminPage;

	//Settings Buttons
	public Button returnToShip, basicRevive, volumeUp, volumeDown, holsterViewToggle, touchpadTurningToggle, showInfoTextToggle, touchpadMoveToggle, buddyAudioToggle;
	public GameObject holsterViewToggleDot, touchpadTurningToggleDot, showInfoTextToggleDot, touchpadMoveToggleDot, buddyAudioToggleDot;

    //ADMIN Buttons
    public Button loadMainMenu, loadSpace, die, revive, heal, grantGems, grantCoins, grantResource, grantAllWeapons, grantAllChests, killAllEnemies, destroyDestructibles, resetUser;
    LevelManager levelManager;

    public void Init()
    {
        if (initialized)
        {
            return;
        }
        initialized = true;
#if DEVELOPMENT_BUILD || UNITY_EDITOR

#else
		adminTab.gameObject.SetActive(false);

#endif
        levelManager = GameObject.FindObjectOfType<LevelManager>();
        instance = this;
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        hips = transform.parent.gameObject;

        //Settings buttons set
        volumeUp.onClick.AddListener(VolumeUp);
        volumeDown.onClick.AddListener(VolumeDown);
        holsterViewToggle.onClick.AddListener(PlayerManager.instance.ToggleHolsters);
		touchpadTurningToggle.onClick.AddListener(PlayerManager.instance.ToggleTouchpadTurning);
		touchpadMoveToggle.onClick.AddListener(PlayerManager.instance.ToggleTouchpadMovement);
        showInfoTextToggle.onClick.AddListener(PlayerManager.instance.ToggleInfoText);
        buddyAudioToggle.onClick.AddListener(PlayerManager.instance.BuddyAudioToggle);
        returnToShip.onClick.AddListener(dgUtil.GoToMainMenu);

        //Admin buttons set
        loadMainMenu.onClick.AddListener(dgUtil.GoToMainMenu);
        loadSpace.onClick.AddListener(dgUtil.GrantLevels);
        die.onClick.AddListener(dgUtil.Die);
        revive.onClick.AddListener(dgUtil.Revive);
        basicRevive.onClick.AddListener(dgUtil.Revive);
        heal.onClick.AddListener(dgUtil.Heal);
        grantGems.onClick.AddListener(dgUtil.Grant1000Gems);
        grantCoins.onClick.AddListener(dgUtil.Grant1000Coins);
        //grantResource.onClick.AddListener(StartButton);
        grantAllWeapons.onClick.AddListener(dgUtil.GrantAllWeapons);
        //grantAllChests.onClick.AddListener(StartButton);
        killAllEnemies.onClick.AddListener(dgUtil.KillAllEnemies);
        destroyDestructibles.onClick.AddListener(dgUtil.DestroyDestructibles);
        resetUser.onClick.AddListener(dgUtil.ResetUser);

		ResetToHips();
		SetTab("settings");
        interactionCamera = PlayerManager.instance.eyeCamera;
        canvas.GetComponent<Canvas>().worldCamera = interactionCamera;
        coinsText.text = PlayerManager.instance.GetResource("coins").ToString();
        gemsText.text = PlayerManager.instance.GetResource("gems").ToString();
        healthText.text = "100";


        if (dgUtil.testData == null)
        {
            dgUtil.testData = new List<string>();
		}
		inventoryUI.GetComponent<Inventory>().Init();
		networkingUI.GetComponent<NetworkingUI>().Init();
		levelSelectUI.GetComponentInChildren<LevelSelect>().Init();

        if(CreateLevel.instance != null)
        {
            switch (CreateLevel.instance.levelType)
            {
                case LevelType.Survive:
                case LevelType.InfiniteWave:
                case LevelType.Wave:
                    basicRevive.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
        else
        {
            basicRevive.gameObject.SetActive(false);
        }
        //StartCoroutine ("AutoSearch");
    }

    IEnumerator AutoSearch()
    {
        yield return null;
        yield return null;
        yield return null;
        if (dgUtil.testCount < 100)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                levelManager.test.seed = Mathf.FloorToInt(Random.value * 10000 * Time.time);
                levelManager.test.SetState();
				GameManager.instance.LoadLevel();
            }
            else if (SceneManager.GetActiveScene().name == "islandgen")
            {
                dgUtil.testCount++;



                //this one is for testing POI numbers
                if (dgUtil.testData.Count == 0)
                {
                    dgUtil.testData.Add("Seed,IslandCount,SpawnPoints,Enemies,Waterfalls,Statues,Fires");
                }
                dgUtil.testData.Add(CreateLevel.instance.seed
                + "," + CreateLevel.instance.actualIslandCount
                + "," + CreateLevel.instance.spawnPoints.Count
                    + "," + CreateLevel.instance.enemies.Count
                    + "," + GameObject.FindObjectsOfType<PolyWater>().Length
                    + "," + GameObject.FindObjectsOfType<Statue>().Length
                    + "," + GameObject.FindObjectsOfType<HealingZone>().Length
                    + "," + GameObject.FindObjectsOfType<Ladder>().Length);


                /*
				//This one is for testing biome distribution
				if (dgUtil.testData.Count == 0) {
					dgUtil.testData.Add ("Seed,IslandCount,Forrests,Rockies,Swamps");
				}
				int forrests = 0;
				int rockies = 0;
				int swamps = 0;
				foreach (CreateIsland island in CreateLevel.instance.islands) {
					if (island.biome.biomeName == "Forrest") {
						forrests++;
					}
					if (island.biome.biomeName == "Rocky") {
						rockies++;
					}
					if (island.biome.biomeName == "Swamp") {
						swamps++;
					}
				}
				dgUtil.testData.Add (CreateLevel.instance.seed
					+ "," + CreateLevel.instance.actualIslandCount
					+ "," + forrests
					+ "," + rockies
					+ "," + swamps);
				*/


				GameManager.instance.LoadMainMenu ();
            }
        }
        else
        {
            dgUtil.SaveTestData(dgUtil.testData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tabs.worldCamera == null)
        {
            canvas.GetComponent<Canvas>().worldCamera = interactionCamera;
            tabs.GetComponent<Canvas>().worldCamera = interactionCamera;
            PlayerManager.instance.hipCanvas.worldCamera = interactionCamera;
        }
		if (PlayerManager.instance != null) {
			youAreDead.enabled = PlayerManager.instance.otherPlayerObject.isDead;
		}
		if (Ship.instance != null) {
			if (Ship.instance.playerOnShip) {
				transform.rotation = Quaternion.identity;
			}
		}
        if (CreateLevel.instance != null)
        {
            if (youAreDead.enabled)
            {
                returnToShip.GetComponent<Pulse>().enabled = true;
                if ((CreateLevel.instance.levelType != LevelType.InfiniteWave) &&
                 (CreateLevel.instance.levelType != LevelType.Survive) &&
                 (CreateLevel.instance.levelType != LevelType.Wave))
                {
                    basicRevive.gameObject.SetActive(true);
                }
            }
            else
            {
                returnToShip.GetComponent<Pulse>().enabled = false;
                basicRevive.gameObject.SetActive(false);
            }
        }
        if (GameManager.GetScene() == "MainMenu")
        {
            returnToShip.gameObject.SetActive(false);
        }
        holsterViewToggleDot.SetActive(PlayerManager.instance.savedSettings["holsterView"]);
        buddyAudioToggleDot.SetActive(PlayerManager.instance.savedSettings["buddyAudio"]);
        touchpadTurningToggleDot.SetActive (PlayerManager.instance.savedSettings["turnWithTouchpad"]);
        showInfoTextToggleDot.SetActive(PlayerManager.instance.savedSettings["showInfoText"]);
        touchpadMoveToggleDot.SetActive(PlayerManager.instance.savedSettings["touchpadMovement"]);


#if DEVELOPMENT_BUILD || UNITY_EDITOR

#else
		adminTab.gameObject.SetActive(false);

#endif
    }

    public void LockUIToWorld()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null);
            transform.rotation = Quaternion.identity;
			gameObject.SetActive (true);
        }
    }

    public void ResetToHips()
    {
        if (transform.parent != hips)
        {
            transform.SetParent(hips.transform);
			gameObject.SetActive (false);
            transform.localPosition = startPosition;
            transform.localRotation = startRotation;
        }
    }

    public void WorldLockToggle()
    {
        isWorldLocked = !isWorldLocked;
        if (isWorldLocked)
        {
            LockUIToWorld();
        }
        else
        {
            ResetToHips();
        }
    }

    public void SetTab(string tab)
    {
        adminPage.SetActive(false);
        settingsPage.SetActive(false);
        switch (tab)
        {
            case "admin":
                adminPage.SetActive(true);
                break;
            case "settings":
                settingsPage.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void VolumeUp()
    {
        if (AudioListener.volume < 0.9f)
        {
            AudioListener.volume += 0.1f;
        }
        else
        {
            AudioListener.volume = 1f;
        }
    }

    public void VolumeDown()
    {
        if (AudioListener.volume > 0.1f)
        {
            AudioListener.volume -= 0.1f;
        }
        else
        {
            AudioListener.volume = 0f;
        }
    }
}