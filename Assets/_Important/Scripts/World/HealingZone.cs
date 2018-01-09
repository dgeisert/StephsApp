using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingZone : MonoBehaviour {
	public float healingPerSecond = 5f;

    public void OnTriggerEnter(Collider col)
    {
        if (PlayerManager.instance.otherPlayerObject != null)
        {
            if (col.GetComponentInParent<PlayerManager>() != null)
            {
                if (!PlayerManager.instance.healingZones.Contains(this))
                {
                    PlayerManager.instance.healingZones.Add(this);
                }
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if (col.GetComponentInParent<PlayerManager>() != null)
        {
            if (PlayerManager.instance.healingZones.Contains(this))
            {
                PlayerManager.instance.healingZones.Remove(this);
            }
        }
    }
}
