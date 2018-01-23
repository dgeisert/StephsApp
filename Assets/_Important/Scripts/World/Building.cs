using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

	public List<Resource> consumedResource = new List<Resource> ();
	public List<Resource> displayResource = new List<Resource>();
	public Resource producedResource;
	public List<int> consumeRate = new List<int> ();
	public int produceRate = 1; 
	public List<CreateIsland.Node> nodes = new List<CreateIsland.Node>();
	public CreateIsland.Node myNode;
	public int maxRate = 10, minRate = 0, maxHold = 20, starting = 0, unlockLevel = 1;
	public float rate = 1, radius = 10, baseRate = 10, buildTime = 10;
	public TextMesh text;
	public bool claimSources = false;
	public Vector2 size = Vector2.one;
	public GameObject radiusVisualizer;
	bool initialized = false;
	public Material highlightBadMaterial;
	public Dictionary<MeshRenderer, Material>  matsHold;
	public string Category;

	public Resource buildResource;
	public int buildCost, buildCostBase = 10;
	public float costIncrease = 1.2f;

	void Start(){
		if (!initialized) {
			Init ();
			SetNodes(
				ResourceManager.instance.ClaimResource (consumedResource
					, transform.position + new Vector3 (-(size.x - 1) / 2, 0, (size.y - 1) / 2)
					, radius
					, this)
				, CreateLevel.instance.GetNode (transform.position + new Vector3 (0.5f, 0, 0.5f)));
			AddResource (starting);
		}
	}

	public void Init(){
		ParticleSystem.ShapeModule shape = radiusVisualizer.GetComponent<ParticleSystem> ().shape;
		shape.radius = radius;
		radiusVisualizer.transform.localPosition = new Vector3 (-(size.x - 1) / 2, 0.5f, (size.y - 1) / 2);
		radiusVisualizer.SetActive (true);
		initialized = true;
	}

	public void SetNodes(List<CreateIsland.Node> setNodes, CreateIsland.Node setMyNode){
		//Debug.Log ("Set Nodes");
		radiusVisualizer.SetActive(false);
		nodes = setNodes;
		foreach (CreateIsland.Node n in nodes) {
			if (n.claimants == null) {
				n.claimants = new List<Building> ();
			}
			if (!n.claimants.Contains (this)) {
				n.claimants.Add (this);
			}
		}
		myNode = setMyNode;
		rate = Mathf.Max (Mathf.Min (nodes.Count, maxRate), minRate);
		myNode.resource = producedResource;
		myNode.item = gameObject;
		for (int i = 0; i < size.x; i++) {
			for (int j = 0; j < size.y; j++) {
				CreateIsland.Node setNode = CreateLevel.instance.GetNode (transform.position + new Vector3 (-i + 0.5f, 0, j + 0.5f));
				setNode.occupied = true;
				if (setNode != myNode) {
					setNode.reference = myNode;
				}
			}
		}
		if (rate > 0) {
			if (consumedResource.Count > 1) {
				CancelInvoke ("ResourceExchange");
				Invoke ("ResourceExchange", baseRate / rate * ResourceManager.instance.rateMult);
			} else {
				if (consumeRate [0] == 0) {
					CancelInvoke ("SimplestExchange");
					Invoke ("SimplestExchange", baseRate / rate * ResourceManager.instance.rateMult);
				} else {
					CancelInvoke ("SimpleExchange");
					Invoke ("SimpleExchange", baseRate / rate * ResourceManager.instance.rateMult);
				}
			}
		}
		Invoke ("Save", 0.1f);
	}

	public bool CheckPlacement(Vector3 point){
		for (int i = 0; i < size.x; i++) {
			for (int j = 0; j < size.y; j++){
				if(CreateLevel.instance.GetNode(point+ new Vector3(0.5f - i, 0, 0.5f + j)).occupied){
					return false;
				}
			}
		}
		return true;
	}

	public void SimplestExchange(){
		if (!CheckResourceCap ()) {
			Invoke ("SimplestExchange", baseRate/rate * ResourceManager.instance.rateMult);
			return;
		}
		AddResource (produceRate);
		Invoke ("SimplestExchange", baseRate/rate * ResourceManager.instance.rateMult);
	}

	public void SimpleExchange(){
		if (!CheckResourceCap ()) {
			Invoke ("SimpleExchange", baseRate/rate * ResourceManager.instance.rateMult);
			return;
		}
		int count = 0;
		foreach (CreateIsland.Node node in nodes) {
			if (node.resource == consumedResource [0]) {
				count += node.resourceCount;
				if (count >= consumeRate [0]) {
					foreach (CreateIsland.Node node2 in nodes) {
						if (node2.resource == consumedResource [0]) {
							int deduct = Mathf.Min(consumeRate[0], node2.resourceCount);
							count -= deduct;
							ResourceManager.instance.AddResource(consumedResource[0], -deduct, node2);
							if (count == 0) {
								break;
							}
						}
					}
					AddResource (produceRate);
					break;
				}
			}
		}
		Invoke ("SimpleExchange", baseRate/rate * ResourceManager.instance.rateMult);
	}

	public void ResourceExchange(){
		if (!CheckResourceCap ()) {
			Invoke ("ResourceExchange", baseRate/rate * ResourceManager.instance.rateMult);
			return;
		}
		bool canExchange = true;
		for(int i = 0; i < consumedResource.Count; i++){
			if (consumeRate[i] > 0) {
				int count = 0;
				foreach (CreateIsland.Node node in nodes) {
					if (node.resource == consumedResource [i]) {
						count += node.resourceCount;
					}
				}
				if (count < consumeRate [i]) {
					canExchange = false;
					break;
				}
			}
		}
		if (canExchange) {
			for(int i = 0; i < consumedResource.Count; i++){
				if (consumeRate[i] > 0) {
					int count = 0;
					CreateIsland.Node maxNode = null;
					foreach (CreateIsland.Node node in nodes) {
						if (node.resource == consumedResource [i]) {
							if (maxNode == null) {
								maxNode = node;
							}
							else if (node.resourceCount > maxNode.resourceCount) {
								maxNode = node;
							}
						}
					}
					ResourceManager.instance.AddResource(consumedResource[i], -consumeRate[i], maxNode);
				}
			}
			AddResource (produceRate);
		}
		Invoke ("ResourceExchange", baseRate/rate * ResourceManager.instance.rateMult);
	}

	public void AddResource(int toAdd){
		//Debug.Log ("Adding " + toAdd.ToString () + " " + producedResource.ToString () + " to " + myNode.resourceCount.ToString ());
		ResourceManager.instance.AddResource(producedResource, toAdd, myNode);
	}

	public bool CheckResourceCap(){
		return myNode.resourceCount + produceRate <= maxHold;
	}

	public void BadHighlight(){
		if (matsHold == null ? true : matsHold.Count == 0) {
			matsHold = new Dictionary<MeshRenderer, Material> ();
			foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>()) {
				matsHold.Add (renderer, renderer.material);
				renderer.material = highlightBadMaterial;
			}
		}
	}
	public void RemoveBadHighlight(){
		if (matsHold != null) {
			foreach (KeyValuePair<MeshRenderer, Material> kvp in matsHold) {
				kvp.Key.material = kvp.Value;
			}
			matsHold.Clear ();
		}
	}

	public void Load(CreateIsland.Node node, string nodes, int toAdd){
		Init ();
		string[] nodeIndex = nodes.Split('-');
		List<CreateIsland.Node> setNodes = new List<CreateIsland.Node> ();
		for (int i = 0; i < nodeIndex.Length - 2; i += 4) {
			CreateIsland.Node claimNode = CreateLevel.instance.islands [
				new Vector2 (int.Parse (nodeIndex [i]), int.Parse (nodeIndex [i + 1]))]
				.nodes [int.Parse (nodeIndex [i + 2]), int.Parse (nodeIndex [i + 3])];
			claimNode.claimed = claimSources;
			setNodes.Add(claimNode);
		}
		ResourceManager.instance.constructedBuildings.Add (this);
		SetNodes (setNodes, node);
		AddResource (toAdd);
	}

	public void SetBuildMode(){
		ResourceManager.instance.currentBuilding = gameObject;
		MenuManager.instance.CloseMenu ();
		if (name == "Warehouse") {
			MenuManager.instance.WarehouseSelect ();
			return;
		}
		MenuManager.instance.ToggleBuildMode ();
	}

	public void SetWarehouseResource(){
		producedResource = ResourceManager.instance.currentWarehouseResourceBuild;
		consumedResource[0] = ResourceManager.instance.currentWarehouseResourceBuild;
	}

	public void Save(){
		myNode.Save ();
	}
}