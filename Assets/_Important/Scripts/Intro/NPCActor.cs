using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class NPCActor : MonoBehaviour
{

    public Transform[] leftPositions;
    public Transform[] rightPositions;
    public Transform[] headPositions;
    public Transform[] bodyPositions;
    public Transform[] patrolPositions;
    int leftSet = 0, rightSet = 0, headSet = 0, bodySet = 0, patrolSet = 0;
    public float duration = 1f;
    public int baseSeed = 2;
    public Head head;
    public Body body;
    public Hand right, left;
    public Transform target;
    public GameObject redAura;
    BaseEnemy be;

    void Start()
    {
        if (Tutorial.instance != null)
        {
            Tutorial.instance.actors.Add(this);
        }
        be = GetComponent<BaseEnemy>();
        if (be != null)
        {
            be.patrolPath = new List<Vector3>();
            foreach (Transform t in patrolPositions)
            {
                be.patrolPath.Add(t.position);
            }
            return;
        }
        if (head != null)
        {
            head.seed = baseSeed;
            head.Init();
        }
        if (body != null)
        {
            body.seed = baseSeed;
            body.Init();
        }
        if (right != null)
        {
            right.seed = baseSeed;
            right.Init();
        }
        if (left != null)
        {
            left.seed = baseSeed;
            left.Init();
        }
        SpawnObjects();
    }

    public void SpawnObjects()
    {
        if (rightPositions.Length > 0)
        {
            if (rightSet >= rightPositions.Length)
            {
                rightSet = 0;
            }
            HOTween.To(right.transform, duration, "eulerAngles", rightPositions[rightSet].eulerAngles, false, EaseType.EaseInOutQuad, 0);
            HOTween.To(right.transform, duration, "position", rightPositions[rightSet].position, false, EaseType.EaseInOutQuad, 0);
            rightSet++;
        }
        if (leftPositions.Length > 0)
        {
            if (leftSet >= leftPositions.Length)
            {
                leftSet = 0;
            }
            HOTween.To(left.transform, duration, "eulerAngles", leftPositions[leftSet].eulerAngles, false, EaseType.EaseInOutQuad, 0);
            HOTween.To(left.transform, duration, "position", leftPositions[leftSet].position, false, EaseType.EaseInOutQuad, 0);
            leftSet++;
        }
        if (headPositions.Length > 0)
        {
            if (headSet >= headPositions.Length)
            {
                headSet = 0;
            }
            HOTween.To(head.transform, duration, "eulerAngles", headPositions[headSet].eulerAngles, false, EaseType.EaseInOutQuad, 0);
            HOTween.To(head.transform, duration, "position", headPositions[headSet].position, false, EaseType.EaseInOutQuad, 0);
            headSet++;
        }
        if (bodyPositions.Length > 0)
        {
            if (bodySet >= bodyPositions.Length)
            {
                bodySet = 0;
            }
            HOTween.To(body.transform, duration, "eulerAngles", bodyPositions[bodySet].eulerAngles, false, EaseType.EaseInOutQuad, 0);
            HOTween.To(body.transform, duration, "position", bodyPositions[bodySet].position, false, EaseType.EaseInOutQuad, 0);
            bodySet++;
        }
        if (patrolPositions.Length > 0)
        {
            if (patrolSet >= patrolPositions.Length)
            {
                patrolSet = 0;
            }
            HOTween.To(transform, duration, "eulerAngles", patrolPositions[patrolSet].eulerAngles, false, EaseType.EaseInOutQuad, 0);
            HOTween.To(transform, duration, "position", patrolPositions[patrolSet].position, false, EaseType.EaseInOutQuad, 0);
            patrolSet++;
        }

        Invoke("SpawnObjects", duration);
    }

    public void EvilSwap()
    {
        if (be != null)
        {
            GameObject go = dgUtil.Instantiate(
                redAura
                , transform.position
                , transform.rotation);
            Destroy(go, 5);
            Invoke("TurnEvil", 2.5f);
        }
        else
        {
            Invoke("TurnEvil", 3f);
        }
    }

    public void TurnEvil()
    {
        PlayerManager.instance.isAgroed = true;
        if (be != null)
        {
            be = dgUtil.Instantiate(
                GameManager.instance.GetResourcePrefab(be.name.Split('G')[0] + "Red")
                , transform.position
                , transform.rotation).GetComponent<BaseEnemy>();
            be.target = target.GetComponent<NPCActor>().target;
            be.otherPlayerObject = target.GetComponent<NPCActor>().target.GetComponent<OtherPlayerObject>();
            be.critDamageMult = 0;
            be.normalDamageMult = 0;
            gameObject.SetActive(false);
        }
        else
        {
            if (target != null)
            {
                target.gameObject.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}