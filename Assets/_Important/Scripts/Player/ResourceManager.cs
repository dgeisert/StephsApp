using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Resource {
	Open,
	Grain,
	Tree,
	Logs,
	Wood,
	Boulder,
	Rock,
	Stone,
	Grass,
	Clay,
	Brick,
	IronOre,
	Iron,
	Gold,
	GoldOre,
	Hides,
	Leather,
	People,
	Fruit
}

public class ResourceManager : MonoBehaviour {

	public static ResourceManager instance;
	public List<Sprite> resourceSpritesInput;
	public Dictionary<Resource, Sprite> resourceSprites;
	public GameObject claimResourceParticles;
	Dictionary<Resource, int> resourceCounts = new Dictionary<Resource, int>();
	Dictionary<Resource, List<CreateIsland.Node>> resourceLocations = new Dictionary<Resource, List<CreateIsland.Node>> ();
	public List<GameObject> buildings;
	public GameObject currentBuilding;

	void Start(){
		instance = this;
		currentBuilding = buildings [0];
		resourceSprites = new Dictionary<Resource, Sprite> ();
		foreach (Sprite spr in resourceSpritesInput) {
			//Debug.Log (spr.name);
			resourceSprites.Add ((Resource)System.Enum.Parse(typeof(Resource), spr.name), spr);
		}
	}

	public List<CreateIsland.Node> HighlightResource(List<Resource> rTypes, Vector3 center, float radius){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		foreach (CreateIsland.Node node in CreateLevel.instance.GetNodes(center + new Vector3(0.5f, 0, 0.5f), radius)) {
			CreateIsland.Node setNode = node.reference == null ? node : node.reference;
			if (rTypes.Contains(setNode.resource) && !setNode.claimed && !nodes.Contains (setNode)) {
				setNode.Highlight ();
				nodes.Add (setNode);
			}
		}
		return nodes;
	}

	public List<CreateIsland.Node> ClaimResource(List<Resource> rTypes, Vector3 center, float radius, Building building){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		List<Vector3> verts = new List<Vector3> ();
		List<int> tris = new List<int> ();
		foreach (CreateIsland.Node node in CreateLevel.instance.GetNodes(center + new Vector3(0.5f, 0, 0.5f), radius)) {
			CreateIsland.Node setNode = node.reference == null ? node : node.reference;
			if (rTypes.Contains(setNode.resource) && !setNode.claimed && !nodes.Contains (setNode)) {
				node.claimed = building.claimSources;
				nodes.Add (setNode);
				for (int i = setNode.splatIndexStart; i < setNode.splatIndexEnd; i++) {
					tris.Add (verts.Count);
					tris.Add (verts.Count);
					tris.Add (verts.Count);
					verts.Add (setNode.island.vertsSplats[i] + setNode.island.transform.position);
				}
			}
		}
		Mesh mesh = new Mesh ();
		mesh.SetVertices (verts);
		mesh.SetTriangles (tris, 0);
		mesh.RecalculateNormals();
		GameObject particles = dgUtil.Instantiate (claimResourceParticles, Vector3.zero, Quaternion.identity);
		ParticleSystem.ShapeModule shape = particles.GetComponent<ParticleSystem> ().shape;
		shape.mesh = mesh;
		Destroy(particles, 5);
		return nodes;
	}

	public void AddResource(Resource rType, int amount, CreateIsland.Node node){
		if (resourceCounts.ContainsKey (rType)) {
			resourceCounts [rType] += amount;
			if (!resourceLocations[rType].Contains(node)){
				resourceLocations [rType].Add (node);
			}
		} else {
			resourceCounts.Add (rType, amount);
			resourceLocations.Add (rType, new List<CreateIsland.Node> (){ node });
		}
		node.resourceCount += amount;
		Building b = node.item.GetComponent<Building> ();
		if (b != null) {
			b.text.text = node.resourceCount.ToString ();
		}
	}

	public List<CreateIsland.Node> RemoveResources(Resource rType, int amount){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		if (resourceCounts [rType] >= amount) {
			while (amount > 0) {
				CreateIsland.Node maxNode = null;
				foreach (CreateIsland.Node node in resourceLocations[rType]) {
					if (maxNode == null) {
						maxNode = node;
					} else if (node.resourceCount > maxNode.resourceCount) {
						maxNode = node;
					}
				}
				nodes.Add (maxNode);
				AddResource (rType, -Mathf.Min (amount, maxNode.resourceCount), maxNode);
			}
		}
		return nodes;
	}

}
