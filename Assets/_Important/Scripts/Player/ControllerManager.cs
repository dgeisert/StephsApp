using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ControllerManager : MonoBehaviour {

	public VRTK_InteractGrab grab;
	public VRTK_InteractTouch touch;
	public VRTK_InteractUse use;
	public VRTK_UIPointer uiPointer;
	public VRTK_Pointer teleportPointer, menuPointer;
	public VRTK_BezierPointerRenderer teleportRenderer;
    public VRTK_ControllerEvents events;
    public ControllerManager otherController;
	public float TeleportCooldown = 0.3f;
	public bool is_left = false;
    Vector3 touchpadMove;
    Vector2 touchpadAngle;

    bool controllerSet = false;
    public void Update()
    {
        if (!controllerSet)
        {
            Init();
        }
        if (PlayerManager.instance.GetSetting("touchpadMovement") && is_left)
        {
            if (PlayerManager.instance.otherPlayerObject != null && touchpadMove != Vector3.zero)
            {
                float a = transform.eulerAngles.y / 180 * Mathf.PI;
                touchpadMove = new Vector3(
                    Mathf.Cos(a) * touchpadAngle.x + Mathf.Sin(a) * touchpadAngle.y,
                    0,
                    -Mathf.Sin(a) * touchpadAngle.x + Mathf.Cos(a) * touchpadAngle.y).normalized;
                RaycastHit hit;
                Physics.Raycast(PlayerManager.instance.otherPlayerObject.transform.position + Vector3.up / 5 + touchpadMove, -Vector3.up, out hit, 5, (1 << 0));
                if (hit.collider != null)
                {
                    if(hit.collider.tag == "Teleportable")
                    {
                        PlayerManager.instance.transform.position += touchpadMove * Time.deltaTime * 2;
                    }
                }
            }
        }
    }

    public void Init()
    {
        controllerSet = true;
        SetControls();
        events = GetComponentInParent<VRTK_ControllerEvents>();
        events.TouchpadAxisChanged += new ControllerInteractionEventHandler(DoTouchpadAxisChanged);
        if (VRTK_DeviceFinder.Headsets.OculusRift == VRTK_DeviceFinder.GetHeadsetType()
            || VRTK_DeviceFinder.Headsets.OculusRiftCV1 == VRTK_DeviceFinder.GetHeadsetType())
        {
            events.ButtonOnePressed += new ControllerInteractionEventHandler(OnTeleportPressed);
        }
        else
        {
            events.TouchpadPressed += new ControllerInteractionEventHandler(OnTeleportPressed);
            events.TouchpadTouchStart += new ControllerInteractionEventHandler(TouchpadStart);
            events.TouchpadTouchEnd += new ControllerInteractionEventHandler(TouchpadEnd);
        }
        events.ButtonTwoPressed += new ControllerInteractionEventHandler(MenuButtonPress);
        events.TriggerPressed += new ControllerInteractionEventHandler(Trigger);
        if (PlayerManager.instance.leftManager == this)
        {
            otherController = PlayerManager.instance.rightManager;
			is_left = true;
        }
        else
        {
            otherController = PlayerManager.instance.leftManager;
        }
    }

    public void SetControls()
    {
        uiPointer.activationMode = VRTK_UIPointer.ActivationMethods.ToggleButton;
        switch (VRTK_DeviceFinder.GetHeadsetType())
        {
            case VRTK_DeviceFinder.Headsets.OculusRift:
            case VRTK_DeviceFinder.Headsets.OculusRiftCV1:
				grab.grabButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                use.useButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                menuPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                menuPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress;
                teleportPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.ButtonOnePress;
                teleportPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.ButtonOnePress;
                uiPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress;
                uiPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                break;
            case VRTK_DeviceFinder.Headsets.Vive:
            case VRTK_DeviceFinder.Headsets.ViveDVT:
			case VRTK_DeviceFinder.Headsets.ViveMV:
			default:
				grab.grabButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                use.useButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                menuPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                menuPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress;
                teleportPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TouchpadPress;
                teleportPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.TouchpadPress;
                uiPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress;
                uiPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                break;
        }
    }

    public void Die()
    {
        grab.ForceRelease();
        use.useButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
        grab.grabButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
        teleportPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
        teleportPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
        uiPointer.activationMode = VRTK_UIPointer.ActivationMethods.AlwaysOn;
    }

    public bool centered = false;
	private void OnTeleportPressed(object sender, ControllerInteractionEventArgs e)
	{
		if (PrimaryUI.instance.isWorldLocked)
		{
			PrimaryUI.instance.WorldLockToggle();
		}
	}

	Vector2 touchpadStart;
	float touchTimeStart;
    private void TouchpadStart(object sender, ControllerInteractionEventArgs e)
    {
        touchTimeStart = Time.time;
        touchpadStart = e.touchpadAxis;
        centered = true;
    }
    private void TouchpadEnd(object sender, ControllerInteractionEventArgs e)
    {
        if (is_left)
        {
            touchpadMove = Vector3.zero;
        }
    }
    
    private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
	{
		if(PlayerManager.instance.GetSetting("touchpadMovement") && is_left)
        {
            if (e.touchpadAxis.magnitude > 0.3f)
            {
                touchpadAngle = e.touchpadAxis;
                float a = transform.eulerAngles.y / 180 * Mathf.PI;
				touchpadMove = new Vector3 (
					Mathf.Cos (a) * touchpadAngle.x + Mathf.Sin(a) * touchpadAngle.y, 
					0,
					-Mathf.Sin (a) * touchpadAngle.x + Mathf.Cos(a) * touchpadAngle.y).normalized;
			}
		}
        else if (allowTouchpadTurns)
		{
			if (VRTK_DeviceFinder.Headsets.OculusRift == VRTK_DeviceFinder.GetHeadsetType()
				|| VRTK_DeviceFinder.Headsets.OculusRiftCV1 == VRTK_DeviceFinder.GetHeadsetType())
			{
				if (e.touchpadAxis.x < 0.5f && e.touchpadAxis.x > -0.5f && !centered)
				{
					centered = true;
				}
				if (e.touchpadAxis.x > 0.8f && centered)
				{
					centered = false;
					PlayerManager.instance.transform.RotateAround(PlayerManager.instance.head.transform.position, new Vector3(0, 1, 0), 45f);
					if (Tutorial.instance != null)
					{
						if (Tutorial.instance.currentStep == "Turn")
						{
							Tutorial.instance.NextTutorialStep();
						}
					}
				}
				if (e.touchpadAxis.x < -0.8f && centered)
				{
					centered = false;
					PlayerManager.instance.transform.RotateAround(PlayerManager.instance.head.transform.position, new Vector3(0, 1, 0), -45f);
					if (Tutorial.instance != null)
					{
						if (Tutorial.instance.currentStep == "Turn")
						{
							Tutorial.instance.NextTutorialStep();
						}
					}
				}
			}
			else
			{
				if (Time.time - touchTimeStart < 1f && centered) {
					if (e.touchpadAxis.x - touchpadStart.x > 0.5f) {
						centered = false;
						PlayerManager.instance.transform.RotateAround(PlayerManager.instance.head.transform.position, new Vector3(0, 1, 0), 45f);
						if (Tutorial.instance != null)
						{
							if (Tutorial.instance.currentStep == "Turn")
							{
								Tutorial.instance.NextTutorialStep();
							}
						}
					}
					if (e.touchpadAxis.x - touchpadStart.x < -0.5f) {
						centered = false;
						PlayerManager.instance.transform.RotateAround(PlayerManager.instance.head.transform.position, new Vector3(0, 1, 0), -45f);
						if (Tutorial.instance != null)
						{
							if (Tutorial.instance.currentStep == "Turn")
							{
								Tutorial.instance.NextTutorialStep();
							}
						}
					}
				}
			}
        }
    }

	private void MenuButtonPress(object sender, ControllerInteractionEventArgs e)
	{
		PrimaryUI.instance.WorldLockToggle ();
    }

    private void Trigger(object sender, ControllerInteractionEventArgs e)
    {
        if (grab.grabbedObject == null && otherController.grab.grabbedObject != null && !PrimaryUI.instance.isWorldLocked)
        {
            if (otherController.grab.grabbedObject.GetComponentInParent<Bow>() != null)
            {
                touch.ForceTouch(otherController.grab.grabbedObject.GetComponentInParent<Bow>().bowString.gameObject);
            }
        }
    }

    bool allowTouchpadTurns = false;
    public void ToggleTouchpadTurning(bool allow)
    {
        allowTouchpadTurns = allow;
    }

    public void Drop()
    {
        GameObject destroyGo = grab.grabbedObject;
        if (destroyGo != null)
        {
            if (destroyGo.GetComponent<BaseWeapon>() != null)
            {
                Destroy(destroyGo);
                grab.ForceRelease();
            }
        }
    }
}
