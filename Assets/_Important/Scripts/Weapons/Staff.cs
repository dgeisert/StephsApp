using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Staff : BaseWeapon
{

	public override float DamageMod(){
		return base.DamageMod () + PlayerManager.instance.staffDamageMod / 100;
	}
	public override float SpeedMod(){
		return base.SpeedMod () + PlayerManager.instance.staffSpeedMod / 100;
	}
	public override float CritMultMod(){
		return base.CritMultMod () + PlayerManager.instance.staffCritMultMod / 100;
	}
	public override float CritPercentMod(){
		return base.CritPercentMod () + PlayerManager.instance.staffCritPercentMod / 100;
	}
	public override void ReleaseTrigger(GameObject usingObject)
	{
		if (specialCharged) {
			switch (special) {
			case "Fireball":
				Fireball ();
				break;
			case "Frost Nova":
				FrostNova ();
				break;
			case "Flame Thrower":
				FlameThrower ();
				break;
			default:
				break;
			}
		}
	}
	public void Fireball(){
		float bulletSpeed = -500;
		Quaternion angle = bulletSpawnLoc.rotation;
		GameObject proj = PlayerManager.instance.CreateProjectile ("Fireball", bulletSpawnLoc.position, angle, Color.white, bulletSpeed * SpeedMod (), false, damage * DamageMod());
		proj.transform.localScale *= 1 + (DamageMod () - 1) / 4;
		VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.63f, 0.2f, 0.01f);
	}

	public void FrostNova(){
		GameObject proj = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("AOEFrostNova")
			, bulletSpawnLoc.position
			, Quaternion.identity);
		VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 1f, 0.3f, 0.01f);
		Destroy (proj, 4f);
	}

	public void FlameThrower(){
		GameObject proj = dgUtil.Instantiate(
			GameManager.instance.GetResourcePrefab("AOEFlamethrower")
			, Vector3.zero
			, Quaternion.identity
			, true
			, bulletSpawnLoc);
        proj.transform.localEulerAngles = new Vector3(90, 0, 0);
		VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.2f, 3f, 0.01f);
		Destroy (proj, 6f);
	}
}
