using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOfInterest : MonoBehaviour
{

	//these values are used for determining which POI to use
	public int requiredLevel;
    public int maxPerIsland = -1, assignedToIsland = 0;
    public float rarity;

    //these values determine how the POI is displayed
    public Color color;
    public float depression, radius = 0, width = 0, height = 0;
    public GameObject objectOfInterest, objectOfInterestInstance;
    public bool clearTrees = true;
    public List<CreateIsland.Tri> tris = new List<CreateIsland.Tri>();
    public List<int> triangles;
    public int x, z;

    //this section is for custom meshes
    public bool useCustomMesh = false;
    public MeshFilter customMeshFilter;
    public MeshRenderer customMeshRenderer;
    public MeshCollider customMeshCollider;
    public Material customMeshMaterial;
    public float scale = 1f;
	float size = 0;
	public bool important = false;

    public void Init(PointOfInterest basePOI)
    {
		color = basePOI.color;
        depression = basePOI.depression;
        radius = basePOI.radius;
        width = basePOI.width;
        height = basePOI.height;
        objectOfInterest = basePOI.objectOfInterest;
        clearTrees = basePOI.clearTrees;
        maxPerIsland = basePOI.maxPerIsland;
        assignedToIsland = 0;
        useCustomMesh = basePOI.useCustomMesh;
		customMeshMaterial = basePOI.customMeshMaterial;
		scale = basePOI.scale;
    }

    public bool Contains(int i, int j)
    {
        if (radius > 0)
        {
            if (Vector2.Distance(new Vector2(x, z), new Vector2(i, j)) <= radius)
            {
                return true;
            }
        }
        if (width > 0)
        {
            if (height > 0)
            {
                return Mathf.Abs(i - x) < (width / 2) && Mathf.Abs(j - z) < (height / 2);
            }
        }
        return false;
    }

    public void SetCustomMesh(List<Vector3> verts)
    {
        if (objectOfInterestInstance == null)
        {
            return;
        }
        customMeshFilter = objectOfInterestInstance.GetComponent<MeshFilter>();
        customMeshCollider = objectOfInterestInstance.GetComponent<MeshCollider>();
        customMeshRenderer = objectOfInterestInstance.GetComponent<MeshRenderer>();
        if (customMeshFilter == null || customMeshRenderer == null || customMeshCollider == null)
        {
            return;
        }
        Dictionary<int, int> vertLookup = new Dictionary<int, int>();
        List<Vector3> newVerts = new List<Vector3>();
        List<int> newTris = new List<int>();
		if (triangles != null) {
			foreach (int i in triangles) {
				if (vertLookup.ContainsKey (i)) {
					newTris.Add (vertLookup [i]);
				} else {
					vertLookup.Add (i, newVerts.Count);
					newVerts.Add (verts [i] + new Vector3 (-x + size / 2, 0.01f, -z + size / 2));
					newTris.Add (vertLookup [i]);
				}
			}
		}
        Mesh mesh = new Mesh();
        mesh.SetVertices(newVerts);
        mesh.SetTriangles(newTris, 0);
        customMeshFilter.mesh = mesh;
        customMeshCollider.sharedMesh = mesh;
        mesh.RecalculateNormals();
        customMeshRenderer.material = customMeshMaterial;
    }

	public void SetObject(CreateIsland island){
		size = island.size;
		objectOfInterestInstance = dgUtil.Instantiate(objectOfInterest, new Vector3(x - size / 2, -depression, z - size / 2), Quaternion.identity, true, island.top.transform);
		objectOfInterestInstance.transform.localScale *= scale;
		foreach(ProceduralSplat splat in objectOfInterestInstance.GetComponentsInChildren<ProceduralSplat>()) {
			Vector3 setPosition = island.vertsTop [island.nodes [x, z].vert];
			if (splat.transform != objectOfInterestInstance.transform) {
				setPosition += splat.transform.localPosition;
			}
			splat.Init (
				island
				, setPosition
				, scale
				, splat.is_tree);
		}
		Cage cage = objectOfInterestInstance.GetComponent<Cage>();
		if(cage != null)
		{
			cage.Init();
		}
		TreasureChest chest = objectOfInterestInstance.GetComponent<TreasureChest>();
		if(chest != null)
		{
			chest.seed = Mathf.FloorToInt(Random.value * 10000 + 1000);
		}
		SpawnPoint spawnPoint = objectOfInterestInstance.GetComponent<SpawnPoint>();
		if(spawnPoint != null)
		{
			spawnPoint.Init();
		}
		Statue statue = objectOfInterestInstance.GetComponent<Statue> ();
		if (statue != null) {
			statue.Init (island);
		}
	}
}
