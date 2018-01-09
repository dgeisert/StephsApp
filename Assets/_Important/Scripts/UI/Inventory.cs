using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using Photon;
using UnityEngine.SceneManagement;

public class Inventory : UnityEngine.MonoBehaviour
{

    public static Inventory instance;
    bool initialized = false;
    public GameObject itemSlotPrefab;
    public ItemSlot selectedSlot;
    ItemSlot rightShoulder, leftShoulder, rightHip, leftHip, rightHand, leftHand, head, body;
    public Transform rightShoulderPosition, leftShoulderPosition, rightHipPosition, leftHipPosition, rightHandPosition, leftHandPosition, headPosition, bodyPosition;
    public Text selectionPageTitle;
    public Camera interactionCamera;
	public Button nextPage, previousPage, back;
	int currentPage = 0;
	List<string> currentItemList = new List<string> ();
    Canvas canvas;

    public GameObject selectionPage, inventoryPage;

    public void Init()
    {
        if (initialized)
        {
            return;
        }
        initialized = true;
        Inventory.instance = this;
        interactionCamera = PlayerManager.instance.eyeCamera;
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = interactionCamera;

        SetItemSlots();
        SetupSelectionArea();

        SetTab("inventory");

		back.onClick.AddListener(Back);
		nextPage.onClick.AddListener(NextPage);
		previousPage.onClick.AddListener(PreviousPage);
    }

	void NextPage(){
		PopulateSelectionArea (currentItemList, currentPage + 1);
	}
	void PreviousPage(){
		PopulateSelectionArea (currentItemList, currentPage - 1);
	}
	void Back(){
		SetTab ("inventory");
	}

    void SetItemSlots()
    {
        rightShoulder = dgUtil.Instantiate(itemSlotPrefab, itemSlotPrefab.transform.position, itemSlotPrefab.transform.rotation, true, rightShoulderPosition).GetComponent<ItemSlot>();
        leftShoulder = dgUtil.Instantiate(itemSlotPrefab, itemSlotPrefab.transform.position, itemSlotPrefab.transform.rotation, true, leftShoulderPosition).GetComponent<ItemSlot>();
        rightHip = dgUtil.Instantiate(itemSlotPrefab, itemSlotPrefab.transform.position, itemSlotPrefab.transform.rotation, true, rightHipPosition).GetComponent<ItemSlot>();
        leftHip = dgUtil.Instantiate(itemSlotPrefab, itemSlotPrefab.transform.position, itemSlotPrefab.transform.rotation, true, leftHipPosition).GetComponent<ItemSlot>();
        rightHand = dgUtil.Instantiate(itemSlotPrefab, itemSlotPrefab.transform.position, itemSlotPrefab.transform.rotation, true, rightHandPosition).GetComponent<ItemSlot>();
        leftHand = dgUtil.Instantiate(itemSlotPrefab, itemSlotPrefab.transform.position, itemSlotPrefab.transform.rotation, true, leftHandPosition).GetComponent<ItemSlot>();
        head = dgUtil.Instantiate(itemSlotPrefab, itemSlotPrefab.transform.position, itemSlotPrefab.transform.rotation, true, headPosition).GetComponent<ItemSlot>();
        body = dgUtil.Instantiate(itemSlotPrefab, itemSlotPrefab.transform.position, itemSlotPrefab.transform.rotation, true, bodyPosition).GetComponent<ItemSlot>();
        rightShoulder.Init(ItemSlotType.Weapon, PlayerManager.instance.weaponSettings[2], PlayerManager.instance.rightShoulder.gameObject, "Right Shoulder");
        leftShoulder.Init(ItemSlotType.Weapon, PlayerManager.instance.weaponSettings[3], PlayerManager.instance.leftShoulder.gameObject, "Left Shoulder");
        rightHip.Init(ItemSlotType.Weapon, PlayerManager.instance.weaponSettings[0], PlayerManager.instance.rightHip.gameObject, "Right Hip");
        leftHip.Init(ItemSlotType.Weapon, PlayerManager.instance.weaponSettings[1], PlayerManager.instance.leftHip.gameObject, "Left Hip");
        rightHand.Init(ItemSlotType.Hand, PlayerManager.instance.bodyParts[2], PlayerManager.instance.bodyPartHandRight.gameObject, "Right Hand");
        leftHand.Init(ItemSlotType.Hand, PlayerManager.instance.bodyParts[3], PlayerManager.instance.bodyPartHandLeft.gameObject, "Left Hand");
        head.Init(ItemSlotType.Head, PlayerManager.instance.bodyParts[0], PlayerManager.instance.bodyPartHead.gameObject, "Head");
        body.Init(ItemSlotType.Body, PlayerManager.instance.bodyParts[1], PlayerManager.instance.bodyPartBody.gameObject, "Body");
    }

    public void SetWeaponSlots()
    {
        rightShoulder.Change(PlayerManager.instance.rightShoulder.specialHold.name);
        leftShoulder.Change(PlayerManager.instance.leftShoulder.specialHold.name);
        rightHip.Change(PlayerManager.instance.rightHip.specialHold.name);
        leftHip.Change(PlayerManager.instance.leftHip.specialHold.name);
    }

    public void SetBodySlots()
    {
        rightHand.Change(PlayerManager.instance.bodyPartHandRight.name);
        leftHand.Change(PlayerManager.instance.bodyPartHandLeft.name);
        head.Change(PlayerManager.instance.bodyPartHead.name);
        body.Change(PlayerManager.instance.bodyPartBody.name);
    }

    GameObject[] selectionSlots = new GameObject[25];
    void SetupSelectionArea()
    {
        for (float i = 0; i < 16; i++)
        {
            float x = -90 + (i % 4) * 60;
            float y = 80 - Mathf.Floor(i / 4) * 60;
            GameObject go = dgUtil.Instantiate(itemSlotPrefab, new Vector3(x, y, 0), Quaternion.identity, true, selectionPage.transform);
            selectionSlots[(int)i] = go;
        }
    }

    public void PopulateSelectionArea(List<string> selectables, int page = 0)
    {
		if (page == 0) {
			previousPage.gameObject.SetActive (false);
		} else {
			previousPage.gameObject.SetActive (true);
		}
		nextPage.gameObject.SetActive (true);
		currentItemList = selectables;
		currentPage = page;
        SetTab("selection");
		int k = 0;
		for(int i = 0 + page * 16; i < 16 + page * 16; i++)
        {
			if (i >= selectables.Count) {
				break;
			}
			GameObject itemSlot = selectionSlots[i - page * 16];
            itemSlot.SetActive(true);
			itemSlot.GetComponent<ItemSlot>().Init(ItemSlotType.Selection, selectables[i]);
			k++;
			if (i + 1 >= selectables.Count) {
				nextPage.gameObject.SetActive (false);
			}
        }
        for (int j = k; j < 16; j++)
        {
            selectionSlots[j].gameObject.SetActive(false);
        }
        selectionPageTitle.text = selectedSlot.type.ToString() + " type for " + selectedSlot.label.text;
    }

    public void SetTab(string tab)
    {
        selectionPage.SetActive(false);
        inventoryPage.SetActive(false);
        switch (tab)
        {
            case "inventory":
                inventoryPage.SetActive(true);
                break;
            case "selection":
                selectionPage.SetActive(true);
                break;
            default:
                break;
        }
    }
}
