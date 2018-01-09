using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class LootDrop : MonoBehaviour
{

    Vector3 target;
    public string resource;
    public int amount;
    public Color flytextColor;
    public string specialText = "";
    bool readyToGrant = false;

    // Use this for initialization
    public void Start()
    {
        HOTween.To(transform, 3, "position", Vector3.up * 2, true, EaseType.EaseInOutQuad, 0);
        Invoke("MoveToPlayer", 3);
    }

    void MoveToPlayer()
    {
        readyToGrant = true;
        target = PlayerManager.instance.head.position - Vector3.up;
        HOTween.To(transform, Mathf.Pow(Vector3.Distance(transform.position, target), 0.2f), "position", target, false, EaseType.EaseInOutQuad, 0);
    }

    Vector3 setPosition, setAngle, setScale;
    public void NextFrameSet(Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
    {
        setPosition = localPosition;
        setAngle = localEulerAngles;
        setScale = localScale;
        StartCoroutine("SetPosition");
    }

    public IEnumerator SetPosition()
    {
        yield return null;
        transform.localPosition = setPosition;
        transform.localEulerAngles = setAngle;
        transform.localScale = setScale;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, target) < 1 && readyToGrant)
        {
            DoGrant();
        }
    }

    void DoGrant()
    {
        switch (resource)
        {
            case "weapon":
            case "hand":
            case "head":
            case "body":
                break;
            default:
                PlayerManager.instance.AddResource(resource, amount);
                break;
        }
        Destroy(gameObject);
    }
}
