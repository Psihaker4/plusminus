using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
using System;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
//using UnityEngine.Advertisements;

public class GameUI : MonoBehaviour {

	static Color[] modeColors = new Color[] {
		new Color (128 / 255f, 1, 128 / 255f),
		new Color (1, 1, 128 / 255f),
		new Color (1, 128 / 255f, 64 / 255f),
		new Color (1, 64 / 255f, 64 / 255f),
		new Color (128 / 255f, 192 / 255f, 1)
	};

	public GameObject loseScreen;
	public GameObject pauseScreen;
	public GameObject field;
	public GameObject bottomBar;
	public Text scoreText, recordText, worldRecordText, statsText, stats2Test;
	public GameObject timer;
	public GameObject audioC;
	public GameObject progressColor;

	public string adBannerId;
	public string adBanner2Id;
	public string adInterId;
	public string adInter2Id;

	public int tNumber;

	FieldControlls fieldControlls;

	GameObject chooser;
	GameObject activeCell;

	bool paused;
	int modeNumber;
	int score;
	bool timerStop, timerPartsStop;
	public bool timerAnimation1,timerAnimation2;
	public bool timerNumberAnimation1, timerNumberAnimation2;
	public bool backTimerAnimation;
	float timerTime, timerNumberTime;
	public int timerPart = 0;
	float barLength;

	InterstitialAd inter;
	RewardBasedVideoAd inter2;

	BannerView banner;
	NativeExpressAdView banner2;

