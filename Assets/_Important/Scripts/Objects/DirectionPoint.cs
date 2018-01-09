using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionPoint : MonoBehaviour {

    public Text uiText;
    public TextMesh mesh;
 
    private void Start()
    {
        if(PlayerManager.instance != null)
        {
            Init(PlayerManager.instance.transform);
        }
    }

    bool initialized = false;
    public void Init(Transform target, string text = "This Way") {
        if (!initialized)
        {
            initialized = true;
            transform.LookAt(target);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);
            if(mesh != null)
            {
                mesh.text = text;
            }
            if(uiText != null)
            {
                uiText.text = text;
            }
        }
	}
}
