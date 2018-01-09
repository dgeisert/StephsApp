using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using VRTK;
using VRTK.GrabAttachMechanics;

public class RigidbodySync : Photon.MonoBehaviour {

	PhotonView pv;
	Rigidbody rb;
	BaseWeapon bw;

    private void Start()
    {
        if(rb == null)
        {
            Init();
        }
    }

	void Init () {
		pv = GetComponent<PhotonView> ();
		rb = GetComponent<Rigidbody> ();
		bw = GetComponent<BaseWeapon> ();
	}

	public void OnCollisionEnter (Collision col) 
	{
		if (rb == null || pv == null) {
			Init ();
		}
		if (pv.isMine && PhotonNetwork.connected) 
		{
			SendPosition ();
			return;
		}
		// the below section is meant to allow other players to interact with our objects
		/*
		PhotonView cpv = col.collider.GetComponentInParent<PhotonView> ();
		if (cpv != null && cpv.isMine) {
			BaseWeapon cbw = cpv.GetComponent<BaseWeapon> ();
			if (cbw != null) {
				SendPosition ();
				return;
			}
		}
		*/
	}

	public void SendPosition (bool forceParentNull = false)
    {
        if (rb == null)
        {
            Init();
        }
        if (PhotonNetwork.connected)
        {
            pv.RPC("SetPosition", PhotonTargets.Others, transform.localPosition, transform.localRotation, rb.velocity, rb.angularVelocity, forceParentNull);
        }
	}

    Vector3 holdPosition, holdVelocity, holdAngularVelocity;
    Quaternion holdRotation;
	[PunRPC]
	public void SetPosition(Vector3 setPosition, Quaternion setRotation, Vector3 setVelocity, Vector3 setAngularVelocity, bool forceParentNull)
	{
        holdPosition = setPosition;
        holdRotation = setRotation;
        holdAngularVelocity = setAngularVelocity;
        holdVelocity = setVelocity;
        if (forceParentNull)
        {
            rb.isKinematic = false;
            transform.SetParent(null);
            DoSetPosition();
        }
        else
        {
            DoSetPosition();
        }
	}

    public void DoSetPosition()
    {
        transform.localPosition = holdPosition;
        transform.localRotation = holdRotation;
        rb.velocity = holdVelocity;
        Debug.Log("Velocity: " + holdVelocity);
        rb.angularVelocity = holdAngularVelocity;
        Debug.Log("Angular Velocity: " + holdAngularVelocity);
    }
}
