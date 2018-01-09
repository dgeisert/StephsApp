using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Cage : VRTK_InteractableObject
{

    public GameObject rescueCreature, comet;
	public GameObject[] bars;
    GameObject rescueCreatureInstance;
    bool claimed = false;
	public Transform spawnPoint;

    public void Init()
    {
        rescueCreatureInstance = dgUtil.Instantiate(rescueCreature, spawnPoint.localPosition, Quaternion.identity, true, transform);
        dgUtil.Disable(rescueCreatureInstance);
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        Claim();
    }

    public override void OnInteractableObjectUsed(InteractableObjectEventArgs e)
    {
        Claim();
    }

    public void Claim()
    {
        if (!claimed)
        {
			CreateLevel.instance.Rescue ();
            Destroy(rescueCreatureInstance);
			dgUtil.Instantiate(
				rescueCreature
				, spawnPoint.localPosition
				, Quaternion.identity
				, true
				, transform
			).GetComponent<Pet>().patrolBox = PlayerManager.instance.petArea;
			comet.SetActive (false);
			foreach (GameObject go in bars) {
				go.AddComponent<Rigidbody> ();
				go.AddComponent<MeshCollider> ().convex = true;
			}
			GetComponent<BoxCollider> ().enabled = false;
			Invoke ("RemoveCageParts", 10);
            claimed = true;
        }
    }

	void RemoveCageParts(){
		foreach (GameObject go in bars) {
			Destroy(go);
		}
	}
}
