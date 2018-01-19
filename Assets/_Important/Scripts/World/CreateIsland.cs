using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateIsland : MonoBehaviour
{

    public class Tri
    {
        public int a, b, c;
        public Tri(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        public bool Contains(int i)
        {
            return a == i || b == i || c == i;
        }
    }

    public class Node
    {
        public float y;
        public int x, z;
		public Vector3 GetPoint(){
			return new Vector3 (x - CreateLevel.instance.islandSize / 2, y, z - CreateLevel.instance.islandSize / 2);
		}
		public Node reference;
		public bool occupied = false, claimed = false;
		public GameObject item;
		public List<Building> claimants;
		public int splatIndexStart, splatIndexEnd;
		public CreateIsland island;
		public List<Color> colorHold;
		public Dictionary<MeshRenderer, Material> matsHold;
		public Resource resource;
		public int resourceCount = 0;
		public void Load(string str){
			if (resource == Resource.Tree) {
				ChopTree ();
			}
			resource = (Resource)System.Enum.Parse(typeof(Resource), str.Split ('.') [1]);
			foreach (GameObject go in ResourceManager.instance.buildings) {
				if (go.name == str.Split ('.') [0]) {
					Building b = dgUtil.Instantiate (go, GetPoint (), Quaternion.identity, true, island.transform).GetComponent<Building>();
					b.Load (this, str.Split ('.') [3], int.Parse (str.Split ('.') [2]));
				}
			}
		}
		public string Save(){
			string toSave = "";
			toSave += item == null ? "" : item.name;
			toSave += ".";
			toSave += resource == null ? "" : resource.ToString();
			toSave += ".";
			toSave += resourceCount.ToString();
			toSave += ".";
			if (item != null) {
				string str = "";
				Building b = item.GetComponent<Building> ();
				if (b != null) {
					foreach (Node n in b.nodes) {
						str += n.island.x + "-" + n.island.z + "-" + n.x + "-" + n.z + "-";
					}
				}
				toSave += str;
			}
			toSave += ".";
			GameManager.AddSaveData (new Vector2Int (island.x, island.z), new Vector2Int (x, z), toSave);
			return toSave;
		}
		public void ChopTree(){
			resource = Resource.Open;
			item = null;
			for (int i = splatIndexStart; i <= splatIndexEnd; i++) {
				island.vertsSplats [i] = island.vertsSplats [i] - Vector3.up * 100;
			}
			occupied = false;
		}
		public void Highlight(){
			if (splatIndexEnd > 0) {
				if (colorHold == null ? true : colorHold.Count == 0) {
					colorHold = new List<Color> ();
					for (int i = splatIndexStart; i <= splatIndexEnd; i++) {
						colorHold.Add (island.colorListSplats [i]);
						island.colorListSplats [i] = Color.yellow;
					}
					island.filterSplats.mesh.SetColors (island.colorListSplats);
					CreateLevel.instance.highlightedNodes.Add (this);
					if (TouchManager.instance.mode == TouchManager.Mode.Move) {
						switch (resource) {
						case Resource.Tree:
							MenuManager.instance.SetOption (1, null);
							break;
						default:
							break;
						}
					}
				}
			} else {
				matsHold = new Dictionary<MeshRenderer, Material>();
				foreach (MeshRenderer renderer in item.GetComponentsInChildren<MeshRenderer>()) {
					matsHold.Add (renderer, renderer.material);
					renderer.material = island.highlightMaterial;
				}
				CreateLevel.instance.highlightedNodes.Add (this);
			}
		}
		public void RemoveHighlight(){
			if (splatIndexEnd > 0) {
				if (colorHold != null ? colorHold.Count > 0 : false) {
					for (int i = splatIndexStart; i <= splatIndexEnd; i++) {
						island.colorListSplats [i] = colorHold [i - splatIndexStart];
					}
					colorHold.Clear ();
				}
			} else {
				if (matsHold != null ? matsHold.Count > 0 : false) {
					foreach (KeyValuePair<MeshRenderer, Material> kvp in matsHold) {
						kvp.Key.material = kvp.Value;
					}
					matsHold.Clear ();
				}
			}
			MenuManager.instance.ResetOptions ();
			island.filterSplats.mesh.SetColors (island.colorListSplats);
		}
    }

    public Node[,] nodes;
	public List<Vector3> vertsSplats;
	public List<Vector2> uvsSplats;
    int vert = 0;
	public List<Color> colorListSplats;
	public List<int> trianglesSplats;
    MeshRenderer rendererSplats;
    MeshFilter filterSplats;
    public GameObject splatsObject, waterObject, waterPrefab, newLandFog, newLandFogInstance;
    public int size = 10, randomSeed = 0;
	public Biome biome;
	public Material mat, highlightMaterial;
    List<PointOfInterest> pois, specialPOIs;
	public int x, z, exploreCost;

	public int index;

	//these used to confirm connections across map

	CreateLevel level;

	public bool initialized = false;

	public void Init(CreateLevel levelSet, List<PointOfInterest> setSpecialPOIs, int setX, int setZ)
	{
		if (initialized) {
			gameObject.SetActive (true);
			return;
		}
		x = setX;
		z = setZ;
		level = levelSet;
		specialPOIs = setSpecialPOIs == null ? new List<PointOfInterest>() : setSpecialPOIs;
		MeshSetup();
		if (GameManager.saveData.ContainsKey (new Vector2Int (x, z))) {
			AssignMesh (splatsObject, vertsSplats, colorListSplats, trianglesSplats, uvsSplats);
		} else {
			newLandFogInstance = dgUtil.Instantiate (newLandFog, Vector3.zero, Quaternion.identity, true, transform);
			newLandFogInstance.GetComponent<Fog> ().island = this;
			newLandFogInstance.GetComponent<Fog> ().landPosition = new Vector2Int (x, z);
		}
        initialized = true;
	}

	public void MakeVisisble(){
		AssignMesh (splatsObject, vertsSplats, colorListSplats, trianglesSplats, uvsSplats);
		if (newLandFogInstance != null) {
			Destroy (newLandFogInstance);
		}
	}

    void BuildNodeMap()
    {
        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                ExpandNodeMap(i, j);
            }
        }
    }

    bool ExpandNodeMap(int i, int j)
    {
		nodes[i, j] = new Node();
		nodes[i, j].island = this;
        nodes[i, j].x = i;
        nodes[i, j].z = j;
		bool spawned = false;
		float scale = 50;
		float offset = 1000;
		float perlin = Mathf.PerlinNoise ((i + transform.position.x + offset) / scale, (j + transform.position.z + offset) / scale);
		float perlin2 = Mathf.PerlinNoise ((i + transform.position.x + offset) / scale / 2, (j + transform.position.z + offset) / scale / 2);

		if (!spawned) {
			if((transform.position.x + i + (transform.position.z + j) * perlin2) > (perlin * 10 + 14)
				&& (transform.position.x + i + (transform.position.z + j) * perlin2) < (perlin * 10 + 31 + Mathf.Abs((transform.position.z + j) / 40))){
				if (waterObject == null) {
					waterObject = dgUtil.Instantiate (waterPrefab, new Vector3(0, -0.5f, 0), Quaternion.identity, true, transform);
				}
				nodes [i, j].occupied = true;
				spawned = true;
			}
		}
		if (!spawned) {
			scale = 15;
			offset = 12000;
			perlin = Mathf.PerlinNoise ((i + transform.position.x + offset) / scale, (j + transform.position.z + offset) / scale);
			if (Random.value * 100 < biome.percentSplat && perlin > 0.85f) {
				nodes [i, j].splatIndexStart = colorListSplats.Count;
				biome.splats [Mathf.FloorToInt (Random.value * biome.splats.Count)].Init (
					this
				, new Vector3 (i - size / 2, 0, j - size / 2)
				, ((biome.splatMaxSize - biome.splatMinSize) * Random.value + biome.splatMinSize) * (perlin - 0.8f) / 0.15f);
				nodes [i, j].occupied = true;
				nodes [i, j].item = splatsObject;
				nodes [i, j].splatIndexEnd = colorListSplats.Count - 1;
				nodes [i, j].resource = Resource.Boulder;
				nodes [i, j].resourceCount = -1;
				spawned = true;
			}
		}
		if (!spawned)
		{
			scale = 20;
			offset = 10000;
			perlin = Mathf.PerlinNoise ((i + transform.position.x + offset) / scale, (j + transform.position.z + offset) / scale);
			if (Random.value * 100 < biome.percentTree && perlin > 0.5f)
			{
				nodes [i, j].splatIndexStart = colorListSplats.Count;
				biome.trees[Mathf.FloorToInt(Random.value * biome.trees.Count)].Init(
					this
					, new Vector3(i - size / 2, 0, j - size / 2)
					, ((biome.treeMaxSize - biome.treeMinSize) * Random.value + biome.treeMinSize) * (perlin - 0.35f)
					, true);
				spawned = true;
				nodes [i, j].occupied = true;
				nodes [i, j].item = splatsObject;
				nodes [i, j].splatIndexEnd = colorListSplats.Count - 1;
				nodes [i, j].resource = Resource.Tree;
				nodes [i, j].resourceCount = -1;
			}
			spawned = true;
		}
        vert++;
        return true;
    }

    void MeshSetup()
	{
		trianglesSplats = new List<int>();
		colorListSplats = new List<Color>();
		vertsSplats = new List<Vector3>();
		nodes = new Node[size, size];
		vert = 0;


        Random.InitState(randomSeed);

        //determine shape
		BuildNodeMap();
		bool spawned = false;
    }

    float NeighborY(int i, int j)
    {
        if (i <= 0 || j <= 0 || i >= size || j >= size)
        {
            return 0;
        }
        else if (nodes[i, j] != null)
        {
            return nodes[i, j].y;
        }
        return 0;
    }

    List<Tri> checkedTris;
    List<Node> checkedNodes;
    int poiCount;

	Mesh assignMesh;
	void AssignMesh(GameObject go, List<Vector3> verts, List<Color> colors, List<int> tris, List<Vector2> uvs)
    {
        assignMesh = new Mesh();
        filterSplats = go.AddComponent<MeshFilter>();
		filterSplats.mesh = assignMesh;
        MeshRenderer render = go.AddComponent<MeshRenderer>();
		assignMesh.SetVertices(verts);
		assignMesh.SetTriangles(tris, 0);
		assignMesh.SetColors (colors);
		if (verts.Count == uvs.Count) {
			assignMesh.SetUVs (0, uvs);
		}
		render.sharedMaterial = mat;
        assignMesh.RecalculateNormals();
        //go.AddComponent<MeshCollider>();
	}
	public void RedoMesh(){
		assignMesh.SetVertices(vertsSplats);
		assignMesh.SetTriangles(trianglesSplats, 0);
		assignMesh.RecalculateNormals();
		filterSplats.mesh = assignMesh;
	}

	public Node GetNode(Vector3 point){
		point -= transform.position;
		point += Vector3.one * size / 2;
		//Debug.Log (Mathf.RoundToInt (point.x) + ", " + Mathf.RoundToInt (point.z));
		return nodes [Mathf.FloorToInt (point.x)
			, Mathf.FloorToInt (point.z)];
	}

	public Node GetNode(Vector2 point){
		point -= new Vector2(transform.position.x, transform.position.z);
		point += Vector2.one * size / 2;
		//Debug.Log (Mathf.RoundToInt (point.x) + ", " + Mathf.RoundToInt (point.z));
		return nodes [Mathf.FloorToInt (point.x)
			, Mathf.FloorToInt (point.y)];
	}
}
