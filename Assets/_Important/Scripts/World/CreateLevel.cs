using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateLevel : MonoBehaviour
{
    public static CreateLevel instance;

    public bool skipDataLoad = false;
    public GameObject islandBase;
    public int seed;
    public List<Biome> biomes;
	public Dictionary<Vector2, CreateIsland> islands;
	public List<CreateIsland> activeIslands = new List<CreateIsland>();
    List<PointOfInterest> specialPOIs;
    public List<GameObject> enemies;
	public int centerX, centerZ, tiles;
	public Material waterMat;
	public float islandSize = 50;

	//this section for the ground mesh
	Mesh groundMesh;
	public MeshFilter groundFilter;
	public MeshRenderer groundRenderer;
	public Material groundMat;
	public MeshCollider groundCollider;
	public Camera camera;

    public void Init()
    {
        CreateLevel.instance = this;
        Random.InitState(seed);

		islands = new Dictionary<Vector2, CreateIsland>();
		centerX = -1;
		centerZ = -1;
		tiles = 3;

		groundMesh = new Mesh ();
		SetupAndCenterMap ();
	}

	void Update(){
		waterMat.SetTextureOffset("_MainTex", new Vector2(Mathf.Cos(Time.time / 5), Mathf.Sin(Time.time / 5)) / 5);
	}

	public void CheckForAreaLoad(Vector3 cameraCenter){
		if (Mathf.FloorToInt (cameraCenter.x / islandSize - 0.5f) != centerX 
			|| Mathf.FloorToInt (cameraCenter.z / islandSize) != centerZ) {
			centerX = Mathf.FloorToInt (cameraCenter.x / islandSize - 0.5f);
			centerZ = Mathf.FloorToInt (cameraCenter.z / islandSize);
			SetupAndCenterMap ();
		}
	}

	public void CreateIsland(int i, int j){
		CreateIsland island = dgUtil.Instantiate (islandBase, new Vector3 (i * islandSize, 0, j * islandSize), Quaternion.identity).GetComponent<CreateIsland> ();
		islands.Add (new Vector2 (i, j), island);
		island.size = (int)islandSize;
		island.randomSeed = i * 100 + j;
		island.biome = island.gameObject.AddComponent<Biome> ();
		island.biome.Init (island.size, biomes [Mathf.FloorToInt (Random.value * biomes.Count)], island);
		island.Init (this, specialPOIs, i, j);
		activeIslands.Add (island);
	}

	void SetupAndCenterMap(){
		foreach(CreateIsland active in activeIslands){
			if(!(active.x >= centerX
				&& active.z >= centerZ
				&& active.x <= centerX + tiles - 1
				&& active.z <= centerZ + tiles - 1)){
				active.gameObject.SetActive (false);
			}
		}
		for (int i = centerX; i < centerX + tiles; i++) {
			for (int j = centerZ; j < centerZ + tiles; j++) {
				if (islands.ContainsKey (new Vector2 (i, j))) {
					islands[new Vector2(i, j)].Init (this, specialPOIs, i, j);
					activeIslands.Add (islands [new Vector2 (i, j)]);
				} else {
					CreateIsland (i, j);
				}
			}
		}
		transform.position = new Vector3 (centerX * (int)islandSize, 0, centerZ * (int)islandSize);
		List<Vector3> verts = new List<Vector3> ();
		List<Color> colors = new List<Color> ();
		List<int> tris = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		for (int i = centerX * (int)islandSize; i < centerX * (int)islandSize + tiles * (int)islandSize; i++) {
			for (int j = centerZ * (int)islandSize; j < centerZ * (int)islandSize + tiles * (int)islandSize; j++) {
				float scale = 50;
				float offset = 1000;
				float perlin = Mathf.PerlinNoise ((i + offset) / scale, (j + offset) / scale);
				float perlin2 = Mathf.PerlinNoise ((i + offset) / scale / 2, (j + offset) / scale / 2);
				float perlin3 = Mathf.PerlinNoise ((i + offset) / scale * 25, (j + offset) / scale * 25) / 3;
				if ((i + j * perlin2) > (perlin * 10 + 15)
					&& (i + j * perlin2) < (perlin * 10 + 30 + Mathf.Abs(j / 40))) {
					verts.Add (new Vector3(i - 25 - transform.position.x, -1 + perlin3, j - 25 - transform.position.z));
					colors.Add (new Color (0.9f, 0.9f, 0.7f));
				} else if ((i + j * perlin2) > (perlin * 10 + 13.5f)
					&& (i + j * perlin2) < (perlin * 10 + 31.5f + Mathf.Abs(j / 40))) {
					verts.Add (new Vector3(i - 25 - transform.position.x, 0 + perlin3, j - 25 - transform.position.z));
					colors.Add (new Color (0.9f, 0.9f, 0.7f));
				} else {
					verts.Add (new Vector3(i - 25 - transform.position.x, 0 + perlin3, j - 25 - transform.position.z));
					colors.Add (new Color ((perlin + 0.25f) / 5f, (perlin + 0.25f) / 1f, (perlin + 0.25f) / 5f));
				}
				uvs.Add (new Vector2 (i, j));
				if (j > centerZ * (int)islandSize && i > centerX * (int)islandSize) {
					tris.Add ((verts.Count - 1) - 1);
					tris.Add ((verts.Count - 1) - tiles * (int)islandSize);
					tris.Add ((verts.Count - 1));
					tris.Add ((verts.Count - 1) - 1);
					tris.Add ((verts.Count - 1) - 1 - tiles * (int)islandSize);
					tris.Add ((verts.Count - 1) - tiles * (int)islandSize);
				}
			}
		}
		groundFilter.mesh = groundMesh;
		groundCollider.sharedMesh = groundMesh;
		groundMesh.SetVertices(verts);
		groundMesh.SetTriangles(tris, 0);
		groundMesh.SetColors (colors);
		if (verts.Count == uvs.Count) {
			groundMesh.SetUVs (0, uvs);
		}
		groundRenderer.sharedMaterial = groundMat;
		groundMesh.RecalculateNormals();
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

	public void Load(){
		foreach (KeyValuePair<Vector2Int, Dictionary<Vector2Int, string>> kvp in GameManager.saveData) {
			if (!islands.ContainsKey (kvp.Key)) {
				CreateIsland (kvp.Key.x, kvp.Key.y);
			}
			foreach (KeyValuePair<Vector2Int, string> kvp2 in kvp.Value) {
				islands [kvp.Key].nodes [kvp2.Key.x, kvp2.Key.y].Load (kvp2.Value);
			}
			islands [kvp.Key].RedoMesh ();
		}
	}
}
