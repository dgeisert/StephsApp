using UnityEngine;
using System.Collections;

public class sheepaction: MonoBehaviour {
	
	public GameObject frog2;
	
	
	
	private Rect FpsRect ;
	private string frpString;
	
	private GameObject instanceObj;
	public GameObject[] gameObjArray=new GameObject[9];
	public AnimationClip[] AniList  = new AnimationClip[4];
	
	
	void Start () {
		frog2.GetComponent<Animation>().wrapMode= WrapMode.Loop;
		frog2.GetComponent<Animation>().CrossFade("basemove");
		
		
	}
	
	void OnGUI() {
		
		
	}
	
	
	void Update () {
		
		
		
		if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
		
	}
	
}
