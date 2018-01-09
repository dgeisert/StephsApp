using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Tutorial : MonoBehaviour
{

    public static Tutorial instance;

    public List<NPCActor> actors = new List<NPCActor>();

    public Material highlight, noHighlight;
    public List<TutorialHighlighter> highlights = new List<TutorialHighlighter>();
    public GameObject gunDemo, swordDemo, currentDemo;
    public GameObject ship;
    public Transform shipPosition, buddyPosition, buddyExit;
    public Pet pet;
    public int chestsOpened = 0;

    public string currentStep = "";

    string[] allSteps = { "", "Turn", "Teleport", "Chest", "UIPointer", "OpenItemSelect", "SetGun", "CloseMenu", "Grip", "Trigger", "Sword", "RedComet", "Ship", "Wheel", "StartLevel", "End" };
    int stepNum = 0;

    void Awake()
    {
        Ship newShip = dgUtil.Instantiate(ship, shipPosition.position, shipPosition.rotation).GetComponent<Ship>();
        newShip.Init();
    }

    public void Init(TutorialHighlighter[] tutorialHighlighters)
    {
        Tutorial.instance = this;
        PlayerManager.instance.TrackEvent("tutorial_start");
        foreach (TutorialHighlighter th in tutorialHighlighters)
        {
            highlights.Add(th);
            if (th.GetComponent<SteamVR_RenderModel>())
            {
                highlights[0].GetComponent<SteamVR_RenderModel>().modelOverride = th.GetComponent<SteamVR_RenderModel>().renderModelName;
            }
        }
        foreach (TutorialHighlighter th in highlights)
        {
            th.highlight = highlight;
            th.noHighlight = noHighlight;
            th.Init();
        }
        NextTutorialStep();
        PlayerManager.instance.infoCenterAlign.text = "Look at hands for instructions";
        PlayerManager.instance.weaponSettings = new List<string>() {
            PlayerManager.instance.weapons[1]
            , PlayerManager.instance.weapons[1]
            , PlayerManager.instance.weapons[0]
            , PlayerManager.instance.weapons[0] };
        PlayerManager.instance.ImmediateSave();
        PlayerManager.instance.LoadWeapons();
        PlayerManager.instance.savedSettings["turnWithTouchpad"] = false;
        PlayerManager.instance.ToggleTouchpadTurning();
    }

    public void Update()
    {
        //SetTutorialStep(currentStep);
        if (currentStep == "UIPointer")
        {
            foreach (VRTK_ControllerEvents vce in GameObject.FindObjectsOfType<VRTK_ControllerEvents>())
            {
                if (vce.buttonTwoPressed)
                {
                    NextTutorialStep();
                }
            }
        }
        if (currentStep == "DropAll")
        {
            if (PlayerManager.instance.leftManager.grab.grabbedObject == null && PlayerManager.instance.rightManager.grab.grabbedObject == null)
            {
                NextTutorialStep();
            }
        }
        if (currentStep == "CloseMenu")
        {
            if (!PrimaryUI.instance.isWorldLocked)
            {
                NextTutorialStep();
            }
        }
        if (currentStep == "Chest" && chestsOpened > 1)
        {
            NextTutorialStep();
        }
    }

    public void EvilSwap()
    {
        Debug.Log(currentStep + Time.time);
        Ship.instance.comet.SetActive(true);
        foreach (NPCActor a in actors)
        {
            if (a != null)
            {
                a.EvilSwap();
            }
        }
        GameManager.instance.EnemyChecks = new List<System.Action>();
    }

    public void NextTutorialStep()
    {
        stepNum++;
        if (allSteps.Length > stepNum)
        {
            SetTutorialStep(allSteps[stepNum]);
        }
    }

    void SetTutorialStep(string step)
    {
        PlayerManager.instance.TrackEvent("tutorial_step_" + step);
        ResetHighlights();
        currentStep = step;
        switch (step)
        {
            case "UIPointer":
                pet.PlayAudio(pet.clipUi);
                HighlightObject("menu");
                TooltipSet("Press to open\nthe menu.");
                NetworkingUI.instance.disableStart = true;
                break;
            case "Chest":
                pet.PlayAudio(pet.clipChest);
                pet.Invoke("TutorialChest", 3f);
                HighlightObject("teleport");
                TooltipSet("Hit the chest\nto open.");
                break;
            case "RedComet":
                GameObject.FindObjectOfType<RedCometIntro>().StartSequence();
                HighlightObject("none");
                TooltipSet("");
                break;
            case "CloseMenu":
                pet.PlayAudio(pet.clipCloseMenu);
                HighlightObject("menu");
                TooltipSet("Press to close\nthe menu.");
                break;
            case "UITrigger":
                HighlightObject("trigger");
                TooltipSet("Pull trigger to\nselect 'Join Room.'");
                break;
            case "Joining":
                HighlightObject("trigger");
                TooltipSet("Joining...");
                break;
            case "Grip":
                pet.PlayAudio(pet.clipGun);
                if (currentDemo == null)
                {
                    currentDemo = dgUtil.Instantiate(gunDemo, PlayerManager.instance.GetPlayerPosition() + PlayerManager.instance.otherPlayerObject.transform.forward * 3, Quaternion.identity);
                    currentDemo.transform.LookAt(PlayerManager.instance.GetPlayerPosition());
                }
                HighlightObject("trigger");
                TooltipSet("Draw a gun\nlike a cowboy.");
                break;
            case "Trigger":
                currentDemo = null;
                HighlightObject("trigger");
                TooltipSet("Pull trigger\nto fire.");
                break;
            case "Drop":
                HighlightObject("grip");
                TooltipSet("Hold then release\ngrip buttons to drop\nor throw weapons.");
                break;
            case "Sword":
                pet.PlayAudio(pet.clipSword);
                if (currentDemo == null)
                {
                    currentDemo = dgUtil.Instantiate(swordDemo, PlayerManager.instance.GetPlayerPosition() + PlayerManager.instance.otherPlayerObject.transform.forward * 3, Quaternion.identity);
                    currentDemo.transform.LookAt(PlayerManager.instance.GetPlayerPosition());
                }
                HighlightObject("trigger");
                TooltipSet("Pull melee weapon\nfrom over shoulder.");
                break;
            case "Teleport":
                pet.StopWave();
                pet.PlayAudio(pet.clipIntro);
                pet.Invoke("ConditionalAttachAudio2", 12f);
                currentDemo = null;
                HighlightObject("teleport");
                TooltipSet("Hold to teleport.");
                break;
            case "Start":
                HighlightObject("trigger");
                TooltipSet("In menu,\nselect 'Networking'\nthen\nselect 'Start.'");
                break;
            case "OpenItemSelect":
                pet.PlayAudio(pet.clipInventory);
                HighlightObject("trigger");
                TooltipSet("In the inventory,\nselect the Left Hip.");
                break;
            case "SetGun":
                pet.PlayAudio(pet.clipAssignGuns);
                HighlightObject("trigger");
                TooltipSet("Choose a gun\nfor that slot.");
                break;
            case "WeaponSetup":
                HighlightObject("trigger");
                TooltipSet("Set the other\nhip to a gun.");
                break;
            case "DropAll":
                HighlightObject("grip");
                TooltipSet("Drop all\nitems.");
                break;
            case "Turn":
                if (VRTK_DeviceFinder.Headsets.OculusRift == VRTK_DeviceFinder.GetHeadsetType()
                    || VRTK_DeviceFinder.Headsets.OculusRiftCV1 == VRTK_DeviceFinder.GetHeadsetType())
                {
                    HighlightObject("joystick");
                    TooltipSet("Use the joystick\nto turn\nthe view.");
                }
                else
                {
                    HighlightObject("teleport");
                    TooltipSet("Swipe the touchpad\nto turn\nthe view.");
                }
                break;
            case "End":
                End();
                break;
            case "Ship":
                pet.PlayAudio(pet.clipGetToShip);
                pet.Invoke("TutorialExit", 3f);
                HighlightObject("teleport");
                TooltipSet("Head to the boat");
                break;
            case "Wheel":
                HighlightObject("trigger");
                TooltipSet("Turn the wheel\nto select level 1.");
                break;
            case "StartLevel":
                HighlightObject("trigger");
                TooltipSet("Push the 'Start'\nlever to start.");
                break;
            case "MenuPlatform":
                HighlightObject("teleport");
                TooltipSet("Find the boat\ncontrols");
                break;
            default:
                break;
        }
    }

    public void HighlightObject(string toHighlight)
    {
        foreach (TutorialHighlighter th in highlights)
        {
            th.HighlightObject(toHighlight);
        }
    }

    public void ResetHighlights()
    {
        foreach (TutorialHighlighter th in highlights)
        {
            th.ResetHighlights();
        }
    }

    public void TooltipSet(string tooltip)
    {
        PlayerManager.instance.infoCenterAlign.text = string.Concat("Look at hands for buttons.\n", tooltip);
        foreach (TutorialHighlighter th in highlights)
        {
            th.TooltipSet(tooltip);
        }
    }

    public void End()
    {
        foreach (TutorialHighlighter th in highlights)
        {
            th.End();
        }
    }
}