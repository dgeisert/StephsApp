using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{

    public static Compass instance;

    public GameObject pointer;
    public List<GuidePoint> guidePoints;
    List<GameObject> pointers;

    void Awake()
    {
        instance = this;
        guidePoints = new List<GuidePoint>();
        foreach (GuidePoint gp in GameObject.FindObjectsOfType<GuidePoint>())
        {
            guidePoints.Add(gp);
        }
        pointers = new List<GameObject>();
    }

    void Update()
    {
        Quaternion currentRot = transform.rotation;
        transform.rotation = Quaternion.identity;
        if (transform.rotation != currentRot)
        {
            OnEnable();
        }
    }

    void OnEnable()
    {
        while (pointers.Count > guidePoints.Count)
        {
            Destroy(pointers[pointers.Count - 1]);
            pointers.RemoveAt(pointers.Count - 1);
        }
        while (pointers.Count < guidePoints.Count)
        {
            pointers.Add(dgUtil.Instantiate(pointer, Vector3.zero, Quaternion.identity, true, transform));
        }
        int i = 0;
        foreach (GuidePoint gp in guidePoints)
        {
            if (gp.gameObject.GetActive())
            {
                Vector3 pos = new Vector3(
                    gp.transform.position.x - PlayerManager.instance.GetPlayerPosition().x
                , 0
                , gp.transform.position.z - PlayerManager.instance.GetPlayerPosition().z
                              ).normalized;
                pos = new Vector3(pos.x, 0.2f, pos.z);
                pointers[i].SetActive(true);
                pointers[i].transform.position = transform.position + pos;
                i++;
            }
            else
            {
                pointers[i].SetActive(false);
            }
        }
    }
}
