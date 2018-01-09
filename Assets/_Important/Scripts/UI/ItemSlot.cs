using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemSlotType
{
    Weapon,
    Head,
    Hand,
    Body,
	Selection
}

public class ItemSlot : MonoBehaviour
{

    public List<string> itemList = new List<string>();
    public GameObject uiItemPrefab;
    UIItem objectSet;
    public int objectsToShow = 5;
    public string setItem;
    int focus;
    public ItemSlotType type;
    public Vector3 outerScaling, outerSpacing;
    public Text label;
    public Button slotButton;
    public GameObject referenceObject;
    public Text infoText;
    public GameObject infoDialog;

    public void Init(ItemSlotType setType, string specificItem, GameObject setReferenceObject = null, string setLabel = "")
    {
        referenceObject = setReferenceObject;
        type = setType;
        label.text = setLabel;
        if (setLabel == "")
        {
            label.gameObject.SetActive(false);
        }
        switch (type)
        {
            case ItemSlotType.Weapon:
                itemList = PlayerManager.instance.weapons;
                slotButton.onClick.AddListener(SlotButtonClick);
                break;
            case ItemSlotType.Hand:
                itemList = PlayerManager.instance.hands;
                slotButton.onClick.AddListener(SlotButtonClick);
                break;
            case ItemSlotType.Head:
                itemList = PlayerManager.instance.heads;
                slotButton.onClick.AddListener(SlotButtonClick);
                break;
            case ItemSlotType.Body:
                itemList = PlayerManager.instance.bodies;
                slotButton.onClick.AddListener(SlotButtonClick);
                break;
            case ItemSlotType.Selection:
                slotButton.onClick.AddListener(SelectionButtonClick);
                break;
            default:
                break;
        }
        Change(specificItem);
    }

    public void Change(string specificItem)
    {
        if (Tutorial.instance != null)
        {
            if (Tutorial.instance.currentStep == "SetGun" && specificItem.Contains("Gun") && label.text == "Left Hip")
            {
                Tutorial.instance.NextTutorialStep();
            }
            if (Tutorial.instance.currentStep == "WeaponSetup")
            {
                if (PlayerManager.instance.rightHip.specialHold.name.Contains("Gun")
                    && PlayerManager.instance.leftHip.specialHold.name.Contains("Gun"))
                {
                    Tutorial.instance.NextTutorialStep();
                }
            }
        }
        foreach (UIItem uiItem in transform.GetComponentsInChildren<UIItem>())
        {
            Destroy(uiItem.gameObject);
        }
        GameObject go = dgUtil.Instantiate(uiItemPrefab, uiItemPrefab.transform.position, uiItemPrefab.transform.rotation, true, transform);
        objectSet = go.GetComponent<UIItem>();
        infoText.text = objectSet.Init(specificItem);
        setItem = specificItem;
        if(referenceObject != null)
        {
            Holster holster = referenceObject.GetComponent<Holster>();
            if (holster != null)
            {
                holster.ChangeItemSlot(setItem);
            }
            Head head = referenceObject.GetComponent<Head>();
            if(head != null)
            {
                head.ChangeItemSlot(setItem);
                referenceObject = PlayerManager.instance.bodyPartHead.gameObject;
            }
            Body body = referenceObject.GetComponent<Body>();
            if(body != null)
            {
                body.ChangeItemSlot(setItem);
                referenceObject = PlayerManager.instance.bodyPartBody.gameObject;
            }
            Hand hand = referenceObject.GetComponent<Hand>();
            if (hand != null)
            {
                if (hand.isLeft)
                {
                    hand.ChangeItemSlot(setItem);
                    referenceObject = PlayerManager.instance.bodyPartHandLeft.gameObject;
                }
                else
                {
                    hand.ChangeItemSlot(setItem);
                    referenceObject = PlayerManager.instance.bodyPartHandRight.gameObject;
                }
            }
        }
    }

    public void SlotButtonClick()
    {
        if (Tutorial.instance != null)
        {
            if (Tutorial.instance.currentStep == "OpenItemSelect" && label.text == "Left Hip")
            {
                Tutorial.instance.NextTutorialStep();
            }
        }
        Inventory.instance.selectedSlot = this;
        Inventory.instance.PopulateSelectionArea(itemList);
    }

    public void SelectionButtonClick()
    {
        Inventory.instance.selectedSlot.Change(setItem);
        Inventory.instance.SetTab("inventory");
    }

    public void ChangeSlotItem(string newItem)
    {
        objectSet.Init(newItem);
        setItem = newItem;
    }

    private void Update()
    {
        switch (type)
        {
            case ItemSlotType.Body:
                objectSet.transform.eulerAngles = new Vector3(0, Inventory.instance.transform.eulerAngles.y * 2 - PlayerManager.instance.head.eulerAngles.y, 0);
                break;
            case ItemSlotType.Head:
                objectSet.transform.eulerAngles = new Vector3(-PlayerManager.instance.head.eulerAngles.x, Inventory.instance.transform.eulerAngles.y * 2 - PlayerManager.instance.head.eulerAngles.y, PlayerManager.instance.head.eulerAngles.z);
                break;
            case ItemSlotType.Hand:
                if (label.text.ToLower().Contains("left"))
                {
                    objectSet.transform.eulerAngles = new Vector3(-PlayerManager.instance.left.eulerAngles.x, Inventory.instance.transform.eulerAngles.y * 2 - PlayerManager.instance.left.eulerAngles.y, PlayerManager.instance.left.eulerAngles.z);
                }
                else
                {
                    objectSet.transform.eulerAngles = new Vector3(-PlayerManager.instance.right.eulerAngles.x, Inventory.instance.transform.eulerAngles.y * 2 - PlayerManager.instance.right.eulerAngles.y, PlayerManager.instance.right.eulerAngles.z);
                }
                break;
            default:
                break;
        }
        if(PlayerManager.instance != null)
        {
            bool hover = false;
            if (PlayerManager.instance.leftManager.uiPointer.hoveringElement != null)
            {
                hover = (PlayerManager.instance.leftManager.uiPointer.hoveringElement == gameObject);
            }
            if (PlayerManager.instance.rightManager.uiPointer.hoveringElement != null)
            {
                hover = hover || (PlayerManager.instance.rightManager.uiPointer.hoveringElement == gameObject);
            }
            infoDialog.SetActive(hover);
        }
    }
}
