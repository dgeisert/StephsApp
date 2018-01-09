using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ---------------------------------------------------------------------------------------------------------------------------
// Procedural Tree - Simple tree mesh generation - � 2015 Wasabimole http://wasabimole.com
// ---------------------------------------------------------------------------------------------------------------------------
// BASIC USER GUIDE
//
// - Choose GameObject > Create Procedural > Procedural Tree from the Unity menu
// - Select the object to adjust the tree's properties
// - Click on Rand Seed to get a new tree of the same type
// - Click on Rand Tree to change the tree type
//
// ADVANCED USER GUIDE
//
// - Drag the object to a project folder to create a Prefab (to keep a static snapshot of the tree)
// - To add a collision mesh to the object, choose Add Component > Physics > Mesh Collider
// - To add or remove detail, change the number of sides
// - You can change the default diffuse bark materials for more complex ones (with bump-map, specular, etc.)
// - Add or replace default materials by adding them to the SampleMaterials\ folder
// - You can also change the tree generation parameters in REAL-TIME from your scripts (*)
// - Use Unity's undo to roll back any unwanted changes
//
// ADDITIONAL NOTES
// 
// The generated mesh will remain on your scene, and will only be re-computed if/when you change any tree parameters.
//
// Branch(...) is the main tree generation function (called recursively), you can inspect/change the code to add new 
// tree features. If you add any new generation parameters, remember to add them to the checksum in the Update() function 
// (so the mesh gets re-computed when they change). If you add any cool new features, please share!!! ;-)
//
// To generate a new tree at runtime, just follow the example in Editor\ProceduralSplatEditor.cs:CreateProceduralSplat()

// Additional scripts under ProceduralSplat\Editor are optional, used to better integrate the trees into Unity.
//
// (*) To change the tree parameters in real-time, just get/keep a reference to the ProceduralSplat component of the 
// tree GameObject, and change any of the public properties of the class.
//
// >>> Please visit http://wasabimole.com/procedural-tree for more information
// ---------------------------------------------------------------------------------------------------------------------------
// VERSION HISTORY
//
// 1.02 Error fixes update
// - Fixed bug when generating the mesh on a rotated GameObject
// - Fix error when building the project
//
// 1.00 First public release
// ---------------------------------------------------------------------------------------------------------------------------
// Thank you for choosing Procedural Tree, we sincerely hope you like it!
//
// Please send your feedback and suggestions to mailto://contact@wasabimole.com
// ---------------------------------------------------------------------------------------------------------------------------

[ExecuteInEditMode]
public class ProceduralSplat : MonoBehaviour
{
    public const int CurrentVersion = 102;

    // ---------------------------------------------------------------------------------------------------------------------------
    // Tree parameters (can be changed real-time in editor or game)
    // ---------------------------------------------------------------------------------------------------------------------------

    public int Seed; // Random seed on which the generation is based
    [Range(3, 8)]
    public int NumberOfSides = 16; // Number of sides for tree
    [Range(0.25f, 20f)]
    public float BaseRadius = 2f; // Base radius in meters
    [Range(0.5f, 0.95f)]
    public float RadiusStep = 0.8f; // Controls how quickly radius decreases
    [Range(0.25f, 10f)]
    public float MinimumRadius = 0.02f; // Minimum radius for the tree's smallest branches
    [Range(0f, 1f)]
    public float BranchRoundness = 0.8f; // Controls how round branches are
    [Range(0.1f, 2f)]
    public float SegmentLength = 0.5f; // Length of branch segments
    [Range(0f, 40f)]
	public float Twisting = 20f; // How much branches twist
	[Range(0f, 0.5f)]
	public float BranchProbability = 0.1f; // Branch probability
    
	public List<Color> baseColorList, leavesColorList;
	Color color = Color.clear, leaves = Color.clear;
    public bool pointEnd = false;
    public bool point2Bloat = false;
    public int maxSteps = 5;
	public float scale = 1;
    float scalingFactor = 1;
    public Vector3 positionFactor = Vector3.zero;

    // ---------------------------------------------------------------------------------------------------------------------------

