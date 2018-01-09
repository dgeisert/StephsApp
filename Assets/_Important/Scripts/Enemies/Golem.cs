using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class Golem : BaseEnemy {

	public GameObject rightHand, leftHand, body;

	public override void Telegraph(){
		Tweener tLeft = HOTween.To(leftHand.transform, telegraphTime - 0.1f, "localPosition", new Vector3(-0.138f, 0.398f, 0.11f), false, EaseType.EaseInOutQuad, 0);
		Tweener tRight = HOTween.To(rightHand.transform, telegraphTime - 0.1f, "localPosition", new Vector3(0.02f, 0.4f, 0.147f), false, EaseType.EaseInOutQuad, 0);
	}
	public override void EnemyAttack(){
		Tweener tLeft = HOTween.To(leftHand.transform, 0.2f, "localPosition", new Vector3(-0.097f, 0.137f, 0.187f), false, EaseType.EaseInOutQuad, 0);
		Tweener tRight = HOTween.To(rightHand.transform, 0.2f, "localPosition", new Vector3(0.025f, 0.133f, 0.183f), false, EaseType.EaseInOutQuad, 0);
		Invoke ("ReturnHands", 0.2f);
	}

	public void ReturnHands(){
		Tweener tLeft = HOTween.To(leftHand.transform, 0.2f, "localPosition", new Vector3(-0.2366883f, 0.1831745f, 0.03305821f), false, EaseType.EaseInOutQuad, 0);
		Tweener tRight = HOTween.To(rightHand.transform, 0.2f, "localPosition", new Vector3(0.1622316f, 0.2095435f, 0.05266001f), false, EaseType.EaseInOutQuad, 0);
	}

	public override void CancelAttack(){
		ReturnHands ();
	}
}
