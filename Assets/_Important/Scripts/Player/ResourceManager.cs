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
	Ore,
	Metal,
	Gold,
	Leather,
	Apples,
	Wheat,
	Water,
	Peasants,
	Craftsmen,
	Gentry,
	Lords,
	Nobles,
	Grapes,
	Cider,
	Wine,
	Flour,
	Bread,
	Beer,
	Charcoal,
	Clothes,
	Wool,
	Milk,
	Cows,
	Sheep,
	Horse,
	Mead,
	Eggs,
	Jewelry,
	Fish,
	OreDeposit
}

public class ResourceManager : MonoBehaviour {

	public static ResourceManager instance;
	public List<Sprite> resourceSpritesInput;
	public Dictionary<Resource, Sprite> resourceSprites;
	public GameObject claimResourceParticles;
	public Dictionary<Resource, int> resourceCounts = new Dictionary<Resource, int>();
	Dictionary<Resource, List<CreateIsland.Node>> resourceLocations = new Dictionary<Resource, List<CreateIsland.Node>> ();
	public List<GameObject> buildings;
	public List<Building> constructedBuildings = new List<Building> ();
	public GameObject currentBuilding;

	public void Init(){
		instance = this;
		currentBuilding = buildings [0];
		resourceSprites = new Dictionary<Resource, Sprite> ();
		foreach (Sprite spr in resourceSpritesInput) {
			//Debug.Log (spr.name);
			resourceSprites.Add ((Resource)System.Enum.Parse(typeof(Resource), spr.name), spr);
		}
		resourceCounts.Add (Resource.Gold, 0);
	}

	public List<CreateIsland.Node> HighlightResource(List<Resource> rTypes, Vector3 center, float radius){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		foreach (CreateIsland.Node node in GetResourceNodes(rTypes, center, radius)) {
			node.Highlight ();
			nodes.Add (node);
		}
		return nodes;
	}

	public List<CreateIsland.Node> GetResourceNodes(List<Resource> rTypes, Vector3 center, float radius){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		foreach (CreateIsland.Node node in CreateLevel.instance.GetNodes(center + new Vector3(0.5f, 0, 0.5f), radius)) {
			CreateIsland.Node setNode = node.reference == null ? node : node.reference;
			if (rTypes.Contains(setNode.resource) && !setNode.claimed && !nodes.Contains (setNode)) {
				nodes.Add (setNode);
			}
		}
		return nodes;
	}

	public List<CreateIsland.Node> HighlightConsumers(Resource rType, CreateIsland.Node focusNode){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		foreach (Building b in ResourceManager.instance.constructedBuildings) {
			if(b.consumedResource.Contains(rType)){
				if(CreateLevel.instance.GetNodes(b.transform.position + new Vector3 (-(b.size.x - 1) / 2, 0, (b.size.y - 1) / 2)
					, b.radius
					,false
					,true).Contains(focusNode)){
					b.myNode.HighlightConsumer();
					nodes.Add(b.myNode);
				}
			}
		}
		return nodes;
	}

	public List<CreateIsland.Node> ClaimResource(List<Resource> rTypes, Vector3 center, float radius, Building building){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		List<Vector3> verts = new List<Vector3> ();
		List<int> tris = new List<int> ();
		foreach (CreateIsland.Node node in GetResourceNodes(rTypes, center, radius)) {
			node.claimed = building.claimSources;
			nodes.Add (node);
			for (int i = node.splatIndexStart; i < node.splatIndexEnd; i++) {
				tris.Add (verts.Count);
				tris.Add (verts.Count);
				tris.Add (verts.Count);
				verts.Add (node.island.vertsSplats [i] + node.island.transform.position);
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
		if (node.resource != rType) {
			float closestDistance = 1000000;
			foreach (Building other in constructedBuildings) {
				if (other.producedResource == rType) {
					if (Vector3.Distance (node.GetPoint () + node.island.transform.position, other.transform.position) < closestDistance) {
						closestDistance = Vector3.Distance (node.GetPoint () + node.island.transform.position, other.transform.position);
						node = other.myNode;
					}
				}
			}
		}
		if (resourceCounts.ContainsKey (rType)) {
			resourceCounts [rType] += amount;
			if (!resourceLocations.ContainsKey (rType)) {
				resourceLocations.Add (rType, new List<CreateIsland.Node> ());
			}
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
		if (Time.time > 1) {
			node.Save ();
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
				int deduct = -Mathf.Min (amount, maxNode.resourceCount);
				AddResource (rType, deduct, maxNode);
				amount += deduct;
			}
		}
		return nodes;
	}

	public int HasResource(Resource r, int amount){
		if (!resourceCounts.ContainsKey (r)) {
			resourceCounts.Add (r, 0);
			resourceLocations.Add (r, new List<CreateIsland.Node> ());
		}
		if (resourceCounts [r] >= amount) {
			// can afford without gold
			return 1;
		}
		if (resourceCounts [r] + resourceCounts [Resource.Gold] >= amount) {
			// can afford but requires gold
			return 2;
		}
		//cannot afford
		return 0;
	}

}
