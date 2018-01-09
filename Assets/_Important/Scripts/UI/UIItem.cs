using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItem : MonoBehaviour {

	public string Init (string setItem) {
		string infoText = "";
		GameObject go = dgUtil.Instantiate(GameManager.instance.GetResourcePrefab(setItem.Split('.')[0]), GameManager.instance.GetResourcePrefab(setItem.Split('.')[0]).transform.position, GameManager.instance.GetResourcePrefab(setItem.Split('.')[0]).transform.rotation, true, transform);
        BaseWeapon bw = go.GetComponent<BaseWeapon>();
        if (bw != null)
        {
            if (setItem.Split('.').Length > 1)
            {
                bw.SetData(setItem.Substring(setItem.Split('.')[0].Length));
            }
            if (bw.uiPositionScaling != null)
            {
                go.transform.localPosition = bw.uiPositionScaling.localPosition;
                go.transform.localRotation = bw.uiPositionScaling.localRotation;
                go.transform.localScale = bw.uiPositionScaling.localScale;
            }
			bw.Init ();
            if (go.GetComponent<Sword>() != null)
            {
                infoText = go.GetComponent<Sword>().GetInfoText();
            }
            if (go.GetComponent<Bow>() != null)
            {
                infoText = go.GetComponent<Bow>().GetInfoText();
            }
            if (go.GetComponent<Gun>() != null)
            {
                infoText = go.GetComponent<Gun>().GetInfoText();
            }
            if (go.GetComponent<Staff>() != null)
            {
                infoText = go.GetComponent<Staff>().GetInfoText();
            }
            if (go.GetComponent<Shield>() != null)
            {
                infoText = go.GetComponent<Shield>().GetInfoText();
            }
        }
        BodyPart bp = go.GetComponent<BodyPart>();
        if (bp != null)
        {
            if (setItem.Split('.').Length > 1)
            {
                bp.Init(setItem.Substring(setItem.Split('.')[0].Length));
            }
            else
            {
                bp.Init();
            }
            if (bp.uiPositionScaling != null)
            {
                go.transform.localPosition = bp.uiPositionScaling.localPosition;
                go.transform.localRotation = bp.uiPositionScaling.localRotation;
                go.transform.localScale = bp.uiPositionScaling.localScale;
            }
            infoText = bp.GetInfoText();
        }
		dgUtil.Disable(go);
		return infoText;
    }
}
