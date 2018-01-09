using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponPartType{
	Spike,
	SpearHead,
	AxeHandle,
	AxeHead,
	SickleHead,
	BowHandle,
	BowArrow,
	DecorationGlow3D,
	DecorationGlow2D,
	DecorationNonGlow3D,
	DecorationNonGlow2D,
	SwordHandle,
	SwordBlade,
	SwordGuard,
	Staff,
	StaffFloaties,
	StaffHead,
	StaffSurround,
	GunHandle,
	GunBarrel,
	GunSight,
	MaterialNonGlow,
    MaterialGlow,
	DaggerHandle,
	Shield
}

public class WeaponPart : MonoBehaviour
{

    public BaseWeapon weapon;
    public WeaponPartType partType;
    public string partName = "";
    public int nameIndex = 0;
    public int partIndex = 0;
    public bool glow;
    public Transform attachPoint;
	public WeaponAttachPoint myAttachPoint;
    public List<WeaponAttachPoint> attachPoints = new List<WeaponAttachPoint>();
    List<WeaponPart> weaponParts = new List<WeaponPart>();
	List<string> setPartValues = new List<string>();
    public Material mat;

	public float damageModPercent = 0
		, deathTimeModPercent = 0
		, chargeTimeModePercent = 0
		, bulletSpeedModPercent = 0
		, attackCooldownModPercent = 0
		, knockbackModPercent = 0
		, sneakBonusModPercent = 0
		, critPercentMod = 0
		, critAmountMod = 0;

	public WeaponPart Init(BaseWeapon baseWeapon, WeaponPart glowMat, WeaponPart baseMat)
	{
        if (GetComponent<MeshRenderer>() != null)
        {
            GetComponent<MeshRenderer>().material = glow ? glowMat.mat : baseMat.mat;
		}
		baseWeapon.damage *= (1 + damageModPercent / 100);
		baseWeapon.timeToDeath *= (1 + deathTimeModPercent / 100);
        baseWeapon.specialChargeTime /= (1 + chargeTimeModePercent / 100);
        baseWeapon.projectileSpeed *= (1 + bulletSpeedModPercent / 100);
        baseWeapon.attackCooldown /= (1 + attackCooldownModPercent / 100);
		baseWeapon.knockbackMult *= (1 + knockbackModPercent / 100);
		baseWeapon.sneakDamageBonus *= (1 + sneakBonusModPercent / 100);
		baseWeapon.critAmount *= (1 + critAmountMod / 100);
		baseWeapon.critChance *= (1 + critPercentMod / 100);
        if(mat == null) {
            for (int i = 0; i < transform.childCount; i++)
            {
                WeaponAttachPoint weaponAttachPoint = transform.GetChild(i).GetComponent<WeaponAttachPoint>();
                if (weaponAttachPoint != null)
                {
					bool exclude = false;
					foreach (WeaponAttachPoint wap in weaponAttachPoint.mutuallyExclusivePoints) {
						exclude = exclude || wap.part != null;
					}
					if (weaponAttachPoint.matchingPoint != null) {
						exclude = exclude || weaponAttachPoint.matchingPoint.part == null;
					}
					if (!exclude) {
						if (!attachPoints.Contains (weaponAttachPoint)) {
							attachPoints.Add (weaponAttachPoint);
						}
						bool createPart = !weaponAttachPoint.optional || weaponAttachPoint.rarity / 100 > Random.value;
						if (myAttachPoint != null) {
							createPart = createPart || myAttachPoint.matchingPoint != null;
						}
						if (createPart) {
							int point = 1 + Mathf.FloorToInt (Random.value * WeaponManager.instance.weaponPartCounts [weaponAttachPoint.partType]);
							if (myAttachPoint != null) {
								if (myAttachPoint.matchingPoint != null) {
									weaponAttachPoint.matchingPoint = myAttachPoint.matchingPoint.part.transform.GetChild (i).GetComponent<WeaponAttachPoint> ();
								}
							}
							if (weaponAttachPoint.matchingPoint != null) {
								if (weaponAttachPoint.matchingPoint.part == null) {
									break;
								}
								point = weaponAttachPoint.matchingPoint.part.partIndex;
							}
							GameObject go = dgUtil.Instantiate (
								                                  GameManager.instance.GetResourcePrefab (
									                                  weaponAttachPoint.partType.ToString () + (point).ToString ()
								                                  ),
								                                  weaponAttachPoint.transform.position,
								                                  weaponAttachPoint.transform.rotation,
								                                  true,
								                                  weaponAttachPoint.transform);
							go.transform.localScale = Vector3.one * weaponAttachPoint.scalingFactor;
							WeaponPart wp = go.GetComponent<WeaponPart> ();
							wp.partIndex = point;
							wp.myAttachPoint = weaponAttachPoint;
							weaponAttachPoint.part = wp;
							go.transform.localRotation = Quaternion.Inverse (wp.attachPoint.localRotation);
							go.transform.position += weaponAttachPoint.transform.position - wp.attachPoint.position;
							weaponParts.Add (wp.Init (baseWeapon, glowMat, baseMat));
						}
					}
                }
            }
        }
        return this;
    }
}
