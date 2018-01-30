using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingObjectCard : MonoBehaviour {

	public bool isPurchasePackage = false;

	public List<Image> requiredResources;
	public Image producedResource, buildResource;
	public Text title, buildTime, size, buildResourceCount, buildResourceCountShadow, lockLevel;
	public GameObject lockedImage, activeSection;

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

	public void Init(Building b){
		title.text = b.singular;
		if (GameManager.instance.level < b.unlockLevel) {
			lockedImage.SetActive (true);
			lockLevel.text = "lvl:" + b.unlockLevel.ToString ();
			//GetComponent<Button> ().enabled = false;
			activeSection.SetActive(false);
		} else {
			lockedImage.SetActive (false);
			activeSection.SetActive(true);
			GetComponent<Button> ().enabled = true;
			for (int i = 0; i < requiredResources.Count; i++) {
				if (b.displayResource.Count > i) {
					//Debug.Log (b.displayResource [i]);
					requiredResources [i].sprite = ResourceManager.instance.resourceSprites [b.displayResource [i]];
				} else {
					requiredResources [i].enabled = false;
				}
			}
			//Debug.Log (b.producedResource);
			producedResource.sprite = ResourceManager.instance.resourceSprites [b.producedResource];
			buildResource.sprite = ResourceManager.instance.resourceSprites [b.buildResource];
			int bCount = 0;
			foreach (Building cb in ResourceManager.instance.constructedBuildings) {
				bCount += (cb.name == b.name) ? 1 : 0;
			}
			b.buildCost = Mathf.FloorToInt (b.buildCostBase * Mathf.Pow (b.costIncrease, bCount));
			buildResourceCount.text = b.buildCost.ToString();
			buildResourceCountShadow.text = b.buildCost.ToString();
			buildTime.text = dgUtil.FormatTime (b.buildTime);
			size.text = b.size.x + "x" + b.size.y;
		}
	}

	public void Init(PaymentPackage p){
		title.text = p.packageName;
		package.text = p.packageAmout.ToString();
		packageShadow.text = p.packageAmout.ToString ();
		price.text = dgUtil.FormatPrice (p.packagePrice);
		packageImage.sprite = p.packageImage;
	}
}