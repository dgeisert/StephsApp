using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public List<Action> EnemyChecks;
	public int level = 1;

    private Dictionary<string, GameObject> resources;
    public GameObject GetResourcePrefab(string loadResource)
    {
        if (resources.ContainsKey(loadResource))
        {
            return resources[loadResource];
        }
        else
        {
            GameObject go = (GameObject)Resources.Load(loadResource);
            if(go == null)
            {
                Debug.Log("no resource found for: " + loadResource);
            }
            resources.Add(go.name, go);
            return go;
        }
        return null;
    }

    private void Awake()
    {
        Init();
    }

    bool initialized = false;
	public MenuManager menuManager;
	public ResourceManager resourceManager;
	public TouchManager touchManager;
	public CreateLevel createLevel;
    public void Init()
	{
		if (GameManager.instance != null) {
			Destroy (gameObject);
			return;
		}
        if (initialized)
        {
            return;
		}
		Load ();
		GameManager.instance = this;
		if (createLevel != null) {
			createLevel.Init ();
		}
		if (resourceManager != null) {
			resourceManager.Init ();
		}
		if (touchManager != null) {
			touchManager.Init ();
		}
		if (menuManager != null) {
			menuManager.Init ();
		}
        initialized = true;
        EnemyChecks = new List<Action>();
        resources = new Dictionary<string, GameObject>();
		CreateLevel.instance.Load ();
		InvokeRepeating ("DoSave", 1, 1);
    }

    public static bool isLoading = false;
	public void LoadScene(string level)
    {
        if (isLoading)
        {
            return;
        }
        isLoading = true;
        instance.levelToLoad = level;
        instance.Invoke("DoSceneLoad", 0.5f);
    }

    public static string GetScene()
    {
        return SceneManager.GetActiveScene().name;
    }

    string levelToLoad = "MainMenu";
	void DoSceneLoad()
    {
        GameManager.isLoading = false;
        SceneManager.LoadScene(levelToLoad);
    }

	public static void AddSaveData(Vector2Int island, Vector2Int node, string data){
		if (saveData.ContainsKey (island)) {
			if (saveData [island].ContainsKey (node)) {
				saveData [island] [node] = data;
			} else {
				saveData [island].Add(node, data);
			}
		}else{
			saveData.Add(island, new Dictionary<Vector2Int, string>());
			saveData [island].Add(node, data);
		}
	}

	public static void AddVisibleLand(Vector2Int island){
		if (!saveData.ContainsKey (island)) {
			saveData.Add (island, new Dictionary<Vector2Int, string> ());
		}
	}

	public void DoSave(){
		GameManager.Save ();
	}
		
	public static Dictionary<Vector2Int, Dictionary<Vector2Int, string>> saveData = new Dictionary<Vector2Int, Dictionary<Vector2Int, string>>();
	public static void Save(){
		List<string> saveIslands = new List<string> ();
		foreach(KeyValuePair<Vector2Int, Dictionary<Vector2Int, string>> kvp in saveData){
			List<string> nodepoint = new List<string>();
			foreach (KeyValuePair<Vector2Int, string> kvp2 in kvp.Value) {
				nodepoint.Add (kvp2.Key.x + "*" + kvp2.Key.y + ";" + kvp2.Value);
			}
			ES2.Save (nodepoint, kvp.Key.x + "*" + kvp.Key.y);
			saveIslands.Add (kvp.Key.x + "*" + kvp.Key.y);
		}
		ES2.Save (saveIslands, "Islands");
	}
	public static void Load(){
		saveData = new Dictionary<Vector2Int, Dictionary<Vector2Int, string>> ();
		if (ES2.Exists ("Islands") && ES2.Load<int>("playerState") > 0) {
			List<string> saveIslands = ES2.LoadList<string> ("Islands");
			foreach (string str in saveIslands) {
				if (ES2.Exists (str)) {
					int x = int.Parse (str.Split ('*') [0]);
					int z = int.Parse (str.Split ('*') [1]);
					Dictionary<Vector2Int, string> islandDic = new Dictionary<Vector2Int, string> ();
					List<string> loadList = ES2.LoadList<string> (str);
					foreach (string str2 in loadList) {
						string kvp = str2.Split (';') [0];
						int x2 = int.Parse (kvp.Split ('*') [0]);
						int z2 = int.Parse (kvp.Split ('*') [1]);
						islandDic.Add (new Vector2Int (x2, z2), str2.Split (';') [1]);
					}
					saveData.Add (new Vector2Int (x, z), islandDic);
				}
			}
		} else {
			//set new player values
			AddVisibleLand (new Vector2Int(0, 0));
			AddVisibleLand (new Vector2Int(1, 0));
			AddVisibleLand (new Vector2Int(0, 1));
			AddVisibleLand (new Vector2Int(-1, 0));
			AddVisibleLand (new Vector2Int(0, -1));
			AddVisibleLand (new Vector2Int(1, 1));
			AddVisibleLand (new Vector2Int(-1, 1));
			AddVisibleLand (new Vector2Int(1, -1));
			AddVisibleLand (new Vector2Int(-1, -1));
			AddVisibleLand (new Vector2Int(2, 0));
			AddVisibleLand (new Vector2Int(0, 2));
			AddVisibleLand (new Vector2Int(-2, 0));
			AddVisibleLand (new Vector2Int(0, -2));
			ES2.Save (1, "playerState");
		}
	}

	public static void ClearData(){
		if(ES2.Exists("Islands")){
			List<string> saveIslands = ES2.LoadList<string> ("Islands");
			foreach (string str in saveIslands) {
				ES2.Delete (str);
			}
		}
		ES2.Delete ("Islands");
		ES2.Delete ("test");
		saveData = new Dictionary<Vector2Int, Dictionary<Vector2Int, string>> ();
		ES2.Save (0, "playerState");
	}

	void OnApplicationQuit(){
		GameManager.Save();
	}
}
