using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class LightFlicker : MonoBehaviour {

    public float low = 0.5F, high = 1.5F, minTime = 0.1F, maxTime = 0.5F;
    public Color color;
    public Light light;

	void Start () {
        light.color = color;
        StartCoroutine(Flicker());
	}

    public IEnumerator Flicker()
    {
        float duration = Random.value * (maxTime - minTime) + minTime;
        HOTween.To(light, duration, "intensity", Random.value * (high - low) + low);
        yield return new WaitForSeconds(duration);
        StartCoroutine(Flicker());
    }
}