    float checksum; // Serialized & Non-Serialized checksums for tree rebuilds only on undo operations, or when parameters change (mesh kept on scene otherwise)
    [SerializeField, HideInInspector]
    float checksumSerialized;

    float[] ringShape; // Tree ring shape array

#if UNITY_EDITOR
    [HideInInspector]
    public string MeshInfo; // Used in ProceduralSplatEditor to show info about the tree mesh
#endif


    // ---------------------------------------------------------------------------------------------------------------------------
    // Generate tree (only called when parameters change, or there's an undo operation)
    // ---------------------------------------------------------------------------------------------------------------------------

    CreateIsland island;
	public bool is_tree;
	public float[] leafArray = {8, 1.2f, 1, 1, 0.5f, 0.5f};
	public float[] leafStepArray = {0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f};
	public void Init(ProceduralSplat ps){
		NumberOfSides = ps.NumberOfSides;
		BaseRadius = ps.BaseRadius;
		RadiusStep = ps.RadiusStep;
		MinimumRadius = ps.MinimumRadius;
		BranchRoundness = ps.BranchRoundness;
		SegmentLength = ps.SegmentLength;
		Twisting = ps.Twisting;
		BranchProbability = ps.BranchProbability;
		pointEnd = ps.pointEnd;
		point2Bloat = ps.point2Bloat;
		maxSteps = ps.maxSteps;
		scale = ps.scale;
		is_tree = ps.is_tree;
		leafArray = ps.leafArray;
		leafStepArray = ps.leafStepArray;
		baseColorList = ps.baseColorList;
		leavesColorList = ps.leavesColorList;
	}
	public void Init(int setSeed, CreateIsland setIsland, Vector3 setPosition, float setScale, bool setIsTree = false)
    {
        gameObject.isStatic = false;
		is_tree = setIsTree;
        island = setIsland;
        Seed = setSeed;
        scalingFactor = setScale * scale;
        positionFactor = setPosition;

        var originalRotation = transform.localRotation;
		Random.InitState(Seed);
		if (baseColorList.Count > 0) {
			color = baseColorList [Mathf.FloorToInt (Random.value * baseColorList.Count)];
		}
		if (leavesColorList.Count > 0) {
			leaves = leavesColorList [Mathf.FloorToInt (Random.value * leavesColorList.Count)];
		}

        SetTreeRingShape(); // Init shape array for current number of sides

        // Main recursive call, starts creating the ring of vertices in the trunk's base
        Branch(new Quaternion(), Vector3.zero, -1, BaseRadius, 0f);

        transform.localRotation = originalRotation; // Restore original object rotation
        


    }

    // ---------------------------------------------------------------------------------------------------------------------------
    // Main branch recursive function to generate tree
    // ---------------------------------------------------------------------------------------------------------------------------

