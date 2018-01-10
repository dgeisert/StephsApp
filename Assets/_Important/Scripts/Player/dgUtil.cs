using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using System.IO;

public class dgUtil
{

	public static string FormatTime(float seconds){
		return (seconds >= 3600 ? (Mathf.FloorToInt (seconds / 3600).ToString () 
			+ ":" ) : "")
			+ ((seconds % 3600) >= 600 ? "": "0")
			+ Mathf.FloorToInt((seconds % 3600) / 60)
			+ ":"
			+ ((seconds % 60) >= 10 ? "": "0")
			+ Mathf.FloorToInt(seconds % 60);
	}
    public static string FormatNum(float value, float decimals = 1)
    {
        float factor = Mathf.Pow(10, decimals);
        return (Mathf.Round(value * factor) / factor).ToString();
    }

	private static GameObject InstantiateLocalOnParent(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool world_local, object[] data = null)
    {
		GameObject go = (GameObject)GameObject.Instantiate(prefab, position, rotation);
		go.transform.SetParent(parent);
		if (world_local) 
		{
			go.transform.localPosition = position;
			go.transform.localRotation = rotation;
		} 
		else 
		{
			go.transform.position = position;
			go.transform.rotation = rotation;
		}
        go.name = prefab.name;
		if (data != null) {
			if (data.Length > 0) {
				go.SendMessage ("SetData", data [0].ToString(), SendMessageOptions.DontRequireReceiver);
			}
		}
        return go;
    }

	public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool world_local = true, Transform parent = null, bool network_local = true, object[] data = null)
	{
		if(network_local)
		{
			return InstantiateLocalOnParent (prefab, position, rotation, parent, world_local, data);
		}
		else
		{
			if (PhotonNetwork.connected) 
			{
				return PhotonNetwork.Instantiate (prefab.name, position, rotation, 0, data);
			} 
			else 
			{
				return InstantiateLocalOnParent (prefab, position, rotation, parent, world_local, data);
			}
		}
	}

    public static void GhostMode(GameObject go)
    {
        Material ghostModeMaterial = Resources.Load<Material>("GhostModeMaterial");
        foreach (MeshRenderer component in go.GetComponentsInChildren<MeshRenderer>())
        {
            List<Material> mats = new List<Material>();
            foreach(Material mat in component.materials)
            {
                mats.Add(ghostModeMaterial);
            }
            component.materials = mats.ToArray();
            component.UpdateGIMaterials();
        }
    }

    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int k = Mathf.FloorToInt(Random.value * list.Count); ;
            T value = list[k];
            list[k] = list[i];
            list[i] = value;
        }
    }

	static bool enemiesSet = false;
	static List<string> enemyList = new List<string> ();
	public static Dictionary<string, int> enemyValues = new Dictionary<string, int>();
	public static string RandomEnemy(){
		if(!enemiesSet){
			enemyValues.Add ("spriteRed", 0);
			enemyValues.Add ("golemRed", 2);
			enemyValues.Add ("fireMageRed", 5);
			enemyValues.Add ("eyeLaserRed", 4);
			enemyValues.Add ("bombRed", 1);
			enemyValues.Add ("bullRed", 4);
			enemyValues.Add ("spriteYellow", 3);
			enemyValues.Add ("golemYellow", 6);
			enemyValues.Add ("fireMageYellow", 8);
			enemyValues.Add ("eyeLaserYellow", 8);
			enemyValues.Add ("bombYellow", 4);
			enemyValues.Add ("bullYellow", 7);
			enemyValues.Add ("spriteOrange", 3);
			enemyValues.Add ("golemOrange", 8);
			enemyValues.Add ("fireMageOrange", 7);
			enemyValues.Add ("eyeLaserOrange", 7);
			enemyValues.Add ("bombOrange", 3);
			enemyValues.Add ("bullOrange", 9);
			enemyValues.Add ("spritePurple", 6);
			enemyValues.Add ("golemPurple", 12);
			enemyValues.Add ("fireMagePurple", 15);
			enemyValues.Add ("eyeLaserPurple", 12);
			enemyValues.Add ("bombPurple", 5);
			enemyValues.Add ("bullPurple", 14);
			enemyValues.Add ("spriteWhite", 9);
			enemyValues.Add ("golemWhite", 16);
			enemyValues.Add ("fireMageWhite", 22);
			enemyValues.Add ("eyeLaserWhite", 18);
			enemyValues.Add ("bombWhite", 9);
			enemyValues.Add ("bullWhite", 18);
			foreach (KeyValuePair<string, int> kvp in enemyValues) {
				enemyList.Add (kvp.Key);
			}
			enemiesSet = true;
		}
		int playingLevel = PlayerPrefs.GetInt ("playingLevel");
		if (CreateLevel.instance != null) {
			if (CreateLevel.instance.badWordSpawn > 0) {
				return CreateLevel.instance.badWordEnemy.name;
			}
		}
		if (playingLevel <= 0) {
			return "spriteRed";
		}
		string selectEnemy = "";
		while (selectEnemy == "") {
			string selection = enemyList[Mathf.FloorToInt(Random.value * enemyList.Count)];
			if (enemyValues [selection] <= playingLevel / 2) {
				selectEnemy = selection;
			}
		}
		return selectEnemy;
	}

    // PAST HERE IS THE ADMIN SECTION
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public static void ResetUser()
    {
	}

	public static void GoToScene(string scene)
	{
		SceneManager.LoadScene (scene);
	}
	public static void GoToMainMenu()
	{
        GameManager.instance.LoadMainMenu();
	}
		
	public static void GrantResource(string resource, int amount)
	{
	}
	public static void Grant1000Gems()
	{
		GrantResource ("gems", 1000);
	}
	public static void Grant1000Coins()
	{
		GrantResource ("coins", 1000);
    }

    public static void KillAllEnemies()
    {
    }

    public static void DestroyDestructibles()
    {
    }

    public static void Revive()
	{
	}

	public static void Die()
	{
	}

	public static void Heal()
	{
	}

	public static void GrantAllWeapons()
    {
    }

	public static void GrantAllChests()
	{

	}

	public static List<string> testData = new List<string> ();
	public static int testCount = 0;
	/*
	public static void SaveTestData(List<string> saveData){
		StreamWriter outStream = System.IO.File.CreateText(Application.dataPath +"TestData.csv");
		foreach (string str in saveData) {
			outStream.WriteLine (str);
		}
		outStream.Close();
	}*/

    public static void GrantLevels() {
        PlayerPrefs.SetInt("currentLevel", 100);
    }
#else
	public static List<string> testData = new List<string> ();
	public static int testCount = 0;
	public static void SaveTestData(List<string> testData){}
	public static void ResetUser(){}
	public static void GrantLevels(){}
	public static void GoToScene(string scene){}
	public static void GoToMainMenu()
	{
        GameManager.instance.LoadMainMenu();
	}
	public static void Grant1000Coins(){}
	public static void Grant1000Gems(){}
	public static void GrantResource(string resource, int amount){}
	public static void KillAllEnemies(){}
    public static void DestroyDestructibles(){}
    public static void Revive()
	{
		PlayerManager.instance.otherPlayerObject.Revive ();
	}
	public static void Die(){}
	public static void Heal(){}
	public static void GrantAllWeapons(){}
	public static void GrantAllChests(){}
#endif
}
