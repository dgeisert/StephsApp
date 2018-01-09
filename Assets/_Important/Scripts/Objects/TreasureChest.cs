using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using Holoville.HOTween;

public class TreasureChest : MonoBehaviour
{

    public LootTable lootTable;
    public Text costText;
    public int openCost = 1000;
    public Transform top, openTopPosition, claimParticles;
    public Vector3 weaponSpawnPosition = Vector3.zero;
    public GameObject displayItem;
    public bool isOpen = false;
    public int seed = 0;
    public AudioSource audioSource;
    public AudioClip open;
    string levelString = "0", baseString = "";
    public List<string> bases = new List<string>()
    {
        "AxeBase",
        "SwordBase",
        "BowBase",
        "GunBase",
        "DaggerBase",
        "StaffBase",
        "ShieldBase"
    };
    public bool instantGrant = false, display = false;

    void Start()
    {
		/*
		if (seed != 0 & GameManager.GetScene() == "islandgen") {
			if (PlayerManager.instance.claimedChests.Contains (seed) || PlayerManager.instance.chestsToClaim.Contains (seed)) {
				gameObject.SetActive (false);
				return;
			}
		}
        if (lootTable == null)
        {
            foreach (LootTable lt in GetComponents<LootTable>())
            {
                if (lt.tableName == "primary")
                {
                    lootTable = lt;
                }
            }
        }
        if (instantGrant)
        {
            seed = Mathf.FloorToInt(Random.value * 10000);
            AttemptOpen();
        }
        if (display)
        {
            if (CreateLevel.instance != null)
            {
                levelString = CreateLevel.instance.level.ToString();
            }
            else
            {
                levelString = PlayerPrefs.GetInt("currentLevel").ToString();
            }
            openCost = int.Parse(levelString) * 100;
            Random.InitState(seed);
			baseString = bases[Mathf.FloorToInt(Random.value * bases.Count)];
            displayItem = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(baseString), Vector3.zero, Quaternion.identity, true, transform);
            BaseWeapon bw = displayItem.GetComponent<BaseWeapon>();
            if (bw != null)
            {
                bw.seed = seed;
                bw.Init();
                if (bw.chestPositionScaling != null)
                {
                    bw.transform.localPosition = bw.chestPositionScaling.localPosition + weaponSpawnPosition;
                    bw.transform.localEulerAngles = bw.chestPositionScaling.localEulerAngles;
                    bw.transform.localScale = bw.chestPositionScaling.localScale;
                }
			}
			Head head = displayItem.GetComponent<Head>();
			if (head != null)
			{
				head.seed = seed;
				head.Init();
				if (head.chestPositionScaling != null)
				{
					head.transform.localPosition = head.chestPositionScaling.localPosition + weaponSpawnPosition;
					head.transform.localEulerAngles = head.chestPositionScaling.localEulerAngles;
					head.transform.localScale = head.chestPositionScaling.localScale;
				}
			}
			Body body = displayItem.GetComponent<Body>();
			if (body != null)
			{
				body.seed = seed;
				body.Init();
				if (body.chestPositionScaling != null)
				{
					body.transform.localPosition = body.chestPositionScaling.localPosition + weaponSpawnPosition;
					body.transform.localEulerAngles = body.chestPositionScaling.localEulerAngles;
					body.transform.localScale = body.chestPositionScaling.localScale;
				}
			}
			Hand hand = displayItem.GetComponent<Hand>();
			if (hand != null)
			{
				hand.seed = seed;
				hand.Init();
				if (hand.chestPositionScaling != null)
				{
					hand.transform.localPosition = hand.chestPositionScaling.localPosition + weaponSpawnPosition;
					hand.transform.localEulerAngles = hand.chestPositionScaling.localEulerAngles;
					hand.transform.localScale = hand.chestPositionScaling.localScale;
				}
			}
            costText.text = openCost.ToString() + " Gems\nHit to buy";
            dgUtil.Disable(displayItem);
        }
        else
        {
            if (openCost == 0)
            {
                costText.text = "Hit to Open";
                costText.color = Color.white;
            }
            else
            {
                costText.text = openCost.ToString() + " Gems to Open";
            }
            audioSource.clip = open;
		}
		gameObject.AddComponent<VRTK_InteractableObject>().InteractableObjectTouched += AttemptOpen;
		*/
    }

