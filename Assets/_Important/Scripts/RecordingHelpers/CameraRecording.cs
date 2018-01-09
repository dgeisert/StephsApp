using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRecording : MonoBehaviour {

    public bool rotate, primaryCamera;
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (rotate)
        {
            transform.RotateAround(transform.parent.position, Vector3.up, Time.deltaTime * 10);
        }
        if (primaryCamera)
        {
            foreach(Camera cam in GameObject.FindObjectsOfType<Camera>())
            {
                if(cam.gameObject != gameObject)
                {
                    cam.enabled = false;
                }else
                {
                    cam.enabled = true;
                }
            }
        }
	}
}
