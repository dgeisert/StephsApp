using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine.UI;
using UnityEngine.Analytics;
using VRTK;
using Holoville.HOTween;
using Steamworks;

public class PlayerManager : Photon.MonoBehaviour
{
    public OtherPlayerObject otherPlayerObject;
    public PhotonPlayer photonPlayer;
    public GameObject renderer, body;
    public Transform head, left, right;
    public Head bodyPartHead;
    public Body bodyPartBody;
    public Hand bodyPartHandLeft, bodyPartHandRight;
    public ControllerManager leftManager, rightManager;
    private Vector3 correctHeadPos, CorrectBodyPos, correctRightPos, correctLeftPos;
    private Quaternion correctHeadRot, CorrectBodyRot, correctRightRot, correctLeftRot;
    public GameObject otherPlayerObjectPrefab;
    public static PlayerManager instance;
    public GameObject petPrefab;
    public Pet pet;
    public Transform petArea;
    public GameObject particles;
    public List<AudioClip> grunts;
    public AudioSource myAudioSource;
    public VRTK_HeightAdjustTeleport teleporter;
    public GameObject spark;
    public bool isAgroed = false;
    public AudioSource battleMusic, ambientMusic;

    public List<OtherPlayerObject> players;

    //This section for UI elements
    public VRTK_SDKManager sdkManager;
    PrimaryUI primaryUI;
    public GameObject primaryUIPrefab;
    public Camera eyeCamera;
    public Transform playerHips;
    public Holster rightShoulder, leftShoulder, rightHip, leftHip;
    public Text healthText, coinsText, gemsText, deadText;
    public Image healthIcon, coinsIcon, gemsIcon;
    public Canvas hipCanvas;
    public TutorialHighlighter[] tutorialHighlighters;
    public GameObject flytext; //flytext prefab used by this player
    public VRTK_HeadsetFade headsetFader; //this is for controlling the damage and death fading;
    public Text infoRightAlign, infoLeftAlign, infoCenterAlign;
    public Dictionary<string, bool> savedSettings;
    public float scaleModStart, scaleModTimer = 10f;

    //this section is for resource tracking
    private Dictionary<string, int> resources;
    public Dictionary<int, float> records;
    public List<string> weapons, weaponSettings, hands, bodies, heads, bodyParts;
    public List<int> chestsToClaim, claimedChests;
    public List<GameObject> lootDropImport = new List<GameObject>();
    public Dictionary<string, GameObject> lootDrops;
    public GameObject levelCompleteReward;
    public float version = 0.1f;
    Dictionary<string, object> sendData = new Dictionary<string, object>();
    public int score = 0;

    //this section for body part mods
    public float health
    , staffDamageMod
    , meleeDamageMod
    , bowDamageMod
    , gunDamageMod
    , overallDamageMod
    , staffCritPercentMod
    , meleeCritPercentMod
    , bowCritPercentMod
    , gunCritPercentMod
    , overallCritPercentMod
    , staffCritMultMod
    , meleeCritMultMod
    , bowCritMultMod
    , gunCritMultMod
    , overallCritMultMod
    , staffSpeedMod
    , meleeSpeedMod
    , bowSpeedMod
    , gunSpeedMod
    , overallSpeedMod
    , bowRangeModifier
    , gunRangeModifier
    , agroDistanceMod
    , lightMod
    , blockChance
    , dodgeChance
    , thornsDamage
    , teleportDistanceMod
    , teleportCooldownMod;

    public void Init()
	{
        PlayerManager.instance = this;
        float playerVersion = PlayerPrefs.GetFloat("playerVersion");
        if (version > playerVersion)
        {
            ResetResources();
            PlayerPrefs.SetFloat("playerVersion", version);
        }
        else
        {
            LoadFromES2();
            LoadWeapons();
            LoadBodyParts();
        }
        spark = GameManager.instance.GetResourcePrefab("Spark");
        primaryUI = dgUtil.Instantiate((primaryUIPrefab), primaryUIPrefab.transform.position, primaryUIPrefab.transform.rotation, true, playerHips).GetComponent<PrimaryUI>();
        primaryUI.Init();
        Tutorial tutorial = FindObjectOfType<Tutorial>();
        if (tutorial != null)
        {
            tutorial.Init(tutorialHighlighters);
        }
        if (NetworkManager.instance != null)
        {
            if (NetworkManager.instance.singlePlayer)
            {
                SpawnPlayerObject();
            }
        }
        lootDrops = new Dictionary<string, GameObject>();
        foreach (GameObject loot in lootDropImport)
        {
            lootDrops.Add(loot.name, loot);
        }
        for (int i = 0; i < 9; i++)
        {
            Physics.IgnoreLayerCollision(10, i);
            Physics.IgnoreLayerCollision(11, i);
        }
        myAudioSource = eyeCamera.gameObject.AddComponent<AudioSource>();
        myAudioSource.playOnAwake = false;
        myAudioSource.volume = 0.5f;
        myAudioSource.maxDistance = 20f;
        myAudioSource.spatialBlend = 1f;
        foreach (Camera c in GameObject.FindObjectsOfType<Camera>())
        {
            c.layerCullSpherical = true;
        }
        if (PlayerPrefs.GetInt("levelCompleteReward") > 0)
        {
            int toGrant = Mathf.FloorToInt(Random.value * 10000);
            if (!chestsToClaim.Contains(toGrant) && !claimedChests.Contains(toGrant))
            {
                chestsToClaim.Add(toGrant);
            }
            Invoke("SetLevelCompleteReward", 0.5f);
        }
        if (PlayerPrefs.GetInt("levelCompleteDialog") > 0)
        {
            Invoke("SetLevelCompleteSuccessDialog", 0.5f);
        }
        if (PlayerPrefs.GetInt("levelFailDialog") > 0)
        {
            Invoke("SetLevelCompleteFailDialog", 0.5f);
        }
        Invoke("SetChests", 0.5f);
        teleporter = GetComponentInChildren<VRTK_HeightAdjustTeleport>();
        teleporter.Teleported += new TeleportEventHandler(OnTeleport);
        StartCoroutine("StartTeleport");
        ZeroPlayer();
        savedSettings["holsterView"] = !GetSetting("holsterView");
        ToggleHolsters();
        savedSettings["touchpadMovement"] = !GetSetting("touchpadMovement");
        ToggleTouchpadMovement();
        savedSettings["turnWithTouchpad"] = !GetSetting("turnWithTouchpad");
        ToggleTouchpadTurning();
        SetPlayerData();
        if (GameManager.GetScene() == "Loading")
        {
            Invoke("SpawnLogo", 0.1f);
        }
        healthText.text = Mathf.Ceil(health).ToString();
        primaryUI.healthText.text = Mathf.Ceil(health).ToString();
        if (GameManager.GetScene() == "Loading")
        {
            battleMusic.gameObject.SetActive(false);
            ambientMusic.gameObject.SetActive(false);
        }
    }

