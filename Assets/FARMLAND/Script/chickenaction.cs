using UnityEngine;
using System.Collections;

public class chickenaction: MonoBehaviour {
	
	public GameObject frog1;
	
	
	
	private Rect FpsRect ;
	private string frpString;
	
	private GameObject instanceObj;
	public GameObject[] gameObjArray=new GameObject[9];
	public AnimationClip[] AniList  = new AnimationClip[4];
	
	
	void Start () {
		frog1.GetComponent<Animation>().wrapMode= WrapMode.Loop;
		frog1.GetComponent<Animation>().CrossFade("chickenbasemove");
		
		
	}
	
	void OnGUI() {
		
		
	}
	
	
	void Update () {
		
		
		
		if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
		
	}
	
}
