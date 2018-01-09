using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetUserTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        dgUtil.ResetUser();
    }
}
