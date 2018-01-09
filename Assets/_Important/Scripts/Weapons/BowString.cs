using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class BowString : BaseWeapon
{

    public Vector3 startPosition;
    public Bow bow;
    public bool isPulled = false;
    public Transform pullingHand;
    public BoxCollider bc;

    public override void WeaponStart()
    {
        startPosition = transform.localPosition;
		//Invoke ("FireMultiple", 1f);
    }

	void FireMultiple(){
		FireBullet (5);
	}

    public override void WeaponUpdate()
    {
        if (isPulled)
        {
            transform.position = pullingHand.position;
            VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.1f, 0.1f, 0.01f);
            if (!pullingHand.GetComponent<VRTK_ControllerEvents>().triggerPressed)
            {
                ReleaseTrigger(pullingHand.gameObject);
            }
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition, Time.deltaTime * 20);
        }
    }

    public override void PullTrigger(GameObject usingObject)
    {
        isPulled = true;
        pullingHand = usingObject.transform;
        VRTK.VRTK_InteractGrab vrtkG = usingObject.GetComponentInParent<VRTK.VRTK_InteractGrab>();
        if (vrtkG != null)
        {
            GameObject destroyGo = vrtkG.grabbedObject;
            if (destroyGo != null)
            {
                if (destroyGo.GetComponent<BaseWeapon>() != null)
                {
                    Destroy(destroyGo);
                }
            }
        }
    }

    public override void ReleaseTrigger(GameObject previousUsingObject)
    {
        if (bow.canFire && !pullingHand.GetComponent<VRTK_ControllerEvents>().triggerClicked)
        {
			if (bow.specialCharged && special != "") {
				switch (special) {
				case "Multi Shot":
					FireBullet (5);
					break;
				case "Rapid Fire":
					FireBullet ();
					Invoke ("FireBulletRepeat", 0.2f);
					Invoke ("FireBulletRepeat", 0.4f);
					Invoke ("FireBulletRepeat", 0.6f);
					Invoke ("FireBulletRepeat", 0.8f);
					Invoke ("FireBulletRepeat", 1f);
					break;
				case "Homing":
					FireBullet (1, true);
					break;
				case "Power Shot":
					FireBullet (1, false);
					break;
				default:
					break;
				}
			} else {
				FireBullet ();
			}
        }
    }

    public void FireBulletRepeat()
    {
        FireBullet(1, false, true);
    }

    float storedBulletSpeed = -1000;
	public void FireBullet(int count = 1, bool seeking = false, bool usedStored = false){
		float bulletSpeed = -projectileSpeed * Vector3.Distance (transform.localPosition, startPosition) / 0.5f * RangeMod ();
        if (usedStored)
        {
            bulletSpeed = storedBulletSpeed;
        }
        else
        {
            storedBulletSpeed = bulletSpeed;
        }
		for (int i = 0; i < count; i++) {
			if (seeking) {
				PlayerManager.instance.CreateProjectile (bow.projectilePrefab.name, bulletSpawnLoc.position, bulletSpawnLoc.rotation, bow.glowMat.mat.color, bulletSpeed * SpeedMod (), false, bow.damage * bow.DamageMod ());
			} else {
				Quaternion angle = bulletSpawnLoc.rotation;
				if (i > 0) {
					angle = new Quaternion(angle.x + (Random.value - 0.5f) / 5, angle.y + (Random.value - 0.5f) / 5, angle.z + (Random.value - 0.5f) / 5, angle.w);
				}
				GameObject proj = PlayerManager.instance.CreateProjectile (bow.projectilePrefab.name, bulletSpawnLoc.position, angle, bow.glowMat.mat.color, bulletSpeed * SpeedMod (), false, bow.damage * bow.DamageMod ());
				//proj.transform.localScale *= 1 + (DamageMod () - 1) / 4;
			}
		}
		VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.63f, 0.2f, 0.01f);
		bow.canFire = false;
		isPulled = false;
		bow.FireCooldown();
    }

    public override float DamageMod()
    {
		float damageMod = base.DamageMod() + PlayerManager.instance.bowDamageMod / 100;
		if (specialCharged && special == "Power Shot") {
			damageMod *= 5;
		}
		return damageMod;
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
