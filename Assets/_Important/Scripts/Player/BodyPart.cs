using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartType{
	clothMat, 
	skinMat,
	decoMat,
	wrist,
	palm,
	thumb,
	pointerFinger,
	grabFinger,
	decoration,
	eye,
	ear,
	head,
	hair,
	hairMat,
	mouth,
	hat,
	shoulder,
	belt,
	chest,
	mouthExpression,
	eyeExpression
}

public class BodyPart : MonoBehaviour
{
    public Transform uiPositionScaling, chestPositionScaling, decoPositionScaling;
    public int position = 0;
    public BodyPart skinMat, decoMat, clothMat;
    WeaponPart deco;
    public Material mat;
    public int seed, bodyPartLevel;

    float levelScalingFactor = 1.05f;

    //these are the possible modifiers from body parts;
    public float health
        , staffDamageMod
        , meleeDamageMod
        , bowDamageMod
        , gunDamageMod
        , overallDamageMod
        , staffCritPercentMod
        , meleeCritPercentMod
        , bowCritPercentMod
        , gunCritPercentMod
        , overallCritPercentMod
        , staffCritMultMod
        , meleeCritMultMod
        , bowCritMultMod
        , gunCritMultMod
        , overallCritMultMod
        , staffSpeedMod
        , meleeSpeedMod
        , bowSpeedMod
        , gunSpeedMod
        , overallSpeedMod
        , bowRangeModifier
        , gunRangeModifier
        , agroDistanceMod
        , lightMod
        , blockChance
        , dodgeChance
        , thornsDamage
        , teleportDistanceMod
        , teleportCooldownMod;

    public virtual void ChangeItemSlot(string setItem)
    {
        PlayerManager.instance.SetBodyPart(position, setItem);
    }

