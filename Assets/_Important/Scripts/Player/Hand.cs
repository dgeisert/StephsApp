using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : BodyPart
{
	GameObject handInstance;
	public Transform wristT, palmT, thumbT, pointerFingerT, grabFingerT;
	public BodyPart wrist, palm, thumb, pointerFinger, grabFinger;

    public bool isLeft = false;

    public void Start()
    {
        if (!isLeft)
        {
            transform.GetChild(0).localEulerAngles += new Vector3(0, 0, 180);
        }
        if(Tutorial.instance != null && transform.parent.name != "UIItem")
        {
            foreach(MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
            {
                mr.enabled = false;
            }
        }
    }

    public override void ChangeItemSlot(string setItem)
    {
        string[] split = setItem.Split('.');
        PlayerManager.instance.SetBodyPart(position, setItem);
        Destroy(gameObject);
    }

    public void ClearBodyParts()
    {
        Destroy(wrist.gameObject);
        Destroy(palm.gameObject);
        Destroy(thumb.gameObject);
        Destroy(pointerFinger.gameObject);
        Destroy(grabFinger.gameObject);
        Destroy(clothMat.gameObject);
        Destroy(skinMat.gameObject);
        Destroy(decoMat);
    }

	public void ChangeOtherPlayerSlot(string setItem){
		PlayerManager.instance.otherPlayerObject.GetComponent<PhotonView>().RPC("SetHand", PhotonTargets.Others, setItem, isLeft);
	}

	public override void Init(){
		base.Init ();
		int partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.wrist]);
		wrist = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("wrist" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			wristT).GetComponent<BodyPart>();
		wrist.Init (this, clothMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.palm]);
		palm = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("palm" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			palmT).GetComponent<BodyPart>();
		palm.Init (this, skinMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.thumb]);
		thumb = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("thumb" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			thumbT).GetComponent<BodyPart>();
		thumb.Init (this, skinMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.pointerFinger]);
		pointerFinger = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("pointerFinger" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			pointerFingerT).GetComponent<BodyPart>();
		pointerFinger.Init (this, skinMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.grabFinger]);
		grabFinger = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("grabFinger" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			grabFingerT).GetComponent<BodyPart>();
		grabFinger.Init (this, skinMat);
        CullMods();
    }
}
