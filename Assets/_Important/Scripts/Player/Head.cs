using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : BodyPart
{
	GameObject headInstance;

	public Transform leftEyeT, rightEyeT, mouthT, ear1T, ear2T, headT, hairT, hatT;
	public BodyPart leftEye, rightEye, mouth, ear1, ear2, head, hair, hairMat, hat;

	public override void ChangeItemSlot(string setItem)
    {
        string[] split = setItem.Split('.');
        PlayerManager.instance.SetBodyPart(position, setItem);
        Destroy(gameObject);
    }

    public void ClearBodyParts()
    {
        Destroy(leftEye.gameObject);
        Destroy(rightEye.gameObject);
        Destroy(mouth.gameObject);
        Destroy(ear1.gameObject);
        Destroy(ear2.gameObject);
        Destroy(head.gameObject);
        Destroy(hair.gameObject);
        Destroy(hairMat.gameObject);
        Destroy(hat.gameObject);
        Destroy(clothMat.gameObject);
        Destroy(skinMat.gameObject);
        Destroy(decoMat.gameObject);
    }

	public void ChangeOtherPlayerSlot(string setItem){
		PlayerManager.instance.otherPlayerObject.GetComponent<PhotonView>().RPC("SetHead", PhotonTargets.Others, setItem);
	}

	public override void Init(){
		base.Init ();
		int partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.hairMat]);
		hairMat = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("hairMat" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			transform).GetComponent<BodyPart>();
		hairMat.Init (this);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.eye]);
        leftEye = dgUtil.Instantiate(
            GameManager.instance.GetResourcePrefab("eye" + partNum),
            Vector3.zero,
            Quaternion.identity,
            true,
			leftEyeT).GetComponent<BodyPart>();
		leftEye.transform.localPosition = Vector3.zero;
		leftEye.transform.localEulerAngles = Vector3.zero;
		leftEye.transform.localScale = GameManager.instance.GetResourcePrefab ("eye" + partNum).transform.localScale;
        leftEye.Init(this, leftEye);
        rightEye = dgUtil.Instantiate(
            GameManager.instance.GetResourcePrefab("eye" + partNum),
            Vector3.zero,
            Quaternion.identity,
            true,
			rightEyeT).GetComponent<BodyPart>();
		rightEye.transform.localPosition = Vector3.zero;
		rightEye.transform.localEulerAngles = Vector3.zero;
		rightEye.transform.localScale = GameManager.instance.GetResourcePrefab ("eye" + partNum).transform.localScale;
		rightEye.Init(this, rightEye);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.mouth]);
		mouth = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("mouth" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			mouthT).GetComponent<BodyPart>();
		mouth.transform.localPosition = Vector3.zero;
		mouth.transform.localEulerAngles = Vector3.zero;
		mouth.transform.localScale = GameManager.instance.GetResourcePrefab ("mouth" + partNum).transform.localScale;
		mouth.Init (this, mouth);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.ear]);
		ear1 = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("ear" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			ear1T).GetComponent<BodyPart>();
		ear1.transform.localPosition = Vector3.zero;
		ear1.transform.localEulerAngles = Vector3.zero;
		ear1.transform.localScale = GameManager.instance.GetResourcePrefab ("ear" + partNum).transform.localScale;
		ear1.Init (this, skinMat);
		ear2 = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("ear" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			ear2T).GetComponent<BodyPart>();
		ear2.transform.localPosition = Vector3.zero;
		ear2.transform.localEulerAngles = Vector3.zero;
		ear2.transform.localScale = GameManager.instance.GetResourcePrefab ("ear" + partNum).transform.localScale;
		ear2.Init (this, skinMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.head]);
		head = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("head" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			headT).GetComponent<BodyPart>();
		head.transform.localPosition = Vector3.zero;
		head.transform.localEulerAngles = Vector3.zero;
		head.transform.localScale = GameManager.instance.GetResourcePrefab ("head" + partNum).transform.localScale;
		head.Init (this, skinMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.hair]);
		hair = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("hair" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			hairT).GetComponent<BodyPart>();
		hair.transform.localPosition = Vector3.zero;
		hair.transform.localEulerAngles = Vector3.zero;
		hair.transform.localScale = GameManager.instance.GetResourcePrefab ("hair" + partNum).transform.localScale;
		hair.Init (this, hairMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.hat]);
		hat = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("hat" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			hatT).GetComponent<BodyPart>();
		hat.transform.localPosition = Vector3.zero;
		hat.transform.localEulerAngles = Vector3.zero;
		hat.transform.localScale = GameManager.instance.GetResourcePrefab ("hat" + partNum).transform.localScale;
		hat.Init (this, clothMat);
        CullMods();
    }
}
