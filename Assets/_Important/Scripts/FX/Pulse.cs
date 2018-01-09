using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class Pulse : MonoBehaviour
{

    public float duration = 1;
    public Vector3 maxSize;

    void Start()
    {
        if (maxSize != Vector3.zero)
        {
            Tweener t = HOTween.To(transform, duration, "localScale", maxSize, false, EaseType.EaseInOutQuad, 0);
            t.loops = -1;
            t.loopType = LoopType.Yoyo;
        }
    }
}
