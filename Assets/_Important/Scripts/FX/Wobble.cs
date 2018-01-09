using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class Wobble : MonoBehaviour {

    public float duration = 1;
    public Vector3 amount, rotation;
	Tweener tPos, tRot;

    public void Start()
    {
        if (amount != Vector3.zero)
        {
			if (tPos == null) {
				tPos = HOTween.To (transform, duration, "localPosition", 0.5f * 1 * amount, true, EaseType.EaseInOutQuad, 0);
				tPos.loops = -1;
				tPos.loopType = LoopType.Yoyo;
			} else {
				tPos.loops = -1;
			}
        }
        if(rotation != Vector3.zero)
        {
			if (tRot == null) {
				tRot = HOTween.To (transform, duration, "localEulerAngles", 0.5f * 1 * rotation, true, EaseType.EaseInOutQuad, 0);
				tRot.loops = -1;
				tRot.loopType = LoopType.Yoyo;
			} else {
				tRot.loops = -1;
			}
        }
    }

	public void Stop(){
		tRot.loops = 1;
		tPos.loops = 1;
	}
}
