using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour {
	public int unlockCost = 10;
	public Resource unlockResource = Resource.Logs;
	public Material highlight, noHighlight;
	public Vector2Int landPosition;
	public CreateIsland island;

	public void Highlight(){
		GetComponent<MeshRenderer> ().material = highlight;
		MenuManager.instance.SetOption (1, null);
	}
	public void RemoveHighlight(){
		GetComponent<MeshRenderer> ().material = noHighlight;
		MenuManager.instance.ResetOptions ();
	}
	public void Unlock(){
		GameManager.AddVisibleLand (landPosition);
		island.MakeVisisble ();
	}
}
