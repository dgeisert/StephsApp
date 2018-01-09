using UnityEngine;
using VRTK;

public class Gun : BaseWeapon
{

    public AudioClip noFireSound, fireSound;
    public GameObject muzzleFlash;
    public bool raycast = true;

    public override void HitEnemy(BaseEnemy be, Collision col)
    {

    }

    public override void WeaponStart()
    {
        if (Tutorial.instance != null)
        {
            if (grabbingObject != null)
            {
                if (Tutorial.instance.currentStep == "Grip" && grabbingObject.GetComponentInParent<PlayerManager>() != null)
                {
                    Tutorial.instance.NextTutorialStep();
                }
            }
        }
        if (bulletSpawnLoc == null)
        {
            bulletSpawnLocs = transform.GetComponentsInChildren<ProjectileSpawnLocation>();
            bulletSpawnLoc = bulletSpawnLocs[0].transform;
        }
        if (muzzleFlash == null)
        {
            muzzleFlash = spark;
        }
    }

    public float lastFireTime = 0;
    public override void PullTrigger(GameObject usingObject)
    {
        if (bulletSpawnLoc == null)
        {
            return;
        }
        if (attackCooldown * SpeedMod() + lastFireTime < Time.time)
        {
            lastFireTime = Time.time;
        }
        else
        {
            audioSource.clip = noFireSound;
            audioSource.Play();
            return;
        }
        if (Tutorial.instance != null)
        {
            if (Tutorial.instance.currentStep == "Trigger")
            {
                Tutorial.instance.NextTutorialStep();
            }
        }
        VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.2f, 0.1f, 0.01f);
        FireBullet(1);
    }

    public override void ReleaseTrigger(GameObject usingObject)
    {
		if (specialCharged) {
			switch (special) {
			case "Power Shot":
				VRTK_ControllerHaptics.TriggerHapticPulse (VRTK_ControllerReference.GetControllerReference (grabbingObject), 0.63f, 0.2f, 0.01f);
				audioSource.clip = fireSound;
				audioSource.Play ();
				FireBullet (5);
				break;
			case "Multi Shot":
				FireBullet (1, 5);
				break;
			case "Rapid Fire":
				FireBullet ();
				Invoke ("FireBullet", 0.1f);
				Invoke ("FireBullet", 0.2f);
				Invoke ("FireBullet", 0.3f);
				Invoke ("FireBullet", 0.4f);
				Invoke ("FireBullet", 0.5f);
				Invoke ("FireBullet", 0.6f);
				Invoke ("FireBullet", 0.7f);
				Invoke ("FireBullet", 0.8f);
				Invoke ("FireBullet", 0.9f);
				Invoke ("FireBullet", 1f);
				break;
			default:
				break;
			}
		}
    }

    private void FireBullet()
    {
        FireBullet(1, 1);
    }

	private void FireBullet(float damageMod = 1, int count = 1)
	{
		audioSource.clip = fireSound;
		audioSource.Play();
        if (raycast)
        {
            FireRaycast(damageMod, count);
        }
        else
        {
            if (muzzleFlash != null)
            {
                GameObject flash = dgUtil.Instantiate(muzzleFlash, bulletSpawnLoc.position, bulletSpawnLoc.rotation);
                flash.GetComponent<Spark>().Init(glowMat.mat.color);
                Destroy(flash, 0.5f);
            }
            audioSource.Play();
        }
    }

	private void FireProjectile(float damageMod, int count = 1)
	{
		Quaternion angle = bulletSpawnLoc.rotation;
		GameObject flash = dgUtil.Instantiate(muzzleFlash, bulletSpawnLoc.position, angle);
		flash.GetComponent<Spark>().Init(glowMat.mat.color);
		Destroy(flash, 0.5f);
		for (int i = 0; i < count; i++) {
			if (i > 0) {
				angle = new Quaternion(angle.x + (Random.value - 0.5f) / 5, angle.y + (Random.value - 0.5f) / 5, angle.z + (Random.value - 0.5f) / 5, angle.w);
			}
			GameObject bulletClone = PlayerManager.instance.CreateProjectile(projectilePrefab.name, bulletSpawnLoc.position, angle, glowMat.mat.color, projectileSpeed, false, damage * damageMod * DamageMod());
			bulletClone.transform.localScale *= (1 + damageMod / 100) * DamageMod();
		}
	}
	private void FireRaycast(float damageMod, int count = 1)
	{
		Vector3 angle = bulletSpawnLoc.forward;
		GameObject flash = dgUtil.Instantiate(muzzleFlash, bulletSpawnLoc.position, bulletSpawnLoc.rotation);
		flash.GetComponent<Spark>().Init(glowMat.mat.color);
		Destroy(flash, 0.5f);
		for (int i = 0; i < count; i++) {
			if (i > 0) {
				angle = new Vector3(angle.x + (Random.value - 0.5f) / 5, angle.y + (Random.value - 0.5f) / 5, angle.z + (Random.value - 0.5f) / 5);
			}
			RaycastHit hit = new RaycastHit();
			Physics.Raycast(new Ray(bulletSpawnLoc.position, -angle), out hit, 100, (1 << 0));
			LineRenderer lr = flash.GetComponent<LineRenderer>();
			if (hit.collider != null)
			{
				dgUtil.Instantiate(spark, hit.point, Quaternion.identity).GetComponent<Spark>().Init(glowMat.mat.color);
				if (lr != null && i == 0)
				{
					lr.startColor = glowMat.mat.color;
					lr.endColor = glowMat.mat.color;
					lr.widthMultiplier = damageMod * DamageMod() / 100;
					lr.SetPositions(new Vector3[] { bulletSpawnLoc.position, hit.point });
				}
                BaseEnemy be = hit.collider.GetComponent<BaseEnemy>();
                if(be == null)
                {
                    be = hit.collider.GetComponentInParent<BaseEnemy>();
                }
				if (be != null)
				{
					if (Random.value < critChance * CritPercentMod())
					{
						be.Hit(damage * damageMod * DamageMod() * critAmount * CritMultMod(), hit.collider);
					}
					else
					{
						be.Hit(damage * damageMod * DamageMod(), hit.collider);
					}
				}
			}
			else
			{
				if (lr != null && i == 0)
				{
					lr.startColor = glowMat.mat.color;
					lr.endColor = glowMat.mat.color;
					lr.widthMultiplier = damageMod * DamageMod() / 100;
					lr.SetPositions(new Vector3[] { bulletSpawnLoc.position, bulletSpawnLoc.position - angle * 100 });
				}
			}
		}
	}

    public override void WeaponUngrabbed(GameObject previousGrabbingObject)
    {
        base.WeaponUngrabbed(previousGrabbingObject);
        if (Tutorial.instance != null)
        {
            if (Tutorial.instance.currentStep == "Drop")
            {
                Tutorial.instance.NextTutorialStep();
            }
        }
    }

    public override float DamageMod()
    {
        return base.DamageMod() + PlayerManager.instance.gunDamageMod / 100;
    }
    public override float SpeedMod()
    {
        return base.SpeedMod() + PlayerManager.instance.gunSpeedMod / 100;
    }
    public override float CritMultMod()
    {
        return base.CritMultMod() + PlayerManager.instance.gunCritMultMod / 100;
    }
    public override float CritPercentMod()
    {
        return base.CritPercentMod() + PlayerManager.instance.gunCritPercentMod / 100;
    }
    public override float RangeMod()
    {
        return base.RangeMod() + PlayerManager.instance.gunRangeModifier / 100;
	}
}