using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WishingWell : MonoBehaviour {
	
	public LootTable lootTable;
	public Vector3 spawnPosition;
	public SpawnPoint spawnPoint;
    public float lastUseTime;
	int uses;
	List<string> bases = new List<string>()
	{
		"AxeBase",
		"SwordBase",
		"BowBase",
		"GunBase",
		"DaggerBase",
		"StaffBase",
		"ShieldBase",
		"simpleBody",
		"simpleHand",
		"baseHead"
	};
	public void Roll ()
    {
        if(lastUseTime + 5 > Time.time)
        {
            return;
        }
        lastUseTime = Time.time;
        uses++;
        if (Random.value > (0.5f - uses / 10)) {
			if (Random.value > 0.5f) {
				RollEnemies ();
			} else {
				lootTable.RollTable ();
			}
		} else {
			RollLoot ();
		}
	}

	void RollEnemies(){
		foreach (BaseEnemy be in spawnPoint.Spawn(PlayerPrefs.GetInt ("currentLevel") + uses)) {
			CreateLevel.instance.enemies.Add (be.gameObject);
			be.LockOnPlayer ();
		}
	}

	void RollLoot()
	{
		object[] data = new object[1];
		int weaponSeed = Mathf.FloorToInt(Random.value * 10000 + 11000);
		PlayerManager.instance.ClaimChest (weaponSeed);
		string levelString = (PlayerPrefs.GetInt ("currentLevel") - uses).ToString();
		string baseString = bases[Mathf.FloorToInt(Random.value * bases.Count)];
		data[0] = weaponSeed.ToString() + "." + levelString;
		GameObject weapon = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(baseString), transform.position, Quaternion.identity, data: data);
		LootDrop ld = weapon.AddComponent<LootDrop>();
		BaseWeapon bw = weapon.GetComponent<BaseWeapon>();
		if (bw != null)
		{
			ld.resource = "weapon";
			bw.seed = weaponSeed;
			bw.Init();
			if (bw.chestPositionScaling != null)
			{
				bw.transform.localScale = Vector3.one;
				ld.NextFrameSet(bw.chestPositionScaling.localPosition + spawnPosition
					, bw.chestPositionScaling.localEulerAngles
					, bw.chestPositionScaling.localScale);
			}
			PlayerManager.instance.AddWeapon(baseString + "." + weaponSeed.ToString() + "." + levelString);
		}
		Head head = weapon.GetComponent<Head>();
		if (head != null)
		{
			ld.resource = "head";
			head.seed = weaponSeed;
			head.Init();
			if (head.chestPositionScaling != null)
			{
				head.transform.localScale = Vector3.one;
				ld.NextFrameSet(head.chestPositionScaling.localPosition + spawnPosition
					, head.chestPositionScaling.localEulerAngles
					, head.chestPositionScaling.localScale);
			}
			PlayerManager.instance.AddHead(baseString + "." + weaponSeed.ToString() + "." + levelString);
		}
		Body body = weapon.GetComponent<Body>();
		if (body != null)
		{
			ld.resource = "body";
			body.seed = weaponSeed;
			body.Init();
			if (body.chestPositionScaling != null)
			{
				body.transform.localScale = Vector3.one;
				ld.NextFrameSet(body.chestPositionScaling.localPosition + spawnPosition
					, body.chestPositionScaling.localEulerAngles
					, body.chestPositionScaling.localScale);
			}
			PlayerManager.instance.AddBody(baseString + "." + weaponSeed.ToString() + "." + levelString);
		}
		Hand hand = weapon.GetComponent<Hand>();
		if (hand != null)
		{
			ld.resource = "hand";
			hand.seed = weaponSeed;
			hand.Init();
			if (hand.chestPositionScaling != null)
			{
				hand.transform.localScale = Vector3.one;
				ld.NextFrameSet(hand.chestPositionScaling.localPosition + spawnPosition
					, hand.chestPositionScaling.localEulerAngles
					, hand.chestPositionScaling.localScale);
			}
			PlayerManager.instance.AddHand(baseString + "." + weaponSeed.ToString() + "." + levelString);
		}
		dgUtil.Disable(weapon);
		ld.specialText = "Sending to Ship";
	}
}
