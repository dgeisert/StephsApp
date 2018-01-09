using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootTable : MonoBehaviour
{

    [System.Serializable]
    public class LootGrant
    {
        [SerializeField]
        public string item;
        [SerializeField]
        public int weight;
    }

    public string tableName = "";
    [SerializeField]
    public List<LootGrant> weightedTable;

    public void RollTable()
    {
        int totalWeight = 0;
        foreach (LootGrant lootGrant in weightedTable)
        {
            if (lootGrant.weight <= 0)
            {
                FindGrant(lootGrant.item);
            }
            else
            {
                totalWeight += lootGrant.weight;
            }
        }
        if (totalWeight <= 0)
        {
            return;
        }
        float rollResult = Random.value * totalWeight;
        float stepResult = 0;
        foreach (LootGrant lootGrant in weightedTable)
        {
            if (lootGrant.weight > 0)
            {
                stepResult += lootGrant.weight;
                if (stepResult > rollResult)
                {
                    FindGrant(lootGrant.item);
                    break;
                }
            }
        }
    }

    public void FindGrant(string item)
    {
        if (item.Substring(0, 5) == "grant")
        {
            Grant(item.Substring(6, item.Length - 6));
        }
        if (item.Substring(0, 5) == "table")
        {
            foreach (LootTable lt in GetComponents<LootTable>())
            {
                if (lt.tableName == item.Substring(6, item.Length - 6))
                {
                    lt.RollTable();
                }
            }
        }
    }

    /*  This section parses the loot strings and determines what should be done
     *  TODO describe what each part here does
     * 
    */
    public void Grant(string item)
    {
        string levelString = "0";
        if (CreateLevel.instance != null)
        {
            levelString = Mathf.FloorToInt((1 - Random.value * 0.5f) * CreateLevel.instance.level).ToString();
        }
        else
        {
            levelString = Mathf.FloorToInt((1 - Random.value * 0.5f) * PlayerPrefs.GetInt("currentLevel")).ToString();
        }
        switch (item.Split('_')[0])
        {
            case "gems":
                int toGrantGems = int.Parse(item.Split('_')[1]);
                for (int i = 0; i < Mathf.FloorToInt(toGrantGems / 1000); i++)
                {
                    toGrantGems -= 1000;
                    dgUtil.Instantiate(PlayerManager.instance.lootDrops["gems1000"], SpawnPosition(), PlayerManager.instance.lootDrops["gems1000"].transform.rotation);
                }
                for (int i = 0; i < Mathf.FloorToInt(toGrantGems / 100); i++)
                {
                    toGrantGems -= 100;
                    dgUtil.Instantiate(PlayerManager.instance.lootDrops["gems100"], SpawnPosition(), PlayerManager.instance.lootDrops["gems100"].transform.rotation);
                }
                for (int i = 0; i < Mathf.FloorToInt(toGrantGems / 10); i++)
                {
                    toGrantGems -= 10;
                    dgUtil.Instantiate(PlayerManager.instance.lootDrops["gems10"], SpawnPosition(), PlayerManager.instance.lootDrops["gems10"].transform.rotation);
                }
                for (int i = 0; i < toGrantGems; i++)
                {
                    dgUtil.Instantiate(PlayerManager.instance.lootDrops["gems1"], SpawnPosition(), PlayerManager.instance.lootDrops["gems1"].transform.rotation);
                }
                break;
            case "coins":
                int toGrantCoins = int.Parse(item.Split('_')[1]);
                for (int i = 0; i < Mathf.FloorToInt(toGrantCoins / 1000); i++)
                {
                    toGrantCoins -= 1000;
                    dgUtil.Instantiate(PlayerManager.instance.lootDrops["coins1000"], SpawnPosition(), PlayerManager.instance.lootDrops["coins1000"].transform.rotation);
                }
                for (int i = 0; i < Mathf.FloorToInt(toGrantCoins / 100); i++)
                {
                    toGrantCoins -= 100;
                    dgUtil.Instantiate(PlayerManager.instance.lootDrops["coins100"], SpawnPosition(), PlayerManager.instance.lootDrops["coins100"].transform.rotation);
                }
                for (int i = 0; i < Mathf.FloorToInt(toGrantCoins / 10); i++)
                {
                    toGrantCoins -= 10;
                    dgUtil.Instantiate(PlayerManager.instance.lootDrops["coins10"], SpawnPosition(), PlayerManager.instance.lootDrops["coins10"].transform.rotation);
                }
                for (int i = 0; i < toGrantCoins; i++)
                {
                    dgUtil.Instantiate(PlayerManager.instance.lootDrops["coins1"], SpawnPosition(), PlayerManager.instance.lootDrops["coins1"].transform.rotation);
                }
                break;
            case "resource":
                PlayerManager.instance.AddResource(item.Split('_')[1], int.Parse(item.Split('_')[2]));
                break;
		case "weapon":
			object[] data = new object[1];
			int weaponSeed = Mathf.FloorToInt (Random.value * 10000);
                string thisItem = item.Split('_')[1];
			if (thisItem.Contains (".")) {
				levelString = thisItem.Split ('.') [2];
				weaponSeed = int.Parse(thisItem.Split ('.') [1]);
                    thisItem = thisItem.Split('.')[0];

            }
                PlayerManager.instance.AddWeapon(thisItem + "." + weaponSeed.ToString() + "." + levelString);
                data[0] = weaponSeed.ToString() + "." + levelString;
                GameObject weapon = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(thisItem), Vector3.zero, Quaternion.identity, true, transform, data: data);
                BaseWeapon bw = weapon.GetComponent<BaseWeapon>();
                LootDrop ld = weapon.AddComponent<LootDrop>();
                ld.resource = "weapon";
                if (bw != null)
                {
                    bw.seed = weaponSeed;
                    bw.Init();
                    if (bw.chestPositionScaling != null)
                    {
                        bw.transform.localScale = Vector3.one * 0.1f;
                        ld.NextFrameSet(bw.chestPositionScaling.localPosition
                            , bw.chestPositionScaling.localEulerAngles
                            , bw.chestPositionScaling.localScale);
                    }
                }
                dgUtil.Disable(weapon);
                ld.specialText = "New Weapon!";
                break;
            case "hand":
                object[] dataHand = new object[1];
                int handSeed = Mathf.FloorToInt(Random.value * 10000);
                PlayerManager.instance.AddHand(item.Split('_')[1] + "." + handSeed.ToString() + "." + levelString);
                dataHand[0] = handSeed.ToString() + "." + levelString;
                GameObject handObject = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(item.Split('_')[1]), Vector3.zero, Quaternion.identity, true, transform, data: dataHand);
                Hand hand = handObject.GetComponent<Hand>();
                LootDrop ldHand = handObject.AddComponent<LootDrop>();
                ldHand.resource = "hand";
                if (hand != null)
                {
                    hand.seed = handSeed;
                    hand.Init();
                    if (hand.chestPositionScaling != null)
                    {
                        hand.transform.localScale = Vector3.one * 0.1f;
                        ldHand.NextFrameSet(hand.chestPositionScaling.localPosition
                            , hand.chestPositionScaling.localEulerAngles
                            , hand.chestPositionScaling.localScale);
                    }
                }
                dgUtil.Disable(handObject);
                ldHand.specialText = "New Hand!";
                break;
            case "body":
                object[] dataBody = new object[1];
                int bodySeed = Mathf.FloorToInt(Random.value * 10000);
                PlayerManager.instance.AddBody(item.Split('_')[1] + "." + bodySeed.ToString() + "." + levelString);
                dataBody[0] = bodySeed.ToString() + "." + levelString;
                GameObject bodyObject = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(item.Split('_')[1]), Vector3.zero, Quaternion.identity, true, transform, data: dataBody);
                Body body = bodyObject.GetComponent<Body>();
                LootDrop ldBody = bodyObject.AddComponent<LootDrop>();
                ldBody.resource = "body";
                if (body != null)
                {
                    body.seed = bodySeed;
                    body.Init();
                    if (body.chestPositionScaling != null)
                    {
                        body.transform.localScale = Vector3.one * 0.1f;
                        ldBody.NextFrameSet(body.chestPositionScaling.localPosition
                            , body.chestPositionScaling.localEulerAngles
                            , body.chestPositionScaling.localScale);
                    }
                }
                dgUtil.Disable(bodyObject);
                ldBody.specialText = "New Body!";
                break;
            case "head":
                object[] dataHead = new object[1];
                int headSeed = Mathf.FloorToInt(Random.value * 10000);
                PlayerManager.instance.AddHead(item.Split('_')[1] + "." + headSeed.ToString() + "." + levelString);
                dataHead[0] = headSeed.ToString() + "." + levelString;
                GameObject headObject = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(item.Split('_')[1]), Vector3.zero, Quaternion.identity, true, transform, data: dataHead);
                Head head = headObject.GetComponent<Head>();
                LootDrop ldHead = headObject.AddComponent<LootDrop>();
                ldHead.resource = "head";
                if (head != null)
                {
                    head.seed = headSeed;
                    head.Init();
                    if (head.chestPositionScaling != null)
                    {
                        head.transform.localScale = Vector3.one * 0.1f;
                        ldHead.NextFrameSet(head.chestPositionScaling.localPosition
                            , head.chestPositionScaling.localEulerAngles
                            , head.chestPositionScaling.localScale);
                    }
                }
                dgUtil.Disable(headObject);
                ldHead.specialText = "New Head!";
                break;
            default:
                PlayerManager.instance.AddResource(item, 1);
                break;
        }
    }

    Vector3 SpawnPosition()
    {
        return new Vector3(transform.position.x + Random.value / 8, transform.position.y + Random.value / 8, transform.position.z + Random.value / 8);
    }
}
