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
        public bool edge = false;
        public bool nearEdge = false;
    }

    public Node[,] nodes;
	public List<Vector3> vertsSplats, vertsBottom, vertsTop;
	public List<Vector2> uvsSplats, uvsBottom, uvsTop;
    int vert = 0;
	public List<Color> colorListSplats, colorListTop, colorListBottom;
    List<Tri> trisBottom, trisTop;
	public List<int> trianglesSplats, trianglesBottom, trianglesTop;
    MeshRenderer rendererBottom, rendererTop, rendererSplats;
    MeshFilter filterBottom, filterTop, filterSplats;
    public GameObject top, splatsObject;
    public int size = 10, randomSeed = 0;
	public Biome biome;
    public Material mat;
    List<PointOfInterest> pois, specialPOIs;
    SpawnPoint[] spawnPoints;

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
            Init(null, null, 0);
        }
    }

	public void Init(CreateLevel levelSet, List<PointOfInterest> setSpecialPOIs, float enemyCount = 0)
	{
		level = levelSet;
		specialPOIs = setSpecialPOIs == null ? new List<PointOfInterest>() : setSpecialPOIs;
		MeshSetup();
		TranslateTris();
		AddSteps ();
		AssignMesh(gameObject, vertsBottom, colorListBottom, trianglesBottom, uvsBottom);
		AssignMesh(top, vertsTop, colorListTop, trianglesTop, uvsTop);
		AssignMesh(splatsObject, vertsSplats, colorListSplats, trianglesSplats, uvsSplats);
		top.tag = "Teleportable";
        if (levelSet != null)
        {
            if (levelSet.levelType != LevelType.Wave 
				&& levelSet.levelType != LevelType.Survive 
				&& levelSet.levelType != LevelType.InfiniteWave)
            {
                InitialEnemySpawn(enemyCount);
            }
        }
        initialized = true;
	}

	public void InitialEnemySpawn(float enemyCount)
	{
		if (enemyCount <= 0) {
			return;
		}
		spawnPoints = GetComponentsInChildren<SpawnPoint>();
		float spawnEnemyCount = enemyCount / spawnPoints.Length + 1;
		foreach (SpawnPoint sp in spawnPoints)
		{
			sp.Spawn(spawnEnemyCount);
		}
	}

	public void AddSteps(){
		if (closestIsland != null) {
			if (!(closestIsland.closestIsland == this && closestIsland.closestSteps)) {
				AddSteps (closestIsland);
				closestSteps = true;
			}
		}
		if (closestIsland != fromIsland) {
			if (fromIsland != null) {
				if (fromIsland.closestIsland != null) {
					if (fromIsland.closestIsland != this || !fromIsland.closestSteps) {
						AddSteps (fromIsland);
						fromSteps = true;
					}
				} else {
					AddSteps (fromIsland);
					fromSteps = true;
				}
			}
		}
	}

	void AddSteps(CreateIsland otherIsland){
		Vector3 direction = (otherIsland.transform.position - transform.position).normalized;
		direction = new Vector3 (direction.x, 0, direction.z).normalized;
		float distance = Vector3.Distance (otherIsland.transform.position, transform.position) - otherIsland.size / 2 - size / 2;
		int steps = Mathf.FloorToInt (distance / 10);
		BaseEnemy be = null;
            if (steps > 3 && CreateLevel.instance != null && CreateLevel.instance.levelType != LevelType.Wave)
        {
            be = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(dgUtil.RandomEnemy()), Vector3.zero, Quaternion.identity).GetComponent<BaseEnemy>();
			CreateLevel.instance.enemies.Add (be.gameObject);
			CreateLevel.instance.totalEnemies++;
		}
		for (int i = 0; i <= steps; i++) {
			Vector3 stepPosition = direction * size / 2
				+ new Vector3 (0, (float)(i + 1) / (float)(steps + 1) * (otherIsland.transform.position.y - transform.position.y), 0)
				+ (distance / (float)(steps + 1)) * direction * (i)
				+ Vector3.up * Random.value / 5;
			Color stepColorTop = biome.colorTop;
			Color stepColorBottom = biome.colorBottom;
			if (i == 0 || i == steps) {
				stepColorTop = new Color (stepColorTop.r * 2f, stepColorTop.g * 2f, stepColorTop.b * 3f, 1);
				stepColorBottom = new Color (stepColorBottom.r * 1.5f, stepColorBottom.g * 1.5f, stepColorBottom.b * 2f, 1);
				dgUtil.Instantiate (lantern, stepPosition, Quaternion.identity, true, top.transform);
				if (i == 0 && be != null) {
					be.transform.position = transform.position + stepPosition + Vector3.up * 2;
					be.patrolPath.Add(transform.position + stepPosition + Vector3.up);
				}
				if (i == steps && be != null) {
					be.patrolPath.Add(transform.position + stepPosition + Vector3.up);
				}
			}
			//determine shape
			int startVert = vertsTop.Count;
			float setSize = 2;
			Vector3 vert = Vector3.zero;
			vertsBottom.Add (stepPosition + vert + Vector3.down * 2);
			uvsBottom.Add (new Vector2(vert.x * 2, vert.z * 2));
			colorListBottom.Add (stepColorBottom);
			vertsTop.Add (stepPosition + vert);
			uvsTop.Add (new Vector2(vert.x * 2, vert.z * 2));
			colorListTop.Add (stepColorTop);

			for (int j = 0; j < 8; j++) {
				vert = new Vector3(Mathf.Cos(j * 0.8f), 0, Mathf.Sin(j * 0.8f)) * (setSize * (1 + (Random.value - 0.5f)));
				vertsBottom.Add (stepPosition + vert);
				uvsBottom.Add (new Vector2(vert.x * 2, vert.z * 2));
				colorListBottom.Add (stepColorBottom);
				vertsTop.Add (stepPosition + vert);
				uvsTop.Add (new Vector2(vert.x / 10, vert.z / 10));
				colorListTop.Add (stepColorTop);
				if(j != 0){
					trianglesBottom.Add (vertsTop.Count - 1);
					trianglesBottom.Add (startVert);
					trianglesBottom.Add (vertsTop.Count - 2);
					trianglesTop.Add (vertsTop.Count - 2);
					trianglesTop.Add (startVert);
					trianglesTop.Add (vertsTop.Count - 1);
				}
			}
			trianglesBottom.Add (startVert + 1);
			trianglesBottom.Add (startVert + 0);
			trianglesBottom.Add (startVert + 8);
			trianglesTop.Add (startVert + 8);
			trianglesTop.Add (startVert + 0);
			trianglesTop.Add (startVert + 1);
		}
	}

	float CalcY(int x, int z)
	{
		return -(Mathf.PerlinNoise(x * biome.perlinCrunch - biome.perlinOffset, z * biome.perlinCrunch - biome.perlinOffset)
			- biome.spaciness
			- (Vector2.Distance(new Vector2(x, z), new Vector2(size / 2, size / 2)) / size - 0.5f) * biome.centerBias)
			* biome.heightMod;
	}

	float CalcY2(int x, int z)
	{
		return Mathf.PerlinNoise(x * biome.perlinCrunch - biome.perlinOffset, z * biome.perlinCrunch - biome.perlinOffset) - 0.5f;
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
        float y = CalcY(i, j);
        if (y > 0)
        {
            return false;
        }
        nodes[i, j] = new Node();
        nodes[i, j].x = i;
        nodes[i, j].z = j;
        if (i == 0 || j == 0 || i == size - 1 || j == size - 1)
        {
            nodes[i, j].y = 0;
            nodes[i, j].edge = true;
        }
        else
        {
            nodes[i, j].y = y;
        }
        nodes[i, j].vert = vert;
        if(nodes[i, j].y < 0)
        {
            foreach (PointOfInterest poi in pois)
            {
                if (poi.Contains(i, j))
                {
                    nodes[i, j].poi = poi;
                    break;
                }
            }
        }
        float randX = (Random.value - 0.5f) * biome.wiggle;
        float randZ = (Random.value - 0.5f) * biome.wiggle;
        float topY = 0;
        if (nodes[i, j].poi != null)
        {
            topY = -nodes[i, j].poi.depression;
        }
		vertsTop.Add(new Vector3(nodes[i, j].x + randX - size / 2, topY + CalcY2(nodes[i, j].x, nodes[i, j].z), nodes[i, j].z + randZ - size / 2));
		uvsTop.Add (new Vector2((float)nodes[i, j].x * 2, (float)nodes[i, j].z * 2));
		colorListTop.Add (biome.colorTop);
		vertsBottom.Add(new Vector3(nodes[i, j].x + randX - size / 2, nodes[i, j].y, nodes[i, j].z + randZ - size / 2));
		uvsBottom.Add (new Vector2(nodes[i, j].x, nodes[i, j].z));
		colorListBottom.Add (biome.colorBottom);
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
		vertsBottom = new List<Vector3>();
		vertsTop = new List<Vector3>();
		colorListTop = new List<Color> ();
		colorListBottom = new List<Color> ();


        Random.InitState(randomSeed);

        //set up POIs
        List<int[]> poiCandidates = new List<int[]>();
        for (int i = 1; i <= Mathf.Floor(size / biome.poiSpacing) - 1; i++)
        {
            for (int j = 1; j <= Mathf.Floor(size / biome.poiSpacing) - 1; j++)
            {
                int x = Mathf.FloorToInt(biome.poiSpacing * i + biome.poiSpacing * (Random.value * 0.4f - 0.2f));
                int z = Mathf.FloorToInt(biome.poiSpacing * j + biome.poiSpacing * (Random.value * 0.4f - 0.2f));
				if (CalcY (x, z) < -1f) {
					poiCandidates.Add(new int[] { x, z });
				}
            }
        }
        dgUtil.Shuffle<int[]>(poiCandidates);
        pois = new List<PointOfInterest>();
		foreach (PointOfInterest poi in biome.importantPois) {
			specialPOIs.Add (poi);
		}
        poiCount = 0;
		if (poiCandidates.Count < specialPOIs.Count) {
			for (int i = 0; i < specialPOIs.Count; i++) {
				poiCandidates.Add(new int[] { Mathf.FloorToInt(size/2) + i, Mathf.FloorToInt(size/2) + i});
			}
		}
		for (int i = 0; i < poiCandidates.Count; i++) {
			int[] candidate = poiCandidates [i];
			PointOfInterest assignPoi = null;
			if (i < specialPOIs.Count) {
				assignPoi = specialPOIs[i];
			} else {
				assignPoi = biome.RollPOI ();
			}
			if (assignPoi != null) {
				assignPoi.x = candidate [0];
				assignPoi.z = candidate [1];
				pois.Add (assignPoi);
				poiCount++;
			}
		}

        //determine shape
        BuildNodeMap();

		//set poi objects
		foreach (PointOfInterest poi in pois) {
			poi.SetObject (this);
		}

        //set tris
        trisBottom = new List<Tri>();
        trisTop = new List<Tri>();
        foreach (Node node in nodes)
        {
            if (node != null)
            {
                float sumY = 0;
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        sumY += NeighborY(node.x + i, node.z + j);
                        if (node.x + i >= 0 && node.z + j >= 0 && node.x + i < size && node.z + j < size)
                        {
                            if (nodes[node.x + i, node.z + j] != null)
                            {
                                node.nearEdge = nodes[node.x + i, node.z + j].edge || node.nearEdge;
                            }
                            else
                            {
                                node.edge = true;
                            }
                        }
                    }
                }
                if (node.edge)
                {
					vertsBottom[node.vert] -= new Vector3(0f, vertsBottom[node.vert].y - CalcY2(node.x, node.z), 0f);
                }
                if (node.edge || node.nearEdge)
				{
                    node.poi = null;
					vertsTop[node.vert] -= new Vector3(0f, vertsTop[node.vert].y - CalcY2(node.x, node.z), 0f);
                }
                if (sumY < 0)
                {
                    if (node.x > 0 && node.z > 0)
                    {
                        PointOfInterest poi = null;
                        Node a = nodes[node.x - 1, node.z - 1];
                        Node b = nodes[node.x, node.z - 1];
                        Node c = nodes[node.x - 1, node.z];
                        if (a != null)
                        {
							poi = a.poi;
							if (poi != null) {
								if (poi.color != Color.clear) {
									colorListTop [node.vert] = poi.color;
								}
							}
                            if (b != null)
                            {
                                Tri topTriB = new Tri(node.vert, b.vert, a.vert);
                                if (poi == null)
                                {
                                    poi = b.poi;
                                }
                                if (poi != null)
								{
									poi.tris.Add (topTriB);
								}
								trisTop.Add(topTriB);
                                trisBottom.Add(new Tri(a.vert, b.vert, node.vert));
                            }
                            if (c != null)
							{
								Tri topTriC = new Tri(node.vert, a.vert, c.vert);
                                if (poi == null)
                                {
                                    poi = c.poi;
                                }
                                if (poi != null)
								{
									poi.tris.Add (topTriC);
								}
								trisTop.Add(topTriC);
                                trisBottom.Add(new Tri(c.vert, a.vert, node.vert));
                            }
                        }
                    }
					bool spawned = false;
                    if (AllowTree(node))
					{
						biome.trees[Mathf.FloorToInt(Random.value * biome.trees.Count)].Init(
							Mathf.FloorToInt(10000 * Random.value)
							, this
							, vertsTop[node.vert]
							, (biome.treeMaxSize - biome.treeMinSize) * Random.value + biome.treeMinSize
							, true);
						spawned = true;
                    }
					if (!spawned)
					{
						if (AllowSplat(node))
						{
                            biome.splats[Mathf.FloorToInt(Random.value * biome.splats.Count)].Init(
                                Mathf.FloorToInt(10000 * Random.value)
                                , this
                                , vertsTop[node.vert]
                                , (biome.splatMaxSize - biome.splatMinSize) * Random.value + biome.splatMinSize);
                        }
						spawned = true;
					}
                }
                else
                {
                    nodes[node.x, node.z] = null;
                }
            }
        }
    }

    bool AllowTree(Node node)
    {
		if (node.y > -biome.heightMod / 5 || node.edge || node.nearEdge)
        {
            return false;
        }
        if (Random.value * 100 > biome.percentTree)
        {
            return false;
        }
        if (node.poi != null)
        {
            if (node.poi.clearTrees)
            {
                return false;
            }
        }
		if (biome.trees.Count == 0)
        {
            return false;
        }
        return true;
	}

	bool AllowSplat(Node node)
	{
		if (node.y > -biome.heightMod / 10 || node.edge || node.nearEdge)
		{
			return false;
		}
		if (Random.value * 100 > biome.percentSplat)
		{
			return false;
		}
		if (node.poi != null)
		{
			if (node.poi.clearTrees)
			{
				return false;
			}
		}
		if (biome.splats.Count == 0)
		{
			return false;
		}
		return true;
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

    void TranslateTris()
    {
        trianglesTop = new List<int>();
        trianglesBottom = new List<int>();
        foreach (Tri tri in trisTop)
        {
            trianglesTop.Add(tri.a);
            trianglesTop.Add(tri.b);
            trianglesTop.Add(tri.c);
        }
        foreach (Tri tri in trisBottom)
        {
            trianglesBottom.Add(tri.a);
            trianglesBottom.Add(tri.b);
            trianglesBottom.Add(tri.c);
        }
        foreach (PointOfInterest poi in pois)
        {
			if (poi.tris.Count > 0)
            {
                poi.triangles = new List<int>();
                foreach (Tri tri in poi.tris)
                {
                    poi.triangles.Add(tri.a);
                    poi.triangles.Add(tri.b);
                    poi.triangles.Add(tri.c);
                }
            }
            if (poi.useCustomMesh)
            {
                poi.SetCustomMesh(vertsTop);
            }
        }
    }

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
        go.AddComponent<MeshCollider>();
    }
}
