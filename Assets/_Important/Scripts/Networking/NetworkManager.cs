using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : Photon.PunBehaviour {

	string version = "1.0";
	public float delay;
	public static NetworkManager instance;
    public bool singlePlayer = false;
    public string LoadToLevelOverride = "";

    void Awake()
    {
		NetworkManager.instance = this;
        singlePlayer = true;
		DontDestroyOnLoad (gameObject);
        // #Critical
        // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
        PhotonNetwork.autoJoinLobby = false;

        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

		if (SceneManager.GetActiveScene ().name != "MainMenu" && SceneManager.GetActiveScene().name != "Loading") 
		{
			Connect ();
		}
		Invoke ("DoLoad", delay);
    }

	public void DoLoad(){
		if(SceneManager.GetActiveScene().name == "Loading")
		{
			if(LoadToLevelOverride != "")
			{
				SceneManager.LoadSceneAsync(LoadToLevelOverride);
			}
			else if(PlayerPrefs.GetInt("tutorial") == 0)
			{
				SceneManager.LoadSceneAsync("Tutorial");
			}
			else
			{
				SceneManager.LoadSceneAsync("MainMenu");
			}
		}
	}

	public void Connect()
	{
		// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
		if (PhotonNetwork.connected)
		{
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            //PhotonNetwork.JoinRandomRoom();
            //PhotonNetwork.JoinRandomRoom(null, (byte)(2));
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 }, null);
        }
		else
		{
			Debug.Log("Connecting to Photon");

			// #Critical, we must first and foremost connect to Photon Online Server.
			PhotonNetwork.ConnectUsingSettings(version);
		}
	}

    #region Photon.PunBehaviour CallBacks
    // below, we implement some callbacks of PUN
    // you can find PUN's callbacks in the class PunBehaviour or in enum PhotonNetworkingMessage


    /// <summary>
    /// Called after the connection to the master is established and authenticated but only when PhotonNetwork.autoJoinLobby is false.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon master");
        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
    /// </summary>
    /// <remarks>
    /// Most likely all rooms are full or no rooms are available. <br/>
    /// </remarks>
    /// <param name="codeAndMsg">codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.</param>
    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("Random Photon Join Failed");
        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 }, null);
    }


    /// <summary>
    /// Called after disconnecting from the Photon server.
    /// </summary>
    /// <remarks>
    /// In some cases, other callbacks are called before OnDisconnectedFromPhoton is called.
    /// Examples: OnConnectionFail() and OnFailedToConnectToPhoton().
    /// </remarks>
    public override void OnDisconnectedFromPhoton()
    {

        // #Critical: we failed to connect or got disconnected. There is not much we can do. Typically, a UI system should be in place to let the user attemp to connect again.
        Debug.LogError("Disconnected from photon");

    }

    /// <summary>
    /// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
    /// </summary>
    /// <remarks>
    /// This method is commonly used to instantiate player characters.
    /// If a match has to be started "actively", you can call an [PunRPC](@ref PhotonView.RPC) triggered by a user's button-press or a timer.
    ///
    /// When this is called, you can usually already access the existing players in the room via PhotonNetwork.playerList.
    /// Also, all custom properties should be already available as Room.customProperties. Check Room..PlayerCount to find out if
    /// enough players are in the room to start playing.
    /// </remarks>
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room in Photon");
		PlayerManager.instance.SpawnPlayerObject ();
        if(Tutorial.instance != null)
        {
            if (Tutorial.instance.currentStep == "Joining")
            {
                Tutorial.instance.NextTutorialStep();
            }
        }
    }

    #endregion




}
