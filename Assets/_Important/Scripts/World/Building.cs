using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

	public List<Resource> consumedResource = new List<Resource> ();
	public Resource producedResource;
	Dictionary<Resource, int> buildCost;
	public List<int> consumeRate = new List<int> ();
	public int produceRate = 1; 
	public List<CreateIsland.Node> nodes = new List<CreateIsland.Node>();
	public CreateIsland.Node myNode;
	public int rate = 1, maxRate = 10, maxHold = 20;
	public float radius = 10;
	public TextMesh text;
	public bool claimSources = false;
	public Vector2 size = Vector2.one;
	public GameObject radiusVisualizer;

	void Start(){
		ParticleSystem.ShapeModule shape = radiusVisualizer.GetComponent<ParticleSystem> ().shape;
		shape.radius = radius;
		radiusVisualizer.transform.localPosition = new Vector3 ((size.x - 1) / 2, 0.5f, (size.y - 1) / 2);
		radiusVisualizer.SetActive (true);
	}

	public void SetNodes(List<CreateIsland.Node> setNodes, CreateIsland.Node setMyNode){
		//Debug.Log ("Set Nodes");
		radiusVisualizer.SetActive(false);
		nodes = setNodes;
		myNode = setMyNode;
		rate = Mathf.Min(nodes.Count, maxRate);
		myNode.resource = producedResource;
		myNode.item = gameObject;
		if (rate > 0) {
			if (consumedResource.Count > 1) {
				Invoke ("ResourceExchange", 10 / rate);
			} else {
				if (consumeRate [0] == 0) {
					Invoke ("SimplestExchange", 10 / rate);
				} else {
					Invoke ("SimpleExchange", 10 / rate);
				}
			}
		}
	}

	public bool CheckPlacement(Vector3 point){
		for (int i = 0; i < size.x; i++) {
			for (int j = 0; j < size.y; j++){
				if(CreateLevel.instance.GetNode(point+ new Vector3(0.5f + i, 0, 0.5f + j)).occupied){
					return false;
				}
			}
		}
		return true;
	}

	public void SimplestExchange(){
		if (!CheckResourceCap ()) {
			Invoke ("SimplestExchange", 10/rate);
			return;
		}
		AddResource (produceRate);
		Invoke ("SimplestExchange", 10/rate);
	}

	public void SimpleExchange(){
		if (!CheckResourceCap ()) {
			Invoke ("SimpleExchange", 10/rate);
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
							ResourceManager.instance.AddResource(producedResource, -deduct, node2);
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
		Invoke ("SimpleExchange", 10/rate);
	}

	public void ResourceExchange(){
		if (!CheckResourceCap ()) {
			Invoke ("ResourceExchange", 10/rate);
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
					ResourceManager.instance.AddResource(producedResource, -consumeRate[i], maxNode);
				}
			}
			AddResource (produceRate);
		}
		Invoke ("ResourceExchange", 10/rate);
	}

	public void AddResource(int toAdd){
		//Debug.Log ("Adding " + toAdd.ToString () + " " + producedResource.ToString () + " to " + myNode.resourceCount.ToString ());
		ResourceManager.instance.AddResource(producedResource, toAdd, myNode);
	}

	public bool CheckResourceCap(){
		return myNode.resourceCount + produceRate <= maxHold;
	}
}
