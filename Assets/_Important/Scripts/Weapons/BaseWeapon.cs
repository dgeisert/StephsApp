using UnityEngine;
using System.Collections.Generic;
using VRTK;

public class BaseWeapon : VRTK_InteractableObject
{
    public Transform bulletSpawnLoc;
    public ProjectileSpawnLocation[] bulletSpawnLocs;
    public float damage = 1;
    public float timeToDeath = 10, startDeathTimer;
    public float attackCooldown = 0.5f;
    public float sneakDamageBonus = 1.5f;
    public float knockbackMult = 1f;
    public float projectileSpeed = 1000f;
    public float critChance = 0.1f;
    public float critAmount = 1.2f;
    public float speciaRate = 0.5f;
    public bool triggerHeld = false;
    public string side = "";
    public GameObject deathParticles, chargingParticles, chargedParticles, chargingParticlesInstance, chargedParticlesInstance;
    public AudioClip weaponSound, specialChargedAudio;
    public AudioSource audioSource;
    public GameObject grabbingObject;
    PhotonView pv;
    public RigidbodySync rbs;
    public float specialChargeTime = 0, specialCooldownTime = 0;
    public string special = "";
    public List<string> possibleSpecials;
    public bool specialCharged = false;
    public Transform uiPositionScaling, chestPositionScaling;
    public GameObject projectilePrefab;
    public WeaponAttachPoint weaponBaseAttachPoint;
    public List<WeaponPart> weaponParts = new List<WeaponPart>();
    public int seed = -1, weaponLevel = -1;
    public WeaponPart glowMat, baseMat;
    public GameObject spark;
    bool initialized = false;

    float levelScalingFactor = 1.05f;

    private void Start()
    {
        Init();
        if (Tutorial.instance != null)
        {
            dgUtil.GhostMode(gameObject);
        }
    }

