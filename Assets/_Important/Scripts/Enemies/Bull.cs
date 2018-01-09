using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class Bull : BaseEnemy {

	public Transform storeTarget;
	public GameObject chargeCollider, Snort;

	public override void EnemyStart()
	{
	}

	public override void EnemyAttack()
	{
        lockOnPlayer = false;
		Snort.SetActive (false);
		chargeCollider.SetActive (true);
		Tweener charge = HOTween.To(transform, 3f, "position", transform.forward * 20, true, EaseType.EaseInOutQuad, 0);
		Tweener charge2 = HOTween.To(transform, 0.5f, "localEulerAngles", new Vector3(0, 0, 5), true, EaseType.EaseInOutQuad, 0);
		charge2.loops = 6;
		charge2.loopType = LoopType.Yoyo;
		storeTarget = target;
		target = null;
		movingTo = transform.forward * 21;
		Invoke ("ResetTarget", 3.1f);
	}

	void ResetTarget(){
		target = storeTarget;
		chargeCollider.SetActive (false);
	}

	public override void Telegraph()
	{
		Snort.SetActive (true);
	}

	public override void EnemyTakeDamage()
	{
	}

}