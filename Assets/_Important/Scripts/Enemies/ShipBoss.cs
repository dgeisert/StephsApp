using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class ShipBoss : BaseEnemy
{

	int stage = 0, checkSails = 0, stageHitPoints = 100;
	public Transform[] cannons, doors, decks;
	public Destructible backSail, frontTopSail, frontBottomSail, midTopSail, midBottomSail;
	public GameObject wheel, cannonFireParticles, teleportParticles;
	public Material glow;
	string[] spawnStage = new string[]{ "bombRed", "bombOrange", "bombYellow", "bombPurple", "bombWhite", "golemOrange"};

	public override void EnemyStart()
	{
		hitPoints = stageHitPoints;
		backSail.LockOnPlayer();
		frontTopSail.LockOnPlayer();
		frontBottomSail.LockOnPlayer();
		midTopSail.LockOnPlayer();
		midBottomSail.LockOnPlayer();
		LockOnPlayer ();
		wheel.SetActive (false);
		if (CreateLevel.instance != null)
		{
			if (!CreateLevel.instance.enemies.Contains(gameObject))
			{
				CreateLevel.instance.enemies.Add(gameObject);
			}
		}
	}

	public override void PatrolPointReached(){

	}

	public override void EnemyAttack()
	{
		SpawnMinions ();
	}

	public override void Telegraph()
	{

	}

	Vector3 targetPosition;
	void TeleportHome()
    {
        CancelInvoke("TeleportAway");
        CancelInvoke("GoToTargetPosition");
        GameObject go = dgUtil.Instantiate(teleportParticles, transform.position + Vector3.up * 5, Quaternion.identity);
		Destroy(go, 10f);
		targetPosition = startPosition - Vector3.up * 4;
		GameObject go2 = dgUtil.Instantiate(teleportParticles, targetPosition + Vector3.up * 5, Quaternion.identity);
		Destroy(go2, 10f);
        frozen = true;
		Invoke ("GoToTargetPosition", 2f);
	}
	void TeleportAway(){
		GameObject go = dgUtil.Instantiate(teleportParticles, transform.position + Vector3.up * 5, transform.rotation);
		Destroy(go, 10f);
		targetPosition = new Vector3 (Random.value - 0.5f, 0, Random.value - 0.5f).normalized;
		targetPosition =  PlayerManager.instance.GetPlayerPosition () + new Vector3 (targetPosition.x * 50, -3, targetPosition.z * 50);
		GameObject go2 = dgUtil.Instantiate(teleportParticles, targetPosition + Vector3.up * 5, transform.rotation);
		Destroy(go2, 10f);
		Invoke ("GoToTargetPosition", 2f);
	}
	void GoToTargetPosition(){
		transform.position = targetPosition;
	}

	public override void EnemyTakeDamage()
	{
        SetStage();
        if (Vector3.Distance(PlayerManager.instance.GetPlayerPosition(), transform.position) >= stopDistance)
        {
            SpawnMinions();
        }
        if (hitPoints <= 0)
		{
			stage++;
			if (stage > 4)
			{
				Ship.instance.comet.SetActive(true);
				return;
			}
			hitPoints = stageHitPoints;
		}
        if(stage > 4)
        {
            transform.rotation = Quaternion.identity;
        }
	}

	public void SpawnMinions()
	{
        SetStage();

        EnableSail(backSail);
        EnableSail(frontTopSail);
        EnableSail(frontBottomSail);
        EnableSail(midTopSail);
        EnableSail(midBottomSail);
        if (stage < 5) {
			attackSpeed = 20;
			for (int i = 0; i < cannons.Length; i++) {
				GameObject cannonBlast = dgUtil.Instantiate (cannonFireParticles
					, cannons [i].position
					, cannons [i].rotation);
				Destroy (cannonBlast, 1);
				BaseEnemy be = dgUtil.Instantiate (GameManager.instance.GetResourcePrefab (spawnStage [stage])
				, cannons [i].position
				, Quaternion.identity).GetComponent<BaseEnemy> ();
				be.LockOnPlayer ();
				be.transform.LookAt (PlayerManager.instance.GetPlayerPosition ());
				CreateLevel.instance.totalEnemies++;
				CreateLevel.instance.enemies.Add (be.gameObject);
			}
			Invoke ("TeleportAway", 2f);
		} else {
            transform.rotation = Quaternion.identity;
			attackSpeed = 10;
			for (int i = 0; i < doors.Length; i++) {
				BaseEnemy be = dgUtil.Instantiate (GameManager.instance.GetResourcePrefab (spawnStage [stage])
					, doors [i].position
					, doors [i].rotation).GetComponent<BaseEnemy> ();
				be.LockOnPlayer ();
				be.transform.LookAt (PlayerManager.instance.GetPlayerPosition ());
				CreateLevel.instance.totalEnemies++;
				CreateLevel.instance.enemies.Add (be.gameObject);
			}
		}
	}

	public override void EnemyUpdate(){
        if(stage > 4)
        {
            transform.rotation = Quaternion.identity;
        }
	}

	void SetStage()
    {
        int sails = 5;
        sails -= backSail == null ? 1 : 0;
        sails -= frontTopSail == null ? 1 : 0;
        sails -= frontBottomSail == null ? 1 : 0;
        sails -= midTopSail == null ? 1 : 0;
        sails -= midBottomSail == null ? 1 : 0;
        if (stage != 5 - sails)
        {
            stage = 5 - sails;
            switch (stage)
            {
                case 5:
                    wheel.SetActive(true);
                    foreach(Transform t in decks)
                    {
                        //t.tag = "Teleportable";
                    }
                    normalDamageMult = 0.5f;
                    Invoke("TeleportHome", 2f);
                    break;
                case 1:
                case 3:
                    break;
                default:
                    break;
            }
        }
	}
	void EnableSail(Destructible sail){
		if (sail != null) {
            sail.gameObject.SetActive(true);
		}
	}
}