    public void Init()
    {
        if (initialized)
        {
            return;
        }
        initialized = true;
        spark = PlayerManager.instance.spark;
        pv = GetComponent<PhotonView>();
        if (deathParticles == null)
        {
            deathParticles = GameManager.instance.GetResourcePrefab("deathParticles");
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = weaponSound;
        if (weaponBaseAttachPoint != null)
        {
            if (weaponBaseAttachPoint != null)
            {
                if (seed == -1)
                {
                    seed = Mathf.FloorToInt(Random.value * 10000);
                }
                Random.InitState(seed);
                int point = 1 + Mathf.FloorToInt(Random.value * WeaponManager.instance.weaponPartCounts[WeaponPartType.MaterialGlow]);
                glowMat = GameManager.instance.GetResourcePrefab(
                        "MaterialGlow" + (point).ToString()
                    ).GetComponent<WeaponPart>();
                point = 1 + Mathf.FloorToInt(Random.value * WeaponManager.instance.weaponPartCounts[WeaponPartType.MaterialNonGlow]);
                baseMat = GameManager.instance.GetResourcePrefab(
                        "MaterialNonGlow" + (point).ToString()
                    ).GetComponent<WeaponPart>();
                glowMat.Init(this, glowMat, baseMat);
                baseMat.Init(this, glowMat, baseMat);
                point = 1 + Mathf.FloorToInt(Random.value * WeaponManager.instance.weaponPartCounts[weaponBaseAttachPoint.partType]);
                GameObject go = dgUtil.Instantiate(
                    GameManager.instance.GetResourcePrefab(
                        weaponBaseAttachPoint.partType.ToString() + (point).ToString()
                    ),
                weaponBaseAttachPoint.transform.position,
                weaponBaseAttachPoint.transform.rotation,
                    true,
                    weaponBaseAttachPoint.transform);
                go.transform.localScale = Vector3.one * weaponBaseAttachPoint.scalingFactor;
                WeaponPart wp = go.GetComponent<WeaponPart>();
                wp.partIndex = point;
                weaponBaseAttachPoint.part = wp;
                go.transform.localRotation = Quaternion.Inverse(wp.attachPoint.localRotation);
                go.transform.position += weaponBaseAttachPoint.transform.position - wp.attachPoint.position;
                weaponParts.Add(wp.Init(this, glowMat, baseMat));
            }
            damage *= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
            timeToDeath *= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
            specialChargeTime /= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
            projectileSpeed *= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
            attackCooldown /= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
            knockbackMult *= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
            sneakDamageBonus *= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
            critAmount *= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
            critChance *= 1 + (levelScalingFactor - 1) * (float)weaponLevel;
        }
        special = "";
        if (Random.value < speciaRate)
        {
            if (possibleSpecials != null)
            {
                if (possibleSpecials.Count > 0)
                {
                    special = possibleSpecials[Mathf.FloorToInt(Random.value * possibleSpecials.Count)];
                }
            }
        }
        if(glowMat != null)
        {
            if (bulletSpawnLoc != null)
            {
                chargingParticlesInstance = dgUtil.Instantiate(chargingParticles, bulletSpawnLoc.position, bulletSpawnLoc.rotation, false, transform);
                chargedParticlesInstance = dgUtil.Instantiate(chargedParticles, bulletSpawnLoc.position, bulletSpawnLoc.rotation, false, transform);
            }
            else
            {
                chargingParticlesInstance = dgUtil.Instantiate(chargingParticles, transform.position, transform.rotation, false, transform);
                chargedParticlesInstance = dgUtil.Instantiate(chargedParticles, transform.position, transform.rotation, false, transform);
            }
            ParticleSystem psCharging = chargingParticlesInstance.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule mainCharging = psCharging.main;
            mainCharging.startColor = new Color(glowMat.mat.color.r, glowMat.mat.color.g, glowMat.mat.color.b, 0.2f);
            ParticleSystem psCharged = chargedParticlesInstance.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule mainCharged = psCharged.main;
            mainCharged.startColor = new Color(glowMat.mat.color.r, glowMat.mat.color.g, glowMat.mat.color.b, 0.2f);
            chargingParticlesInstance.SetActive(false);
            chargedParticlesInstance.SetActive(false);
        }
        WeaponStart();
    }

    public virtual void WeaponStart()
    {

    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        NetworkGrab();
        base.OnInteractableObjectGrabbed(e);
        grabbingObject = e.interactingObject;
        WeaponGrabbed(e.interactingObject);
    }

    void NetworkGrab()
    {
        if (PlayerManager.instance.otherPlayerObject == null)
        {
            return;
        }
        int previousRGrab = PlayerManager.instance.otherPlayerObject.rightGrab;
        PlayerManager.instance.otherPlayerObject.rightGrab = -1;
        if (PlayerManager.instance.rightManager.grab.grabbedObject != null)
        {
            PhotonView rpv = PlayerManager.instance.rightManager.grab.grabbedObject.GetComponent<PhotonView>();
            if (rpv != null)
            {
                PlayerManager.instance.otherPlayerObject.rightGrab = rpv.viewID;
            }
            BaseWeapon rbw = PlayerManager.instance.rightManager.grab.grabbedObject.GetComponent<BaseWeapon>();
            if (rbw != null)
            {
                rbw.side = "r";
                rbs.SendPosition();
            }
            if (previousRGrab != PlayerManager.instance.otherPlayerObject.rightGrab && previousRGrab != -1)
            {
                PhotonView prgpv = PhotonView.Find(previousRGrab);
                if (prgpv != null)
                {
                    BaseWeapon prgpvbw = prgpv.GetComponent<BaseWeapon>();
                    if (prgpvbw != null)
                    {
                        prgpvbw.side = "";
                    }
                }
            }
        }
        int previousLGrab = PlayerManager.instance.otherPlayerObject.leftGrab;
        PlayerManager.instance.otherPlayerObject.leftGrab = -1;
        if (PlayerManager.instance.leftManager.grab.grabbedObject != null)
        {
            PhotonView lpv = PlayerManager.instance.leftManager.grab.grabbedObject.GetComponent<PhotonView>();
            if (lpv != null)
            {
                PlayerManager.instance.otherPlayerObject.leftGrab = lpv.viewID;
            }
            BaseWeapon lbw = PlayerManager.instance.leftManager.grab.grabbedObject.GetComponent<BaseWeapon>();
            if (lbw != null)
            {
                lbw.side = "l";
                rbs.SendPosition();
            }
            if (previousLGrab != PlayerManager.instance.otherPlayerObject.leftGrab && previousLGrab != -1)
            {
                PhotonView plgpv = PhotonView.Find(previousLGrab);
                if (plgpv != null)
                {
                    BaseWeapon plgpvbw = plgpv.GetComponent<BaseWeapon>();
                    if (plgpvbw != null)
                    {
                        plgpvbw.side = "";
                    }
                }
            }
        }
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectUngrabbed(e);
        transform.SetParent(null);
        startDeathTimer = Time.time;
        WeaponUngrabbed(e.interactingObject);
        side = "";
        grabbingObject = null;
        rbs.SendPosition(true);
    }

    public virtual void WeaponGrabbed(GameObject currentGrabbingObject)
    {

    }

    public virtual void WeaponUngrabbed(GameObject previousGrabbingObject)
    {

    }

    public void Update()
    {
        if (pv != null)
        {
            if (pv.isMine || (pv.owner == null && PhotonNetwork.isMasterClient) || NetworkManager.instance.singlePlayer)
            {
                if (triggerHeld && special != "")
                {
                    specialChargeTime += Time.deltaTime;
                }
                else
                {
                    specialChargeTime = 0;
                    chargingParticlesInstance.SetActive(false);
                    chargedParticlesInstance.SetActive(false);
                }
                if (!triggerHeld && special != "")
                {
                    specialCooldownTime += Time.deltaTime;
                }
                if (specialChargeTime > 0 && special != "")
                {
                    switch (special)
                    {
                        case "Power Shot":
                            if (specialChargeTime > 1f)
                            {
                                GenericSpeacialReady();
                            }
                            break;
                        case "Mega":
                            if (specialChargeTime > 3f)
                            {
                                GenericSpeacialReady();
                            }
                            break;
                        case "Flame":
                            if (specialChargeTime > 2f)
                            {
                                GenericSpeacialReady();
                            }
                            break;
                        case "Multi Shot":
                            if (specialChargeTime > 1f)
                            {
                                GenericSpeacialReady();
                            }
                            break;
                        case "Homing":
                            if (specialChargeTime > 2f)
                            {
                                GenericSpeacialReady();
                            }
                            break;
                        case "Splitting":
                            if (specialChargeTime > 2f)
                            {
                                GenericSpeacialReady();
                            }
                            break;
                        case "Rapid Fire":
                            if (specialChargeTime > 2f)
                            {
                                GenericSpeacialReady();
                            }
                            break;
                        case "Reflect":
                            break;
                        case "Push":
                            if (specialChargeTime > 2f)
                            {
                                GenericSpeacialReady();
                            }
                            break;
                        case "Fireball":
                            if (specialChargeTime > 2f)
                            {
                                GenericSpeacialReady();
                            }
						break;
					case "Frost Nova":
						if (specialChargeTime > 4f)
						{
							GenericSpeacialReady();
						}
						break;
					case "Flame Thrower":
						if (specialChargeTime > 6f)
						{
							GenericSpeacialReady();
						}
						break;
                        default:
                            break;
                    }
                    if (specialChargeTime > 0.5f && !specialCharged)
                    {
                        if (chargingParticlesInstance != null)
                        {
                            chargingParticlesInstance.SetActive(true);
                            chargedParticlesInstance.SetActive(false);
                        }
                    }
                    else if(!specialCharged)
                    {
                        if (chargingParticlesInstance != null)
                        {
                            chargingParticlesInstance.SetActive(false);
                            chargedParticlesInstance.SetActive(false);
                        }
                    }
                    else
                    {
                        if (chargingParticlesInstance != null)
                        {
                            chargingParticlesInstance.SetActive(false);
                            chargedParticlesInstance.SetActive(true);
                        }
                    }
                }
                else
                {
                    chargingParticlesInstance.SetActive(false);
                    chargedParticlesInstance.SetActive(false);
                }
                if (startDeathTimer + timeToDeath < Time.time && !IsGrabbed())
                {
                    DeathParticles();
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
        WeaponUpdate();
    }

    void GenericSpeacialReady()
    {
        if (chargedParticlesInstance != null)
        {
            chargingParticlesInstance.SetActive(false);
            chargedParticlesInstance.SetActive(true);
        }
        PlayAudio(specialChargedAudio);
        specialCharged = true;
    }

    public virtual void WeaponUpdate()
    {

    }

    public override void OnInteractableObjectUsed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectUsed(e);
        if (GetComponent<BowString>() == null)
        {
            triggerHeld = true;
        }
        specialCharged = false;
        specialChargeTime = 0;
        VRTK_InteractGrab vig = e.interactingObject.GetComponent<VRTK_InteractGrab>();
        if (vig == null)
        {
            PullTrigger(e.interactingObject);
        }
        if (vig.grabbedObject == gameObject)
        {
            PullTrigger(e.interactingObject);
        }
        else if (CanPickup(vig))
        {
            vig.AttemptGrab();
        }
        else
        {
            PullTrigger(e.interactingObject);
        }
    }

    public override void OnInteractableObjectUnused(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectUnused(e);
        ReleaseTrigger(e.interactingObject.gameObject);
        triggerHeld = false;
        specialChargeTime = 0;
    }

    bool CanPickup(VRTK_InteractGrab hand)
    {
        if (hand == null)
        {
            return false;
        }
        if (hand.grabbedObject != null)
        {
            return false;
        }
        if (GetComponent<VRTK_InteractGrab>() == null)
        {
            return false;
        }
        return true;
    }

    public virtual void PullTrigger(GameObject usingObject)
    {

    }

    public virtual void ReleaseTrigger(GameObject previousUsingObject)
    {

    }

    void OnCollisionEnter(Collision col)
    {
        if (pv == null)
        {
            return;
        }
        if (glowMat != null)
        {
            dgUtil.Instantiate(spark, col.contacts[0].point, Quaternion.identity).GetComponent<Spark>().Init(glowMat.mat.color);
        }
        else
        {
            dgUtil.Instantiate(spark, col.contacts[0].point, Quaternion.identity).GetComponent<Spark>().Init(Color.green);
        }
        if ((pv.isMine || NetworkManager.instance.singlePlayer) && col.transform.GetComponentInParent<BaseEnemy>() != null)
        {
            BaseEnemy be = col.transform.GetComponentInParent<BaseEnemy>();
            HitEnemy(be, col);
        }
        else if ((pv.isMine || NetworkManager.instance.singlePlayer) && col.transform.GetComponentInParent<BaseWeapon>() != null)
        {
            HitWeapon(col.transform.GetComponentInParent<BaseWeapon>(), col);
        }
        else if ((pv.isMine || NetworkManager.instance.singlePlayer) && col.collider.GetComponent<Bullet>() != null)
        {
            HitBullet(col.transform.GetComponentInParent<Bullet>(), col);
        }
        else if ((pv.isMine || NetworkManager.instance.singlePlayer) && col.collider.GetComponentInParent<TreasureChest>() != null)
        {
            if (IsGrabbed())
            {
                col.transform.GetComponentInParent<TreasureChest>().AttemptOpen();
            }
        }
        else if ((pv.isMine || NetworkManager.instance.singlePlayer) && col.collider.GetComponentInParent<WishingWell>() != null)
        {
            if (IsGrabbed())
            {
                col.transform.GetComponentInParent<WishingWell>().Roll();
            }
        }
        else if ((pv.isMine || NetworkManager.instance.singlePlayer) && col.collider.GetComponentInParent<Cage>() != null)
        {
            col.transform.GetComponentInParent<Cage>().Claim();
        }
        else if (col.collider.GetComponentInParent<VRTK_Control>())
        {
            Destroy(gameObject);
        }
        else if (col.transform.GetComponent<PlayAudio>())
        {
        }
        else
        {
            OtherCollision(col);
        }
    }

    public Dictionary<BaseEnemy, float> hitLimits = new Dictionary<BaseEnemy, float>();
    public virtual void HitEnemy(BaseEnemy be, Collision col)
    {
        if (hitLimits.ContainsKey(be))
        {
            if (hitLimits[be] + (attackCooldown * SpeedMod()) > Time.time)
            {
                return;
            }
            hitLimits[be] = Time.time;
        }
        else
        {
            hitLimits.Add(be, Time.time);
        }
        if (be.target == null)
        {
            float doDamage = damage * sneakDamageBonus * DamageMod();
            if (Random.value < (critChance * CritPercentMod()))
            {
                doDamage *= critAmount * CritMultMod();
            }
            be.Hit(doDamage, col.collider);
        }
        else
        {
            float doDamage = damage * DamageMod();
            if (Random.value < critChance * CritPercentMod())
            {
                doDamage *= critAmount * CritMultMod();
            }
            be.Hit(doDamage, col.collider);
        }
        VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.63f, 0.2f, 0.01f);
        PlayAudio(weaponSound);
        CleanUpHitLimits();
    }
    void CleanUpHitLimits()
    {
        Dictionary<BaseEnemy, float> toRemove = new Dictionary<BaseEnemy, float>();
        foreach (KeyValuePair<BaseEnemy, float> kvp in hitLimits)
        {
            if (kvp.Value - Time.time > 0.5f)
            {
                toRemove.Add(kvp.Key, kvp.Value);
            }
        }
        foreach (KeyValuePair<BaseEnemy, float> kvp in toRemove)
        {
            hitLimits.Remove(kvp.Key);
        }
    }

    public virtual void HitWeapon(BaseWeapon bw, Collision col)
    {
        VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.63f, 0.2f, 0.01f);
        PlayAudio(weaponSound);
    }

    public virtual void HitBullet(Bullet b, Collision col)
    {
        VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.63f, 0.2f, 0.01f);
    }

