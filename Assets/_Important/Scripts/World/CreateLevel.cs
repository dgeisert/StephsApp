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
	public Dictionary<Vector2, CreateIsland> islands;
    List<PointOfInterest> specialPOIs;
    public List<GameObject> enemies;
    public int specialValue, specialComplete = 0;
    public bool levelFailed = false;
    float levelStartTime;
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
	public int centerX, centerZ, tiles;
	public float islandSize = 50;

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

        CreateLevel.instance = this;
        Random.InitState(seed);
		levelStartTime = Time.time;

		islands = new Dictionary<Vector2, CreateIsland>();
		centerX = -1;
		centerZ = -1;
		tiles = 3;
		SetupAndCenterMap ();
    }

	void SetupAndCenterMap(){

		for (int i = centerX; i < centerX + tiles; i++) {
			int j = centerZ - 1;
			if (islands.ContainsKey (new Vector2 (i, j))) {
				islands [new Vector2 (i, j)].gameObject.SetActive (false);
			}
		}
		for (int i = centerX; i < centerX + tiles; i++) {
			int j = centerZ + tiles;
			if (islands.ContainsKey (new Vector2 (i, j))) {
				islands [new Vector2 (i, j)].gameObject.SetActive (false);
			}
		}
		for (int j = centerZ; j < centerZ + tiles; j++) {
			int i = centerX - 1;
			if (islands.ContainsKey (new Vector2 (i, j))) {
				islands [new Vector2 (i, j)].gameObject.SetActive (false);
			}
		}
		for (int j = centerZ; j < centerZ + tiles; j++) {
			int i = centerX + tiles;
			if (islands.ContainsKey (new Vector2 (i, j))) {
				islands [new Vector2 (i, j)].gameObject.SetActive (false);
			}
		}
		for (int i = centerX; i < centerX + tiles; i++) {
			for (int j = centerZ; j < centerZ + tiles; j++) {
				if (islands.ContainsKey (new Vector2 (i, j))) {
					islands[new Vector2(i, j)].Init (this, specialPOIs);
				} else {
					CreateIsland island = dgUtil.Instantiate (islandBase, new Vector3 (i * islandSize, 0, j * islandSize), Quaternion.identity).GetComponent<CreateIsland> ();
					islands.Add (new Vector2 (i, j), island);
					island.size = (int)islandSize;
					island.randomSeed = i * 100 + j;
					island.biome = island.gameObject.AddComponent<Biome> ();
					island.biome.Init (island.size, biomes [Mathf.FloorToInt (Random.value * biomes.Count)], island);
					island.Init (this, specialPOIs);
				}
			}
		}
	}

    public void Update()
    {
		timeInLevel = Time.time - levelStartTime;
		if(Input.GetKeyDown(KeyCode.W)){
			centerZ++;
			SetupAndCenterMap ();
		}
		if(Input.GetKeyDown(KeyCode.S)){
			centerZ--;
			SetupAndCenterMap ();
		}
		if(Input.GetKeyDown(KeyCode.A)){
			centerX--;
			SetupAndCenterMap ();
		}
		if(Input.GetKeyDown(KeyCode.D)){
			centerX++;
			SetupAndCenterMap ();
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
