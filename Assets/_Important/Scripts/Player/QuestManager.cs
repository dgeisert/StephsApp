using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

	public static QuestManager instance;

	public List<Quest> quests;
	public GameObject baseQuest;

	public List<Quest> activeQuests;

	public List<string> completedQuests = new List<string>();

	public void Init(QuestList ql){
		quests = new List<Quest> ();
		foreach (Quest q in ql.quests) {
			quests.Add (q);
		}
		instance = this;
	}

	public void CheckQuests(){
		for (int j = activeQuests.Count - 1; j >= 0; j--) {
			bool completed = true;
			for(int i = 0; i < activeQuests[j].TaskResource.Length; i++) {
				if(activeQuests[j].TaskResource[i] == Resource.ConstructedBuildings){
					int bCount = 0;
					foreach(Building b in ResourceManager.instance.constructedBuildings){
						bCount += activeQuests [j].TaskBuilding [i] == b.name ? 1 : 0;
					}
					if (activeQuests [j].TaskResourceRequirements [i] > bCount) {
						completed = false;
						break;
					}
				}
				else if(activeQuests[j].TaskResource[i] == Resource.ConstructedDeco){
					int bCount = 0;
					foreach(Building b in ResourceManager.instance.constructedBuildings){
						bCount ++;
					}
					if (activeQuests [j].TaskResourceRequirements [i] > bCount) {
						completed = false;
						break;
					}
				}
				else if (activeQuests[j].TaskResource[i] == Resource.ExploredTiles) {
					if (activeQuests [j].TaskResourceRequirements [i] > GameManager.saveData.Count) {
						completed = false;
						break;
					}
				}
				else if (!ResourceManager.instance.resourceCounts.ContainsKey (activeQuests[j].TaskResource[i])) {
					completed = false;
					break;
				}
				else if (ResourceManager.instance.resourceCounts [activeQuests[j].TaskResource[i]] < activeQuests[j].TaskResourceRequirements[i]) {
					completed = false;
					break;
				}
			}
			if (completed) {
				/*  commenting out the section that removes resources, may use it but probably not
				for(int i = 0; i < activeQuests[j].TaskResource.Length; i++) {
					ResourceManager.instance.RemoveResources(activeQuests[j].TaskResource [i],  activeQuests[j].TaskResourceRequirements [i], false);
				}
				*/
				MenuManager.instance.QuestRewardDialogOpen (activeQuests[j]);
				if (activeQuests [j].levelUp) {
					GameManager.instance.level++;
				}
				ResourceManager.instance.AddResource (activeQuests [j].rewardResource, activeQuests [j].reward, null, true, false);
				completedQuests.Add (activeQuests[j].QuestTitle);
				activeQuests.Remove (activeQuests[j]);
			}
		}
		foreach (Quest q in quests) {
			if (activeQuests.Count >= 3) {
				break;
			}
			if (!completedQuests.Contains (q.QuestTitle) && !activeQuests.Contains(q)) {
				if (q.level <= GameManager.instance.level) {
					activeQuests.Add (q);
					if (GameManager.instance.tutorialComplete == 0) {
						GameManager.instance.tutorialComplete++;
						MenuManager.instance.IntroQuest(activeQuests[0]);
					}
					else if (Time.time > 1) {
						MenuManager.instance.IntroQuest (q);
					}
				}
			}
		}
	}

}
