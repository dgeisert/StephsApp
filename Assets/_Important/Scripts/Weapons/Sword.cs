using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Sword : BaseWeapon
{
    public override void WeaponStart()
    {
        if (Tutorial.instance != null)
        {
            if (grabbingObject != null)
            {
                if (Tutorial.instance.currentStep == "Sword" && grabbingObject.GetComponentInParent<PlayerManager>() != null)
                {
                    Tutorial.instance.NextTutorialStep();
                }
            }
        }
	}

	public override void ReleaseTrigger(GameObject usingObject)
	{
		if (specialCharged) {
			switch (special) {
			case "Mega":
				specialCooldownTime = 0;
				transform.localScale = Vector3.one * 3;
				break;
			default:
				break;
			}
		}
	}

	public override void WeaponUpdate(){
		switch (special) {
		case "Mega":
			if (specialCooldownTime > 3) {
				transform.localScale = Vector3.one;
			}
			break;
		default:
			break;
		}
	}

	public override float DamageMod(){
		return base.DamageMod () + PlayerManager.instance.meleeDamageMod / 100;
	}
	public override float SpeedMod(){
		return base.SpeedMod () + PlayerManager.instance.meleeSpeedMod / 100;
	}
	public override float CritMultMod(){
		return base.CritMultMod () + PlayerManager.instance.meleeCritMultMod / 100;
	}
	public override float CritPercentMod(){
		return base.CritPercentMod () + PlayerManager.instance.meleeCritPercentMod / 100;
	}
}
