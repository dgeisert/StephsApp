using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttachPoint : MonoBehaviour {

	public WeaponPartType partType;
	public bool optional = false;
	public float rarity = 0f;
	public float scalingFactor = 1f;
    public WeaponAttachPoint matchingPoint;
	public List<WeaponAttachPoint> mutuallyExclusivePoints;
    public WeaponPart part;
}
