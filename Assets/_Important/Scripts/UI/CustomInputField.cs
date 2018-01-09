using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomInputField : InputField {

	public InputField nextInput;
	public VRTK.Examples.UI_Keyboard keyboard;

	public InputField NextFocus(){
		return nextInput;
	}

	public override void OnSelect (UnityEngine.EventSystems.BaseEventData eventData)
	{
		base.OnSelect (eventData);
		keyboard.focus = this;
	}

}
