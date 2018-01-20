using UnityEngine;
using System.Collections;

public class characterButton : MonoBehaviour {

	public GameObject frog;
	public GameObject basket; 
	public GameObject skirt; 
	
	private Rect FpsRect ;
	private string frpString;
	
	private GameObject instanceObj;
	public GameObject[] gameObjArray=new GameObject[9];
	public AnimationClip[] AniList  = new AnimationClip[4];


	
	// Use this for initialization
	void Start () {

		frog.GetComponent<Animation>().wrapMode= WrapMode.Loop;
		frog.GetComponent<Animation>().CrossFade("breath");


	}
	
 void OnGUI() {
	  if (GUI.Button(new Rect(20, 20, 70, 40),"Normal")){
		 frog.GetComponent<Animation>().wrapMode= WrapMode.Loop;
		  	frog.GetComponent<Animation>().CrossFade("breath");
	  }
	    if (GUI.Button(new Rect(100, 20, 70, 40),"Walk")){
		  frog.GetComponent<Animation>().wrapMode= WrapMode.Loop;
		  	frog.GetComponent<Animation>().CrossFade("walk");
	  }
		if (GUI.Button(new Rect(180, 20, 70, 40),"Jump")){
		  frog.GetComponent<Animation>().wrapMode= WrapMode.Loop;
		  	frog.GetComponent<Animation>().CrossFade("jump");
	  }


		  if (GUI.Button(new Rect(580, 20, 120, 40),"Basket ON")){

			basket.SetActive (true);
	  }

		if (GUI.Button(new Rect(700, 20, 120, 40),"Basket OFF")){
			basket.SetActive (false);		
			
		}
		
		if (GUI.Button(new Rect(830, 20, 120, 40),"Skirt ON")){
			skirt.SetActive (true);		
			
		}

		if (GUI.Button(new Rect(950, 20, 120, 40),"Skirt OFF")){
			skirt.SetActive (false);		
			
		}
		
		
		
 }
	
	// Update is called once per frame
	void Update () {
		
		//if(Input.GetMouseButtonDown(0)){
		
			//touchNum++;
			//touchDirection="forward";
		 // transform.position = new Vector3(0, 0,Mathf.Lerp(minimum, maximum, Time.time));
			//Debug.Log("touchNum=="+touchNum);
		//}
		/*
		if(touchDirection=="forward"){
			if(Input.touchCount>){
				touchDirection="back";
			}
		}
	*/
		 
		//transform.position = Vector3(Mathf.Lerp(minimum, maximum, Time.time), 0, 0);
	if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
		//frog.transform.Rotate(Vector3.up * Time.deltaTime*30);
	}
	
}
