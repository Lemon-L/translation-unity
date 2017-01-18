using UnityEngine;
using System.Collections;
using UnityEngine.UI;  
using UnityEngine.EventSystems; 

using MicrosoftTranslatorSdk;

public class Example : MonoBehaviour {

	public Button translater;
	public InputField input;


	// Use this for initialization
	void Start () {
		translater.onClick.AddListener (OnClick);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnClick(){
		var original = input.text;
		//c# webrequest
		Debug.Log (TranslationSystem.Instance.TranslateMethod (original, "zh-CHS"));

		//unity coroutine(www)
		TranslationSystem.Instance.TranslateMethod (original, "zh-CHS", (arg) => {
			Debug.Log (arg);
		});
	}
}
