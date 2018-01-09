using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

	public List<LevelSetType> levelSettings;
	public Level setLevel, test, custom;
	public static LevelManager instance;
	int currentLevel;

	//other screens
	public Level infinite;

	void Awake(){
		LevelManager.instance = this;
		currentLevel = PlayerPrefs.GetInt ("currentLevel");
		float levelToShow = Mathf.Min (currentLevel, levelSettings.Count);
		setLevel = gameObject.AddComponent<Level>();
		setLevel.sizeMax = 60;
		setLevel.sizeMin = 30;
	}

	public Level GetLevel(int levelNum){
		setLevel.Init (levelSettings [levelNum - 1]);
		return setLevel;
	}
}
