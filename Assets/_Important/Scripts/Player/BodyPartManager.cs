using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartManager : MonoBehaviour {

	[System.Serializable]
	public class BodyPartCount{
		[SerializeField] public BodyPartType type;
		[SerializeField] public int count;
	}

	[SerializeField] public List<BodyPartCount> assignBodyPartCounts = new List<BodyPartCount>();
	public Dictionary<BodyPartType, int> bodyPartCounts = new Dictionary<BodyPartType, int>();

	public static BodyPartManager instance;

	void Awake(){
		BodyPartManager.instance = this;
		foreach (BodyPartCount wpc in assignBodyPartCounts) {
			bodyPartCounts.Add (wpc.type, wpc.count);
		}

	}
}
