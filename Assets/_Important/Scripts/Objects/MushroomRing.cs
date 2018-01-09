using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomRing : MonoBehaviour {

	public float[] sizes = new float[]{0.2f, 0.3f, 2f, 3f, 4f};
    Dictionary<OtherPlayerObject, float> cooldown = new Dictionary<OtherPlayerObject, float>();

	void OnTriggerEnter(Collider col){
		PlayerManager pm = col.GetComponentInParent<PlayerManager> ();
		if (pm != null && col.GetComponentInParent<ControllerManager>() == null) {
            if (cooldown.ContainsKey(pm.otherPlayerObject))
            {
                if(cooldown[pm.otherPlayerObject] + 10 > Time.time)
                {
                    return;
                }
            }

			pm.ScaleUp (sizes [Mathf.FloorToInt (Random.value * sizes.Length)]);

            if (!cooldown.ContainsKey(pm.otherPlayerObject))
            {
                cooldown.Add(pm.otherPlayerObject, Time.time);
                return;
            }
            cooldown[pm.otherPlayerObject] = Time.time;
        }
	}
}