    void Branch(Quaternion quaternion, Vector3 position, int lastRingVertexIndex, float radius, float texCoordV, int step = 0)
    {
        if(step == 1 && point2Bloat)
        {
            radius *= 2;
        }
        var offset = Vector3.zero;
        var texCoord = new Vector2(0f, texCoordV);
        var textureStepU = 1f / NumberOfSides;
        var angInc = 2f * Mathf.PI * textureStepU;
        var ang = 0f;

		if (lastRingVertexIndex < 0) {
			island.vertsSplats.Add(positionFactor + scalingFactor * (position + Vector3.down * BaseRadius / 2)); // Add Vertex position
			island.uvsSplats.Add (new Vector2(Random.value * 10, Random.value * 10));
			island.colorListSplats.Add(color);
			for (var n = island.vertsSplats.Count; n < island.vertsSplats.Count + NumberOfSides; n++) // Add cap
			{
				island.trianglesSplats.Add(n + 1);
				island.trianglesSplats.Add(island.vertsSplats.Count - 1);
				island.trianglesSplats.Add(n);
			}
		}

        // Add ring vertices
        for (var n = 0; n <= NumberOfSides; n++, ang += angInc)
        {
            var r = ringShape[n] * radius;
            offset.x = r * Mathf.Cos(ang); // Get X, Z vertex offsets
            offset.z = r * Mathf.Sin(ang);
			island.vertsSplats.Add(positionFactor + scalingFactor * (position + quaternion * offset)); // Add Vertex position
			island.uvsSplats.Add (new Vector2(Random.value * 10, Random.value * 10));
			island.colorListSplats.Add(color);
            texCoord.x += textureStepU;
        }

        if (lastRingVertexIndex >= 0) // After first base ring is added ...
        {
            // Create new branch segment quads, between last two vertex rings
            for (var currentRingVertexIndex = island.vertsSplats.Count - NumberOfSides - 1; currentRingVertexIndex < island.vertsSplats.Count - 1; currentRingVertexIndex++, lastRingVertexIndex++)
            {
                island.trianglesSplats.Add(lastRingVertexIndex + 1); // Triangle A
                island.trianglesSplats.Add(lastRingVertexIndex);
                island.trianglesSplats.Add(currentRingVertexIndex);
                island.trianglesSplats.Add(currentRingVertexIndex); // Triangle B
                island.trianglesSplats.Add(currentRingVertexIndex + 1);
                island.trianglesSplats.Add(lastRingVertexIndex + 1);
            }
        }

        // Do we end current branch?
        radius *= RadiusStep;
        if (radius < MinimumRadius || step >= maxSteps) // End branch if reached minimum radius, or ran out of vertices
        {
		if (is_tree) {
			for (var n = 0; n <= NumberOfSides; n++, ang += angInc)
			{
				var r = ringShape[n] * radius / RadiusStep;
				offset.x = r * Mathf.Cos(ang); // Get X, Z vertex offsets
				offset.z = r * Mathf.Sin(ang);
				island.vertsSplats.Add(positionFactor + scalingFactor * (position + quaternion * offset)); // Add Vertex position
				island.uvsSplats.Add (new Vector2(Random.value * 10, Random.value * 10));
				island.colorListSplats.Add(leaves);
				texCoord.x += textureStepU;
			}
				texCoordV += 0.0625f * (SegmentLength + SegmentLength / radius);
				position += quaternion * new Vector3(0f, SegmentLength / 5f, 0f);
				transform.rotation = quaternion;
				lastRingVertexIndex = island.vertsSplats.Count - NumberOfSides - 1;
				AddLeaves(Quaternion.identity, position, lastRingVertexIndex, radius, 0);
			} else {
				// Create a cap for ending the branch
				if (pointEnd) {
					island.vertsSplats.Add (positionFactor + scalingFactor * (position + Vector3.up * SegmentLength)); // Add Vertex position
					island.uvsSplats.Add (new Vector2(Random.value * 10, Random.value * 10));
					island.colorListSplats.Add (color);
				} else {
					island.vertsSplats.Add (positionFactor + scalingFactor * (position)); // Add Vertex position
					island.uvsSplats.Add (new Vector2(Random.value * 10, Random.value * 10));
					island.colorListSplats.Add (color);
				}
				for (var n = island.vertsSplats.Count - NumberOfSides - 2; n < island.vertsSplats.Count - 2; n++) { // Add cap
					island.trianglesSplats.Add (n);
					island.trianglesSplats.Add (island.vertsSplats.Count - 1);
					island.trianglesSplats.Add (n + 1);
				}
			}
            return;
        }

        // Continue current branch (randomizing the angle)
        texCoordV += 0.0625f * (SegmentLength + SegmentLength / radius);
        position += quaternion * new Vector3(0f, SegmentLength, 0f);
        transform.rotation = quaternion;
        var x = (Random.value - 0.5f) * Twisting;
        var z = (Random.value - 0.5f) * Twisting;
        transform.Rotate(x, 0f, z);
        lastRingVertexIndex = island.vertsSplats.Count - NumberOfSides - 1;
		Branch(transform.rotation, position, lastRingVertexIndex, radius, texCoordV, step + 1); // Next segment

		// Do we branch?
		if (Random.value > BranchProbability)
			return;

		// Yes, add a new branch
		transform.rotation = quaternion;
		x = Random.value * 70f - 35f;
		x += x > 0 ? 10f : -10f;
		z = Random.value * 70f - 35f;
		z += z > 0 ? 10f : -10f;
		transform.Rotate (x, 0f, z);
		Branch (transform.rotation, position, lastRingVertexIndex, radius, texCoordV);
	}
	void AddLeaves(Quaternion quaternion, Vector3 position, int lastRingVertexIndex, float radius, int leafIndex)
	{
		var offset = Vector3.zero;
		var texCoord = new Vector2(0f, 0f);
		var textureStepU = 1f / NumberOfSides;
		var angInc = 2f * Mathf.PI * textureStepU;
		var ang = 0f;
		radius *= leafArray[leafIndex];
		for (var n = 0; n <= NumberOfSides; n++, ang += angInc)
		{
			var r = ringShape[n] * radius;
			offset.x = r * Mathf.Cos(ang); // Get X, Z vertex offsets
			offset.z = r * Mathf.Sin(ang);
			island.vertsSplats.Add(positionFactor + scalingFactor * (position + quaternion * offset)); // Add Vertex position
			island.uvsSplats.Add (new Vector2(Random.value * 10, Random.value * 10));
			island.colorListSplats.Add(leaves);
			texCoord.x += textureStepU;
		}

		// Create new branch segment quads, between last two vertex rings
		for (var currentRingVertexIndex = island.vertsSplats.Count - NumberOfSides - 1; currentRingVertexIndex < island.vertsSplats.Count - 1; currentRingVertexIndex++, lastRingVertexIndex++)
		{
			island.trianglesSplats.Add(lastRingVertexIndex + 1); // Triangle A
			island.trianglesSplats.Add(lastRingVertexIndex);
			island.trianglesSplats.Add(currentRingVertexIndex);
			island.trianglesSplats.Add(currentRingVertexIndex); // Triangle B
			island.trianglesSplats.Add(currentRingVertexIndex + 1);
			island.trianglesSplats.Add(lastRingVertexIndex + 1);
		}
		if (leafIndex >= leafArray.Length - 1)
		{
			// Create a cap for ending the branch
			if (pointEnd) {
				island.vertsSplats.Add (positionFactor + scalingFactor * (position + Vector3.up * SegmentLength)); // Add Vertex position
				island.uvsSplats.Add (new Vector2(Random.value * 10, Random.value * 10));
				island.colorListSplats.Add (leaves);
			} else {
				island.vertsSplats.Add (positionFactor + scalingFactor * (position)); // Add Vertex position
				island.uvsSplats.Add (new Vector2(Random.value * 10, Random.value * 10));
				island.colorListSplats.Add (leaves);
			}
			for (var n = island.vertsSplats.Count - NumberOfSides - 2; n < island.vertsSplats.Count - 2; n++) // Add cap
			{
				island.trianglesSplats.Add(n);
				island.trianglesSplats.Add(island.vertsSplats.Count - 1);
				island.trianglesSplats.Add(n + 1);
			}
			return;
		}
		position += quaternion * new Vector3(0f, leafStepArray[leafIndex], 0f);
		transform.rotation = quaternion;
		var xLeaves = (Random.value - 0.5f) * Twisting;
		var zLeaves = (Random.value - 0.5f) * Twisting;
		transform.Rotate(xLeaves, 0f, zLeaves);
		lastRingVertexIndex = island.vertsSplats.Count - NumberOfSides - 1;
		AddLeaves(transform.rotation, position, lastRingVertexIndex, radius, leafIndex + 1);
	}

    // ---------------------------------------------------------------------------------------------------------------------------
    // Set tree shape, by computing a random offset for every ring vertex
    // ---------------------------------------------------------------------------------------------------------------------------

    private void SetTreeRingShape()
    {
        ringShape = new float[NumberOfSides + 1];
        var k = (1f - BranchRoundness) * 0.5f;
        // Randomize the vertex offsets, according to BranchRoundness
        Random.InitState(Seed);
        for (var n = 0; n < NumberOfSides; n++) ringShape[n] = 1f - (Random.value - 0.5f) * k;
        ringShape[NumberOfSides] = ringShape[0];
    }
}