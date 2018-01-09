using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Shield : BaseWeapon
{
    public override void HitWeapon(BaseWeapon bw, Collision col)
    {
        if (col.collider.GetComponentInParent<OtherPlayerObject>() != null)
        {
            col.collider.GetComponentInParent<OtherPlayerObject>().GetComponent<PhotonView>().RPC("DropWeapon", PhotonTargets.All, bw.side);
        }
        else if (col.collider.GetComponentInParent<PlayerManager>() != null)
        {
            PlayerManager.instance.otherPlayerObject.GetComponent<PhotonView>().RPC("DropWeapon", PhotonTargets.All, bw.side);
        }
    }
}