    public virtual void OtherCollision(Collision col)
    {
        /*
        VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(grabbingObject), 0.63f, 0.2f, 0.01f);
        if (audioSource != null)
        {
            audioSource.clip = weaponSound;
            audioSource.Play();
        }
        */
    }

    public void DeathParticles()
    {
        if (GetComponentInChildren<Renderer>() == null)
        {
            return;
        }
        if (deathParticles != null)
        {
            GameObject deathParticlesInstance = dgUtil.Instantiate(deathParticles, Vector3.zero, Quaternion.identity);
            ParticleSystem ps = deathParticlesInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ParticleSystem.MainModule psm = ps.main;
                ParticleSystem.ShapeModule pss = ps.shape;
                ps.transform.position = transform.position;
                ps.transform.rotation = transform.rotation;
            }
            Destroy(deathParticlesInstance, 5);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (info.photonView != null)
        {
            if (info.photonView.instantiationData != null)
            {
                if (info.photonView.instantiationData.Length > 0)
                {
                    SetData(info.photonView.instantiationData[0].ToString());
                }
            }
        }
    }

    public void SetData(string dataToSet)
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
                weaponLevel = setLevel;
            }
        }
    }

    public virtual string GetInfoText()
    {
        string infoText = "";
        infoText += "Damage: " + dgUtil.FormatNum(damage * DamageMod()) + "\n";
        infoText += "Cooldown: " + dgUtil.FormatNum(attackCooldown * SpeedMod()) + "s\n";
        infoText += "Level: " + weaponLevel + "\n";
        if (sneakDamageBonus != 1)
        {
            infoText += "Sneak Bonus: " + dgUtil.FormatNum((sneakDamageBonus - 1) * 100) + "%\n";
        }
        if (knockbackMult != 1)
        {
            infoText += "Knockback Bonus: " + dgUtil.FormatNum((knockbackMult - 1) * 100) + "%\n";
        }
        if (critChance != 0)
        {
            infoText += "Crit Chance: " + dgUtil.FormatNum((critChance) * 100) + "%\n";
        }
        if (critAmount != 1)
        {
            infoText += "Crit Damage: +" + dgUtil.FormatNum((critAmount - 1) * 100) + "%\n";
        }
        if (special != "")
        {
            infoText += "Special: " + special;
        }
        return infoText;
    }

    public virtual float DamageMod()
    {
        return 1 + PlayerManager.instance.overallDamageMod / 100;
    }
    public virtual float SpeedMod()
    {
        return 1 + PlayerManager.instance.overallSpeedMod / 100;
    }
    public virtual float CritMultMod()
    {
        return 1 + PlayerManager.instance.overallCritMultMod / 100;
    }
    public virtual float CritPercentMod()
    {
        return 1 + PlayerManager.instance.overallCritPercentMod / 100;
    }
    public virtual float RangeMod()
    {
        return 1;
    }

    void PlayAudio(AudioClip clip)
    {
        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.pitch = Random.value * 0.4f + 0.8f;
            audioSource.Play();
        }
    }
}