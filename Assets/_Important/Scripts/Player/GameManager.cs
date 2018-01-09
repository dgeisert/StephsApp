using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon;

public class GameManager : Photon.MonoBehaviour
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
        initialized = true;
        EnemyChecks = new List<Action>();
        resources = new Dictionary<string, GameObject>();
        GameManager.instance = this;
        if (PlayerManager.instance == null)
        {
            PlayerManager pm = dgUtil.Instantiate(player, transform.position, transform.rotation, false, null).GetComponent<PlayerManager>();
            //pm.Init();
        }
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
        if (SceneManager.GetActiveScene().name == "islandgen")
        {
            switch (CreateLevel.instance.levelType)
            {
                case LevelType.Survive:
                    if (PlayerManager.instance.records.ContainsKey(CreateLevel.instance.level))
                    {
                        if (PlayerManager.instance.records[CreateLevel.instance.level] < CreateLevel.instance.enemiesKilled)
                        {
                            PlayerManager.instance.records[CreateLevel.instance.level] = CreateLevel.instance.enemiesKilled;
                        }
                    }
                    else
                    {
                        PlayerManager.instance.records[CreateLevel.instance.level] = CreateLevel.instance.enemiesKilled;
                    }
                    break;
                default:
                    if (PlayerManager.instance.records.ContainsKey(CreateLevel.instance.level))
                    {
                        if (PlayerManager.instance.records[CreateLevel.instance.level] > CreateLevel.instance.timeInLevel)
                        {
                            PlayerManager.instance.records[CreateLevel.instance.level] = CreateLevel.instance.timeInLevel;
                        }
                    }
                    else
                    {
                        PlayerManager.instance.records[CreateLevel.instance.level] = CreateLevel.instance.timeInLevel;
                    }
                    break;
            }
            PlayerManager.instance.ImmediateSave();
        }
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
        PhotonNetwork.LoadLevel(levelToLoad);
    }

    public GameObject CreateEnemyProjectile(string pName, Vector3 pPosition, Quaternion pRotation, Color col, float pSpeed, bool pGravity, BaseEnemy enemy)
    {
        Quaternion colTemp = new Quaternion(col.r, col.g, col.b, col.a);
        if (enemy.isDead || (!NetworkManager.instance.singlePlayer && !PhotonNetwork.isMasterClient))
        {
            return null;
        }
        if (PhotonNetwork.connected)
        {
            enemy.GetComponent<PhotonView>().RPC("CreateProjectile", PhotonTargets.Others, pName, pPosition, pRotation, colTemp, pSpeed, pGravity, false);
        }
        return CreateEnemyProjectile(pName, pPosition, pRotation, colTemp, pSpeed, pGravity, true);
    }

    public GameObject CreateEnemyProjectile(string pName, Vector3 pPosition, Quaternion pRotation, Quaternion colTemp, float pSpeed, bool pGravity, bool isMine = false)
    {
        GameObject bulletClone = dgUtil.Instantiate(GetResourcePrefab(pName), pPosition, pRotation, false, null, false) as GameObject;
        Rigidbody rb = bulletClone.GetComponent<Rigidbody>();
        rb.AddForce(-bulletClone.transform.forward * pSpeed);
        bulletClone.GetComponent<Bullet>().isMine = isMine;
        bulletClone.GetComponent<Bullet>().SetMaterial(colTemp);
        return bulletClone;
    }
}
