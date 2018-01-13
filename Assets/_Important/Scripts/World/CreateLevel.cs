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
    public float timeInLevel;
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

	public CreateIsland.Node GetNode(Vector3 point){
		return islands[new Vector2(Mathf.RoundToInt(point.x/islandSize)
			, Mathf.RoundToInt(point.z/islandSize))
		].GetNode (point);
	}
	public CreateIsland.Node GetNode(Vector2 point){
		return islands[new Vector2(Mathf.RoundToInt(point.x/islandSize)
			, Mathf.RoundToInt(point.y/islandSize))
		].GetNode (point);
	}

	public List<CreateIsland.Node> highlightedNodes = new List<CreateIsland.Node> ();
	public void ResetHighlights(){
		foreach (CreateIsland.Node node in highlightedNodes) {
			node.RemoveHighlight ();
		}
		highlightedNodes = new List<CreateIsland.Node> ();
	}

	public List<CreateIsland.Node> GetNodes(Vector3 point, float radius, bool square = false){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		for (float i = -radius; i < radius; i++) {
			for (float j = -radius; j < radius; j++) {
				if (square || new Vector2 (i, j).magnitude < radius) {
					Vector3 point2 = new Vector3 (point.x + i, point.y, point.z + j);
					nodes.Add(islands[new Vector2(Mathf.RoundToInt(point2.x/islandSize)
						, Mathf.RoundToInt(point2.z/islandSize))
					].GetNode (point2));
				}
			}
		}
		return nodes;
	}

	public CreateIsland GetIsland(Vector3 point){
		return islands [new Vector2 (Mathf.RoundToInt (point.x / islandSize)
			, Mathf.RoundToInt (point.z / islandSize))
		];
	}
}