	void Start () {
		
		switch (PlayerPrefs.GetString ("Mode")) {
		case "Easy":
			modeNumber = 0;
			tNumber = 32;
			break;
		case "Medium":
			modeNumber = 1;
			tNumber = 24;
			break;
		case "Hard":
			modeNumber = 2;
			tNumber = 16;
			break;
		case "Insane":
			modeNumber = 3;
			tNumber = 8;
			transform.GetChild (0).GetChild (4).GetChild (6).GetComponent<Text> ().text = "  end";
			break;
		}

		audioC.transform.GetChild (modeNumber).gameObject.SetActive (true);
		audioC.transform.GetChild (modeNumber).GetComponent<AudioSource> ().Play ();

		if (PlayerPrefs.GetString ("Sound") == "on") {
			pauseScreen.transform.GetChild(5).GetChild (1).gameObject.SetActive (true);
			pauseScreen.transform.GetChild(5).GetChild (2).gameObject.SetActive (false);
		} else {
			pauseScreen.transform.GetChild(5).GetChild (1).gameObject.SetActive (false);
			pauseScreen.transform.GetChild(5).GetChild (2).gameObject.SetActive (true);
		}

		if (PlayerPrefs.GetString ("Music") == "on") {
			pauseScreen.transform.GetChild(4).GetChild (1).gameObject.SetActive (true);
			pauseScreen.transform.GetChild(4).GetChild (2).gameObject.SetActive (false);
			Firebase.Analytics.FirebaseAnalytics.LogEvent ("start_game_with_music");
		} else {
			pauseScreen.transform.GetChild(4).GetChild (1).gameObject.SetActive (false);
			pauseScreen.transform.GetChild(4).GetChild (2).gameObject.SetActive (true);
			audioC.transform.GetChild (modeNumber).GetComponent<AudioSource> ().Pause ();
			Firebase.Analytics.FirebaseAnalytics.LogEvent ("start_game_without_music");
		}


		fieldControlls = field.GetComponent<FieldControlls> ();



		PlayerPrefs.SetInt ("Attempts" + PlayerPrefs.GetString ("Mode"), PlayerPrefs.GetInt ("Attempts" + PlayerPrefs.GetString ("Mode")) + 1);

		timer.transform.GetChild(0).GetComponent<Text>().text = tNumber.ToString ();
		scoreText.text = "0";
		recordText.text = PlayerPrefs.GetInt ("BestScore" + PlayerPrefs.GetString ("Mode")).ToString ();

		progressColor.GetComponent<Image> ().color = modeColors [modeNumber];

		transform.GetChild (0).GetChild (0).GetChild(1).GetComponent<Text> ().color = modeColors [modeNumber];
		transform.GetChild (0).GetChild (1).GetChild(1).GetComponent<Text> ().color = modeColors [modeNumber];
		transform.GetChild (0).GetChild (1).GetChild(2).GetComponent<Text> ().color = modeColors [modeNumber];
		transform.GetChild (0).GetChild (2).GetChild(1).GetComponent<Text> ().color = modeColors [modeNumber];
		transform.GetChild (0).GetChild (3).GetChild(1).GetComponent<Text> ().color = modeColors [modeNumber];

		transform.GetChild (1).GetComponent<Image> ().color = modeColors [modeNumber];
		transform.GetChild (2).GetComponent<Image> ().color = modeColors [modeNumber];

		transform.GetChild (3).GetChild (0).GetChild(0).GetComponent<Text> ().color = modeColors [modeNumber];

		loseScreen.transform.GetChild (2).GetComponent<Text> ().color = modeColors [modeNumber] - new Color (0, 0, 0, 1);
		worldRecordText.color = modeColors [modeNumber] - new Color (0, 0, 0, 1);

		barLength = fieldControlls.GetFullScore ();

		timer.transform.GetChild(0).GetComponent<Text>().color = modeColors [modeNumber];
		for (int i = 1; i < 9; i++) {
			timer.transform.GetChild(i).GetComponent<Image> ().color = modeColors [modeNumber];
		}

		StartCoroutine (StartCellsAnimations ());

		if (UnityEngine.Random.Range (0, 2) == 0) {
			banner = new BannerView (adBannerId, AdSize.SmartBanner, AdPosition.Bottom);
			banner.LoadAd (new AdRequest.Builder ().Build ());
		} else {
			banner2 = new NativeExpressAdView (adBanner2Id, AdSize.SmartBanner, AdPosition.Bottom);
			banner2.LoadAd (new AdRequest.Builder ().Build ());
		}

		inter = new InterstitialAd (adInterId);
		inter.LoadAd (new AdRequest.Builder ().Build ());

		/*inter2 = RewardBasedVideoAd.Instance;
		inter2.LoadAd (new AdRequest.Builder ().Build (), adInter2Id);
*/

		Firebase.Analytics.FirebaseAnalytics.LogEvent ("start_game_" + PlayerPrefs.GetString ("Mode"));
		Firebase.Analytics.FirebaseAnalytics.LogEvent ("start_game_all_modes");



	}

