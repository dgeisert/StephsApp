namespace VRTK.Examples
{
    using UnityEngine;
    using UnityEngine.UI;
	using System.Collections.Generic;

    public class UI_Keyboard : MonoBehaviour
    {
        public InputField focus;
		public List<InputField> inputs;
		public Text typeText, sizeText;

        public void ClickKey(string character)
        {
			focus.text += character;
        }

		public void ToggleType(){
			if ((int)LevelManager.instance.custom.type < 4) {
				LevelManager.instance.custom.type = (LevelType) ((int)LevelManager.instance.custom.type + 1);
			} else {
				LevelManager.instance.custom.type = (LevelType)0;
			}
			typeText.text = "Level Type: " + LevelManager.instance.custom.type;
		}
		public void ToggleSize(){
			if (LevelManager.instance.custom.sizeMax < 140) {
				LevelManager.instance.custom.sizeMax += 20;
			} else {
				LevelManager.instance.custom.sizeMax = 40;
			}
			sizeText.text = "Island Sizes: " + (LevelManager.instance.custom.sizeMax / 20 - 1).ToString();
		}

        public void Backspace()
        {
			if (focus.text.Length > 0)
            {
				focus.text = focus.text.Substring(0, focus.text.Length - 1);
            }
        }

        public void Enter()
        {
			focus = focus.GetComponent<CustomInputField>().NextFocus ();
        }

        private void Start()
        {
			focus = inputs [0];
			for (int i = 0; i < inputs.Count; i++){
				if(i + 1 == inputs.Count){
					inputs [i].GetComponent<CustomInputField> ().nextInput = inputs [0];
				}else{
					inputs [i].GetComponent<CustomInputField> ().nextInput = inputs [i+1];
				}
				inputs [i].GetComponent<CustomInputField> ().keyboard = this;
			}
			ToggleSize ();
			ToggleType ();
        }
    }
}