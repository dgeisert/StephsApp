﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaymentPackage : MonoBehaviour {

	public string packageName;
	public float packagePrice;
	public int packageAmout;
	public Sprite packageImage;

	public void Purchase(){
		ResourceManager.instance.AddResource (Resource.Gold, packageAmout, null, true);
		MenuManager.instance.CloseMenu ();
	}
}
