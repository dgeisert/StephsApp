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
	ConstructedDeco,
	DefeatedEncampments,
	Ore,
	Metal,
	Gold,
	XLeather,
	Apples,
	Wheat,
	Water,
	Peasants,
	Craftsmen,
	Gentry,
	Lords,
	Royals,
	Grapes,
	Cider,
	Wine,
	Flour,
	Bread,
	Beer,
	Charcoal,
	Cloth,
	DefeatedEnemies,
	Milk,
	XCow,
	Sheep,
	Horse,
	Mead,
	Eggs,
	Jewelry,
	Fish,
	OreDeposit,
	OpenWater,
	Honey,
	Happiness,
	Cat,
	Unknown,
	ConstructedBuildings,
	ExploredTiles
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
	public Resource currentWarehouseResourceBuild = Resource.Unknown;
	public List<Resource> warehouseResources;
	int consumesToHappiness = 1, consecutiveConsumes = 0;
	public float PeasantConsumeRate = 40, CraftsmenConsumeRate = 35, GentryConsumeRate = 30, LordsConsumeRate = 25, RoyalConsumeRate = 10;

	//this is a debug value to make the game operate faster
	public float rateMult = 0.5f;

	public void Update(){
		if(Input.GetKeyDown(KeyCode.J)){
			rateMult = 0.1f;
		}
		if(Input.GetKeyUp(KeyCode.J)){
			rateMult = 0.5f;
		}
	}

	public void Init(){
		instance = this;
		currentBuilding = buildings [0];
		resourceSprites = new Dictionary<Resource, Sprite> ();
		foreach (Sprite spr in resourceSpritesInput) {
			//Debug.Log (spr.name);
			resourceSprites.Add ((Resource)System.Enum.Parse(typeof(Resource), spr.name), spr);
		}
		buildings.Sort(delegate(GameObject a, GameObject b)
			{
				Building x = a.GetComponent<Building>();
				Building y = b.GetComponent<Building>();
				if (x.unlockLevel == null && y.unlockLevel == null) return 0;
				else if (x.unlockLevel == null) return -1;
				else if (y.unlockLevel == null) return 1;
				else return x.unlockLevel.CompareTo(y.unlockLevel);
			});
		resourceCounts.Add (Resource.Gold, 0);
		if (!resourceCounts.ContainsKey (Resource.Royals)) {
			resourceCounts.Add (Resource.Royals, 0);
		}
		if (!resourceCounts.ContainsKey (Resource.Lords)) {
			resourceCounts.Add (Resource.Lords, 0);
		}
		if (!resourceCounts.ContainsKey (Resource.Gentry)) {
			resourceCounts.Add (Resource.Gentry, 0);
		}
		if (!resourceCounts.ContainsKey (Resource.Craftsmen)) {
			resourceCounts.Add (Resource.Craftsmen, 0);
		}
		if (!resourceCounts.ContainsKey (Resource.Peasants)) {
			resourceCounts.Add (Resource.Peasants, 0);
		}
		if (!resourceCounts.ContainsKey (Resource.Happiness)) {
			resourceCounts.Add (Resource.Happiness, 0);
		}
		Invoke ("PeasantConsume", 5);
		Invoke ("CraftsmemConsume", 5);
		Invoke ("GentryConsume", 5);
		Invoke ("LordsConsume", 5);
		Invoke ("RoyalsConsume", 5);
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

	public void AddResource(Resource rType, int amount, CreateIsland.Node node, bool overflow = false, bool checkQuests = true){
		if (node == null) {
			foreach (CreateIsland.Node n in resourceLocations[rType]) {
				if (node == null) {
					node = n;
				} else {
					if (node.resourceCount > n.resourceCount) {
						node = n;
					}
				}
			}
			if (node == null) {
				return;
			}
		}
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
		Building b = node.item.GetComponent<Building> ();
		if (b != null) {
			if (!overflow && node.resourceCount + amount > b.maxHold) {
				if (rType == Resource.Happiness) {
					AddResource (Resource.Gold, 1, null);
				}
				node.resourceCount = Mathf.Max(b.maxHold, node.resourceCount);
			} else {
				node.resourceCount += amount;
			}
			b.text.text = node.resourceCount.ToString ();
			b.textShadow.text = node.resourceCount.ToString ();
		}
		if (checkQuests) {
			QuestManager.instance.CheckQuests ();
		}
		if (Time.time > 1) {
			node.Save ();
		}
	}

	public List<CreateIsland.Node> RemoveResources(Resource rType, int amount, bool checkQuests = true){
		List<CreateIsland.Node> nodes = new List<CreateIsland.Node> ();
		if(!resourceCounts.ContainsKey(rType)){
			resourceCounts.Add (rType, 0);
		}
		if (!resourceLocations.ContainsKey (rType)) {
			resourceLocations.Add (rType, new List<CreateIsland.Node> ());
		}
		while (amount > 0) {
			CreateIsland.Node maxNode = null;
			foreach (CreateIsland.Node node in resourceLocations[rType]) {
				if (maxNode == null) {
					maxNode = node;
				} else if (node.resourceCount > maxNode.resourceCount) {
					maxNode = node;
				}
			}
			if (maxNode != null) {
				nodes.Add (maxNode);
				int deduct = -Mathf.Min (amount, maxNode.resourceCount);
				AddResource (rType, deduct, maxNode, false, checkQuests);
				amount += deduct;
			}
			if (resourceCounts [rType] <= 0) {
				break;
			}
		}
		if (resourceCounts [rType] <= 0 && rType != Resource.Gold && amount > 0) {
			foreach (CreateIsland.Node node in RemoveResources(Resource.Gold, amount)) {
				nodes.Add (node);
			}
		} else if (resourceCounts [rType] <= 0 && rType == Resource.Gold && amount > 0) {
			RemoveResources (Resource.Happiness, amount);
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

	public void PeasantConsume(){
		if (resourceCounts [Resource.Peasants] > 0) {
			int startHappiness = resourceCounts [Resource.Happiness];
			RemoveResources (Resource.Grain, 1);
			RemoveResources (Resource.Water, 1);
			RemoveResources (Resource.Logs, 1);
			if (startHappiness > resourceCounts [Resource.Happiness]) {
				RemoveResources (Resource.Peasants, 1);
				consecutiveConsumes = 0;
			} else {
				consecutiveConsumes ++;
				if (consecutiveConsumes >= consumesToHappiness) {
					consecutiveConsumes = 0;
					AddResource (Resource.Happiness, 1, null);
				}
			}
		}
		Invoke ("PeasantConsume", PeasantConsumeRate * rateMult 
			/ (resourceCounts [Resource.Peasants] > 0 ? resourceCounts [Resource.Peasants] : 1));
	}
	public void CraftsmemConsume(){
		if (resourceCounts [Resource.Craftsmen] > 0) {
			int startHappiness = resourceCounts [Resource.Happiness];
			RemoveResources (Resource.Logs, 1);
			RemoveResources (Resource.Beer, 1);
			RemoveResources (Resource.Fish, 1);
			RemoveResources (Resource.Milk, 1);
			if (startHappiness > resourceCounts [Resource.Happiness]) {
				RemoveResources (Resource.Craftsmen, 1);
				consecutiveConsumes = 0;
			}
		}
		Invoke ("CraftsmemConsume", CraftsmenConsumeRate * rateMult 
			/ (resourceCounts [Resource.Craftsmen] > 0 ? resourceCounts [Resource.Craftsmen] : 1));
	}
	public void GentryConsume(){
		if (resourceCounts [Resource.Gentry] > 0) {
			int startHappiness = resourceCounts [Resource.Happiness];
			RemoveResources (Resource.Fish, 1);
			RemoveResources (Resource.Cider, 1);
			RemoveResources (Resource.Grapes, 1);
			RemoveResources (Resource.Charcoal, 1);
			if (startHappiness > resourceCounts [Resource.Happiness]) {
				RemoveResources (Resource.Gentry, 1);
				consecutiveConsumes = 0;
			}
		}
		Invoke ("GentryConsume", GentryConsumeRate * rateMult 
			/ (resourceCounts [Resource.Gentry] > 0 ? resourceCounts [Resource.Gentry] : 1));
	}
	public void LordsConsume(){
		if (resourceCounts [Resource.Lords] > 0) {
			int startHappiness = resourceCounts [Resource.Happiness];
			RemoveResources (Resource.Eggs, 1);
			RemoveResources (Resource.Cloth, 1);
			RemoveResources (Resource.Wine, 1);
			RemoveResources (Resource.Honey, 1);
			RemoveResources (Resource.Charcoal, 1);
			if (startHappiness > resourceCounts [Resource.Happiness]) {
				RemoveResources (Resource.Lords, 1);
				consecutiveConsumes = 0;
			}
		}
		Invoke ("LordsConsume", LordsConsumeRate * rateMult 
			/ (resourceCounts [Resource.Lords] > 0 ? resourceCounts [Resource.Lords] : 1));
	}
	public void RoyalsConsume(){
		if (resourceCounts [Resource.Royals] > 0) {
			int startHappiness = resourceCounts [Resource.Happiness];
			RemoveResources (Resource.Charcoal, 1);
			RemoveResources (Resource.Bread, 1);
			RemoveResources (Resource.Mead, 1);
			RemoveResources (Resource.Horse, 1);
			RemoveResources (Resource.Jewelry, 1);
			if (startHappiness > resourceCounts [Resource.Happiness]) {
				RemoveResources (Resource.Royals, 1);
				consecutiveConsumes = 0;
			}
		}
		Invoke ("RoyalsConsume", RoyalConsumeRate * rateMult 
			/ (resourceCounts [Resource.Royals] > 0 ? resourceCounts [Resource.Royals] : 1));
	}

}
