using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestCard : MonoBehaviour {

	public bool initialized;

	public Text Title;
	public Image[] TaskResourceIcon;
	public Text[] TaskResourceText;
	public Text[] TaskTitleText;

	public int[] TaskResourceRequirements;
	public string[] TaskTitles;
	public string QuestTitle, IntroText, RewardText;
	public Resource[] TaskResource;
	public Building[] TaskBuilding;
	public int level, reward = 10;
	public Resource rewardResource = Resource.Gold;
	public bool levelUp = true;

	public void Init(Quest q){
		QuestTitle = q.QuestTitle;
		TaskResource = q.TaskResource;
		reward = q.reward;
		level = q.level;
		TaskResourceRequirements = q.TaskResourceRequirements;
		TaskBuilding = new Building[q.TaskBuilding.Length];
		rewardResource = q.rewardResource;
		levelUp = q.levelUp;
		Title.text = QuestTitle;
		TaskTitles = new string[3];
		for(int i = 0; i < TaskResourceText.Length; i++) {
			if (q.TaskBuilding [i] != "") {
				TaskBuilding [i] = GameManager.instance.GetResourcePrefab (q.TaskBuilding [i]).GetComponent<Building> ();
			}
			if (TaskTitles.Length > i) {
				TaskResourceIcon [i].sprite = ResourceManager.instance.resourceSprites [TaskResource [i]];
				if (!ResourceManager.instance.resourceCounts.ContainsKey (TaskResource [i])) {
					ResourceManager.instance.resourceCounts.Add (TaskResource [i], 0);
				}
				string numerator = "";
				if (TaskResource [i] == Resource.ConstructedBuildings) {
					if (TaskResourceRequirements [i] > 1) {
						TaskTitles [i] = "Build " + TaskBuilding [i].plural;
					} else {
						TaskTitles [i] = "Build " + TaskBuilding [i].singular;
					}
					int bCount = 0;
					foreach (Building b in ResourceManager.instance.constructedBuildings) {
						bCount += TaskBuilding [i].name == b.name ? 1 : 0;
					}
					numerator = bCount.ToString();
				} else if (TaskResource [i] == Resource.ExploredTiles) {
					TaskTitles [i] = "Explore new land";
					numerator = GameManager.saveData.Count.ToString();
				} else if (TaskResource [i] == Resource.ConstructedDeco) {
					TaskTitles [i] = "Build Decorations";
					numerator = ResourceManager.instance.constructedBuildings.Count.ToString();
				} else {
					if (TaskResourceRequirements [i] > 1) {
						TaskTitles [i] = "Gather " + TaskResource [i].ToString();
					} else {
						TaskTitles [i] = "Gather " + TaskResource [i].ToString();
					}
					numerator = ResourceManager.instance.resourceCounts [TaskResource [i]].ToString ();
				}
				TaskResourceText [i].text = numerator
					+ "/" + TaskResourceRequirements [i].ToString ();
				TaskTitleText [i].text = TaskTitles [i];
			} else {
				TaskResourceIcon [i].gameObject.SetActive(false);
				TaskResourceText [i].gameObject.SetActive(false);
				TaskTitleText [i].gameObject.SetActive(false);
			}
		}
	}
}