    public void Init(BodyPart baseBodyPart, BodyPart setMat = null)
    {
        float decoMult = 1;
        if (decoPositionScaling != null)
        {
            if (baseBodyPart.seed % 5 == 0)
            {
                if (decoPositionScaling.name.Contains("3"))
                {
                    deco = dgUtil.Instantiate(
                        GameManager.instance.GetResourcePrefab("DecorationGlow3D" + ((baseBodyPart.seed % WeaponManager.instance.weaponPartCounts[WeaponPartType.DecorationGlow3D]) + 1)),
                        Vector3.zero,
                        Quaternion.identity,
                        true,
                        decoPositionScaling).GetComponent<WeaponPart>();
                }
                else
                {
                    deco = dgUtil.Instantiate(
                        GameManager.instance.GetResourcePrefab("DecorationGlow2D" + ((baseBodyPart.seed % WeaponManager.instance.weaponPartCounts[WeaponPartType.DecorationGlow2D]) + 1)),
                        Vector3.zero,
                        Quaternion.identity,
                        true,
                        decoPositionScaling).GetComponent<WeaponPart>();
                }
                deco.transform.localScale = Vector3.one;
                MeshRenderer mr = deco.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    if (baseBodyPart.seed % 2 == 0)
                    {
                        mr.material = baseBodyPart.clothMat.mat;
                    }
                    else
                    {
                        mr.material = baseBodyPart.decoMat.mat;
                    }
                }
                decoMult += deco.critPercentMod + deco.damageModPercent;
            }
        }
        decoMult *= 1 + (levelScalingFactor - 1) * (float)baseBodyPart.bodyPartLevel;
        baseBodyPart.health += health * decoMult;
        baseBodyPart.staffDamageMod += staffDamageMod * decoMult;
        baseBodyPart.meleeDamageMod += meleeDamageMod * decoMult;
        baseBodyPart.bowDamageMod += bowDamageMod * decoMult;
        baseBodyPart.gunDamageMod += gunDamageMod * decoMult;
        baseBodyPart.overallDamageMod += overallDamageMod * decoMult;
        baseBodyPart.staffCritPercentMod += staffCritPercentMod * decoMult;
        baseBodyPart.meleeCritPercentMod += meleeCritPercentMod * decoMult;
        baseBodyPart.bowCritPercentMod += bowCritPercentMod * decoMult;
        baseBodyPart.gunCritPercentMod += gunCritPercentMod * decoMult;
        baseBodyPart.overallCritPercentMod += overallCritPercentMod * decoMult;
        baseBodyPart.staffCritMultMod += staffCritMultMod * decoMult;
        baseBodyPart.meleeCritMultMod += meleeCritMultMod * decoMult;
        baseBodyPart.bowCritMultMod += bowCritMultMod * decoMult;
        baseBodyPart.gunCritMultMod += gunCritMultMod * decoMult;
        baseBodyPart.overallCritMultMod += overallCritMultMod * decoMult;
        baseBodyPart.staffSpeedMod += staffSpeedMod * decoMult;
        baseBodyPart.meleeSpeedMod += meleeSpeedMod * decoMult;
        baseBodyPart.bowSpeedMod += bowSpeedMod * decoMult;
        baseBodyPart.gunSpeedMod += gunSpeedMod * decoMult;
        baseBodyPart.overallSpeedMod += overallSpeedMod * decoMult;
        baseBodyPart.bowRangeModifier += bowRangeModifier * decoMult;
        baseBodyPart.gunRangeModifier += gunRangeModifier * decoMult;
        baseBodyPart.agroDistanceMod += agroDistanceMod * decoMult;
        baseBodyPart.lightMod += lightMod * decoMult;
        baseBodyPart.blockChance += blockChance * decoMult;
        baseBodyPart.dodgeChance += dodgeChance * decoMult;
        baseBodyPart.thornsDamage += thornsDamage * decoMult;
        baseBodyPart.teleportDistanceMod += teleportDistanceMod * decoMult;
        baseBodyPart.teleportCooldownMod += teleportCooldownMod * decoMult;
        if (setMat != null)
        {
            GetComponentInChildren<MeshRenderer>().material = setMat.mat;
        }
    }

    public virtual void Init()
    {
        Random.InitState(seed);
        decoMat = dgUtil.Instantiate(
            GameManager.instance.GetResourcePrefab("decoMat" + Mathf.FloorToInt(Random.value * BodyPartManager.instance.bodyPartCounts[BodyPartType.decoMat])),
            Vector3.zero,
            Quaternion.identity,
            true,
            transform).GetComponent<BodyPart>();
        decoMat.Init(this);
        skinMat = dgUtil.Instantiate(
            GameManager.instance.GetResourcePrefab("skinMat" + Mathf.FloorToInt(Random.value * BodyPartManager.instance.bodyPartCounts[BodyPartType.skinMat])),
            Vector3.zero,
            Quaternion.identity,
            true,
            transform).GetComponent<BodyPart>();
        skinMat.Init(this);
        clothMat = dgUtil.Instantiate(
            GameManager.instance.GetResourcePrefab("clothMat" + Mathf.FloorToInt(Random.value * BodyPartManager.instance.bodyPartCounts[BodyPartType.clothMat])),
            Vector3.zero,
            Quaternion.identity,
            true,
            transform).GetComponent<BodyPart>();
        clothMat.Init(this);
    }
    public void Init(string dataToSet)
    {
        if (dataToSet.Length == 0)
        {
            return;
        }
        if (dataToSet[0] == '.')
        {
            dataToSet = dataToSet.Substring(1);
        }
        int setSeed = -1;
        int.TryParse(dataToSet.Split('.')[0], out setSeed);
        if (setSeed != -1)
        {
            seed = setSeed;
        }
        if (dataToSet.Split('.').Length > 1)
        {
            int setLevel = -1;
            int.TryParse(dataToSet.Split('.')[1], out setLevel);
            if (setLevel != -1)
            {
                bodyPartLevel = setLevel;
            }
        }
        Init();
    }

    public void CullMods()
    {
        float maxDamageMod = Mathf.Max(overallDamageMod, staffDamageMod, meleeDamageMod, gunDamageMod, bowDamageMod);
        overallDamageMod *= overallDamageMod == maxDamageMod ? 1 : 0;
        if (overallDamageMod != 0)
        {
            maxDamageMod++;
        }
        meleeDamageMod *= meleeDamageMod == maxDamageMod ? 1 : 0;
        if (meleeDamageMod != 0)
        {
            maxDamageMod++;
        }
        bowDamageMod *= bowDamageMod == maxDamageMod ? 1 : 0;
        if (bowDamageMod != 0)
        {
            maxDamageMod++;
        }
        gunDamageMod *= gunDamageMod == maxDamageMod ? 1 : 0;
        if (gunDamageMod != 0)
        {
            maxDamageMod++;
        }
        staffDamageMod *= staffDamageMod == maxDamageMod ? 1 : 0;
        if (staffDamageMod != 0)
        {
            maxDamageMod++;
        }

        float maxCritPercent = Mathf.Max(overallCritPercentMod, staffCritPercentMod, meleeCritPercentMod, gunCritPercentMod, bowCritPercentMod);
        overallCritPercentMod *= overallCritPercentMod == maxCritPercent ? 1 : 0;
        if (overallCritPercentMod != 0)
        {
            maxCritPercent++;
        }
        meleeCritPercentMod *= meleeCritPercentMod == maxCritPercent ? 1 : 0;
        if (meleeCritPercentMod != 0)
        {
            maxCritPercent++;
        }
        bowCritPercentMod *= bowCritPercentMod == maxCritPercent ? 1 : 0;
        if (bowCritPercentMod != 0)
        {
            maxCritPercent++;
        }
        gunCritPercentMod *= gunCritPercentMod == maxCritPercent ? 1 : 0;
        if (gunCritPercentMod != 0)
        {
            maxCritPercent++;
        }
        staffCritPercentMod *= staffCritPercentMod == maxCritPercent ? 1 : 0;
        if (staffCritPercentMod != 0)
        {
            maxCritPercent++;
        }
        float maxCritMult = Mathf.Max(bowCritMultMod, gunCritMultMod, meleeCritMultMod, overallCritMultMod, staffCritMultMod);
        overallCritMultMod *= overallCritMultMod == maxCritMult ? 1 : 0;
        if (overallCritMultMod != 0)
        {
            maxCritMult++;
        }
        meleeCritMultMod *= meleeCritMultMod == maxCritMult ? 1 : 0;
        if (meleeCritMultMod != 0)
        {
            maxCritMult++;
        }
        bowCritMultMod *= bowCritMultMod == maxCritMult ? 1 : 0;
        if (bowCritMultMod != 0)
        {
            maxCritMult++;
        }
        gunCritMultMod *= gunCritMultMod == maxCritMult ? 1 : 0;
        if (gunCritMultMod != 0)
        {
            maxCritMult++;
        }
        staffCritMultMod *= staffCritMultMod == maxCritMult ? 1 : 0;
        if (staffCritMultMod != 0)
        {
            maxCritMult++;
        }
        float maxSpeed = Mathf.Max(bowSpeedMod, gunSpeedMod, meleeSpeedMod, overallSpeedMod, staffSpeedMod);
        overallSpeedMod *= overallSpeedMod == maxSpeed ? 1 : 0;
        if (overallSpeedMod != 0)
        {
            maxSpeed++;
        }
        meleeSpeedMod *= meleeSpeedMod == maxSpeed ? 1 : 0;
        if (meleeCritMultMod != 0)
        {
            meleeSpeedMod++;
        }
        bowSpeedMod *= bowSpeedMod == maxSpeed ? 1 : 0;
        if (bowSpeedMod != 0)
        {
            maxSpeed++;
        }
        gunSpeedMod *= gunSpeedMod == maxSpeed ? 1 : 0;
        if (gunSpeedMod != 0)
        {
            maxSpeed++;
        }
        staffSpeedMod *= staffSpeedMod == maxSpeed ? 1 : 0;
        if (staffSpeedMod != 0)
        {
            maxSpeed++;
        }
    }

    public string GetInfoText()
    {
        string infoText = "";
        infoText += "Level: " + bodyPartLevel + "\n";
        infoText += "HP: " + dgUtil.FormatNum(health) + "\n";
        if (staffDamageMod != 0)
        {
            infoText += "Staff Damage: +" + dgUtil.FormatNum(staffDamageMod) + "%\n";
        }
        if (meleeDamageMod != 0)
        {
            infoText += "Melee Damage: +" + dgUtil.FormatNum(meleeDamageMod) + "%\n";
        }
        if (bowDamageMod != 0)
        {
            infoText += "Bow Damage: +" + dgUtil.FormatNum(bowDamageMod) + "%\n";
        }
        if (gunDamageMod != 0)
        {
            infoText += "Gun Damage: +" + dgUtil.FormatNum(gunDamageMod) + "%\n";
        }
        if (overallDamageMod != 0)
        {
            infoText += "All Damage: +" + dgUtil.FormatNum(overallDamageMod) + "%\n";
        }
        if (staffCritPercentMod != 0)
        {
            infoText += "Staff Crit Chance: +" + dgUtil.FormatNum(staffCritPercentMod) + "%\n";
        }
        if (meleeCritPercentMod != 0)
        {
            infoText += "Melee Crit Chance: +" + dgUtil.FormatNum(meleeCritPercentMod) + "%\n";
        }
        if (bowCritPercentMod != 0)
        {
            infoText += "Bow Crit Chance: +" + dgUtil.FormatNum(bowCritPercentMod) + "%\n";
        }
        if (gunCritPercentMod != 0)
        {
            infoText += "Gun Crit Chance: +" + dgUtil.FormatNum(gunCritPercentMod) + "%\n";
        }
        if (overallCritPercentMod != 0)
        {
            infoText += "Crit Chance: +" + dgUtil.FormatNum(overallCritPercentMod) + "%\n";
        }
        if (staffCritMultMod != 0)
        {
            infoText += "Staff Crit Damage: +" + dgUtil.FormatNum(staffCritMultMod) + "%\n";
        }
        if (meleeCritMultMod != 0)
        {
            infoText += "Melee Crit Damage: +" + dgUtil.FormatNum(meleeCritMultMod) + "%\n";
        }
        if (bowCritMultMod != 0)
        {
            infoText += "Bow Crit Damage: +" + dgUtil.FormatNum(bowCritMultMod) + "%\n";
        }
        if (gunCritMultMod != 0)
        {
            infoText += "Gun Crit Damage: +" + dgUtil.FormatNum(gunCritMultMod) + "%\n";
        }
        if (overallCritMultMod != 0)
        {
            infoText += "Crit Damage: +" + dgUtil.FormatNum(overallCritMultMod) + "%\n";
        }
        if (staffSpeedMod != 0)
        {
            infoText += "Staff Attack Speed: +" + dgUtil.FormatNum(staffSpeedMod) + "%\n";
        }
        if (meleeSpeedMod != 0)
        {
            infoText += "Melee Attack Speed: +" + dgUtil.FormatNum(meleeSpeedMod) + "%\n";
        }
        if (bowSpeedMod != 0)
        {
            infoText += "Bow Attack Speed: +" + dgUtil.FormatNum(bowSpeedMod) + "%\n";
        }
        if (gunSpeedMod != 0)
        {
            infoText += "Gun Attack Speed: +" + dgUtil.FormatNum(gunSpeedMod) + "%\n";
        }
        if (overallSpeedMod != 0)
        {
            infoText += "Attack Speed: +" + dgUtil.FormatNum(overallSpeedMod) + "%\n";
        }
        if (bowRangeModifier != 0)
        {
            infoText += "Bow Range: +" + dgUtil.FormatNum(bowRangeModifier) + "%\n";
        }
        if (gunRangeModifier != 0)
        {
            infoText += "Gun Range: +" + dgUtil.FormatNum(gunRangeModifier) + "%\n";
        }
        if (agroDistanceMod != 0)
        {
            infoText += "Agro Distance: -" + dgUtil.FormatNum(agroDistanceMod) + "%\n";
        }
        if (lightMod != 0)
        {
            infoText += "Light Brightness: +" + dgUtil.FormatNum(lightMod) + "%\n";
        }
        if (blockChance != 0)
        {
            infoText += "Block Chance: +" + dgUtil.FormatNum(blockChance) + "%\n";
        }
        if (dodgeChance != 0)
        {
            infoText += "Dodge Chance: +" + dgUtil.FormatNum(dodgeChance) + "%\n";
        }
        if (thornsDamage != 0)
        {
            infoText += "Thorns Damage: +" + dgUtil.FormatNum(thornsDamage);
        }
        if (teleportDistanceMod != 0)
        {
            infoText += "Teleport Distance: +" + dgUtil.FormatNum(teleportDistanceMod) + "%\n";
        }
        if (teleportCooldownMod != 0)
        {
            infoText += "Teleport Cooldown: -" + dgUtil.FormatNum(teleportCooldownMod) + "%\n";
        }
        return infoText;
    }
}