	void Update(){


		progressColor.transform.localPosition = Vector3.Lerp(new Vector3(-390, 0),Vector3.zero,int.Parse(scoreText.text)/barLength);
		progressColor.transform.localScale = Vector3.Lerp(new Vector3(0,1,1),Vector3.one,int.Parse(scoreText.text)/barLength);

		if (score > int.Parse (scoreText.text)) {
			scoreText.text = (int.Parse (scoreText.text) + 1).ToString ();
		}

		if (Input.GetMouseButtonDown (0)) {

			Vector2 worldPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast (worldPoint, Vector2.zero);

			if (hit.collider != null){

				if (hit.collider.CompareTag ("Chooser")) {
					chooser = hit.collider.gameObject;
					chooser.GetComponent<Chooser> ().Choose ();
					fieldControlls.FadeCells (chooser.GetComponent<Chooser> ().GetNumber ());
					if (PlayerPrefs.GetString ("Sound") == "on") {
						chooser.GetComponent<AudioSource> ().Play ();
					}
				}

			}

		}

		if (Input.GetMouseButton (0)) {

			Vector2 worldPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast (worldPoint, Vector2.zero);
			if (chooser != null) { 
				if (hit.collider != null && hit.collider.CompareTag ("Cell") && !hit.collider.GetComponent<Cell> ().IsFade ()) {

					if (activeCell != null && hit.collider.gameObject!=activeCell && activeCell.GetComponent<Cell>().IsChoose()) {
						activeCell.GetComponent<Animation> ().Play ("CellAntiChoose");
						activeCell.GetComponent<Cell> ().AntiChoose ();
					}

					activeCell = hit.collider.gameObject;

					if (!activeCell.GetComponent<Cell> ().IsChoose ()) {
						activeCell.GetComponent<Cell> ().Choose ();
						activeCell.GetComponent<Animation> ().Play ("CellChoose");
					}

				} else {
					if (activeCell != null && activeCell.GetComponent<Cell> ().IsChoose ()) {
						activeCell.GetComponent<Animation> ().Play ("CellAntiChoose");
						activeCell.GetComponent<Cell> ().AntiChoose ();
					}
				}
			}

		}

		if(Input.GetMouseButtonUp(0)){
			Vector2 worldPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast (worldPoint, Vector2.zero);
			if (chooser != null) {

				if (hit.collider != null && hit.collider.gameObject.CompareTag ("Cell") && chooser.transform.IsChildOf(bottomBar.transform) && hit.collider.gameObject.GetComponent<Cell> ().ChangeNumber (chooser.GetComponent<Chooser> ().GetNumber (), fieldControlls.GetRange ())) {
					chooser.GetComponent<Chooser> ().Delete (hit.collider.gameObject.transform.localPosition);
					fieldControlls.IncreaseChooserCount (false);
					if (PlayerPrefs.GetString ("Sound") == "on") {
						hit.collider.GetComponent<AudioSource> ().Play ();
					}

					StartCoroutine (LateCheckTurns ());

				} else {
					chooser.GetComponent<Chooser> ().GetBack ();
					if (PlayerPrefs.GetString ("Sound") == "on") {
						chooser.transform.GetChild (0).GetComponent<AudioSource> ().Play ();
					}
				}

				if (activeCell != null) {
					activeCell.GetComponent<Cell> ().AntiChoose ();
					activeCell.GetComponent<Animation> ().Play ("CellAntiChoose");
					activeCell = null;
				}

				chooser = null;
				fieldControlls.AntiFadeCells ();
			}

			if (hit.collider != null && hit.collider.name == "Pause") {
				Pause ();
				if (PlayerPrefs.GetString ("Sound") == "on") {
					hit.collider.GetComponent<AudioSource> ().Play ();
				}
			}

			if (hit.collider != null && hit.collider.name == "Restart Button") {
				if (PlayerPrefs.GetString ("Sound") == "on") {
					hit.collider.GetComponent<AudioSource> ().Play ();
					field.GetComponent<AudioSource> ().Play ();
				}
				if (loseScreen.activeSelf) {
					GetComponent<Animation> ().Play ("RestartFromLose");
					StartCoroutine (LoadScene ("Game"));
				} else {
					GetComponent<Animation> ().Play ("RestartFromPause");
					StartCoroutine (LoadScene ("Game"));
				}
				if (banner != null) {
					banner.Destroy ();
				}

				if (banner2 != null) {
					banner2.Destroy();
				}
			}

			if (hit.collider != null && hit.collider.name == "Menu Button") {
				if (PlayerPrefs.GetString ("Sound") == "on") {
					hit.collider.GetComponent<AudioSource> ().Play ();
					field.transform.GetChild (0).GetComponent<AudioSource> ().Play ();
				}
				if (loseScreen.activeSelf) {
					GetComponent<Animation> ().Play ("MenuFromLose");
					StartCoroutine (LoadScene ("Menu"));
				} else {
					GetComponent<Animation> ().Play ("MenuFromPause");
					StartCoroutine (LoadScene ("Menu"));
				}
				if (banner != null) {
					banner.Destroy ();
				}

				if (banner2 != null) {
					banner2.Destroy();
				}
			}


			if (hit.collider != null && hit.collider.name == "Music Button") {
				if (PlayerPrefs.GetString ("Sound") == "on") {
					hit.collider.GetComponent<AudioSource> ().Play ();
				}
				if (PlayerPrefs.GetString ("Music") == "on") {
					hit.collider.GetComponent<Animation> ().Play ("MusicOff");
					PlayerPrefs.SetString ("Music", "off");
					audioC.transform.GetChild (modeNumber).GetComponent<AudioSource> ().Pause ();
				} else {
					hit.collider.GetComponent<Animation> ().Play ("MusicOn");
					PlayerPrefs.SetString ("Music", "on");
					audioC.transform.GetChild (modeNumber).GetComponent<AudioSource> ().UnPause ();
				}
			}

			if (hit.collider != null && hit.collider.name == "Sound Button") {
				if (PlayerPrefs.GetString ("Sound") == "on") {
					hit.collider.GetComponent<AudioSource> ().Play ();
				}
				if (PlayerPrefs.GetString ("Sound") == "on") {
					hit.collider.GetComponent<Animation> ().Play ("SoundOff");
					PlayerPrefs.SetString ("Sound", "off");
				} else {
					hit.collider.GetComponent<Animation> ().Play ("SoundOn");
					PlayerPrefs.SetString ("Sound", "on");
				}
			}
		}

	}

