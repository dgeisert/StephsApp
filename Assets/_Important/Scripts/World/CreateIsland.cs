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
    }

    public Node[,] nodes;
	public List<Vector3> vertsSplats, vertsTop;
	public List<Vector2> uvsSplats, uvsTop;
    int vert = 0;
	public List<Color> colorListSplats, colorListTop;
    List<Tri> trisTop;
	public List<int> trianglesSplats, trianglesTop;
    MeshRenderer rendererTop, rendererSplats;
    MeshFilter filterTop, filterSplats;
    public GameObject top, splatsObject;
    public int size = 10, randomSeed = 0;
	public Biome biome;
    public Material mat;
    List<PointOfInterest> pois, specialPOIs;

	public int index;

	//these used to confirm connections across map
	public CreateIsland fromIsland, closestIsland;
	public bool closestSteps = false, fromSteps = false;
	public GameObject stepIsland, lantern;

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
		top.tag = "Teleportable";
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
        nodes[i, j].x = i;
        nodes[i, j].z = j;
		nodes[i, j].vert = vert;
        float topY = 0;
		vertsTop.Add(new Vector3(i - size / 2, topY, j - size / 2));
		uvsTop.Add (new Vector2(i * 2, j * 2));
		colorListTop.Add (biome.colorTop);
		bool spawned = false;
		float scale = 15;
		float offset = 12000;
		float perlin = Mathf.PerlinNoise ((i + transform.position.x + offset) / scale, (j + transform.position.z + offset) / scale);
		if (Random.value * 100 < biome.percentSplat && perlin > 0.85f)
		{
			biome.splats[Mathf.FloorToInt(Random.value * biome.splats.Count)].Init(
				this
				, vertsTop[vert]
				, ((biome.splatMaxSize - biome.splatMinSize) * Random.value + biome.splatMinSize) * (perlin - 0.8f) / 0.15f);
		}
		if (!spawned)
		{
			scale = 20;
			offset = 10000;
			perlin = Mathf.PerlinNoise ((i + transform.position.x + offset) / scale, (j + transform.position.z + offset) / scale);
			if (Random.value * 100 < biome.percentTree && perlin > 0.6f)
			{
				biome.trees[Mathf.FloorToInt(Random.value * biome.trees.Count)].Init(
					this
					, vertsTop[vert]
					, ((biome.treeMaxSize - biome.treeMinSize) * Random.value + biome.treeMinSize) * (perlin - 0.4f)
					, true);
				spawned = true;
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
		vertsTop = new List<Vector3>();
		colorListTop = new List<Color> ();


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
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.mesh = assignMesh;
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
}
