using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class TutorialAnimator : MonoBehaviour
{

    public GameObject spawnItem;
    public Transform[] startPositions;
    public Transform[] endPositions;
    public float duration = 1f;
    public bool reverseXRotation = false;
    public string tutorialStep;
	public Head head;
	public Body body;
	public Hand right, left;

    void Start()
	{
		if (head != null) {
			head.Init ();
		}
		if (body != null) {
			body.Init ();
		}
		if (right != null) {
			right.Init ();
		}
		if (left != null) {
			left.Init ();
		}
        tutorialStep = Tutorial.instance.currentStep;
        if(startPositions.Length != endPositions.Length || spawnItem == null)
        {
            return;
        }
        SpawnObjects();
    }

    public void SpawnObjects()
    {
        if(Tutorial.instance != null)
        {
            if(Tutorial.instance.currentStep != tutorialStep)
            {
                Destroy(gameObject);
            }
        }
        for (int i = 0; i < startPositions.Length; i++)
		{
			if (i == 0 && left != null) {
				left.transform.localPosition = startPositions [i].localPosition;
				left.transform.localRotation = startPositions [i].localRotation;
				HOTween.To(left.transform, duration, "localPosition", endPositions[i].localPosition - startPositions[i].localPosition, true, EaseType.EaseOutQuad, 0);
				if (reverseXRotation)
				{
					HOTween.To(left.transform, duration, "localEulerAngles", endPositions[i].localEulerAngles - startPositions[i].localEulerAngles - new Vector3(360, 0, 0), true, EaseType.EaseOutQuad, 0);
				}
				else
				{
					HOTween.To(left.transform, duration, "localEulerAngles", endPositions[i].localEulerAngles - startPositions[i].localEulerAngles, true, EaseType.EaseOutQuad, 0);
				}
			}
			else if (i == 1 && right != null) {
				right.transform.localPosition = startPositions [i].localPosition;
				right.transform.localRotation = startPositions [i].localRotation;
				HOTween.To(right.transform, duration, "localPosition", endPositions[i].localPosition - startPositions[i].localPosition, true, EaseType.EaseOutQuad, 0);
				if (reverseXRotation)
				{
					HOTween.To(right.transform, duration, "localEulerAngles", endPositions[i].localEulerAngles - startPositions[i].localEulerAngles - new Vector3(360, 0, 0), true, EaseType.EaseOutQuad, 0);
				}
				else
				{
					HOTween.To(right.transform, duration, "localEulerAngles", endPositions[i].localEulerAngles - startPositions[i].localEulerAngles, true, EaseType.EaseOutQuad, 0);
				}
			}
            GameObject go = dgUtil.Instantiate(spawnItem, startPositions[i].localPosition, startPositions[i].localRotation, true, startPositions[i].parent);
            go.GetComponent<BaseWeapon>().Init();
            dgUtil.Disable(go);
            HOTween.To(go.transform, duration, "localPosition", endPositions[i].localPosition - startPositions[i].localPosition, true, EaseType.EaseOutQuad, 0);
            if (reverseXRotation)
			{
				HOTween.To(go.transform, duration, "localEulerAngles", endPositions[i].localEulerAngles - startPositions[i].localEulerAngles - new Vector3(360, 0, 0), true, EaseType.EaseOutQuad, 0);
            }
            else
            {
                HOTween.To(go.transform, duration, "localEulerAngles", endPositions[i].localEulerAngles - startPositions[i].localEulerAngles, true, EaseType.EaseOutQuad, 0);
            }
            Destroy(go, duration + 1);
        }
        Invoke("SpawnObjects", duration + 1.5f);
    }
}