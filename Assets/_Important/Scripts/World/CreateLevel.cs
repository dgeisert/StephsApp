using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateLevel : MonoBehaviour
{
    public static CreateLevel instance;

	public GameObject gameManager;
    public LevelType levelType;
    public int level;
    public bool skipDataLoad = false;
    public GameObject islandBase, footprints, arbitrarySpawnPoint;
    public int islandCount = 100, seed, actualIslandCount;
    public float sizeMin, sizeMax, perlinCrunch = 500f, perlinVoidThreshhold = 0.4f, islandEnemyStartCount = 0, timeInLevel;
    float perlinOffsetYX = 0, perlinOffsetYZ = 0
        , perlinOffsetX = 0, perlinOffsetY = 0
        , perlinOffsetVoidX = 0, perlinOffsetVoidZ = 0
        , yScale = 0;
    Vector3 zeroing = Vector3.zero;
    public List<Biome> biomes;
    public Biome simpleBiome;
    public List<CreateIsland> islands;
    List<PointOfInterest>[] specialPOIs;
    public List<GameObject> enemies;
    public int specialValue, specialComplete = 0;
    public bool levelFailed = false;
    float levelStartTime;
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    //special POIs
    public PointOfInterest rescueObject;
    public GameObject ship, thisWayMarker;
    public Vector3 exitShipPosition, enterShipPosition;

    public int badWordSpawn;
    public GameObject badWordEnemy;

    //this section for stats
    public float enemiesKilled = 0, totalEnemies = 0, damageTaken = 0, healing = 0;
    public void SaveStats()
    {
        PlayerPrefs.SetFloat("completedKills", enemiesKilled);
        PlayerPrefs.SetFloat("completedTotalEnemies", totalEnemies);
        PlayerPrefs.SetFloat("completedDamage", damageTaken);
        PlayerPrefs.SetFloat("completedHealing", healing);
        PlayerPrefs.SetFloat("completedTime", timeInLevel);
    }

    //this section for bosses
    public List<PointOfInterest> bossPOI;

    void Awake()
    {
        if (!skipDataLoad)
        {
            PlayerPrefs.SetInt("rescued", 0);
            level = PlayerPrefs.GetInt("playingLevel");
            islandCount = PlayerPrefs.GetInt("playingLevelIslandCount");
            sizeMin = PlayerPrefs.GetFloat("playingLevelSizeMin");
            sizeMax = PlayerPrefs.GetFloat("playingLevelSizeMax");
            levelType = (LevelType)PlayerPrefs.GetInt("playingLevelType");
            specialValue = PlayerPrefs.GetInt("playingLevelSpecialValue");
            badWordSpawn = PlayerPrefs.GetInt("playingLevelBadWord");
            if (PlayerPrefs.GetInt("playingLevelSeed") != 0)
            {
                seed = PlayerPrefs.GetInt("playingLevelSeed");
            }
            PlayerPrefs.SetInt("playingLevelSeed", 0);
        }

        CreateLevel.instance = this;
        Random.InitState(seed);
        levelStartTime = Time.time;

        enemies = new List<GameObject>();
        perlinOffsetX = Random.value * 100;
        perlinOffsetY = Random.value * 100;
        perlinOffsetYX = Random.value * 100;
        perlinOffsetYZ = Random.value * 100;
        perlinOffsetVoidX = Random.value * perlinCrunch;
        perlinOffsetVoidZ = Random.value * perlinCrunch;
        yScale = Random.value * 100;

        islands = new List<CreateIsland>();
        float furthestIslandDistance = 0, closestIslandDistance = 0;
        int furthestIslandIndex = 0, closestIslandIndex = 0;
        List<Biome> setBiomes = new List<Biome>();
        if (levelType == LevelType.InfiniteWave)
        {
            setBiomes.Add(simpleBiome);
        }
        else
        {
            foreach (Biome b in biomes)
            {
                if (b.requiredLevel <= level)
                {
                    setBiomes.Add(b);
                }
            }
        }
        biomes = setBiomes;

        //set positions and create CreateIslands bases
        float yOffset = yScale * Mathf.PerlinNoise(
            (perlinOffsetYX) / perlinCrunch
            , (perlinOffsetYZ) / perlinCrunch);
        Vector3 position = Vector3.zero;
        for (int i = 0; i < islandCount * 10; i++)
        {
            int basePosition = Mathf.FloorToInt(Random.value * islands.Count);
            if (islands.Count > 0)
            {
                position = islands[basePosition].transform.position
                + new Vector3((Random.value - 0.5f), 0, (Random.value - 0.5f)).normalized
                    * ((sizeMax - sizeMin) * Random.value + (sizeMin * 1.2f));
            }
            float minDistance = sizeMax * 1.5f;
            foreach (CreateIsland island in islands)
            {
                float distance = Vector3.Distance(position, island.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
            if (minDistance > sizeMin * 1.1f)
            {
                Vector3 placement = new Vector3(
                    position.x
                    , yScale * Mathf.PerlinNoise(
                        (position.x + perlinOffsetYX) / perlinCrunch
                        , (position.y + perlinOffsetYZ) / perlinCrunch)
                    - yOffset
                    , position.z);
                GameObject go = dgUtil.Instantiate(islandBase, placement, Quaternion.identity);
                go.GetComponent<CreateIsland>().index = islands.Count;
                if (go.transform.position.magnitude > furthestIslandDistance)
                {
                    furthestIslandDistance = go.transform.position.magnitude;
                    furthestIslandIndex = islands.Count;
                }
                islands.Add(go.GetComponent<CreateIsland>());
                if (i > 0 && islands[basePosition] != null)
                {
                    go.GetComponent<CreateIsland>().fromIsland = islands[basePosition];
                }
            }
            if (islands.Count >= islandCount)
            {
                break;
            }
        }
		if (islandCount > 1) {
			for (int i = 0; i < islandCount; i++) {
				if (Vector3.Distance (islands [i].transform.position, islands [furthestIslandIndex].transform.position) > closestIslandDistance) {
					closestIslandDistance = Vector3.Distance (islands [i].transform.position, islands [furthestIslandIndex].transform.position);
					closestIslandIndex = i;
				}
			}
		} else {
			closestIslandIndex = 0;
			closestIslandDistance = 0;
		}

        specialPOIs = new List<PointOfInterest>[islands.Count];
        
        //set up the level type
        if (level == 0)
        {
            islandEnemyStartCount = 1;
        }
        else
        {
            switch (levelType)
            {
                case LevelType.Exit:
                    islandEnemyStartCount = 1;
                    break;
                case LevelType.Timed:
                    islandEnemyStartCount = 0.75f;
                    break;
                case LevelType.Boss:
                    if (Mathf.FloorToInt(level / 10) < bossPOI.Count)
                    {
                        if (specialPOIs[specialPOIs.Length - 1] == null)
                        {
                            specialPOIs[specialPOIs.Length - 1] = new List<PointOfInterest>();
                        }
                        specialPOIs[specialPOIs.Length - 1].Add(bossPOI[Mathf.FloorToInt(level / 10)]);
                    }
                    break;
                case LevelType.Rescue:
                    for (int i = 0; i < specialValue; i++)
                    {
                        int poiIndex = Mathf.FloorToInt(islands.Count - i * islands.Count / specialValue - 1);
                        if (specialPOIs[poiIndex] == null)
                        {
                            specialPOIs[poiIndex] = new List<PointOfInterest>();
                        }
                        specialPOIs[poiIndex].Add(rescueObject);
                    }
                    break;
			case LevelType.Survive:
				islandEnemyStartCount = 0;
				break;
			case LevelType.Wave:
				islandEnemyStartCount = 0;
				break;
			case LevelType.InfiniteWave:
				islandEnemyStartCount = 0;
				break;
                default:
                    break;
            }
        }


        //set sizes and set island values
        for (int i = 0; i < islands.Count; i++)
        {
            float minDistance = sizeMax;
            CreateIsland closestIsland = islands[0];
            for (int j = 0; j < islands.Count; j++)
            {
                if (islands[j] != islands[i])
                {
                    float distance = Vector3.Distance(islands[j].transform.position, islands[i].transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestIsland = islands[j];
                    }
                }
            }
            islands[i].closestIsland = closestIsland;
            islands[i].size = Mathf.FloorToInt(minDistance);
            islands[i].randomSeed = Mathf.FloorToInt(10000 * Random.value);
            islands[i].biome = islands[i].gameObject.AddComponent<Biome>();
            islands[i].biome.Init(islands[i].size, biomes[Mathf.FloorToInt(Random.value * biomes.Count)], islands[i]);
            if (!islands[i].initialized)
            {
                islands[i].Init(this, specialPOIs[islands[i].index], islandEnemyStartCount * level);
            }
        }

        //put ship in for entering and exiting


		switch (levelType)
		{
		case LevelType.Survive:
		case LevelType.Wave:
			foreach (SpawnPoint sp in spawnPoints) {
				sp.singleType = false;
			}
			islandEnemyStartCount = 0;
			break;
		case LevelType.InfiniteWave:
			level = 5;
			spawnPoints = new List<SpawnPoint> ();
			for (int i = 0; i < 6; i++) {
				SpawnPoint newSP = dgUtil.Instantiate (
					                   arbitrarySpawnPoint
					, new Vector3 (Mathf.Cos (i * Mathf.PI / 3), 0.01f, Mathf.Sin (i * Mathf.PI / 3)).normalized * 30
					, Quaternion.identity).GetComponent<SpawnPoint> ();
				newSP.Init ();
				spawnPoints.Add (newSP);
			}
			break;
		default:
			break;
		}

		if (levelType == LevelType.InfiniteWave) {
			exitShipPosition = new Vector3 (0, -1000, 0);
			enterShipPosition = new Vector3 (0, islands[0].transform.position.y, 0);
		} else {
			Vector3 exitXZ = islands [furthestIslandIndex].transform.position.normalized * (furthestIslandDistance + islands [furthestIslandIndex].size / 2 + 15);
			exitXZ = (exitXZ.magnitude - 5) * exitXZ.normalized;
			exitShipPosition = new Vector3 (exitXZ.x, islands [furthestIslandIndex].transform.position.y - 5, exitXZ.z);

			closestIslandDistance = (islands [closestIslandIndex].transform.position).magnitude;
			Vector3 enterXZ = islands [closestIslandIndex].transform.position + (islands [closestIslandIndex].transform.position - exitShipPosition).normalized * (islands [closestIslandIndex].size / 2);
			enterShipPosition = new Vector3 (enterXZ.x, islands [closestIslandIndex].transform.position.y, enterXZ.z);

            bool aboveGround = false;
            while (!aboveGround)
            {
                RaycastHit hit;
                Physics.Raycast(enterShipPosition + Vector3.up * 2, -Vector3.up, out hit, 25, (1 << 0));
                if (hit.collider != null)
                {
                    if (hit.collider.name == "Top")
                    {
                        enterShipPosition = hit.point;
                        aboveGround = true;
                        enterShipPosition -= enterShipPosition.normalized * 10;
                    }
                }
                enterShipPosition -= enterShipPosition.normalized;
            }
        }
		GameObject thisGameManager = dgUtil.Instantiate (gameManager, enterShipPosition, Quaternion.identity);
        GameObject thisShip = dgUtil.Instantiate(ship, exitShipPosition, Quaternion.identity);
        thisShip.GetComponent<Ship>().Init();

        actualIslandCount = islands.Count;

        MissionStart();
    }

    public void MissionStart()
    {
        //set up the level type
        if (level == 0)
        {
            SetMissionText("find the ship\nthe comet will lead the way");
        }
        else
        {
            switch (levelType)
            {
                case LevelType.Exit:
                    SetMissionText("find the ship");
                    break;
                case LevelType.Timed:
                    StartTimer((float)specialValue);
                    SetMissionText("find the ship in time");
                    break;
                case LevelType.Boss:
                    SetMissionText("defeat the boss");
                    break;
                case LevelType.Rescue:
                    SetMissionText("rescue the hostages");
                    break;
                case LevelType.Survive:
                    StartTimer((float)specialValue);
                    InvokeRepeating("SurvivalSpawnEnemies", 5, 8);
                    SetMissionText("survive until timer ends");
				break;
			case LevelType.Wave:
				InvokeRepeating ("CheckEnemies", 1, 1);
				break;
			case LevelType.InfiniteWave:
                    InvokeRepeating("CheckEnemies", 1, 1);
                    break;
                default:
                    break;
            }
        }
    }

    public void Start()
    {
        OnTeleport(PlayerManager.instance.GetPlayerPosition());


        //set up the level type
        switch (levelType)
        {
            case LevelType.Exit:
                break;
            case LevelType.Timed:
                break;
            case LevelType.Boss:
                break;
            case LevelType.Rescue:
                break;
            case LevelType.Survive:
			break;
		case LevelType.Wave:
			break;
		case LevelType.InfiniteWave:
			break;
            default:
                break;
        }
    }

    public void Update()
    {
        timeInLevel = Time.time - levelStartTime;
        switch (levelType)
        {
            case LevelType.Timed:
                if (specialValue - timeInLevel < 10)
                {
                    PlayerManager.instance.infoLeftAlign.color = Color.red;
                }
                else if (specialValue - timeInLevel < 60)
                {
                    PlayerManager.instance.infoLeftAlign.color = Color.yellow;
                }
                if (timeInLevel > specialValue)
                {
                    levelFailed = true;
                    SetMissionText("time up, level failed");
                }
                break;
            case LevelType.Survive:
                if (specialValue - timeInLevel < 10)
                {
                    PlayerManager.instance.infoLeftAlign.color = Color.red;
                }
                break;
            case LevelType.InfiniteWave:
                if(PlayerManager.instance != null)
                {
                    PlayerManager.instance.infoLeftAlign.text = "score: " + PlayerManager.instance.score.ToString();
                }
                break;
            default:
                break;
        }
        if(Ship.instance != null)
        {
            if (!Ship.instance.playerOnShip && (PlayerManager.instance.savedSettings["showInfoText"] || timeInLevel < 30))
            {
                PlayerManager.instance.infoRightAlign.text = missionText;
            }
        }
    }

    public void SurvivalSpawnEnemies()
    {
        if (enemies.Count > 100)
        {
            return;
        }
        int enemyCount = Mathf.FloorToInt(level / 2);
        SpawnEnemies(enemyCount, true, 100);
    }
    public void Rescue()
    {
        specialComplete++;
        SetMissionText(specialComplete.ToString() + " of " + specialValue.ToString() + " rescued");
        RescueSpawnEnemies();
        if (specialValue <= specialComplete)
        {
            Ship.instance.comet.SetActive(true);
            Invoke("GoToExit", 2f);
        }
    }
    void GoToExit()
    {
        SetMissionText("Head to your ship");
    }
    public void RescueSpawnEnemies()
    {
        int enemyCount = level + specialComplete;
        SpawnEnemies(enemyCount, true, 50);
    }
    public void WaveSpawnEnemies()
    {
		if (specialValue == -1) {
			PlayerManager.instance.infoCenterAlign.text = "Wave " + specialComplete;
		} else {
			PlayerManager.instance.infoCenterAlign.text = "Wave " + specialComplete + " of " + specialValue;
		}
        int enemyCount = level + specialComplete * 3;
		if (levelType == LevelType.InfiniteWave) {
			enemyCount += specialComplete * 8;
		}
        SpawnEnemies(enemyCount, true, 100);
    }


    public float IslandInitDistance = 100f;
    public void OnTeleport(Vector3 playerPosition)
    {
        if (PlayerManager.instance != null && timeInLevel > 1)
        {
            dgUtil.Instantiate(footprints
                , PlayerManager.instance.GetPlayerStandingPosition()
                , PlayerManager.instance.playerHips.rotation);
        }
        CreateIsland closestIsland = GetClosestIsland(playerPosition);
        if (PlayerManager.instance.particles != null)
        {
            if (closestIsland.biome.particles != null)
            {
                if (PlayerManager.instance.particles.name == closestIsland.biome.particles.name)
                {
                    return;
                }
            }
            DestroyImmediate(PlayerManager.instance.particles);
        }
        if (closestIsland.biome.particles != null)
        {
            PlayerManager.instance.particles = dgUtil.Instantiate(closestIsland.biome.particles, Vector3.zero, Quaternion.identity, true, PlayerManager.instance.eyeCamera.transform);
        }
    }

    CreateIsland GetClosestIsland(Vector3 playerPosition)
    {
        float closestIslandDistance = 1000f;
        CreateIsland closestIsland = null;
        foreach (CreateIsland island in islands)
        {
            float islandDistance = Vector3.Distance(new Vector3(island.size, 0, island.size) + island.transform.position, playerPosition);
            if (islandDistance < closestIslandDistance)
            {
                closestIsland = island;
                closestIslandDistance = islandDistance;
            }
            island.gameObject.SetActive(true);
            /* this section would be for unloading islands in the distance
            if (islandDistance < IslandInitDistance)
            {
                if (islandDistance < closestIslandDistance)
                {
                    closestIsland = island;
                    closestIslandDistance = islandDistance;
                }
                island.gameObject.SetActive(true);
            }
            else
            {
                island.gameObject.SetActive(false);
            }
            */
        }
        return closestIsland;
    }

    float timer;
    public void StartTimer(float time)
    {
        levelStartTime = Time.time;
        timer = time;
        InvokeRepeating("UpdateTimer", 0.5f, 1);
    }
    public void UpdateTimer()
    {
        if (timer - (Time.time - levelStartTime) > 0)
        {
            PlayerManager.instance.infoLeftAlign.text = dgUtil.FormatTime(timer - (Time.time - levelStartTime));
        }
        else
        {
            Ship.instance.comet.SetActive(true);
            SetMissionText("head to your ship");
            if (levelType == LevelType.Survive)
			{
				if (Pet.instance.audioSource.clip != Pet.instance.clipSurvivalWaveComplete) {
					Pet.instance.PlayAudio (Pet.instance.clipSurvivalWaveComplete);
				}
				CancelInvoke("SurvivalSpawnEnemies");
				foreach (GameObject go in enemies) {
					if (go != null) {
						Destroy (go);
					}
				}
                GameManager.instance.EnemyChecks = new List<System.Action>();
            }
            else
            {

            }
        }
    }

    string missionText = "";
    public void SetMissionText(string text)
    {
        missionText = text;
    }

    public void CheckEnemies()
    {
        enemies.RemoveAll(item => item == null);
        if (enemies.Count == 0)
        {
            specialComplete++;
            if (specialValue < specialComplete && specialValue > 0)
            {
                Ship.instance.comet.SetActive(true);
				Pet.instance.PlayAudio (Pet.instance.clipSurvivalWaveComplete);
                SetMissionText("head to your ship");
                CancelInvoke("CheckEnemies");
            }
            else
            {
				//spawn wave
				Pet.instance.PlayAudio (Pet.instance.clipsNewWave);
                WaveSpawnEnemies();
            }
        }
    }

    public void SpawnEnemies(int count, bool targetPlayer = false, float range = 0)
    {
        if (range > 1000 || spawnPoints.Count == 0)
        {
            Debug.LogError("Range too large, or no spawn points found");
            return;
        }
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if(spawnPoints[i] == null)
            {
                spawnPoints.RemoveAt(i);
                i--;
            }
        }
        List<SpawnPoint> sps = spawnPoints;
        if (range > 0)
        {
            sps = new List<SpawnPoint>();
            foreach (SpawnPoint sp in spawnPoints)
            {
				float distanceFromPlayer = Vector3.Distance (sp.transform.position, PlayerManager.instance.otherPlayerObject.transform.position);
                if (distanceFromPlayer <= range && distanceFromPlayer > 10)
                {
                    sps.Add(sp);
                }
            }
        }
        if (sps.Count == 0)
        {
            SpawnEnemies(count, targetPlayer, range + 50);
            return;
        }
        else
        {
            float individualCount = count / sps.Count + 1;
            foreach (SpawnPoint sp in sps)
            {
                List<BaseEnemy> bes = sp.Spawn(individualCount);
                foreach (BaseEnemy be in bes)
                {
                    if (targetPlayer)
                    {
                        be.LockOnPlayer();
                    }
                }
            }
        }
    }
}
