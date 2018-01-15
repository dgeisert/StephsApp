using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{
    public float percentTree = 2f, treeMinSize = 0.5f, treeMaxSize = 1.5f;
	public float percentSplat = 2f, splatMinSize = 0.5f, splatMaxSize = 1.5f;
	public List<ProceduralSplat> splats;
	public List<ProceduralSplat> trees;
	public List<PointOfInterest> pois;
	public List<PointOfInterest> importantPois;
    public float poiSpacing = 15;
	public string biomeName;
    CreateIsland island;

    public void Init(float size, Biome baseBiome, CreateIsland setIsland)
    {
		biomeName = baseBiome.name;
		pois = new List<PointOfInterest>();
		importantPois = new List<PointOfInterest>();
		foreach(PointOfInterest poi in baseBiome.GetComponents<PointOfInterest>())
        {
			if (CreateLevel.instance == null) {
				for (int i = 0; i < poi.maxPerIsland; i++) {
					PointOfInterest newPoi = gameObject.AddComponent<PointOfInterest> ();
					newPoi.Init (poi);
					if (poi.important) {
						importantPois.Add (newPoi);
					} else {
						pois.Add (newPoi);
					}
				}
			} else {
				for (int i = 0; i < poi.maxPerIsland; i++) {
					PointOfInterest newPoi = gameObject.AddComponent<PointOfInterest> ();
					newPoi.Init (poi);
					if (poi.important) {
						importantPois.Add (newPoi);
					} else {
						pois.Add (newPoi);
					}
				}
			}
		}
		splats = new List<ProceduralSplat> ();
		trees = new List<ProceduralSplat> ();
		foreach (ProceduralSplat ps in baseBiome.GetComponents<ProceduralSplat>()) {
			ProceduralSplat newPS = gameObject.AddComponent<ProceduralSplat> ();
			newPS.Init (ps);
			if (newPS.is_tree) {
				trees.Add (newPS);
			} else {
				splats.Add (newPS);
			}
		}
        poiSpacing = 10 + (baseBiome.poiSpacing * size / 100);
        percentTree = baseBiome.percentTree;
        treeMinSize = baseBiome.treeMinSize;
        treeMaxSize = baseBiome.treeMaxSize;
        percentSplat = baseBiome.percentSplat;
        splatMaxSize = baseBiome.splatMaxSize;
		splatMinSize = baseBiome.splatMinSize;
        island = setIsland;
    }

    public PointOfInterest RollPOI()
    {
        if (pois.Count == 0)
        {
            return null;
        }
        int roll = Mathf.FloorToInt(pois.Count * Random.value);
        PointOfInterest rolledPoi = pois[roll];
		pois.Remove(rolledPoi);
        return rolledPoi;
    }
}
