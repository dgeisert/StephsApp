using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{
	public int requiredLevel;
    public float percentTree = 2f, treeMinSize = 0.5f, treeMaxSize = 1.5f;
	public float percentSplat = 2f, splatMinSize = 0.5f, splatMaxSize = 1.5f;
	public List<ProceduralSplat> splats;
	public List<ProceduralSplat> trees;
    public Color colorTop, colorBottom;
	public float spaciness = 0.5f, perlinCrunch = 10f, perlinOffset = 0f, heightMod = 2f, centerBias = 1f, wiggle = 0.2f;
	public List<PointOfInterest> pois;
	public List<PointOfInterest> importantPois;
    public float poiSpacing = 15;
    public GameObject particles;
	public string biomeName;
    CreateIsland island;

    public void Init(float size, Biome baseBiome, CreateIsland setIsland)
    {
		biomeName = baseBiome.name;
        perlinCrunch = 5f / size;
        perlinOffset = Random.value * 1000;
        heightMod = size / 3f;
		centerBias = Mathf.Pow(size, 0.25f);
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
				if (poi.requiredLevel <= CreateLevel.instance.level) {
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
        spaciness = baseBiome.spaciness;
        wiggle = baseBiome.wiggle;
        colorTop = baseBiome.colorTop;
        colorBottom = baseBiome.colorBottom;
        percentTree = baseBiome.percentTree;
        treeMinSize = baseBiome.treeMinSize;
        treeMaxSize = baseBiome.treeMaxSize;
        percentSplat = baseBiome.percentSplat;
        splatMaxSize = baseBiome.splatMaxSize;
		splatMinSize = baseBiome.splatMinSize;
		particles = baseBiome.particles;
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
