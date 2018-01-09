using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class GolemBoss : BaseEnemy
{

    int stage = 0;
    float stageHitPoints = 40;
    public Transform[] minionSpawnPoints;
    public GameObject rightHand, leftHand, body;

    public override void EnemyStart()
    {
        hitPoints = stageHitPoints;
        if (CreateLevel.instance != null)
        {
            if (!CreateLevel.instance.enemies.Contains(gameObject))
            {
                CreateLevel.instance.enemies.Add(gameObject);
            }
        }
    }

    public override void EnemyAttack()
	{
		Tweener tLeft = HOTween.To(leftHand.transform, attackSpeed - telegraphTime, "position", toAttackPosition, false, EaseType.EaseInOutQuad, 0);
		Tweener tRight = HOTween.To(rightHand.transform, attackSpeed - telegraphTime, "position", toAttackPosition, false, EaseType.EaseInOutQuad, 0);
    }

    public override void Telegraph()
    {
		
		Tweener tLeft = HOTween.To(leftHand.transform, telegraphTime, "localPosition", new Vector3(-0.2366883f, 0.1831745f, -0.1f), false, EaseType.EaseInOutQuad, 0);
		Tweener tRight = HOTween.To(rightHand.transform, telegraphTime, "localPosition", new Vector3(0.1622316f, 0.2095435f, -0.1f), false, EaseType.EaseInOutQuad, 0);
    }

    public override void EnemyTakeDamage()
    {
        LockOnPlayer();
        if (hitPoints <= 0)
        {
            stage++;
            if (stage >= 5)
            {
                Ship.instance.comet.SetActive(true);
                return;
            }
            transform.localScale = Vector3.one * (20 - 2 * stage);
            hitPoints = stageHitPoints;
            Tweener tLeft = HOTween.To(leftHand.transform, telegraphTime - 0.1f, "localPosition", new Vector3(-0.138f, 0.398f, 0.11f), false, EaseType.EaseInOutQuad, 0);
            Tweener tRight = HOTween.To(rightHand.transform, telegraphTime - 0.1f, "localPosition", new Vector3(0.02f, 0.4f, 0.147f), false, EaseType.EaseInOutQuad, 0);
            Invoke("SpawnMinions", telegraphTime);
        }
    }

    public void SpawnMinions()
    {
        for (int i = 0; i < stage * 2; i++)
        {
            BaseEnemy be = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(dgUtil.RandomEnemy())
                , minionSpawnPoints[Mathf.FloorToInt(Random.value * minionSpawnPoints.Length)].position
                , Quaternion.identity).GetComponent<BaseEnemy>();
            be.LockOnPlayer();
            be.transform.LookAt(PlayerManager.instance.GetPlayerPosition());
            CreateLevel.instance.totalEnemies++;
            CreateLevel.instance.enemies.Add(be.gameObject);
        }
        Tweener tLeft = HOTween.To(leftHand.transform, 0.2f, "localPosition", new Vector3(-0.2366883f, 0.1831745f, 0.03305821f), false, EaseType.EaseInOutQuad, 0);
        Tweener tRight = HOTween.To(rightHand.transform, 0.2f, "localPosition", new Vector3(0.1622316f, 0.2095435f, 0.05266001f), false, EaseType.EaseInOutQuad, 0);
    }
}
