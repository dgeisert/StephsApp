using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class TutorialHighlighter : MonoBehaviour {
    
    //tooltip setup for general use
	GameObject menuCurrentObject, joystickCurrentObject, teleportCurrentObject, triggerCurrentObject, gripLCurrentObject, gripRCurrentObject, gripCurrentObject;
    public VRTK_ObjectTooltip tooltip;

    public Material highlight, noHighlight;

    public void Init()
    {
        gameObject.SetActive(true);
    }

    private void Start()
    {
        if (Tutorial.instance == null)
        {
            gameObject.SetActive(false);
        }
    }

    bool controllerSet = false;
	public void Update()
	{
		if (!controllerSet) 
		{
            SetControls();
        }
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            End();
        }
    }

    public void SetControls()
    {
        switch (VRTK_DeviceFinder.GetHeadsetType())
        {
            case VRTK_DeviceFinder.Headsets.Vive:
            case VRTK_DeviceFinder.Headsets.ViveDVT:
            case VRTK_DeviceFinder.Headsets.ViveMV:
            case VRTK_DeviceFinder.Headsets.OculusRift:
            case VRTK_DeviceFinder.Headsets.OculusRiftCV1:
                if (transform.GetComponentInParent<PlayerManager>() == null)
                {
                    SteamVR_RenderModel svrm = GetComponent<SteamVR_RenderModel>();
                    if (svrm != null)
                    {
                        switch (VRTK_DeviceFinder.GetHeadsetType())
                        {
                            case VRTK_DeviceFinder.Headsets.Vive:
                            case VRTK_DeviceFinder.Headsets.ViveDVT:
                            case VRTK_DeviceFinder.Headsets.ViveMV:
                                svrm.modelOverride = "oculus_cv1_controller_right";
                                svrm.index = SteamVR_TrackedObject.EIndex.Device3;
                                svrm.UpdateModel();
                                break;
                            case VRTK_DeviceFinder.Headsets.OculusRift:
                            case VRTK_DeviceFinder.Headsets.OculusRiftCV1:
                                svrm.modelOverride = "vr_controller_vive_1_5";
                                svrm.index = SteamVR_TrackedObject.EIndex.Device3;
                                svrm.UpdateModel();
                                break;
					default:
                                Debug.Log("couldn't set controller");
                                break;
                        }
                    }
                }
                gameObject.SetActive(true);
                tooltip.gameObject.SetActive(true);
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform tr = transform.GetChild(i);
                    switch (tr.name)
                    {
                        case "grip":
                            gripCurrentObject = tr.gameObject;
                            break;
                        case "lgrip":
                            gripLCurrentObject = tr.gameObject;
                            break;
                        case "rgrip":
                            gripRCurrentObject = tr.gameObject;
                            break;
                        case "trigger":
                            triggerCurrentObject = tr.gameObject;
							break;
						case "thumbstick":
							joystickCurrentObject = tr.gameObject;
							break;
                        case "a_button":
                        case "x_button":
                        case "trackpad":
                            teleportCurrentObject = tr.gameObject;
                            break;
                        case "b_button":
                        case "y_button":
                        case "button":
                            menuCurrentObject = tr.gameObject;
                            break;
                        default:
                            break;
                    }
                }
                controllerSet = true;
                break;
		default:
			break;
        }
        if(GameManager.GetScene() == "Tutorial")
        {
            Invoke("SetStartHighlight", 0.5f);
        }
    }

    void SetStartHighlight()
    {
        ResetHighlights();
        HighlightObject("teleport");
    }

    public void ResetHighlights()
    {
        foreach (MeshRenderer renderer in transform.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = noHighlight;
        }
    }

    public void HighlightObject(string toHighlight)
    {
        GameObject go = gameObject;
        switch (toHighlight)
        {
            case "menu":
                go = menuCurrentObject;
                break;
            case "trigger":
                go = triggerCurrentObject;
                break;
            case "teleport":
                go = teleportCurrentObject;
                break;
			case "joystick":
				go = joystickCurrentObject;
				break;
            case "grip":
                go = gripCurrentObject;
                HighlightObject("gripL");
                HighlightObject("gripR");
                break;
            case "gripL":
                if(gripLCurrentObject == null)
                {
                    return;
                }
                go = gripLCurrentObject;
                break;
            case "gripR":
                if (gripRCurrentObject == null)
                {
                    return;
                }
                go = gripRCurrentObject;
                break;
            default:
                break;
        }
        if(go == null)
        {
            SetControls();
            return;
        }
        if(go.GetComponent<MeshRenderer>() != null)
        {
            go.GetComponent<MeshRenderer>().material = highlight;
        }
        if (go.transform.GetChild(0) != null)
        {
            tooltip.drawLineTo = go.transform.GetChild(0);
        }
        else
        {
            tooltip.drawLineTo = go.transform;
        }
    }

    public void TooltipSet(string tooltipSet)
    {
        tooltip.UpdateText(tooltipSet);
    }

    public void End()
    {
        tooltip.gameObject.SetActive(false);
        if (transform.GetComponentInParent<PlayerManager>() == null)
        {
            gameObject.SetActive(false);
        }
    }
}