	public void AttemptOpen(object sender, InteractableObjectEventArgs e){
		AttemptOpen ();
	}

    public void AttemptOpen()
    {
        if (!isOpen)
        {
            if (PlayerManager.instance.SubtractResources("gems", openCost))
            {
                OpenChest();
            }
			if (Tutorial.instance != null) {
				if (Tutorial.instance.currentStep == "Chest") {
					Tutorial.instance.chestsOpened++;
				}
			}
        }
    }

    public void OpenChest()
    {
        costText.transform.parent.gameObject.SetActive(false);
        isOpen = true;
        if (display)
        {
            GrantDisplayedItem();
        }
        else
        {
            Random.InitState(seed);
            PlayerManager.instance.ClaimChest(seed);
            audioSource.Play();
            lootTable.RollTable();
            HOTween.To(top, 1, "localPosition", openTopPosition.localPosition, false, EaseType.EaseInOutQuad, 0);
            HOTween.To(top, 1, "localEulerAngles", new Vector3(-95, 0, 0), true, EaseType.EaseInOutQuad, 0);
        }
    }

    public void GrantDisplayedItem()
    {
        object[] data = new object[1];
		int weaponSeed = seed;
		PlayerManager.instance.ClaimChest (seed);
        data[0] = weaponSeed.ToString() + "." + levelString;
        GameObject weapon = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(baseString), Vector3.zero, Quaternion.identity, true, transform, data: data);
		LootDrop ld = weapon.AddComponent<LootDrop>();
		BaseWeapon bw = weapon.GetComponent<BaseWeapon>();
		if (bw != null)
		{
			ld.resource = "weapon";
			bw.seed = weaponSeed;
			bw.Init();
			if (bw.chestPositionScaling != null)
			{
				bw.transform.localScale = Vector3.one * 0.1f;
				ld.NextFrameSet(bw.chestPositionScaling.localPosition + weaponSpawnPosition
					, bw.chestPositionScaling.localEulerAngles
					, bw.chestPositionScaling.localScale);
            }
            PlayerManager.instance.AddWeapon(baseString + "." + seed.ToString() + "." + levelString);
        }
		Head head = weapon.GetComponent<Head>();
		if (head != null)
		{
			ld.resource = "head";
			head.seed = weaponSeed;
			head.Init();
			if (head.chestPositionScaling != null)
			{
				head.transform.localScale = Vector3.one * 0.1f;
				ld.NextFrameSet(head.chestPositionScaling.localPosition + weaponSpawnPosition
					, head.chestPositionScaling.localEulerAngles
					, head.chestPositionScaling.localScale);
            }
            PlayerManager.instance.AddHead(baseString + "." + seed.ToString() + "." + levelString);
        }
		Body body = weapon.GetComponent<Body>();
		if (body != null)
		{
			ld.resource = "body";
			body.seed = weaponSeed;
			body.Init();
			if (body.chestPositionScaling != null)
			{
				body.transform.localScale = Vector3.one * 0.1f;
				ld.NextFrameSet(body.chestPositionScaling.localPosition + weaponSpawnPosition
					, body.chestPositionScaling.localEulerAngles
					, body.chestPositionScaling.localScale);
            }
            PlayerManager.instance.AddBody(baseString + "." + seed.ToString() + "." + levelString);
        }
		Hand hand = weapon.GetComponent<Hand>();
		if (hand != null)
		{
			ld.resource = "hand";
			hand.seed = weaponSeed;
			hand.Init();
			if (hand.chestPositionScaling != null)
			{
				hand.transform.localScale = Vector3.one * 0.1f;
				ld.NextFrameSet(hand.chestPositionScaling.localPosition + weaponSpawnPosition
					, hand.chestPositionScaling.localEulerAngles
					, hand.chestPositionScaling.localScale);
            }
            PlayerManager.instance.AddHand(baseString + "." + seed.ToString() + "." + levelString);
        }
        dgUtil.Disable(weapon);
        Destroy(displayItem);
        ld.specialText = "Sending to Ship";
    }
}
