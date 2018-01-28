using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

	public GameObject field;

	GameObject[] timerParts = new GameObject[8];
	GameObject timerNumber;

	Color sColor,eColor;

	int tNumber;
	int pNumber = 0;
	bool paused;

	void Awake () {

		timerNumber = transform.GetChild (0).gameObject;

		for (int i = 0; i < 8; i++) {
			timerParts [i] = transform.GetChild (i + 1).gameObject;
		}

		pNumber = 0;
	}

	public void Stop(){



		StopAllCoroutines ();
		StartCoroutine (RestartTimer ());
	}

	public void SetTimerNumber(int n){
		tNumber = n;
	}

	public IEnumerator StartTimer(){
		
		yield return new WaitForSeconds (0.25f);

		eColor = timerNumber.GetComponent<Text> ().color;
		sColor = eColor - new Color (0, 0, 0, 1);

		StartCoroutine (TimerTik ());
	}

	IEnumerator TimerTik(){
		yield return new WaitWhile (() => paused);
		yield return new WaitForSeconds (0.5f);
		timerNumber.GetComponent<Animation> ().Play ("TimerNumber");
		if((int.Parse( timerNumber.GetComponent<Text>().text)-1)%(tNumber/8)==0){
			timerParts [pNumber].GetComponent<Animation> ().Play ("TimerPart");
		}
		yield return new WaitForSeconds (0.25f);
		timerNumber.GetComponent<Text> ().text = (int.Parse (timerNumber.GetComponent<Text> ().text) - 1).ToString ();

		yield return new WaitForSeconds (0.25f);
		if (int.Parse (timerNumber.GetComponent<Text> ().text) % (tNumber / 8) == 0) {
			timerParts [pNumber].SetActive (false);
			pNumber++;
		}

		if (timerNumber.GetComponent<Text> ().text == "0") {

			field.GetComponent<FieldControlls> ().CheckChoosers ();
			timerNumber.GetComponent<Animation> ().Play ("TimerNumber");
			yield return new WaitForSeconds (0.25f);

			timerNumber.GetComponent<Text> ().text = tNumber.ToString ();

			for (int i = 0; i < 8; i++) {
				if (!timerParts [i].activeSelf) {
					timerParts [i].SetActive (true);
					timerParts [i].GetComponent<Animation> ().Play ("TimerPartReverse");	
				}
			}
			pNumber = 0;
			yield return new WaitForSeconds (0.75f);

		}

		StartCoroutine (TimerTik ());
	}

	IEnumerator RestartTimer(){

		bool b = false;

		timerNumber.GetComponent<Animation> ().Play ("TimerNumber");
		yield return new WaitForSeconds (0.25f);

		timerNumber.GetComponent<Text> ().text = tNumber.ToString ();

		for (int i = 0; i < 8; i++) {
			if (!timerParts [i].activeSelf || timerParts[i].GetComponent<Image>().color.a!=1) {
				timerParts [i].SetActive (true);
				timerParts [i].GetComponent<Animation> ().Play ("TimerPartReverse");	
			}
		}
		pNumber = 0;
		yield return new WaitForSeconds (0.75f);
		StartCoroutine (TimerTik ());
	}

	/*IEnumerator AnimPart(int i, float t, float c){
		timerParts [i].GetComponent<Image> ().color = Color.Lerp (sColor, eColor, (Time.time - t) / c);
		yield return new WaitForEndOfFrame ();
		if (timerParts [i].GetComponent<Image> ().color.a != 1) {
			StartCoroutine (AnimPart (i, t, c));
		}
	}*/


	public void Pause(){
		paused = true;
	}

	public IEnumerator UnPaused(){
		yield return new WaitForSeconds (1);
		paused = false;
	}
}