	public bool Pause(){
		if (!paused) {
			paused = true;
			timer.GetComponent<Timer> ().Pause ();
			GetComponent<Animation> ().Play ("Pause");
		} else {
			GetComponent<Animation> ().Play ("UnPause");
			StartCoroutine (timer.GetComponent<Timer> ().UnPaused ());
		}

		return paused;
	}

	public void ShowScore(int s, int record){
		score = s;
	}

	public void Lose(int score, string stats, string stats2){

		GetComponent<Animation> ().Play ("Lose");
		loseScreen.SetActive (true);
		if (PlayerPrefs.GetString ("Sound") == "on") {
			loseScreen.GetComponent<AudioSource> ().Play ();
		}
		loseScreen.transform.GetChild (2).GetComponent<Text> ().text = score.ToString ();
		if (score == PlayerPrefs.GetInt ("BestScore" + PlayerPrefs.GetString ("Mode"))) {
			loseScreen.transform.GetChild (3).gameObject.SetActive (true);
		}
		worldRecordText.text = PlayerPrefs.GetInt ("WorldBestScore" + PlayerPrefs.GetString ("Mode")).ToString();
		statsText.text = stats;
		stats2Test.text = stats2;
		StartCoroutine (LateStop ());
		if (PlayerPrefs.GetInt ("Attempts" + PlayerPrefs.GetString ("Mode")) % (modeNumber + 1) == 0) {
			//if (UnityEngine.Random.Range (0, 2) == 0) {
				//if (Advertisement.IsReady ()) {
				//	Advertisement.Show ();
				//} else 
				if (inter.IsLoaded ()) {
					inter.Show ();
				}
			//} else if (inter.IsLoaded ()) {
			//	inter.Show ();
			//} else if (Advertisement.IsReady ()) {
			//	Advertisement.Show ();
			//}
		}

	}

