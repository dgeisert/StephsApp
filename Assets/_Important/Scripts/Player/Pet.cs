using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using Holoville.HOTween;

public class Pet : BaseEnemy
{
	public static Pet instance;
    public Wobble wobble;
    public VRTK_InteractableObject vrtkIO;
    public Transform rightUp1, rightUp2, leftUp1, leftUp2;
    Vector3 rightInitial, leftInitial;
    public Transform body, right, left;
    public float waveDuration = 0.5f;
    bool wave = false;

    //tutorial clips
    public AudioClip clipHi, clipIntro, clipTurn, clipTeleport, clipUi, clipCloseMenu
    , clipSword, clipGun, clipInventory, clipAssignGuns, clipRedCometArrives
    , clipGetToShip, clipAfterFirstTeleport, clipLevel1, clipChest
	, clipSurvivalWaveComplete;

	public List<AudioClip> clipsNewWave, clipsMainMenu, clipsRescued, clipsBattleOver, clipsFindFire, clipsNextToFire;

    //general clips
    public AudioClip clipExitStart, clipSurvivalStart, clipWaveStart, clipTimedStart, clipRescueStart, clipBossStart, clipCredits;

    public override void CheckDeath()
    {
        NewPatrolPoint();
        lootTable.RollTable();
        hitPoints = 1;
        target = null;
        Invoke("BackToPlayer", 5);
    }

    public override void DetermineMovement()
    {
        if (target == null && patrolBox != null)
        {
            if (movingTo == null)
            {
                NewPatrolPoint();
            }
            if (Vector3.Distance(movingTo, transform.position) < 1)
            {
                NewPatrolPoint();
            }
        }
        else if (target != null)
        {
            movingTo = target.position;
        }
    }

    public override void EnemyStart()
    {
		instance = this;
        BackToPlayer();
        vrtkIO.InteractableObjectTouched += Petting;
        vrtkIO.InteractableObjectUntouched += StopPetting;
        rightInitial = right.localPosition;
        leftInitial = left.localPosition;
        switch (GameManager.GetScene())
        {
            case "Tutorial":
                PlayAudio(clipHi);
                Invoke("ConditionalAttachAudio", 3f);
                Wave();
                Tutorial.instance.pet = this;
                break;
		case "MainMenu":
			if (PlayerPrefs.GetInt ("currentLevel") == 0) {
				PlayAudio (clipAfterFirstTeleport);
			} else {
				PlayAudio (clipsMainMenu);
			}
                break;
            case "islandgen":
                if (CreateLevel.instance.level == 0)
                {
                    PlayAudio(clipLevel1);
                }
                else
                {
                    switch (CreateLevel.instance.levelType)
                    {
                        case LevelType.Boss:
                            PlayAudio(clipBossStart);
                            break;
                        case LevelType.Exit:
                            PlayAudio(clipExitStart);
                            break;
                        case LevelType.Survive:
                            PlayAudio(clipSurvivalStart);
                            break;
                        case LevelType.Wave:
                            PlayAudio(clipWaveStart);
                            break;
                        case LevelType.Timed:
                            PlayAudio(clipTimedStart);
                            break;
                        case LevelType.InfiniteWave:
                            PlayAudio(clipWaveStart);
                            break;
				case LevelType.Rescue:
					if (CreateLevel.instance.timeInLevel < 1) {
						PlayAudio (clipRescueStart);
					} else {
						PlayAudio (clipsRescued);
					}
                            break;
                        default:
                            break;
                    }
                }
                break;
            case "Credits":
                break;
            default:
                break;
        }
        transform.LookAt(PlayerManager.instance.head);
    }

    private void Petting(object sender, InteractableObjectEventArgs e)
    {
        //cute audio
    }

    private void StopPetting(object sender, InteractableObjectEventArgs e)
    {

    }

    public void Wave()
    {
        wave = true;
        Wave1();
    }
    public void StopWave()
    {
        wave = false;
    }

    void Wave1()
    {
        HOTween.To(right, waveDuration, "localEulerAngles", rightUp1.transform.localEulerAngles, false, EaseType.EaseInOutQuad, 0);
        HOTween.To(right, waveDuration, "localPosition", rightUp1.transform.localPosition, false, EaseType.EaseInOutQuad, 0);
        HOTween.To(left, waveDuration, "localEulerAngles", leftUp1.transform.localEulerAngles, false, EaseType.EaseInOutQuad, 0);
        HOTween.To(left, waveDuration, "localPosition", leftUp1.transform.localPosition, false, EaseType.EaseInOutQuad, 0);
        if (wave)
        {
            Invoke("Wave2", waveDuration);
        }
        else
        {
            ResetHands();
        }
    }

    void Wave2()
    {
        HOTween.To(right, waveDuration, "localEulerAngles", rightUp2.transform.localEulerAngles, false, EaseType.EaseInOutQuad, 0);
        HOTween.To(right, waveDuration, "localPosition", rightUp2.transform.localPosition, false, EaseType.EaseInOutQuad, 0);
        HOTween.To(left, waveDuration, "localEulerAngles", leftUp2.transform.localEulerAngles, false, EaseType.EaseInOutQuad, 0);
        HOTween.To(left, waveDuration, "localPosition", leftUp2.transform.localPosition, false, EaseType.EaseInOutQuad, 0);
        if (wave)
        {
            Invoke("Wave1", waveDuration);
        }
        else
        {
            ResetHands();
        }
    }

    void ResetHands()
    {
        HOTween.To(right, waveDuration, "localPosition", rightInitial, false, EaseType.EaseInOutQuad, 0);
        HOTween.To(left, waveDuration, "localPosition", leftInitial, false, EaseType.EaseInOutQuad, 0);
    }

    public override void EnemyUpdate()
    {
        if (target != null)
        {
            transform.LookAt(target.position);
        }
    }

    public void BackToPlayer()
    {
        target = PlayerManager.instance.head;
    }

    public override void PlayAudio(AudioClip clip)
    {
        if (!PlayerManager.instance.GetSetting("buddyAudio"))
        {
            return;
        }
        if (clip == null)
        {
            return;
        }
        audioSource.clip = clip;
        audioSource.Play();
    }

	public void PlayAudio(List<AudioClip> clips){
		if (clips != null) {
			if (clips.Count > 0) {
				AudioClip clip = clips [Mathf.FloorToInt (Random.value * clips.Count)];
                PlayAudio(clip);
			}
		}
	}

    void ConditionalAttachAudio()
    {
        if (audioSource.clip == clipHi)
        {
            PlayAudio(clipTurn);
        }
	}

	void ConditionalAttachAudio2()
	{
		if (audioSource.clip == clipIntro)
		{
			PlayAudio(clipTeleport);
		}
	}

	public void ConditionalAttachAudio3()
	{
		if (!clipsNextToFire.Contains(audioSource.clip))
		{
			PlayAudio(clipsNextToFire);
		}
	}

    void TutorialChest()
    {
        target = Tutorial.instance.buddyPosition;
    }

    void TutorialExit()
    {
        target = Tutorial.instance.buddyExit;
    }
}
