using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour {

	[System.Serializable]
	public class WeaponPartCount{
		[SerializeField] public WeaponPartType type;
		[SerializeField] public int count;
	}

	[SerializeField] public List<WeaponPartCount> assignWeaponPartCounts = new List<WeaponPartCount>();
	public Dictionary<WeaponPartType, int> weaponPartCounts = new Dictionary<WeaponPartType, int>();

	public static WeaponManager instance;

	void Awake(){
		WeaponManager.instance = this;
		foreach (WeaponPartCount wpc in assignWeaponPartCounts) {
			weaponPartCounts.Add (wpc.type, wpc.count);
		}

	}
}
