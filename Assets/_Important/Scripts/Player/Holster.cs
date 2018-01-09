using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;
using VRTK.GrabAttachMechanics;

public class Holster : VRTK_InteractableObject
{

    public GameObject specialHold;
    public string specialHoldData;
    public int holsterPosition = 0;
    public Material ghost, highlight, blocked;
    public MeshRenderer holsterRenderer;

    public void Update()
    {
        if (PlayerManager.instance.rightManager.touch.GetTouchedObject() == gameObject
            && IsTouched())
        {
            holsterRenderer.material = highlight;
        }
        else if (PlayerManager.instance.leftManager.touch.GetTouchedObject() == gameObject
            && IsTouched())
        {
            holsterRenderer.material = highlight;
        }
        else
        {
            holsterRenderer.material = ghost;
        }
    }
    public override void OnInteractableObjectUnused(InteractableObjectEventArgs e)
    {
        if (e.interactingObject != null)
        {
            if (e.interactingObject.GetComponentInParent<SteamVR_TrackedObject>() != null)
            {
                if (CheckForTouchOverride(e.interactingObject))
                {
                    VRTK.VRTK_InteractGrab vrtkG = e.interactingObject.GetComponentInParent<VRTK.VRTK_InteractGrab>();
                    BaseWeapon bw = vrtkG.grabbedObject.GetComponent<BaseWeapon>();
                    if (bw != null)
                    {
                        bw.OnInteractableObjectUnused(e);
                    }
                    return;
                }
            }
        }
        base.OnInteractableObjectUnused(e);
    }

    public override void OnInteractableObjectUsed(InteractableObjectEventArgs e)
    {
        if (e.interactingObject != null)
        {
            if (e.interactingObject.GetComponentInParent<SteamVR_TrackedObject>() != null)
            {
                VRTK.VRTK_InteractGrab vrtkG = e.interactingObject.GetComponentInParent<VRTK.VRTK_InteractGrab>();
                if (CheckForTouchOverride(e.interactingObject))
                {
                    BaseWeapon bw = vrtkG.grabbedObject.GetComponent<BaseWeapon>();
                    if(bw != null)
                    {
                        bw.OnInteractableObjectUsed(e);
                    }
                    return;
                }
                GameObject go = PlayerManager.instance.SpawnWeapon(specialHold, specialHoldData);
                if (go == null)
                {
                    return;
                }
                VRTK_InteractableObject io = go.GetComponent<VRTK_InteractableObject>();
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
                    vrtkG.ForceRelease();
                    vrtkG.interactTouch.ForceTouch(io.gameObject);
                    vrtkG.AttemptGrab();
                }
            }
        }
        base.OnInteractableObjectUsed(e);
    }

    public void ChangeItemSlot(string setItem)
	{
		PlayerManager.instance.SetWeapon(this, setItem);
    }

    public void ToggleHolster(bool show)
    {
        holsterRenderer.gameObject.SetActive(show);
    }

	public override void OnInteractableObjectTouched (InteractableObjectEventArgs e)
    {
        if (CheckForTouchOverride(e.interactingObject))
        {
            return;
        }
        base.OnInteractableObjectTouched (e);
	}

    public void OnTriggerEnter(Collider other)
    {
        VRTK.VRTK_InteractGrab vrtkG = other.GetComponentInParent<VRTK.VRTK_InteractGrab>();
        if (vrtkG != null)
        {
            if (CheckForTouchOverride(other.gameObject))
            {
                return;
            }
            other.GetComponentInParent<VRTK.VRTK_InteractUse>().ForceResetUsing();
            vrtkG.interactTouch.ForceTouch(gameObject);
        }
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        if (CheckForTouchOverride(e.interactingObject))
        {
            VRTK.VRTK_InteractGrab vrtkG = e.interactingObject.GetComponentInParent<VRTK.VRTK_InteractGrab>();
            BaseWeapon bw = vrtkG.grabbedObject.GetComponent<BaseWeapon>();
            if (bw != null)
            {
                bw.OnInteractableObjectUnused(e);
            }
            return;
        }
        base.OnInteractableObjectUngrabbed(e);
    }

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        if (CheckForTouchOverride(e.interactingObject))
        {
            VRTK.VRTK_InteractGrab vrtkG = e.interactingObject.GetComponentInParent<VRTK.VRTK_InteractGrab>();
            BaseWeapon bw = vrtkG.grabbedObject.GetComponent<BaseWeapon>();
            if (bw != null)
            {
                bw.OnInteractableObjectUsed(e);
            }
            return;
        }
        base.OnInteractableObjectGrabbed(e);
    }


    public bool CheckForTouchOverride(GameObject go)
    {
        VRTK.VRTK_InteractGrab vrtkG = go.GetComponentInParent<VRTK.VRTK_InteractGrab>();
        if (vrtkG != null)
        {
            if (vrtkG.grabbedObject != null)
            {
                BaseWeapon bw = vrtkG.grabbedObject.GetComponent<BaseWeapon>();
                if (bw != null)
                {
                    if (bw.triggerHeld)
                    {
                        return true;
                    }
                    if (vrtkG.grabbedObject.name == specialHold.name)
                    {
                        if (bw.seed.ToString() == PlayerManager.instance.weaponSettings[holsterPosition].Split('.')[1])
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}