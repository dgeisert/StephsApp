using UnityEngine;
using UnityEngine.Collections;
using VRTK;

public class Bow : BaseWeapon
{

    public Vector3 storedStringRestPosition;
    public Transform arrow, bridge, grabString;
    public MeshRenderer arrowRenderer;
    public LineRenderer lineRenderer;
    public BowString bowString;
    public bool canFire = true;

    public Vector3 stringTop, stringBottom;

    public override void WeaponUpdate()
    {
        lineRenderer.SetPosition(1, grabString.localPosition);
        arrow.localPosition = grabString.localPosition;
        arrow.LookAt(bridge);
        bowString.bow = this;
		bowString.special = special;
        if (!IsGrabbed())
        {
            bowString.GetComponent<BoxCollider>().enabled = false;
            foreach (MeshCollider mc in GetComponentsInChildren<MeshCollider>())
            {
                mc.enabled = true;
            }
            GetComponent<BoxCollider>().enabled = true;
        }
        else
        {
            bowString.GetComponent<BoxCollider>().enabled = true;
            foreach (MeshCollider mc in GetComponentsInChildren<MeshCollider>())
            {
                mc.enabled = false;
            }
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void FireCooldown()
    {
        arrowRenderer.enabled = false;
        specialChargeTime = 0;
        specialCharged = false;
        Invoke("ResetArrow", attackCooldown * SpeedMod());
    }

    public void ResetArrow()
    {
        arrowRenderer.enabled = true;
        canFire = true;
    }

    public override void WeaponStart()
    {
        arrowRenderer.material = glowMat.mat;
        lineRenderer.material = glowMat.mat;
        foreach (BowStringAttachPoint bsap in GetComponentsInChildren<BowStringAttachPoint>())
        {
            if (bsap.isTop)
            {
                stringTop = bsap.transform.parent.localPosition + bsap.transform.parent.localRotation * bsap.transform.localPosition;
            }
            else
            {
                stringBottom = bsap.transform.parent.localPosition + bsap.transform.parent.localRotation * bsap.transform.localPosition;
            }
        }
        grabString.localPosition = new Vector3(stringTop.x, stringTop.y, 0);
        grabString.GetComponent<BoxCollider>().size = new Vector3(0.2f, 0.2f, stringTop.z - stringBottom.z);
        storedStringRestPosition = grabString.transform.localPosition;
        lineRenderer.SetPositions(new Vector3[] {
            stringTop,
            stringTop - new Vector3(0, 0, stringBottom.z)/2,
            stringBottom,
		});
		arrow.localPosition = grabString.localPosition;
		arrow.LookAt(bridge);
    }

    public override void HitEnemy(BaseEnemy be, Collision col)
    {

    }

    public override void PullTrigger(GameObject usingObject)
    {

    }

    public override void ReleaseTrigger(GameObject previousUsingObject)
    {

    }

    public override float DamageMod()
    {
        return base.DamageMod() + PlayerManager.instance.bowDamageMod / 100;
    }
    public override float SpeedMod()
    {
        return base.SpeedMod() + PlayerManager.instance.bowSpeedMod / 100;
    }
    public override float CritMultMod()
    {
        return base.CritMultMod() + PlayerManager.instance.bowCritMultMod / 100;
    }
    public override float CritPercentMod()
    {
        return base.CritPercentMod() + PlayerManager.instance.bowCritPercentMod / 100;
    }
    public override float RangeMod()
    {
        return base.RangeMod() + PlayerManager.instance.bowRangeModifier / 100;
    }
}