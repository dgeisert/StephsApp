using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class WeaponSwitcher : MonoBehaviour {

    public List<GameObject> weapons = new List<GameObject>();
    VRTK_InteractGrab grab;

    public void Start()
    {
        grab = GetComponent<VRTK_InteractGrab>();
        InvokeRepeating("SwitchWeapon", 0.5f, 0.5f);
    }

    void SwitchWeapon()
    {
        Destroy(grab.grabbedObject);
        grab.ForceRelease();
        GameObject go = dgUtil.Instantiate(weapons[Mathf.FloorToInt(Random.value * weapons.Count)], Vector3.zero, Quaternion.identity);
        go.GetComponent<BaseWeapon>().seed = Mathf.FloorToInt(Random.value * 10000);
        go.GetComponent<BaseWeapon>().Init();
        grab.interactTouch.ForceTouch(go);
        grab.AttemptGrab();
    }
}
