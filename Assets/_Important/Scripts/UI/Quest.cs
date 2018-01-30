using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]public class Quest {

	public Resource[] TaskResource;
	public Building[] TaskBuilding;
	public int[] TaskResourceRequirements;
	public string QuestTitle, IntroText, RewardText;
	public int level, reward = 10;
	public Resource rewardResource = Resource.Gold;
	public bool levelUp = true;
}
