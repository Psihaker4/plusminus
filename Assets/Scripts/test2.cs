using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine (A ());
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			StopAllCoroutines ();
		}
	}

	IEnumerator A(){
		for (int i = 0; i < 255; i++) {
			GetComponent<Image> ().color = new Color (1, 1, 1, i /255f);
			yield return new WaitForSeconds (0.1f);
		}
	}
}