    public bool GetSetting(string setting)
    {
        if (!savedSettings.ContainsKey(setting))
        {
            if (setting == "touchpadMovement")
            {
                savedSettings[setting] = false;
            }
            else
            {
                savedSettings[setting] = true;
            }
        }
        return savedSettings[setting];
    }

    void SpawnLogo()
    {
        GameObject Logo = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab("RocketTigerLogo"), PlayerManager.instance.GetPlayerPosition() + PlayerManager.instance.otherPlayerObject.transform.forward * 3, Quaternion.identity);
        Logo.transform.LookAt(PlayerManager.instance.GetPlayerPosition());
    }

    void SetPlayerData()
    {
        sendData.Add("currentLevel", PlayerPrefs.GetInt("currentLevel"));
        sendData.Add("platform", VRTK_DeviceFinder.GetHeadsetType());
        sendData.Add("scene", GameManager.GetScene());
    }

    void SetLevelCompleteReward()
    {
        TrackEvent("level_complete");
        infoLeftAlign.text = "Mission Success!";
        infoCenterAlign.text = LevelStats();
        ClearLevelComplete();
    }
    void SetLevelCompleteSuccessDialog()
    {
        TrackEvent("level_success");
        infoLeftAlign.text = "Mission Success!";
        infoCenterAlign.text = LevelStats();
        ClearLevelComplete();
    }
    void SetLevelCompleteFailDialog()
    {
        TrackEvent("level_fail");
        infoLeftAlign.text = "Mission Failed!";
        infoCenterAlign.text = LevelStats();
        ClearLevelComplete();
    }
    void ClearInfoText()
    {
        infoLeftAlign.text = "";
        infoRightAlign.text = "";
        infoCenterAlign.text = "";
    }
    void SetChests()
    {
        if (GameManager.GetScene() == "MainMenu")
        {
            for (int i = 0; i < chestsToClaim.Count; i++)
            {
                if (i >= Ship.instance.chestPlacements.Count)
                {
                    return;
                }
                dgUtil.Instantiate(
                    levelCompleteReward,
                    Ship.instance.chestPlacements[i].position,
                    Ship.instance.chestPlacements[i].rotation
                ).GetComponent<TreasureChest>().seed = chestsToClaim[i];
            }
        }
    }

    public IEnumerator StartTeleport()
    {
        yield return null;
        teleporter.ForceTeleport(GameManager.instance.transform.position + new Vector3(-0.6f, 0, 3.64f));
        StartCoroutine("SpawnPet");
    }

    public IEnumerator SpawnPet()
    {
        yield return null;
        if (GameManager.GetScene() != "Loading")
        {
            if (pet == null)
            {
                pet = dgUtil.Instantiate(petPrefab, GameManager.instance.transform.position + Vector3.one, Quaternion.identity, false, null, false).GetComponent<Pet>();
                pet.patrolBox = petArea;
            }
        }
    }

    public void CompleteLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("currentLevel");
        int playingLevel = PlayerPrefs.GetInt("playingLevel");
        if (CreateLevel.instance != null)
        {
            CreateLevel.instance.SaveStats();
            if (CreateLevel.instance.levelFailed)
            {
                PlayerPrefs.SetInt("levelFailDialog", 1);
                return;
            }
        }
        if (currentLevel <= playingLevel)
        {
            PlayerPrefs.SetInt("currentLevel", playingLevel + 1);
            PlayerPrefs.SetInt("levelCompleteReward", 1);
        }
        else
        {
            PlayerPrefs.SetInt("levelCompleteDialog", 1);
        }
    }

    string LevelStats()
    {
        string levelStats = "Killed ";
        levelStats += Mathf.RoundToInt(PlayerPrefs.GetFloat("completedKills"));
        levelStats += " of ";
        levelStats += Mathf.RoundToInt(PlayerPrefs.GetFloat("completedTotalEnemies"));
        levelStats += " Enemies.\nDamage Taken: ";
        levelStats += Mathf.RoundToInt(PlayerPrefs.GetFloat("completedDamage"));
        levelStats += "\nDamage Healed: ";
        levelStats += Mathf.RoundToInt(PlayerPrefs.GetFloat("completedHealing"));
        levelStats += "\nTime to Complete: ";
        levelStats += dgUtil.FormatTime(PlayerPrefs.GetFloat("completedTime"));
        return levelStats;
    }
    void ClearLevelComplete()
    {
        PlayerPrefs.SetInt("levelFailDialog", 0);
        PlayerPrefs.SetInt("levelCompleteDialog", 0);
        PlayerPrefs.SetInt("levelCompleteReward", 0);
        PlayerPrefs.SetFloat("completedKills", 0);
        PlayerPrefs.SetFloat("completedTotalEnemies", 0);
        PlayerPrefs.SetFloat("completedDamage", 0);
        PlayerPrefs.SetFloat("completedHealing", 0);
        PlayerPrefs.SetFloat("completedTime", 0);
    }

    public void ZeroPlayer()
    {
        if (sdkManager.loadedSetup != null)
        {
            sdkManager.loadedSetup.actualBoundaries.transform.position = GameManager.instance.transform.position;
        }
    }

    public Vector3 GetPlayerPosition()
    {
        if (otherPlayerObject != null)
        {
            return otherPlayerObject.transform.position;
        }
        return Vector3.zero;
    }

    public Vector3 GetPlayerStandingPosition()
    {
        if (otherPlayerObject != null)
        {
            RaycastHit hit;
            Physics.Raycast(otherPlayerObject.transform.position + Vector3.up, -Vector3.up, out hit, 5, (1 << 0));
            if (hit.collider != null)
            {
                return hit.point;
            }
        }
        return Vector3.zero;
    }

    public float teleportCooldown = 0.3f, lastTeleportTime;
    private void OnTeleport(object sender, DestinationMarkerEventArgs e)
    {
        lastTeleportTime = Time.time;
        if (Tutorial.instance != null)
        {
            if (Tutorial.instance.currentStep == "Teleport")
            {
                Tutorial.instance.NextTutorialStep();
            }
        }
        if (CreateLevel.instance != null)
        {
            CreateLevel.instance.OnTeleport(PlayerManager.instance.GetPlayerPosition());
        }
        foreach (System.Action action in GameManager.instance.EnemyChecks)
        {
            if (action.Target != null)
            {
                action();
            }
        }
        ClearInfoText();
    }

    public void LoadFromES2()
    {
        if (ES2.Exists("resources"))
        {
            resources = ES2.LoadDictionary<string, int>("resources");
        }
        else
        {
            resources = new Dictionary<string, int>();
        }
        if (ES2.Exists("chestsToClaim"))
        {
            chestsToClaim = ES2.LoadList<int>("chestsToClaim");
        }
        else
        {
            chestsToClaim = new List<int>();
        }
        if (ES2.Exists("claimedChests"))
        {
            claimedChests = ES2.LoadList<int>("claimedChests");
        }
        else
        {
            claimedChests = new List<int>();
        }
        if (ES2.Exists("savedSettings"))
        {
            savedSettings = ES2.LoadDictionary<string, bool>("savedSettings");
        }
        else
        {
            savedSettings = new Dictionary<string, bool>() {
                { "holsterView", true},
                { "showInfoText", true},
                { "turnWithTouchpad", true},
                { "touchpadMovement", false} };
        }
        if (ES2.Exists("records"))
        {
            records = ES2.LoadDictionary<int, float>("records");
        }
        else
        {
            records = new Dictionary<int, float>();
        }
        if (ES2.Exists("weapons") && GameObject.FindObjectOfType<Tutorial>() == null)
        {
            weapons = ES2.LoadList<string>("weapons");
        }
        else
        {
            weapons = new List<string>() { "SwordBase.0.0", "GunBase.0.0" };
        }
        if (ES2.Exists("weaponSettings"))
        {
            weaponSettings = ES2.LoadList<string>("weaponSettings");
        }
        else
        {
            weaponSettings = new List<string>() { weapons[0], weapons[0], weapons[1], weapons[1] };
        }
        if (ES2.Exists("hands"))
        {
            hands = ES2.LoadList<string>("hands");
        }
        else
        {
            hands = new List<string>() { "simpleHand.0.0" };
        }
        if (ES2.Exists("bodies"))
        {
            bodies = ES2.LoadList<string>("bodies");
        }
        else
        {
            bodies = new List<string>() { "simpleBody.0.0" };
        }
        if (ES2.Exists("heads"))
        {
            heads = ES2.LoadList<string>("heads");
        }
        else
        {
            heads = new List<string>() { "baseHead.0.0" };
        }
        if (ES2.Exists("bodyParts"))
        {
            bodyParts = ES2.LoadList<string>("bodyParts");
        }
        else
        {
            bodyParts = new List<string>() { heads[0], bodies[0], hands[0], hands[0] };
        }
    }

    public void LoadWeapons()
    {
        rightHip.specialHold = GameManager.instance.GetResourcePrefab(weaponSettings[0].Split('.')[0]);
        rightHip.holsterPosition = 0;
        rightHip.specialHoldData = weaponSettings[0].Substring(weaponSettings[0].Split('.')[0].Length);
        leftHip.specialHold = GameManager.instance.GetResourcePrefab(weaponSettings[1].Split('.')[0]);
        leftHip.holsterPosition = 1;
        rightHip.specialHoldData = weaponSettings[1].Substring(weaponSettings[1].Split('.')[0].Length);
        rightShoulder.specialHold = GameManager.instance.GetResourcePrefab(weaponSettings[2].Split('.')[0]);
        rightShoulder.holsterPosition = 2;
        rightHip.specialHoldData = weaponSettings[2].Substring(weaponSettings[2].Split('.')[0].Length);
        leftShoulder.specialHold = GameManager.instance.GetResourcePrefab(weaponSettings[3].Split('.')[0]);
        leftShoulder.holsterPosition = 3;
        rightHip.specialHoldData = weaponSettings[3].Substring(weaponSettings[3].Split('.')[0].Length);
    }

    public void SetWeapon(Holster holster, string setWeapon)
    {
        weaponSettings[holster.holsterPosition] = setWeapon;
        holster.specialHold = GameManager.instance.GetResourcePrefab(setWeapon.Split('.')[0]);
        holster.specialHoldData = setWeapon.Substring(setWeapon.Split('.')[0].Length);
        StartCoroutine("DoSave");
    }

    public void LoadBodyParts()
    {
        LoadHead();
        LoadBody();
        LoadLeftHand();
        LoadRightHand();
        SetBodyPartValues();
    }

    public void LoadHead()
    {
        bodyPartHead = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(bodyParts[0].Split('.')[0]), Vector3.zero, Quaternion.identity, true, head).GetComponent<Head>();
        bodyPartHead.position = 0;
        if (bodyParts[0].Split('.').Length > 1)
        {
            bodyPartHead.Init(bodyParts[0].Substring(bodyParts[0].Split('.')[0].Length));
        }
        else
        {
            bodyPartHead.Init();
        }
        bodyPartHead.gameObject.SetActive(false);
    }

    public void LoadBody()
    {
        bodyPartBody = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(bodyParts[1].Split('.')[0]), Vector3.zero, Quaternion.identity, true, head).GetComponent<Body>();
        bodyPartBody.position = 1;
        if (bodyParts[1].Split('.').Length > 1)
        {
            bodyPartBody.Init(bodyParts[1].Substring(bodyParts[1].Split('.')[0].Length));
        }
        else
        {
            bodyPartBody.Init();
        }
        bodyPartBody.gameObject.SetActive(false);
    }

    public void LoadRightHand()
    {
        bodyPartHandRight = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(bodyParts[2].Split('.')[0]), Vector3.zero, Quaternion.identity, true, rightManager.transform).GetComponent<Hand>();
        bodyPartHandRight.position = 2;
        if (bodyParts[2].Split('.').Length > 1)
        {
            bodyPartHandRight.Init(bodyParts[2].Substring(bodyParts[2].Split('.')[0].Length));
        }
        else
        {
            bodyPartHandRight.Init();
        }
    }
    public void LoadLeftHand()
    {
        bodyPartHandLeft = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(bodyParts[3].Split('.')[0]), Vector3.zero, Quaternion.identity, true, leftManager.transform).GetComponent<Hand>();
        bodyPartHandLeft.position = 3;
        bodyPartHandLeft.isLeft = true;
        if (bodyParts[3].Split('.').Length > 1)
        {
            bodyPartHandLeft.Init(bodyParts[3].Substring(bodyParts[3].Split('.')[0].Length));
        }
        else
        {
            bodyPartHandLeft.Init();
        }
    }

    public void SetBodyPartValues()
    {
        //set values from body parts
        health = bodyPartHead.health + bodyPartBody.health + bodyPartHandLeft.health + bodyPartHandRight.health;
        staffDamageMod = bodyPartHead.staffDamageMod + bodyPartBody.staffDamageMod + bodyPartHandLeft.staffDamageMod + bodyPartHandRight.staffDamageMod;
        meleeDamageMod = bodyPartHead.meleeDamageMod + bodyPartBody.meleeDamageMod + bodyPartHandLeft.meleeDamageMod + bodyPartHandRight.meleeDamageMod;
        bowDamageMod = bodyPartHead.bowDamageMod + bodyPartBody.bowDamageMod + bodyPartHandLeft.bowDamageMod + bodyPartHandRight.bowDamageMod;
        gunDamageMod = bodyPartHead.gunDamageMod + bodyPartBody.gunDamageMod + bodyPartHandLeft.gunDamageMod + bodyPartHandRight.gunDamageMod;
        overallDamageMod = bodyPartHead.overallDamageMod + bodyPartBody.overallDamageMod + bodyPartHandLeft.overallDamageMod + bodyPartHandRight.overallDamageMod;
        staffCritPercentMod = bodyPartHead.staffCritPercentMod + bodyPartBody.staffCritPercentMod + bodyPartHandLeft.staffCritPercentMod + bodyPartHandRight.staffCritPercentMod;
        meleeCritPercentMod = bodyPartHead.meleeCritPercentMod + bodyPartBody.meleeCritPercentMod + bodyPartHandLeft.meleeCritPercentMod + bodyPartHandRight.meleeCritPercentMod;
        bowCritPercentMod = bodyPartHead.bowCritPercentMod + bodyPartBody.bowCritPercentMod + bodyPartHandLeft.bowCritPercentMod + bodyPartHandRight.bowCritPercentMod;
        gunCritPercentMod = bodyPartHead.gunCritPercentMod + bodyPartBody.gunCritPercentMod + bodyPartHandLeft.gunCritPercentMod + bodyPartHandRight.gunCritPercentMod;
        overallCritPercentMod = bodyPartHead.overallCritPercentMod + bodyPartBody.overallCritPercentMod + bodyPartHandLeft.overallCritPercentMod + bodyPartHandRight.overallCritPercentMod;
        staffCritMultMod = bodyPartHead.staffCritMultMod + bodyPartBody.staffCritMultMod + bodyPartHandLeft.staffCritMultMod + bodyPartHandRight.staffCritMultMod;
        meleeCritMultMod = bodyPartHead.meleeCritMultMod + bodyPartBody.meleeCritMultMod + bodyPartHandLeft.meleeCritMultMod + bodyPartHandRight.meleeCritMultMod;
        bowCritMultMod = bodyPartHead.bowCritMultMod + bodyPartBody.bowCritMultMod + bodyPartHandLeft.bowCritMultMod + bodyPartHandRight.bowCritMultMod;
        gunCritMultMod = bodyPartHead.gunCritMultMod + bodyPartBody.gunCritMultMod + bodyPartHandLeft.gunCritMultMod + bodyPartHandRight.gunCritMultMod;
        overallCritMultMod = bodyPartHead.overallCritMultMod + bodyPartBody.overallCritMultMod + bodyPartHandLeft.overallCritMultMod + bodyPartHandRight.overallCritMultMod;
        staffSpeedMod = bodyPartHead.staffSpeedMod + bodyPartBody.staffSpeedMod + bodyPartHandLeft.staffSpeedMod + bodyPartHandRight.staffSpeedMod;
        meleeSpeedMod = bodyPartHead.meleeSpeedMod + bodyPartBody.meleeSpeedMod + bodyPartHandLeft.meleeSpeedMod + bodyPartHandRight.meleeSpeedMod;
        bowSpeedMod = bodyPartHead.bowSpeedMod + bodyPartBody.bowSpeedMod + bodyPartHandLeft.bowSpeedMod + bodyPartHandRight.bowSpeedMod;
        gunSpeedMod = bodyPartHead.gunSpeedMod + bodyPartBody.gunSpeedMod + bodyPartHandLeft.gunSpeedMod + bodyPartHandRight.gunSpeedMod;
        overallSpeedMod = bodyPartHead.overallSpeedMod + bodyPartBody.overallSpeedMod + bodyPartHandLeft.overallSpeedMod + bodyPartHandRight.overallSpeedMod;
        bowRangeModifier = bodyPartHead.bowRangeModifier + bodyPartBody.bowRangeModifier + bodyPartHandLeft.bowRangeModifier + bodyPartHandRight.bowRangeModifier;
        gunRangeModifier = bodyPartHead.gunRangeModifier + bodyPartBody.gunRangeModifier + bodyPartHandLeft.gunRangeModifier + bodyPartHandRight.gunRangeModifier;
        agroDistanceMod = bodyPartHead.agroDistanceMod + bodyPartBody.agroDistanceMod + bodyPartHandLeft.agroDistanceMod + bodyPartHandRight.agroDistanceMod;
        lightMod = bodyPartHead.lightMod + bodyPartBody.lightMod + bodyPartHandLeft.lightMod + bodyPartHandRight.lightMod;
        blockChance = bodyPartHead.blockChance + bodyPartBody.blockChance + bodyPartHandLeft.blockChance + bodyPartHandRight.blockChance;
        dodgeChance = bodyPartHead.dodgeChance + bodyPartBody.dodgeChance + bodyPartHandLeft.dodgeChance + bodyPartHandRight.dodgeChance;
        thornsDamage = bodyPartHead.thornsDamage + bodyPartBody.thornsDamage + bodyPartHandLeft.thornsDamage + bodyPartHandRight.thornsDamage;
        teleportDistanceMod = bodyPartHead.teleportDistanceMod + bodyPartBody.teleportDistanceMod + bodyPartHandLeft.teleportDistanceMod + bodyPartHandRight.teleportDistanceMod;
        teleportCooldownMod = bodyPartHead.teleportCooldownMod + bodyPartBody.teleportCooldownMod + bodyPartHandLeft.teleportCooldownMod + bodyPartHandRight.teleportCooldownMod;
        leftManager.teleportRenderer.maximumLength = new Vector2(15, 10) * (1 + teleportDistanceMod / 100);
        rightManager.teleportRenderer.maximumLength = new Vector2(15, 10) * (1 + teleportDistanceMod / 100);
        GetComponentInChildren<LightFlicker>().high = 1.2f * (1 + lightMod / 100);
        GetComponentInChildren<LightFlicker>().low = 1f * (1 + lightMod / 100);
        GetComponentInChildren<Light>().range = 10f * (1 + lightMod / 100);
        if (otherPlayerObject != null)
        {
            otherPlayerObject.maxHealth = health;
            otherPlayerObject.health = health;
            otherPlayerObject.UpdateHealthBar();
        }
        healthText.text = Mathf.Ceil(health).ToString();
        if (primaryUI != null)
        {
            primaryUI.healthText.text = Mathf.Ceil(health).ToString();
        }
    }

    public void SetBodyPart(int position, string setBodyPart)
    {
        bodyParts[position] = setBodyPart;
        switch (position)
        {
            case 0:
                LoadHead();
                break;
            case 1:
                LoadBody();
                break;
            case 2:
                LoadRightHand();
                break;
            case 3:
                LoadLeftHand();
                break;
            default:
                break;
        }
        SetBodyPartValues();
        StartCoroutine("DoSave");
    }

    public void SpawnPlayerObject()
    {
        otherPlayerObject = dgUtil.Instantiate(otherPlayerObjectPrefab, transform.position, transform.rotation, true, null, false).GetComponent<OtherPlayerObject>();
        otherPlayerObject.InitLocal(this);
    }

    public GameObject SpawnWeapon(GameObject weapon, string weaponString = "")
    {
        if (otherPlayerObject.isDead)
        {
            return null;
        }
        object[] data = new object[1];
        data[0] = weaponString;
        GameObject go = dgUtil.Instantiate(weapon, transform.position, transform.rotation, false, null, false, data);
        return go;
    }

    bool controllerSet = false;
    public List<HealingZone> healingZones = new List<HealingZone>();
    public void Update()
    {
        if (scaleModStart + scaleModTimer < Time.time && transform.localScale != Vector3.one)
        {
            Vector3 position = GetPlayerStandingPosition();
            transform.localScale = Vector3.one;
            otherPlayerObject.transform.localScale = Vector3.one;
            teleporter.ForceTeleport(position);
        }
        teleporter.ToggleTeleportEnabled(lastTeleportTime + teleportCooldown < Time.time);
        if (healthText.color.a > 0)
        {
            healthIcon.color -= new Color(0, 0, 0, Time.deltaTime / 5);
            healthText.color -= new Color(0, 0, 0, Time.deltaTime / 5);
        }
        if (coinsText.color.a > 0)
        {
            coinsIcon.color -= new Color(0, 0, 0, Time.deltaTime / 5);
            coinsText.color -= new Color(0, 0, 0, Time.deltaTime / 5);
        }
        if (gemsText.color.a > 0)
        {
            gemsIcon.color -= new Color(0, 0, 0, Time.deltaTime / 5);
            gemsText.color -= new Color(0, 0, 0, Time.deltaTime / 5);
        }
        if (otherPlayerObject.isDead)
        {
            if (!PrimaryUI.instance.isWorldLocked)
            {
                PrimaryUI.instance.WorldLockToggle();
            }
            leftManager.menuPointer.Toggle(true);
            rightManager.menuPointer.Toggle(true);
            leftManager.uiPointer.enabled = true;
            rightManager.uiPointer.enabled = true;
            return;
        }
        else
        {
            leftManager.menuPointer.Toggle(primaryUI.isWorldLocked);
            rightManager.menuPointer.Toggle(primaryUI.isWorldLocked);
            leftManager.uiPointer.enabled = primaryUI.isWorldLocked;
            rightManager.uiPointer.enabled = primaryUI.isWorldLocked;
        }
        foreach (HealingZone hz in healingZones)
        {
            if (otherPlayerObject.lastDamageTime + 5 < Time.time)
            {
                if (otherPlayerObject.health < otherPlayerObject.maxHealth)
                {
					pet.ConditionalAttachAudio3 ();
                }
                otherPlayerObject.Heal(hz.healingPerSecond * Time.deltaTime);
            }
        }
        if (!controllerSet)
        {
            SetControls();
        }
        if (isAgroed)
        {
            battleMusic.volume = Mathf.Min(0.3f, 0.02f + battleMusic.volume);
            ambientMusic.volume /= 1.01f;
        }
        else
        {
            ambientMusic.volume = Mathf.Min(0.2f, 0.01f + ambientMusic.volume);
            battleMusic.volume /= 1.01f;
        }


#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (LevelManager.instance != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LevelManager.instance.test.seed = Mathf.FloorToInt(Random.value * 10000);
                LevelManager.instance.test.SetState();
                GameManager.instance.LoadLevel();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerPrefs.SetInt("tutorial", 1);
            GameManager.instance.LoadMainMenu();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameManager.instance.LoadTutorial();
        }