	IEnumerator StartCellsAnimations(){

		GameObject[,] cells = new GameObject[7,6];

		Transform t = transform.GetChild (1).GetChild (3);
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 6; j++) {
				cells [i, j] = t.GetChild (i * 6 + j).gameObject;
			}
		}

		for (int j = 0; j < 3; j++) {
			for (int i = 0; i < 3; i++) {
				cells [i, j].GetComponent<Animation> ().Play ("Cell");
				cells [i, 5 - j].GetComponent<Animation> ().Play ("Cell");
			}
			yield return new WaitForSeconds (0.1f);
		}
		print ("Start");

		//StartCoroutine (WaitForNextNumber ());
		//StartCoroutine (WaitAnim ());
		//StartCoroutine (WaitForNextPart ());

		timer.GetComponent<Timer> ().SetTimerNumber (tNumber);
		StartCoroutine (timer.GetComponent<Timer> ().StartTimer ());
	}

	public void StartNewLineAnimations(){
		
		Transform t = transform.GetChild (1).GetChild (5);
		for (int j = 0; j < 6; j++) {
			t.GetChild (j).GetComponent<Animation> ().Play ("Cell");
		}
	}

	IEnumerator LoadScene(string scene){
		yield return new WaitForSeconds (1f);
		SceneManager.LoadScene (scene);
	}

	public void StopTimer(){
		timerStop = true;
		timerPartsStop = true;
	}

	public void AntiCF(){
		chooser = null;
		fieldControlls.AntiFadeCells ();
		if (activeCell != null) {
			activeCell.GetComponent<Cell> ().AntiChoose ();
			activeCell.GetComponent<Animation> ().Play ("CellAntiChoose");
			activeCell = null;
		}
	}

	public void NewNumber(int nextNumber){
		
		GetComponent<Animation> ().Play ("NewNumber");

		transform.GetChild (3).GetChild (1).gameObject.SetActive (true);
		transform.GetChild (3).GetChild (1).GetChild (1).gameObject.SetActive (true);
		transform.GetChild (3).GetChild (1).GetChild (1).localScale = Vector3.one * 0.35f;
		transform.GetChild (3).GetChild (1).GetChild (1).GetComponent<Cell> ().SetNumber (nextNumber);
		transform.GetChild (3).GetChild (1).GetChild (1).GetComponent<Animation> ().Play("Cell");
		if (PlayerPrefs.GetString ("Sound") == "on") {
			transform.GetChild (3).GetChild (1).GetComponent<AudioSource> ().PlayDelayed (0.15f);
		}
		
	}

	public IEnumerator NewMode(string modeName, int modeN){
		do {
			if(!transform.GetChild (3).GetChild (1).gameObject.activeSelf && !transform.GetChild (3).GetChild (0).gameObject.activeSelf){
				break;
			}

			yield return new WaitForEndOfFrame();
		} while(true);

		transform.GetChild (3).GetChild (2).gameObject.SetActive (true);

		transform.GetChild (3).GetChild (2).GetChild (1).GetComponent<Text> ().text = modeName;
		transform.GetChild (3).GetChild (2).GetChild (1).GetComponent<Text> ().color = modeColors [modeN];
		if (modeName == "awesome") {
			transform.GetChild (3).GetChild (2).GetChild (0).GetComponent<Text> ().text = "you are";
		}
		transform.GetComponent<Animation> ().Play ("NewMode");
		if (PlayerPrefs.GetString ("Sound") == "on") {
			transform.GetChild (3).GetChild (2).GetComponent<AudioSource> ().PlayDelayed (0.15f);
		}

	}
	/*
	IEnumerator WaitForNextNumber(){
		bool b = false;

		if (timerNumber.text == "0") {
			fieldControlls.CheckChoosers ();
			yield return new WaitForSeconds (0.5f);
			timerStoper = false;
		} else {
			yield return new WaitForSeconds (0.75f);
		}

		if (timerStop) {
			b = true;
			timerStoper = false;
		}


		timerNumber.GetComponent<Animation> ().Play ("TimerNumber");

		yield return new WaitForSeconds (0.25f);
		a1 = false;
		timerNumber.text = (int.Parse (timerNumber.text) - 1).ToString ();

		if (timerNumber.text == "-1" || b) {			
			timerNumber.text = tNumber.ToString ();
			yield return new WaitForSeconds (0.25f);
			StartCoroutine (WaitForNextNumber ());
			StartCoroutine (WaitForNextPart ());
			timerStop = false;
			timerStoper = true;
		} else {
			StartCoroutine (WaitForNextNumber ());
			a1 = true;
		}

	}*/

	/*IEnumerator WaitForNextPart(){
		yield return new WaitWhile (() => (int.Parse (timer.transform.GetChild (0).GetComponent<Text> ().text) - 1) % (tNumber/8)!=0);
		if (!timerPartsStop) {
			int k = 8 - (int.Parse (timer.transform.GetChild (0).GetComponent<Text> ().text) - 1) / (tNumber / 8);
			yield return new WaitForSeconds (0.5f);
			if (!timerPartsStop) {
				if (timerPart != k && k > 0 && k < 9) {
					StartCoroutine (LateFade (k));
				}
				timerPart = k;
				//print ("part");
			}
		}
		if (timerPartsStop) {
			timerPartsStop = false;
			timerPart = -1;
		} else {
			StartCoroutine (WaitForNextPart ());
		}
	}

	IEnumerator WaitAnim(){
		yield return new WaitWhile (() =>paused);

		if (timer.transform.GetChild (0).GetComponent<Text> ().text == "1") {
			if (PlayerPrefs.GetString ("Sound") == "on") {
				timer.GetComponent<AudioSource> ().Play ();
			}
		}

		yield return new WaitForSeconds (0.25f);

		yield return new WaitForSeconds (0.25f);
		yield return new WaitForSeconds (0.25f);

		timer.transform.GetChild (0).GetComponent<Animation> ().Play ("TimerNumber");
		/*if (timerPart > -1 && timerPart < 8) {
			if (timer.transform.GetChild (timerPart + 1).GetComponent<Image> ().color.a == 1) {
				StartCoroutine (LateFade (timerPart + 1));
				//timer.transform.GetChild (timerPart + 1).GetComponent<Animation> ().Play ("TimerPart");
			}
		}
		yield return new WaitForSeconds (0.25f);
		timer.transform.GetChild (0).GetComponent<Text> ().text = (int.Parse (timer.transform.GetChild (0).GetComponent<Text> ().text) - 1).ToString ();

		if (timer.transform.GetChild(0).GetComponent<Text>().text != "0" && !timerStop) {
			StartCoroutine (WaitAnim ());
		} else {
			yield return new WaitWhile (() =>paused);
			if (!timerStop) {
				fieldControlls.CheckChoosers ();
			}
			yield return new WaitForSeconds (0.25f);
			timer.GetComponent<Animation>().Play("TimerReverse");

			yield return new WaitForSeconds (0.25f);

			timer.transform.GetChild (0).GetComponent<Text> ().text = tNumber.ToString ();
			for (int i = 1; i < 9; i++) {
				timer.transform.GetChild (i).gameObject.SetActive (true);
			}

			yield return new WaitForSeconds (0.25f);

			for (int i = 1; i < 9; i++) {
				timer.transform.GetChild (i).GetComponent<Animation> ().Stop ();
				timer.transform.GetChild (i).GetComponent<Image> ().color = modeColors [modeNumber];
			}

			StartCoroutine (WaitAnim ());
			StartCoroutine (WaitForNextPart ());
			timerStop = false;
		}
	}
	*/

	IEnumerator LateFade(int k){
		timer.transform.GetChild (k).GetComponent<Animation> ().Play ("TimerPart");
		yield return new WaitForSeconds (0.5f);
		timer.transform.GetChild (k).gameObject.SetActive (false);
	}

	IEnumerator LateStop(){
		yield return new WaitForSeconds (1);
		StopAllCoroutines ();
	}

	IEnumerator SPause(){
		yield return new WaitForSeconds (1);
		paused = false;
	}

	IEnumerator SUPause(){
		yield return new WaitForSeconds (1);

	}

	IEnumerator LateCheckTurns(){
		
		while (bottomBar.transform.childCount != 5) {
			yield return new WaitForEndOfFrame ();
		}

		if (fieldControlls.ChooserCount()!=4 && fieldControlls.ChooserCount()!=0 && fieldControlls.HaveTurns () == 0 ) {
			transform.GetChild (3).GetChild (3).gameObject.SetActive (true);
			GetComponent<Animation> ().Play ("NoTurns");
			transform.GetChild (3).GetChild (3).GetComponent<AudioSource> ().Play ();
			fieldControlls.CheckChoosers ();
		}

	}
}

