using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : BodyPart
{
	GameObject bodyInstance;

	public Transform shoulder1T, shoulder2T, chestT, beltT;
	public BodyPart shoulder1, shoulder2, chest, belt;

	public override void ChangeItemSlot(string setItem)
    {
        string[] split = setItem.Split('.');
        PlayerManager.instance.SetBodyPart(position, setItem);
        Destroy(gameObject);
    }

    public void ClearBodyParts()
    {
        Destroy(shoulder1.gameObject);
        Destroy(shoulder2.gameObject);
        Destroy(chest.gameObject);
        Destroy(belt.gameObject);
        Destroy(skinMat.gameObject);
        Destroy(clothMat.gameObject);
        Destroy(decoMat.gameObject);
    }

	public void ChangeOtherPlayerSlot(string setItem){
		PlayerManager.instance.otherPlayerObject.GetComponent<PhotonView>().RPC("SetBody", PhotonTargets.Others, setItem);
	}

	public override void Init(){
		base.Init ();
		/*
		clothMat2 = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("clothMat" + Mathf.FloorToInt(Random.value * BodyPartManager.instance.bodyPartCounts[BodyPartType.clothMat])),
			Vector3.zero,
			Quaternion.identity,
			true,
			transform).GetComponent<BodyPart>();
		clothMat2.Init (this);
		*/
		int partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.shoulder]);
		shoulder1 = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("shoulder" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			shoulder1T).GetComponent<BodyPart>();
		shoulder1.transform.localPosition = Vector3.zero;
		shoulder1.transform.localEulerAngles = Vector3.zero;
		shoulder1.transform.localScale = GameManager.instance.GetResourcePrefab ("shoulder" + partNum).transform.localScale;
		shoulder1.Init (this, decoMat);
		shoulder2 = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("shoulder" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			shoulder2T).GetComponent<BodyPart>();
		shoulder2.transform.localPosition = Vector3.zero;
		shoulder2.transform.localEulerAngles = Vector3.zero;
		shoulder2.transform.localScale = GameManager.instance.GetResourcePrefab ("shoulder" + partNum).transform.localScale;
		shoulder2.Init (this, decoMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.chest]);
		chest = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("chest" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			chestT).GetComponent<BodyPart>();
		chest.transform.localPosition = Vector3.zero;
		chest.transform.localEulerAngles = Vector3.zero;
		chest.transform.localScale = GameManager.instance.GetResourcePrefab ("chest" + partNum).transform.localScale;
		chest.Init (this, clothMat);
		partNum = Mathf.FloorToInt (Random.value * BodyPartManager.instance.bodyPartCounts [BodyPartType.belt]);
		belt = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("belt" + partNum),
			Vector3.zero,
			Quaternion.identity,
			true,
			beltT).GetComponent<BodyPart>();
		belt.transform.localPosition = Vector3.zero;
		belt.transform.localEulerAngles = Vector3.zero;
		belt.transform.localScale = GameManager.instance.GetResourcePrefab ("belt" + partNum).transform.localScale;
		belt.Init (this, decoMat);
        CullMods();
    }
}
