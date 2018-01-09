using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;
using VRTK.UnityEventHelper;
using VRTK;

public class Ship : MonoBehaviour
{

    public GameObject comet;
	public GameObject warpOut, warpIn, warpAway;
    public static Ship instance;
    public BoxCollider boxCollider;
    public bool playerOnShipTimer = false, playerOnShip = false, shipEntered = false, noWarpIn = false;
    public float timeOffShip = 0;
	public List<Transform> chestPlacements;
    public GameObject thisWayMarker;

	//ship parts controlled by level select script
	public VRTK_Wheel wheel;

    bool initialized = false;
    public void Init()
    {
        if (initialized) {
            return;
        }
        initialized = true;
        Ship.instance = this;
        comet.SetActive(false);
		timeOffShip = 0;
        if (!noWarpIn) {
			GameObject go = dgUtil.Instantiate (warpIn, Vector3.up * 5, Quaternion.identity, true, transform);
			Destroy(go, 6f);
        }
        if(CreateLevel.instance != null)
        {
            if (CreateLevel.instance.levelType == LevelType.Exit || CreateLevel.instance.levelType == LevelType.Timed)
            {
                comet.SetActive(true);
            }
		}
        LevelSelect.instance.ShipInit (this);
    }

	public void HideShip(){
		gameObject.SetActive(false);
	}

    public bool gone = false;
    public void ShipLeaves()
    {
		if (gone || transform.position == CreateLevel.instance.exitShipPosition)
        {
            return;
        }
        gone = true;
		CreateLevel.instance.MissionStart();
		GameObject go = dgUtil.Instantiate(warpAway, CreateLevel.instance.enterShipPosition + new Vector3(-5, 5, 0), Quaternion.identity);
		Destroy(go, 10f);
		GameObject go2 = dgUtil.Instantiate(warpAway, CreateLevel.instance.exitShipPosition + new Vector3(-5, 5, 0), Quaternion.identity);
        Destroy(go2, 10f);
		Invoke ("ShipReadyForExit", 2.5f);
    }
    public void ShipReadyForExit()
    {
        if(thisWayMarker != null)
        {
            Destroy(thisWayMarker);
        }
		Ship newShip = dgUtil.Instantiate (CreateLevel.instance.ship, CreateLevel.instance.exitShipPosition, Quaternion.identity).GetComponent<Ship>();
		newShip.noWarpIn = true;
        newShip.gone = true;
        newShip.Init ();
        if (CreateLevel.instance.levelType == LevelType.Exit || CreateLevel.instance.levelType == LevelType.Timed)
		{
			newShip.comet.SetActive(true);
        }
        newShip.gone = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerOnShipTimer)
        {
            timeOffShip += Time.deltaTime;
            if (timeOffShip > 2f)
            {
                playerOnShip = false;
                if (CreateLevel.instance != null && !gone)
                {
                    ShipLeaves();
                }
                playerOnShipTimer = false;
            }
        }
    }

    bool triggered = false;
    void OnTriggerStay(Collider col)
    {
        if (col.GetComponentInParent<PlayerManager>() != null && col.GetComponent<VRTK.VRTK_PlayerObject>() == null)
        {
			playerOnShip = true;
            playerOnShipTimer = false;
            if (GameManager.GetScene() == "islandgen" || GameManager.GetScene() == "Tutorial")
            {
                if (comet.GetActive() && !triggered)
				{
                    if(GameManager.GetScene() == "islandgen")
                    {
                        PlayerManager.instance.CompleteLevel();
                    }
                    else if(GameManager.GetScene() == "Tutorial")
                    {
                        PlayerPrefs.SetInt("tutorial", 1);
                        PlayerManager.instance.TrackEvent("tutorial_complete");
                    }
					triggered = true;
					GameObject go = dgUtil.Instantiate(warpOut, Vector3.up * 5, Quaternion.identity, true, transform);
                    GameManager.instance.LoadMainMenu(2);
                }
            }
            shipEntered = true;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.GetComponentInParent<PlayerManager>() && col.GetComponent<VRTK.VRTK_PlayerObject>() == null)
        {
            if (playerOnShipTimer == false)
            {
                playerOnShipTimer = true;
                timeOffShip = 0;
            }
        }
    }

	public void WarpOut(){
		GameObject go = dgUtil.Instantiate(warpOut, Vector3.up * 5, Quaternion.identity, true, transform);
	}
}
