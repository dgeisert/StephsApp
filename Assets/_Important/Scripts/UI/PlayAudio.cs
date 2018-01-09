using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour {

	public AudioClip clip;
	public List<AudioClip> clips = new List<AudioClip>();
	public List<string> lines;
	public TextMesh creditText;
	AudioSource source;
	BoxCollider collider;
	Rigidbody rb;

	// Use this for initialization
	void Start () {
		source = gameObject.AddComponent<AudioSource> ();
		source.clip = clip;
		source.playOnAwake = false;
		creditText.text = "";
		foreach (string line in lines) {
			creditText.text += line + '\n';
		}
		collider = gameObject.AddComponent<BoxCollider> ();
		rb = gameObject.AddComponent<Rigidbody> ();
		rb.useGravity = false;
		rb.isKinematic = true;
	}

	void OnCollisionEnter(Collision col){
		if (clips != null) {
			if (clips.Count > 0) {
				source.clip = clips [Mathf.FloorToInt (Random.value * clips.Count)];
			}
		}
		source.Play ();
	}
}