#else

#endif
    }

    public void CheckAgro()
    {
        isAgroed = false;
        if (GameManager.GetScene() != "Tutorial")
        {
            foreach (GameObject be in CreateLevel.instance.enemies)
            {
                if (be != null)
                {
                    if (!be.GetComponent<BaseEnemy>().isDead)
                    {
                        isAgroed = isAgroed || be.GetComponent<BaseEnemy>().otherPlayerObject == otherPlayerObject;
                    }
                }
                if (isAgroed)
                {
                    break;
                }
            }
        }
		if (!isAgroed && CreateLevel.instance == null ? false : 
            (CreateLevel.instance.levelType != LevelType.InfiniteWave
            && CreateLevel.instance.levelType != LevelType.Wave)) {
			if (otherPlayerObject.health < otherPlayerObject.maxHealth / 2) {
				if (Random.value < 0.5f) {
					pet.PlayAudio (pet.clipsFindFire);
				}
			}
			else if (Random.value < 0.2f) {
				pet.PlayAudio (pet.clipsBattleOver);
			}
		}
    }

    public void Die()
    {
        TrackEvent("death");
        primaryUI.healthText.text = Mathf.Ceil(otherPlayerObject.health).ToString();
        healthText.text = Mathf.Ceil(otherPlayerObject.health).ToString();
        if (!PrimaryUI.instance.isWorldLocked)
        {
            PrimaryUI.instance.WorldLockToggle();
        }
        if (leftManager != null)
        {
            leftManager.Die();
        }
        if (rightManager != null)
        {
            rightManager.Die();
        }
        if (CreateLevel.instance != null)
        {
            foreach (GameObject go in CreateLevel.instance.enemies)
            {
                if (go != null)
                {
                    BaseEnemy enemy = go.GetComponent<BaseEnemy>();
                    if (enemy.target == PlayerManager.instance.otherPlayerObject.transform)
                    {
                        enemy.target = null;
                    }
                }
            }
            if (CreateLevel.instance.levelType == LevelType.InfiniteWave)
            {
                Debug.Log(score);
                SteamworksManager.instance.UpdateInfiniteWaveScore(score, CreateLevel.instance.specialComplete);
            }
        }
        leftManager.menuPointer.Toggle(true);
        rightManager.menuPointer.Toggle(true);
        leftManager.uiPointer.enabled = true;
        rightManager.uiPointer.enabled = true;
        if (CreateLevel.instance != null)
        {
            if (CreateLevel.instance.levelType == LevelType.Survive)
            {
                CreateLevel.instance.levelFailed = true;
                CreateLevel.instance.SetMissionText("You died, level failed");
                Ship.instance.comet.SetActive(true);
            }
        }
    }

    public void Revive()
    {
        TrackEvent("revive");
        headsetFader.Fade(new Color(1, 1, 1, 0f), 0.1f);
        SetControls();
        leftManager.menuPointer.Toggle(false);
        rightManager.menuPointer.Toggle(false);
        leftManager.uiPointer.enabled = false;
        rightManager.uiPointer.enabled = false;
        if (primaryUI.isWorldLocked)
        {
            primaryUI.WorldLockToggle();
        }
    }

    public void SetControls()
    {
        if (leftManager != null)
        {
            leftManager.SetControls();
        }
        if (rightManager != null)
        {
            rightManager.SetControls();
        }
        controllerSet = true;
    }

    public GameObject CreateProjectile(string pName, Vector3 pPosition, Quaternion pRotation, Color mat, float pSpeed, bool pGravity, float damage = 1)
    {
        Quaternion tempColor = new Quaternion(mat.r, mat.g, mat.b, mat.a);
        if (otherPlayerObject.isDead)
        {
            return null;
        }
        if (PhotonNetwork.connected)
        {
            otherPlayerObject.GetComponent<PhotonView>().RPC("CreateProjectile", PhotonTargets.Others, pName, pPosition, pRotation, tempColor, pSpeed, pGravity, false, damage);
        }
        return CreateProjectile(pName, pPosition, pRotation, tempColor, pSpeed, pGravity, true, damage);
    }

    public GameObject CreateProjectile(string pName, Vector3 pPosition, Quaternion pRotation, Quaternion mat, float pSpeed, bool pGravity, bool isMine = false, float damage = 1)
    {
        if (otherPlayerObject.isDead)
        {
            return null;
        }
        GameObject bulletClone = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(pName), pPosition, pRotation, false, null, false) as GameObject;
        Rigidbody rb = bulletClone.GetComponent<Rigidbody>();
        Bullet bul = bulletClone.GetComponent<Bullet>();
        rb.AddForce(-bulletClone.transform.forward * pSpeed);
        bul.isMine = isMine;
        bul.damage = damage;
        bul.SetMaterial(mat);
        return bulletClone;
    }

    public void TakeDamage()
    {
        UpdateHealthFade();
        if (myAudioSource != null && grunts.Count > 0)
        {
            PlayAudio(grunts[Mathf.FloorToInt(Random.value * grunts.Count)]);
        }
    }

    public void PlayAudio(AudioClip setClip)
    {
        if (setClip != null)
        {
            myAudioSource.pitch = Random.value * 0.4f + 0.8f;
            myAudioSource.clip = setClip;
            myAudioSource.Play();
        }
    }

    public Color fadeColor = Color.clear;
    public void UpdateHealthFade()
    {
        if (otherPlayerObject.isDead)
        {
            headsetFader.Fade(new Color(0, 0, 0, 0.9f), 0.1f);
            return;
        }
        healthText.text = Mathf.Ceil(otherPlayerObject.health).ToString();
        healthText.color = Color.red;
        healthIcon.color = Color.white;
        primaryUI.healthText.text = Mathf.Ceil(otherPlayerObject.health).ToString();
    }


    //This is the resource management section
    public int GetResource(string resource)
    {
        resource = resource.ToLower();
        if (!resources.ContainsKey(resource))
        {
            CreateResource(resource);
        }
        return resources[resource];
    }
    private void CreateResource(string resource)
    {
        resources.Add(resource, 0);
    }
    public void SetResource(string resource, int count)
    {
        resource = resource.ToLower();
        if (!resources.ContainsKey(resource))
        {
            CreateResource(resource);
        }
        resources[resource] = count;
        StartCoroutine("DoSave");
        UpdateResourceText(resource);
    }
    public bool SubtractResources(string resource, int increment)
    {
        resource = resource.ToLower();
        if (!resources.ContainsKey(resource))
        {
            CreateResource(resource);
        }
        if (resources[resource] - increment < 0)
        {
            UpdateResourceText(resource);
            return false;
        }
        AddResource(resource, -increment);
        return true;
    }
    public void AddResource(string resource, int increment)
    {
        resource = resource.ToLower();
        if (!resources.ContainsKey(resource))
        {
            CreateResource(resource);
        }
        resources[resource] += increment;
        StartCoroutine("DoSave");
        UpdateResourceText(resource, increment);
    }
    public void AddWeapon(string weaponToAdd)
    {
        if (!weapons.Contains(weaponToAdd))
        {
            weapons.Add(weaponToAdd);
            StartCoroutine("DoSave");
        }
    }
    public void AddHand(string handToAdd)
    {
        if (!hands.Contains(handToAdd))
        {
            hands.Add(handToAdd);
            StartCoroutine("DoSave");
        }
    }
    public void AddBody(string bodyToAdd)
    {
        if (!bodies.Contains(bodyToAdd))
        {
            bodies.Add(bodyToAdd);
            StartCoroutine("DoSave");
        }
    }
    public void AddHead(string headToAdd)
    {
        if (!heads.Contains(headToAdd))
        {
            heads.Add(headToAdd);
            StartCoroutine("DoSave");
        }
    }

    bool isSaving = false;
    public IEnumerator DoSave()
    {
        if (isSaving)
        {
            yield return null;
        }
        else
        {
            isSaving = true;
            yield return new WaitForSeconds(1f);
            SaveResources();
        }
    }

    public void ImmediateSave()
    {
        SaveResources();
    }

    public void SaveResources()
    {
        SortList(hands);
        SortList(heads);
        SortList(bodies);
        SortList(weapons);
        ES2.Save(resources, "resources");
        if (Tutorial.instance == null)
        {
            ES2.Save(weapons, "weapons");
        }
        ES2.Save(claimedChests, "claimedChests");
        ES2.Save(chestsToClaim, "chestsToClaim");
        ES2.Save(hands, "hands");
        ES2.Save(bodies, "bodies");
        ES2.Save(heads, "heads");
        ES2.Save(bodyParts, "bodyParts");
        ES2.Save(weaponSettings, "weaponSettings");
        ES2.Save(records, "records");
        ES2.Save(savedSettings, "savedSettings");
        isSaving = false;
    }
    void SortList(List<string> list)
    {
        Dictionary<string, int> dic = new Dictionary<string, int>();
        foreach (string item in list)
        {
            if (item.Split('.').Length > 2)
            {
                dic.Add(item, int.Parse(item.Split('.')[2]));
            }
            else
            {
                dic.Add(item, 0);
            }
        }
        List<string> sortedList = new List<string>();
        for (int i = 0; i < dic.Count; i++)
        {
            string value = list[0];
            for (int j = 1; j < list.Count; j++)
            {
                if (dic[value] < dic[list[j]])
                {
                    value = list[j];
                }
            }
            list.Remove(value);
            sortedList.Add(value);
        }
        foreach (string str in sortedList)
        {
            list.Add(str);
        }
    }

    private Flytext coinsFlytext, gemsFlytext;
    void UpdateResourceText(string resource, int amount = 0)
    {
        switch (resource)
        {
            case "coins":
                primaryUI.coinsText.text = resources[resource].ToString();
                coinsText.text = resources[resource].ToString();
                coinsText.color = Color.yellow;
                coinsIcon.color = Color.white;
                if (amount != 0)
                {
                    coinsFlytext = dgUtil.SpawnFlytext(Color.yellow, amount.ToString(), coinsText.transform.position);
                }
                break;
            case "gems":
                primaryUI.gemsText.text = resources[resource].ToString();
                gemsText.text = resources[resource].ToString();
                gemsText.color = Color.cyan;
                gemsIcon.color = Color.white;
                if (amount != 0)
                {
                    gemsFlytext = dgUtil.SpawnFlytext(Color.cyan, amount.ToString(), gemsText.transform.position);
                }
                break;
            default:
                break;
        }
    }
    public void ResetResources()
    {
        PlayerPrefs.DeleteAll();
        resources = new Dictionary<string, int>();
        weapons = new List<string>() { "SwordBase.0.0", "GunBase.0.0" };
        hands = new List<string>() { "simpleHand.0.0" };
        bodies = new List<string>() { "simpleBody.0.0" };
        heads = new List<string>() { "baseHead.0.0" };
        records = new Dictionary<int, float>();
        savedSettings = new Dictionary<string, bool>() {
            { "holsterView", true},
            { "buddyAudio", true},
            { "touchpadMovement", false},
            { "showInfoText", true},
            { "turnWithTouchpad", true} };
        weaponSettings = new List<string>() { weapons[0], weapons[0], weapons[1], weapons[1] };
        bodyParts = new List<string>() { heads[0], bodies[0], hands[0], hands[0] };
        LoadWeapons();
        LoadBodyParts();
        foreach (KeyValuePair<string, int> kvp in resources)
        {
            UpdateResourceText(kvp.Key);
        }
        ImmediateSave();
        GameManager.instance.LoadMainMenu();
    }

    public void ToggleHolsters()
    {
        savedSettings["holsterView"] = !GetSetting("holsterView");
        rightHip.ToggleHolster(GetSetting("holsterView"));
        leftHip.ToggleHolster(GetSetting("holsterView"));
        rightShoulder.ToggleHolster(GetSetting("holsterView"));
        leftShoulder.ToggleHolster(GetSetting("holsterView"));
        SaveResources();
    }

    public void BuddyAudioToggle()
    {
        savedSettings["buddyAudio"] = !GetSetting("buddyAudio");
        SaveResources();
    }

    public void ToggleTouchpadTurning()
    {
        savedSettings["turnWithTouchpad"] = !GetSetting("turnWithTouchpad");
        leftManager.ToggleTouchpadTurning(GetSetting("turnWithTouchpad"));
        rightManager.ToggleTouchpadTurning(GetSetting("turnWithTouchpad"));
        SaveResources();
    }

    public void ToggleInfoText()
    {
        savedSettings["showInfoText"] = !GetSetting("showInfoText");
        SaveResources();
    }

    public void ToggleTouchpadMovement()
    {
        savedSettings["touchpadMovement"] = !GetSetting("touchpadMovement");
        SaveResources();
    }

    public void AddChest(int seed)
    {
        if (!claimedChests.Contains(seed) && !chestsToClaim.Contains(seed))
        {
            chestsToClaim.Add(seed);
            StartCoroutine("DoSave");
        }
    }

    public void ClaimChest(int seed)
    {
        TrackEvent("claim_chest");
        if (chestsToClaim.Contains(seed))
        {
            chestsToClaim.Remove(seed);
        }
        claimedChests.Add(seed);
        StartCoroutine("DoSave");
    }

    public void TrackEvent(string toTrack)
    {
        Analytics.CustomEvent(toTrack, sendData);
    }

    public void ScaleUp(float scale)
    {
        Vector3 position = GetPlayerStandingPosition();
        scaleModStart = Time.time;
        transform.localScale = Vector3.one * scale;
        otherPlayerObject.transform.localScale = Vector3.one * scale;
        teleporter.ForceTeleport(position);
    }
}