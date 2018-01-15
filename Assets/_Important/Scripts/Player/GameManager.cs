using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public GameObject player;
    public static GameManager instance;
    public List<Action> EnemyChecks;

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
		GameManager.instance = this;
		//dgUtil.Instantiate(player, transform.position, transform.rotation, false, null);
    }

	public void LoadMainMenu(){
		instance.LoadScene ("MainMenu");
	}

	public void LoadMainMenu(float delay = 0)
	{
		Invoke ("LoadMainMenu", delay);
	}

	public void LoadLevel(){
		instance.LoadScene ("islandgen");
	}

	public void LoadLevel(float delay = 0){
		Invoke ("LoadLevel", delay);
	}

	public void LoadTutorial(){
		instance.LoadScene ("Tutorial");
	}

	public void LoadTutorial(float delay = 0){
		Invoke ("LoadTutorial", delay);
	}

	public void LoadCredits(){
		instance.LoadScene ("Credits");
	}

	public void LoadCredits(float delay = 0){
		Invoke ("LoadCredits", delay);
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
}
