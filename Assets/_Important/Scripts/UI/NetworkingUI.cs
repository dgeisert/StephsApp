using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using Photon;
using UnityEngine.SceneManagement;

public class NetworkingUI : UnityEngine.MonoBehaviour {

	public static NetworkingUI instance;
	bool initialized = false;
	public Button joinButton, startButton;
	public Text joiningText;
	public bool disableStart = false;
	public Camera interactionCamera;
	Canvas canvas;

	public void Start(){
		Init ();
	}
	public void Init()
	{
		if (initialized) {
			return;
		}
		initialized = true;
		NetworkingUI.instance = this;
		interactionCamera = PlayerManager.instance.eyeCamera;
		canvas = GetComponent<Canvas> ();
		canvas.worldCamera = interactionCamera;

		//Networking Button Clicks
		joinButton.onClick.AddListener(JoinButton);
		startButton.onClick.AddListener(StartButton);
	}

	void Update(){
		//manage network buttons
		if (Input.GetKey(KeyCode.S))
		{
			NetworkManager.instance.Connect();
		}
		if (PhotonNetwork.inRoom || NetworkManager.instance.singlePlayer)
		{
			joinButton.gameObject.SetActive(false);
			joiningText.gameObject.SetActive(false);
		}
		else if (PhotonNetwork.connecting)
		{
			joinButton.gameObject.SetActive(false);
			joiningText.gameObject.SetActive(true);
		}
		else
		{
			joiningText.gameObject.SetActive(false);
			joinButton.gameObject.SetActive(true);
		}
		if (SceneManager.GetActiveScene().name == "MainMenu" && (PhotonNetwork.connected || NetworkManager.instance.singlePlayer) && !disableStart)
		{
			startButton.gameObject.SetActive(true);
		}
		else
		{
			startButton.gameObject.SetActive(false);
		}
	}

	public void JoinButton()
	{
		if (Tutorial.instance != null)
		{
			if (Tutorial.instance.currentStep == "UITrigger")
			{
				Tutorial.instance.NextTutorialStep();
			}
		}
		NetworkManager.instance.Connect();
	}

	public void StartButton()
	{
		
	}
}
