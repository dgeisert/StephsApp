using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class Flytext : MonoBehaviour {

    public TextMesh text;
    public int amount = 0;
    float duration = 1f;
    float fadeDelay = 0.5f;
    float tweenDistance = 1f;
    
	public void Init (Color color, string value) {
		transform.LookAt(new Vector3(PlayerManager.instance.head.position.x, transform.position.y, PlayerManager.instance.head.position.z));
        text.color = color;
        text.text = value;
        HOTween.To(transform, duration, "localPosition", Vector3.up * tweenDistance, true, EaseType.EaseOutQuad, 0);
        HOTween.To(text, duration, "color", new Color(color.r, color.g, color.b, 0), false, EaseType.EaseOutQuad, fadeDelay);
        Destroy(gameObject, duration + fadeDelay);
    }
}
