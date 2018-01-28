using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test1 : MonoBehaviour {

	LineRenderer l;

	int k = 0;
	// Use this for initialization
	void Start () {

		l = GetComponent<LineRenderer> ();


		for(int i = 0; i < 361; i++){
			l.SetPosition (i, 100 * new Vector3 (Mathf.Cos (Mathf.Deg2Rad * i), Mathf.Sin (Mathf.Deg2Rad * i)));
		}

	}
	
	// Update is called once per frame
	void Update () {
			l.SetPosition (k, Vector3.zero);
			k++;
	}
}
