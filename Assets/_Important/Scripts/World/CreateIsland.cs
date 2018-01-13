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
        public int vert = -1;
        public PointOfInterest poi;
        public int x, z;
		public Vector3 GetPoint(){
			return new Vector3 (x - CreateLevel.instance.islandSize / 2, y, z - CreateLevel.instance.islandSize / 2);
		}
		public Node reference;
		public bool occupied = false, claimed = false;
		public GameObject item;
		public int splatIndexStart, splatIndexEnd;
		public CreateIsland island;
		public List<Color> colorHold;
		public Dictionary<MeshRenderer, Material> matsHold;
		public Resource resource;
		public int resourceCount = 0;
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
				for (int i = splatIndexStart; i <= splatIndexEnd; i++) {
					island.colorListSplats [i] = colorHold[i - splatIndexStart];
				}
				colorHold.Clear ();
			} else {
				foreach (KeyValuePair<MeshRenderer, Material> kvp in matsHold) {
					kvp.Key.material = kvp.Value;
				}
				matsHold.Clear ();
			}
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
    public GameObject splatsObject;
    public int size = 10, randomSeed = 0;
	public Biome biome;
	public Material mat, highlightMaterial;
    List<PointOfInterest> pois, specialPOIs;

	public int index;

	//these used to confirm connections across map

	CreateLevel level;

	public bool initialized = false;

    private void Start()
    {
		if(GameManager.GetScene() != "islandgen")
		{
			gameObject.AddComponent<Biome> ().Init (size, biome, this);
			biome = gameObject.GetComponent<Biome> ();
            Init(null, null);
        }
    }

	public void Init(CreateLevel levelSet, List<PointOfInterest> setSpecialPOIs)
	{
		if (initialized) {
			gameObject.SetActive (true);
			return;
		}
		level = levelSet;
		specialPOIs = setSpecialPOIs == null ? new List<PointOfInterest>() : setSpecialPOIs;
		MeshSetup();
		AssignMesh(splatsObject, vertsSplats, colorListSplats, trianglesSplats, uvsSplats);
        initialized = true;
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
		nodes[i, j].vert = vert;
		bool spawned = false;
		float scale = 15;
		float offset = 12000;
		float perlin = Mathf.PerlinNoise ((i + transform.position.x + offset) / scale, (j + transform.position.z + offset) / scale);
		if (Random.value * 100 < biome.percentSplat && perlin > 0.85f)
		{
			nodes [i, j].splatIndexStart = colorListSplats.Count;
			biome.splats[Mathf.FloorToInt(Random.value * biome.splats.Count)].Init(
				this
				, new Vector3(i - size / 2, 0, j - size / 2)
				, ((biome.splatMaxSize - biome.splatMinSize) * Random.value + biome.splatMinSize) * (perlin - 0.8f) / 0.15f);
			nodes [i, j].occupied = true;
			nodes [i, j].item = splatsObject;
			nodes [i, j].splatIndexEnd = colorListSplats.Count - 1;
			nodes [i, j].resource = Resource.Boulder;
			nodes [i, j].resourceCount = -1;
		}
		if (!spawned)
		{
			scale = 20;
			offset = 10000;
			perlin = Mathf.PerlinNoise ((i + transform.position.x + offset) / scale, (j + transform.position.z + offset) / scale);
			if (Random.value * 100 < biome.percentTree && perlin > 0.6f)
			{
				nodes [i, j].splatIndexStart = colorListSplats.Count;
				biome.trees[Mathf.FloorToInt(Random.value * biome.trees.Count)].Init(
					this
					, new Vector3(i - size / 2, 0, j - size / 2)
					, ((biome.treeMaxSize - biome.treeMinSize) * Random.value + biome.treeMinSize) * (perlin - 0.4f)
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

	void AssignMesh(GameObject go, List<Vector3> verts, List<Color> colors, List<int> tris, List<Vector2> uvs)
    {
        Mesh assignMesh = new Mesh();
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
