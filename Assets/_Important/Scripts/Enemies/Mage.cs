using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class Mage : BaseEnemy {

	public Transform leftHand, rightHand, head;

	public override void EnemyStart()
	{
	}

	public override void EnemyAttack()
	{
		Tweener tRight = HOTween.To(rightHand.transform, 1f, "position", transform.forward, true, EaseType.EaseInOutQuad, 0);
		tRight.loops = 2;
		tRight.loopType = LoopType.Yoyo;
	}

	public override void Telegraph()
	{
	}

	public override void EnemyTakeDamage()
	{
	}

}