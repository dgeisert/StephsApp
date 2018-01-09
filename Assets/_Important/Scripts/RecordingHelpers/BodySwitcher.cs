using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySwitcher : MonoBehaviour {

    BodyPart part;

	// Use this for initialization
	void Start () {
        part = GetComponent<Head>();
        if(part == null)
        {
            part = GetComponent<Hand>();
        }
        if(part == null)
        {
            part = GetComponent<Body>();
        }
        InvokeRepeating("SwitchPart", 0.5f, 0.5f);
    }

    void SwitchPart()
    {
        part.seed = Mathf.FloorToInt(Random.value * 1000);
        if (part.GetComponent<Head>() != null)
        {
            part.GetComponent<Head>().ClearBodyParts();
            part.GetComponent<Head>().Init();
        }
        if(part.GetComponent<Body>() != null)
        {
            part.GetComponent<Body>().ClearBodyParts();
            part.GetComponent<Body>().Init();
        }
        if(part.GetComponent<Hand>() != null)
        {
            part.GetComponent<Hand>().ClearBodyParts();
            part.GetComponent<Hand>().Init();
        }
    }
}
