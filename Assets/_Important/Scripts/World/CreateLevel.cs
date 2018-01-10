using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateLevel : MonoBehaviour
{
    public static CreateLevel instance;

	public GameObject gameManager;
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

	public void CheckForAreaLoad(Vector3 cameraCenter){
		if (Mathf.FloorToInt (cameraCenter.x / islandSize - 0.5f) != centerX 
			|| Mathf.FloorToInt (cameraCenter.z / islandSize) != centerZ) {
			centerX = Mathf.FloorToInt (cameraCenter.x / islandSize - 0.5f);
			centerZ = Mathf.FloorToInt (cameraCenter.z / islandSize);
			SetupAndCenterMap ();
		}
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


    float timer;
    public void StartTimer(float time)
    {
        levelStartTime = Time.time;
        timer = time;
        InvokeRepeating("UpdateTimer", 0.5f, 1);
    }
    public void UpdateTimer()
    {
    }

    string missionText = "";
    public void SetMissionText(string text)
    {
        missionText = text;
    }
}
