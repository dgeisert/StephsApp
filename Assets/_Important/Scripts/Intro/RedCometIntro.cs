using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class RedCometIntro : MonoBehaviour {

	public Transform eyes;
	public Transform[] path;
	public GameObject greenComet, greenExplosion;
	public AudioClip intro;
	AudioSource audioSource;
	public float duration = 5;

	// Use this for initialization
	void Start () {
		eyes.localScale = new Vector3 (1, 0.01f, 1);
		eyes.gameObject.SetActive (false);
		audioSource = gameObject.GetComponent<AudioSource> ();
	}

	public void StartSequence(){
		Tutorial.instance.pet.PlayAudio (Tutorial.instance.pet.clipRedCometArrives);
		HOTween.To (transform, duration, "eulerAngles", path[0].eulerAngles, false, EaseType.EaseOutQuad, 0);
		HOTween.To (transform, duration, "position", path[0].position, false, EaseType.EaseOutQuad, 0);
		Invoke ("StepOneHalf", duration - 1);
	}

	void StepOneHalf(){
		eyes.gameObject.SetActive (true);
		PlayAudio (intro);
		Destroy(dgUtil.Instantiate (greenExplosion, greenComet.transform.position, Quaternion.identity), 5);
		Invoke ("StepTwo", 1);
	}

	void StepTwo(){
		greenComet.SetActive (false);
		HOTween.To (eyes.transform, duration, "localScale", Vector3.one, false, EaseType.EaseOutBounce, 0);
		Tweener t = HOTween.To (transform, 1, "eulerAngles", path[0].eulerAngles + new Vector3(0, -60, 0), false, EaseType.EaseInOutQuad, 0);
		t.loops = 4;
		t.loopType = LoopType.Yoyo;
		Invoke ("StepThree", duration);
	}

	void StepThree(){
		Tutorial.instance.EvilSwap ();
		Invoke ("StepFour", duration);
	}

	void StepFour(){
		//evil laugh?
		Invoke ("StepFive", duration);
	}

	void StepFive(){
		Tutorial.instance.NextTutorialStep();
		HOTween.To (transform, duration * 4, "position", path[1].position, false, EaseType.EaseOutQuad, 0);
	}

	void PlayAudio(AudioClip clip){
		audioSource.clip = clip;
		audioSource.Play ();
	}
}
