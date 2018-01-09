using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.UnityEventHelper;

public class StartSpawnLever : MonoBehaviour {

    public GameObject spawningObject;

    private VRTK_Control_UnityEvents controlEvents;

    private void Start()
    {
        controlEvents = GetComponent<VRTK_Control_UnityEvents>();
        if (controlEvents == null)
        {
            controlEvents = gameObject.AddComponent<VRTK_Control_UnityEvents>();
        }

        controlEvents.OnValueChanged.AddListener(HandleChange);
    }

    private void HandleChange(object sender, VRTK.Control3DEventArgs e)
    {
        spawningObject.SetActive(e.normalizedValue > 90);
    }
}
