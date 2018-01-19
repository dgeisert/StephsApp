using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingObjectCard : MonoBehaviour {

	public bool isPurchasePackage = false;

	public List<Image> requiredResources;
	public Image producedResource, buildResource;
	public Text title, buildTime, size, buildResourceCount, buildResourceCountShadow, lockLevel;
	public GameObject lockedImage;

	public Text package, packageShadow, price;
	public Image packageImage;



	public bool Init(Building b, PaymentPackage p){
		if (isPurchasePackage) {
			Init (p);
		} else {
			Init (b);
		}
		return isPurchasePackage;
	}

	void Init(Building b){
		for (int i = 0; i < requiredResources.Count; i++) {
			if (b.displayResource.Count > i) {
				requiredResources [i].sprite = ResourceManager.instance.resourceSprites [b.displayResource [i]];
			} else {
				requiredResources [i].enabled = false;
			}
		}
		producedResource.sprite = ResourceManager.instance.resourceSprites [b.producedResource];
		buildResource.sprite = ResourceManager.instance.resourceSprites [b.buildResource];
		buildResourceCount.text = b.buildCost.ToString();
		buildResourceCountShadow.text = b.buildCost.ToString();
		title.text = b.name;
		buildTime.text = dgUtil.FormatTime (b.buildTime);
		size.text = b.size.x + "x" + b.size.y;
		if (GameManager.instance.level < b.unlockLevel) {
			lockedImage.SetActive (true);
			lockLevel.text = "lvl:" + b.unlockLevel.ToString ();
		} else {
			lockedImage.SetActive (false);
		}
	}

	void Init(PaymentPackage p){
		title.text = p.packageName;
		package.text = p.packageAmout.ToString();
		packageShadow.text = p.packageAmout.ToString ();
		price.text = dgUtil.FormatPrice (p.packagePrice);
		packageImage.sprite = p.packageImage;
	}
}