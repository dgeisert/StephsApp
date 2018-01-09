using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherCredit : MonoBehaviour {

	public GameObject focus;
	public string name, credit;
	public List<string> lines;
	public TextMesh creditText;

	// Use this for initialization
	void Start () {
		if (lines.Count == 0) {
			creditText.text = name + "\nby " + credit;
		} else {
			creditText.text = "";
			foreach (string line in lines) {
				creditText.text += line + '\n';
			}
		}
	}
}
