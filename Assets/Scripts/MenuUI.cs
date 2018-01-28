using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Database;
using Firebase;
using Firebase.Unity.Editor;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System;

public class MenuUI : MonoBehaviour {

	static Color[] modeColors = new Color[] {
		new Color (128 / 255f, 1, 128 / 255f),
		new Color (1, 1, 128 / 255f),
		new Color (1, 128 / 255f, 64 / 255f),
		new Color (1, 64 / 255f, 64 / 255f),
		new Color (128 / 255f, 192 / 255f, 1)
	};

	public GameObject slider;
	public GameObject bestScore, attempts, averageScore;
	public GameObject dots;
	public GameObject modeChooser;
	public GameObject music, sound;
	public GameObject records, leaderbord;
	public GameObject audioC;
	public GameObject tutorial;

	public InputField nickname;

	public Color dotActiveColor, dotInactiveColor;
	public Color textColor;
	public Color modeColor;

	float start, end, delta;

	int audioN;
	bool game;
	bool back;
	bool backT;
	bool choose;
	int index;
	public int indexT;
	int modeN;
	int recordsMode;
	Color startColor;
	bool bb;

	int bestScoreDelta, averageScoreDelta, attemptsDelta;

	Vector3 position, velocity;

	GameObject choosedMode, hoverMode;

	DatabaseReference reference;

	Dictionary<string,int> recordsTableEasy = new Dictionary<string, int>();
	Dictionary<string,int> recordsTableMedium = new Dictionary<string, int>();
	Dictionary<string,int> recordsTableHard = new Dictionary<string, int>();
	Dictionary<string,int> recordsTableInsane = new Dictionary<string, int>();
	Dictionary<string,int> recordsTableAbsolute = new Dictionary<string, int>();

	string[] modes = new string[]{
		"easy",
		"medium",
		"hard",
		"insane"
	};

	void Start(){
		
		if (!CheckConnection ()) {
			leaderbord.transform.GetChild (9).gameObject.SetActive (true);
			leaderbord.transform.GetChild (8).gameObject.SetActive (false);
		}

		DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

		dependencyStatus = FirebaseApp.CheckDependencies();
		if (dependencyStatus != DependencyStatus.Available) {
			FirebaseApp.FixDependenciesAsync ().ContinueWith (task => {
				dependencyStatus = FirebaseApp.CheckDependencies ();
				if (dependencyStatus == DependencyStatus.Available) {
					//FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://plusminus-94693202.firebaseio.com/");
					reference = FirebaseDatabase.DefaultInstance.RootReference;
					leaderbord.transform.GetChild (9).gameObject.SetActive (false);
					leaderbord.transform.GetChild (8).gameObject.SetActive (true);
				}
			});
		} else {
			//FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://plusminus-94693202.firebaseio.com/");
			reference = FirebaseDatabase.DefaultInstance.RootReference;
			leaderbord.transform.GetChild (9).gameObject.SetActive (false);
			leaderbord.transform.GetChild (8).gameObject.SetActive (true);
		}

		if (PlayerPrefs.HasKey ("Nickname")) {

			if (!PlayerPrefs.HasKey ("Update")) {
				PlayerPrefs.SetInt ("AttemptsEasy", 0);
				PlayerPrefs.SetInt ("BestScoreEasy", 0);
				PlayerPrefs.SetInt ("AverageScoreEasy", 0);
				PlayerPrefs.SetInt ("SumScoreEasy", 0);

				PlayerPrefs.SetInt ("AttemptsMedium", 0);
				PlayerPrefs.SetInt ("BestScoreMedium", 0);
				PlayerPrefs.SetInt ("AverageScoreMedium", 0);
				PlayerPrefs.SetInt ("SumScoreMedium", 0);

				PlayerPrefs.SetInt ("AttemptsHard", 0);
				PlayerPrefs.SetInt ("BestScoreHard", 0);
				PlayerPrefs.SetInt ("AverageScoreHard", 0);
				PlayerPrefs.SetInt ("SumScoreHard", 0);

				PlayerPrefs.SetInt ("AttemptsInsane", 0);
				PlayerPrefs.SetInt ("BestScoreInsane", 0);
				PlayerPrefs.SetInt ("AverageScoreInsane", 0);
				PlayerPrefs.SetInt ("SumScoreInsane", 0);

				PlayerPrefs.SetString ("Update", "v1.1");
			}

			reference.Child ("scoreUp").Child ("easy").Child (PlayerPrefs.GetString("Nickname")).SetValueAsync (PlayerPrefs.GetInt ("BestScoreEasy"));
			reference.Child ("scoreUp").Child ("medium").Child (PlayerPrefs.GetString("Nickname")).SetValueAsync (PlayerPrefs.GetInt ("BestScoreMedium"));
			reference.Child ("scoreUp").Child ("hard").Child (PlayerPrefs.GetString("Nickname")).SetValueAsync (PlayerPrefs.GetInt ("BestScoreHard"));
			reference.Child ("scoreUp").Child ("insane").Child (PlayerPrefs.GetString("Nickname")).SetValueAsync (PlayerPrefs.GetInt ("BestScoreInsane"));

			int abs = (PlayerPrefs.GetInt ("BestScoreEasy") + 2 * PlayerPrefs.GetInt ("BestScoreMedium") + 3 * PlayerPrefs.GetInt ("BestScoreHard") + 4 * PlayerPrefs.GetInt ("BestScoreInsane")) / 10;
			reference.Child ("scoreUp").Child ("absolute").Child (PlayerPrefs.GetString("Nickname")).SetValueAsync (abs);
		}

		reference.Child("scoreUp").ValueChanged += Order;

		FirstOpenSettings ();

		audioN = UnityEngine.Random.Range (0, 4);
		audioC.transform.GetChild (audioN).gameObject.SetActive (true);
		audioC.transform.GetChild (audioN).GetComponent<AudioSource> ().Play ();

		if (PlayerPrefs.GetString ("Sound") == "on") {
			sound.transform.GetChild (1).gameObject.SetActive (true);
			sound.transform.GetChild (2).gameObject.SetActive (false);
			GetComponent<AudioListener> ().enabled = true;
		} else {
			sound.transform.GetChild (1).gameObject.SetActive (false);
			sound.transform.GetChild (2).gameObject.SetActive (true);
			GetComponent<AudioListener> ().enabled = false;
		}

		if (PlayerPrefs.GetString ("Music") == "on") {
			music.transform.GetChild (1).gameObject.SetActive (true);
			music.transform.GetChild (2).gameObject.SetActive (false);
		} else {
			music.transform.GetChild (1).gameObject.SetActive (false);
			music.transform.GetChild (2).gameObject.SetActive (true);
			audioC.transform.GetChild (audioN).GetComponent<AudioSource> ().Pause ();

		}

		CheckModeLocked ("Easy", 0);
		CheckModeLocked ("Medium", 1);
		CheckModeLocked ("Hard", 2);
		CheckModeLocked ("Insane", 3);

		if (PlayerPrefs.GetString ("Easy") == "openNow") {
			StartCoroutine (UnlockMode (0));
			PlayerPrefs.SetString ("Easy", "opened");
		}

		if (PlayerPrefs.GetString ("Medium") == "openNow") {
			StartCoroutine (UnlockMode (1));
			PlayerPrefs.SetString ("Medium", "opened");
		}

		if (PlayerPrefs.GetString ("Hard") == "openNow") {
			StartCoroutine (UnlockMode (2));
			PlayerPrefs.SetString ("Hard", "opened");
		}

		if (PlayerPrefs.GetString ("Insane") == "openNow") {
			StartCoroutine (UnlockMode (3));
			PlayerPrefs.SetString ("Insane", "opened");
		}

		switch (PlayerPrefs.GetString ("Mode")) {
		case "Easy":
			modeN = 0;
			break;
		case "Medium":
			modeN = 1;
			break;
		case "Hard":
			modeN = 2;
			break;
		case "Insane":
			modeN = 3;
			break;
		}

		recordsMode = modeN;

		startColor = modeColors [modeN];
		choosedMode = modeChooser.transform.GetChild (modeN).GetChild (0).gameObject;
		StartCoroutine (LateChoosedMode ());
		choosedMode.GetComponent<Text>().color = startColor;

		modeChooser.GetComponent<Image> ().color = startColor;

		bestScore.GetComponent<Text> ().color = startColor;
		averageScore.GetComponent<Text> ().color = startColor;
		attempts.GetComponent<Text> ().color = startColor;

		if (PlayerPrefs.GetInt ("Attempts" + PlayerPrefs.GetString ("Mode")) != 0) {
			PlayerPrefs.SetInt ("AverageScore" + PlayerPrefs.GetString ("Mode"), PlayerPrefs.GetInt ("SumScore" + PlayerPrefs.GetString ("Mode")) / PlayerPrefs.GetInt ("Attempts" + PlayerPrefs.GetString ("Mode")));
		}

		bestScore.GetComponent<Text>().text = PlayerPrefs.GetInt ("BestScore"+PlayerPrefs.GetString("Mode")).ToString();
		attempts.GetComponent<Text>().text = PlayerPrefs.GetInt ("Attempts"+PlayerPrefs.GetString("Mode")).ToString();
		averageScore.GetComponent<Text>().text = PlayerPrefs.GetInt("AverageScore"+PlayerPrefs.GetString("Mode")).ToString();

		if (PlayerPrefs.HasKey ("Nickname")) {
			GetComponent<Animation> ().Play ("Begining");
		} else {
			transform.GetChild (3).gameObject.SetActive (true);
			GetComponent<Animation> ().Play ("Begining First");
		}

		index = 1;
		game = false;
		back = true;

		records.transform.GetChild (3).GetComponent<Text> ().color = startColor;
		records.transform.GetChild (3).GetComponent<Text> ().text = modes [modeN];

	}

	void Update(){
		if (!tutorial.activeSelf) {
			ChangeTextScore ();

			index = Mathf.RoundToInt (slider.transform.localPosition.x / 1200);

			dots.transform.GetChild (index+1).GetComponent<Image> ().color = Color.Lerp (dotActiveColor, dotInactiveColor, Mathf.Abs (-slider.transform.localPosition.x / 600 + 2 * index));

			if (!game) {

				if (Input.GetMouseButtonDown (0)) {

					Vector2 worldPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
					RaycastHit2D hit = Physics2D.Raycast (worldPoint, Vector2.zero);
					if (hit.collider != null && hit.collider.name != "First Open") {
						if (hit.collider.name == "Slider") {
							start = Input.mousePosition.x;
							position = slider.transform.localPosition;
							velocity = Vector3.zero;
							back = false;	
						} else if (hit.collider.name == "Ok Button") {
							if (PlayerPrefs.GetString ("Sound") == "on") {
								hit.collider.GetComponent<AudioSource> ().Play ();
							}
							hit.collider.GetComponent<Animation> ().Play ("ButtonPress");
						} else if (hit.collider.name == "Exit") {
							if (PlayerPrefs.GetString ("Sound") == "on") {
								hit.collider.GetComponent<AudioSource> ().Play ();
							}
						} else {
							if (index == 0) {
								hoverMode = hit.collider.gameObject;
								if (PlayerPrefs.GetString ("Sound") == "on") {
									modeChooser.GetComponent<AudioSource> ().Play ();
								}
								if (choosedMode != hoverMode) {
									hit.collider.transform.parent.GetComponent<Animation> ().Play ("HoverMode");
								} else {
									hit.collider.transform.parent.GetComponent<Animation> ().Play ("HoverChoosedMode");
								}

								if (Mathf.Abs (Camera.main.ScreenToWorldPoint (Input.mousePosition).x) < 1.8f && Mathf.Abs (Camera.main.ScreenToWorldPoint (Input.mousePosition).y - 0.3f) < 2.1f) {
									choose = true;
								}
							}
							
							if (index == 1) {
								if (hit.collider.name == "Music Button") {
									if (PlayerPrefs.GetString ("Sound") == "on") {
										hit.collider.GetComponent<AudioSource> ().Play ();
									}
									if (PlayerPrefs.GetString ("Music") == "on") {
										hit.collider.GetComponent<Animation> ().Play ("MusicOff");
										PlayerPrefs.SetString ("Music", "off");
										audioC.transform.GetChild (audioN).GetComponent<AudioSource> ().Pause ();
									} else {
										hit.collider.GetComponent<Animation> ().Play ("MusicOn");
										PlayerPrefs.SetString ("Music", "on");
										audioC.transform.GetChild (audioN).GetComponent<AudioSource> ().UnPause ();
									}
								}

								if (hit.collider.name == "Sound Button") {
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

								if (hit.collider.name == "Developer") {
									Application.OpenURL ("https://vk.com/id101579021");
									if (PlayerPrefs.GetString ("Sound") == "on") {
										hit.collider.GetComponent<AudioSource> ().Play ();
									}
								}

								if (hit.collider.name == "Designer") {
									Application.OpenURL ("https://vk.com/id110803181");
									if (PlayerPrefs.GetString ("Sound") == "on") {
										hit.collider.GetComponent<AudioSource> ().Play ();
									}

								}
							}

							if (index == -1) {
								if (hit.collider.name == "Left Arrow") {
									switch (records.transform.GetChild (3).GetComponent<Text> ().text) {
									case "medium":
										recordsMode = 0;
										StartCoroutine (ChangeRecordsHead ("easy"));
										StartCoroutine (ShowRecords (recordsTableEasy));
										hit.collider.GetComponent<Animation> ().Play ("HideArrow");
										if (PlayerPrefs.GetString ("Sound") == "on") {
											hit.collider.GetComponent<AudioSource> ().Play ();
										}
										break;
									case "hard":
										recordsMode = 1;
										StartCoroutine (ChangeRecordsHead ("medium"));
										StartCoroutine (ShowRecords (recordsTableMedium));
										if (PlayerPrefs.GetString ("Sound") == "on") {
											hit.collider.GetComponent<AudioSource> ().Play ();
										}
										break;
									case "insane":
										recordsMode = 2;
										StartCoroutine (ChangeRecordsHead ("hard"));
										StartCoroutine (ShowRecords (recordsTableHard));
										if (PlayerPrefs.GetString ("Sound") == "on") {
											hit.collider.GetComponent<AudioSource> ().Play ();
										}
										break;
									case "absolute":
										recordsMode = 3;
										StartCoroutine (ChangeRecordsHead ("insane"));
										StartCoroutine (ShowRecords (recordsTableInsane));
										records.transform.GetChild (4).GetComponent<Animation> ().Play ("AntiHideArrow");
										if (PlayerPrefs.GetString ("Sound") == "on") {
											hit.collider.GetComponent<AudioSource> ().Play ();
										}
										break;
									}
								}

								if (hit.collider.name == "Right Arrow") {
									switch (records.transform.GetChild (3).GetComponent<Text> ().text) {
									case "easy":
										recordsMode = 1;
										StartCoroutine (ChangeRecordsHead ("medium"));
										StartCoroutine (ShowRecords (recordsTableMedium));
										records.transform.GetChild (2).GetComponent<Animation> ().Play ("AntiHideArrow");
										if (PlayerPrefs.GetString ("Sound") == "on") {
											hit.collider.GetComponent<AudioSource> ().Play ();
										}
										break;
									case "medium":
										recordsMode = 2;
										StartCoroutine (ChangeRecordsHead ("hard"));
										StartCoroutine (ShowRecords (recordsTableHard));
										if (PlayerPrefs.GetString ("Sound") == "on") {
											hit.collider.GetComponent<AudioSource> ().Play ();
										}
										break;
									case "hard":
										recordsMode = 3;
										StartCoroutine (ChangeRecordsHead ("insane"));
										StartCoroutine (ShowRecords (recordsTableInsane));
										if (PlayerPrefs.GetString ("Sound") == "on") {
											hit.collider.GetComponent<AudioSource> ().Play ();
										}
										break;
									case "insane":
										recordsMode = 4;
										StartCoroutine (ChangeRecordsHead ("absolute"));
										StartCoroutine (ShowRecords (recordsTableAbsolute));
										hit.collider.GetComponent<Animation> ().Play ("HideArrow");
										if (PlayerPrefs.GetString ("Sound") == "on") {
											hit.collider.GetComponent<AudioSource> ().Play ();
										}
										break;
									}

								}
							}
						}
					}
				}

				if (Input.GetMouseButton (0)) {
					if (index == 0) {
						if (choose) {
							Vector2 worldPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
							RaycastHit2D hit = Physics2D.Raycast (worldPoint, Vector2.zero);
							if (hoverMode != null) {
								if (hit.collider.name == "Slider") {

									if (choosedMode != hoverMode) {
										hoverMode.transform.parent.GetComponent<Animation> ().Play ("AntiHoverMode");
									} else {
										hoverMode.transform.parent.GetComponent<Animation> ().Play ("AntiHoverChoosedMode");
									}
									hoverMode = null;

								} else if (hit.collider.gameObject != hoverMode) {
									if (choosedMode != hoverMode) {
										hoverMode.transform.parent.GetComponent<Animation> ().Play ("AntiHoverMode");
									} else {
										hoverMode.transform.parent.GetComponent<Animation> ().Play ("AntiHoverChoosedMode");
									}
									if (PlayerPrefs.GetString ("Sound") == "on") {
										modeChooser.GetComponent<AudioSource> ().Play ();
									}
									hoverMode = hit.collider.gameObject;
									if (hoverMode != choosedMode) {
										hoverMode.transform.parent.GetComponent<Animation> ().Play ("HoverMode");
									} else {
										hoverMode.transform.parent.GetComponent<Animation> ().Play ("HoverChoosedMode");
									}
								}

							} else if (hit.collider != null && hit.collider.name != "Slider" && hit.collider.name != "First Open") {
								hoverMode = hit.collider.gameObject;
								if (PlayerPrefs.GetString ("Sound") == "on") {
									modeChooser.GetComponent<AudioSource> ().Play ();
								}
								if (hoverMode != choosedMode) {
									hoverMode.transform.parent.GetComponent<Animation> ().Play ("HoverMode");
								} else {
									hoverMode.transform.parent.GetComponent<Animation> ().Play ("HoverChoosedMode");
								}
							}
						} 
					}
					if (!back) {
						delta = start - Input.mousePosition.x;
						slider.transform.localPosition = Vector3.SmoothDamp (slider.transform.localPosition, position - Vector3.right * delta, ref velocity, 0.05f);

						if (delta > 0 && slider.transform.localPosition.x < -1200) {
							slider.transform.localPosition = Vector3.left * 1200;
						}

						if (delta < 0 && slider.transform.localPosition.x > 1200) {
							slider.transform.localPosition = Vector3.right * 1200;
						}
					}
				}

				if (Input.GetMouseButtonUp (0)) {
					if (choose) {
						choose = false;
						if (hoverMode != null) {
							if (hoverMode == choosedMode) {
								StartCoroutine (LateNewGame ());
							} else {

								StopAllCoroutines ();
								StartCoroutine (ChooseMode (hoverMode));
								choosedMode.transform.parent.GetComponent<Animation> ().Play ("AntiHoverMode");
								choosedMode = hoverMode;
								hoverMode = null;

							}
						}
					}

					Vector2 worldPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);
					RaycastHit2D hit = Physics2D.Raycast (worldPoint, Vector2.zero);

					if (hit.collider != null && hit.collider.name == "Ok Button") {
					
						Regex pattern = new Regex ("^[0-9a-zA-z]+$");
						bool name1 = pattern.Match (nickname.text).Success;
						if (nickname.text == "" || !name1) {
							hit.collider.transform.parent.GetChild (3).GetComponent<Text> ().text = "only english and numbers";
							hit.collider.transform.parent.GetChild (3).GetComponent<Animation> ().Play ("Warning");
						} else {
							bool name2 = true;
							reference.Child ("scoreUp").Child ("easy").GetValueAsync ().ContinueWith (task => {
								if (task.IsCompleted) {
									DataSnapshot snap = task.Result;
									if (snap.ChildrenCount > 0) {
										foreach (var child in snap.Children) {
											if (nickname.text == child.Key) {										
												name2 = false;
												hit.collider.transform.parent.GetChild (3).GetComponent<Text> ().text = "this nickname already exist";
												break;
											}
										}

										if (name2) {
											PlayerPrefs.SetString ("Nickname", nickname.text);
											PlayerPrefs.SetString ("Update", "v1.1");
											GetComponent<Animation> ().Play ("LogIn");

											reference.Child ("scoreUp").Child ("easy").Child (nickname.text).SetValueAsync (0);
											reference.Child ("scoreUp").Child ("medium").Child (nickname.text).SetValueAsync (0);
											reference.Child ("scoreUp").Child ("hard").Child (nickname.text).SetValueAsync (0);
											reference.Child ("scoreUp").Child ("insane").Child (nickname.text).SetValueAsync (0);
											reference.Child ("scoreUp").Child ("absolute").Child (nickname.text).SetValueAsync (0);

											/**/

											print ("login");
											tutorial.SetActive (true);
											StartCoroutine (Tutorial ());
										} else {									
											hit.collider.transform.parent.GetChild (3).GetComponent<Animation> ().Play ("Warning");
										}
									}

								}
							});
						}

						hit.collider.GetComponent<Animation> ().Play ("ButtonUnpress");
					} else if(hit.collider.name == "Exit"){
						Application.Quit ();
						print ("Quit");
					}

					if (!back) {
						back = true;
						velocity = Vector3.zero;
						if (Mathf.Abs (delta) < 200 || Mathf.Abs (delta) > 600) {
							position = Vector3.right * 1200 * index;
							if (Mathf.Abs (delta) > 600) {
								if (PlayerPrefs.GetString ("Sound") == "on") {
									slider.GetComponent<AudioSource> ().Play ();
								}
							}
						} else {
							float i = index - Mathf.Sign (delta);
							if (i > 1) {
								i = 1;
							}
							if (i < -1) {
								i = -1;
							}
							position = Vector3.right * 1200 * i;
							if (i != index) {
								if (PlayerPrefs.GetString ("Sound") == "on") {
									slider.GetComponent<AudioSource> ().Play ();
								}
							}
						}
					}
				}

			} 

			if (back) {
				slider.transform.localPosition = Vector3.SmoothDamp (slider.transform.localPosition, position, ref velocity, 0.1f);
			}

		} else {

			indexT = -Mathf.RoundToInt (tutorial.transform.GetChild (3).localPosition.x / 1500);

			tutorial.transform.GetChild (8).GetChild (indexT).GetComponent<Image> ().color = Color.Lerp (dotActiveColor, dotInactiveColor, Mathf.Abs (tutorial.transform.GetChild (3).localPosition.x / 750 + 2 * indexT));

			if (Input.GetMouseButtonDown (0)) {
				start = Input.mousePosition.x;
				position = tutorial.transform.GetChild (3).localPosition;
				velocity = Vector3.zero;
				backT = false;
			}

			if (Input.GetMouseButton (0)) {
				if (!backT) {
					delta = start - Input.mousePosition.x;

					if (delta > 0 && tutorial.transform.GetChild (3).localPosition.x < -13500) {
						if (delta > 220) {
							tutorial.transform.localPosition = Vector3.SmoothDamp (tutorial.transform.localPosition, Vector3.back * 2 - Vector3.right * (delta-220), ref velocity, 0.05f);
						}
					} else {
						tutorial.transform.GetChild (3).localPosition = Vector3.SmoothDamp (tutorial.transform.GetChild (3).localPosition, position- Vector3.right * delta, ref velocity, 0.05f);
					}

					if (delta < 0 && tutorial.transform.GetChild (3).localPosition.x > 0) {
						tutorial.transform.GetChild (3).localPosition = Vector3.zero;
					}
				}
			}

			if (Input.GetMouseButtonUp (0)) {
				if (!backT) {
					backT = true;
					velocity = Vector3.zero;
					if (Mathf.Abs (delta) < 200 || Mathf.Abs (delta) > 750) {
						if (indexT == 10) {
							bb = true;
						} else {
							position = Vector3.right * 1500 * -indexT;
						}
						if (Mathf.Abs (delta) > 750) {
							if (PlayerPrefs.GetString ("Sound") == "on") {
								tutorial.transform.GetChild (3).GetComponent<AudioSource> ().Play ();
							}
						}
					} else {
						float i = indexT + Mathf.Sign (delta);
						if (i < 0) {
							i = 0;
						}
						if (i > 9) {
							bb = true;
						}

						position = Vector3.right * 1500 * -i;
						if (i != indexT) {
							if (PlayerPrefs.GetString ("Sound") == "on") {
								tutorial.transform.GetChild (3).GetComponent<AudioSource> ().Play ();
							}
						}
					}
				}
			}

			tutorial.GetComponent<Image> ().color = Color.Lerp (new Color (39 / 255f, 39 / 255f, 53 / 255f, 1), new Color (39 / 255f, 39 / 255f, 53 / 255f, 0), -tutorial.transform.localPosition.x / 1200f);

			if (bb) {
				tutorial.transform.localPosition = Vector3.SmoothDamp (tutorial.transform.localPosition, Vector3.back*2-Vector3.right * 2000, ref velocity, 0.1f);
				if (tutorial.transform.localPosition.x + 1500 < 100) {
					bb = false;
					transform.GetChild (4).GetComponent<Animation> ().Play ("LogoBack");
					StartCoroutine (UnlockMode (0));
					PlayerPrefs.SetString ("Easy", "opened");
					tutorial.SetActive (false);
					position = Vector3.zero;
				}
			} else {
				if (backT) {
					tutorial.transform.GetChild (3).localPosition = Vector3.SmoothDamp (tutorial.transform.GetChild (3).localPosition, position, ref velocity, 0.1f);
				}
			}
		}
	}

	IEnumerator Tutorial(){
		int ind = indexT;
		yield return new WaitForSeconds (0.5f);
		yield return new WaitWhile (() => tutorial.GetComponent<Animation> ().isPlaying);
		if (indexT < 10) {
			tutorial.GetComponent<Animation> ().Play ("Slide " + (indexT + 1));
			yield return new WaitWhile (() => ind >= indexT);
			StartCoroutine (Tutorial ());
		}
	}

	void FirstOpenSettings(){

		if(!PlayerPrefs.HasKey("Sound")){
			PlayerPrefs.SetString ("Sound", "on");
			PlayerPrefs.SetString ("Music", "on");
		}
			

		if (!PlayerPrefs.HasKey ("AttemptsEasy")) {
			PlayerPrefs.SetInt ("AttemptsEasy", 0);
			PlayerPrefs.SetInt ("BestScoreEasy", 0);
			PlayerPrefs.SetInt ("AverageScoreEasy", 0);
			PlayerPrefs.SetInt ("SumScoreEasy", 0);
		}

		if (!PlayerPrefs.HasKey ("AttemptsMedium")) {
			PlayerPrefs.SetInt ("AttemptsMedium", 0);
			PlayerPrefs.SetInt ("BestScoreMedium", 0);
			PlayerPrefs.SetInt ("AverageScoreMedium", 0);
			PlayerPrefs.SetInt ("SumScoreMedium", 0);
		}

		if (!PlayerPrefs.HasKey ("AttemptsHard")) {
			PlayerPrefs.SetInt ("AttemptsHard", 0);
			PlayerPrefs.SetInt ("BestScoreHard", 0);
			PlayerPrefs.SetInt ("AverageScoreHard", 0);
			PlayerPrefs.SetInt ("SumScoreHard", 0);
		}

		if (!PlayerPrefs.HasKey ("AttemptsInsane")) {
			PlayerPrefs.SetInt ("AttemptsInsane", 0);
			PlayerPrefs.SetInt ("BestScoreInsane", 0);
			PlayerPrefs.SetInt ("AverageScoreInsane", 0);
			PlayerPrefs.SetInt ("SumScoreInsane", 0);
		}


		if (!PlayerPrefs.HasKey ("Mode")) {
			PlayerPrefs.SetString ("Mode", "Easy");
		}


		if (!PlayerPrefs.HasKey ("Easy")) {
			PlayerPrefs.SetString ("Easy", "locked");
		}

		if (!PlayerPrefs.HasKey ("Medium")) {
			PlayerPrefs.SetString ("Medium", "locked");
		}

		if (!PlayerPrefs.HasKey ("Hard")) {
			PlayerPrefs.SetString ("Hard", "locked");
		}

		if (!PlayerPrefs.HasKey ("Insane")) {
			PlayerPrefs.SetString ("Insane", "locked");
		}
	}

	void ChangeTextScore(){
		if (int.Parse (bestScore.GetComponent<Text> ().text)*bestScoreDelta > PlayerPrefs.GetInt ("BestScore" + PlayerPrefs.GetString ("Mode"))*bestScoreDelta) {
			bestScore.GetComponent<Text> ().text = (int.Parse (bestScore.GetComponent<Text> ().text) - bestScoreDelta-Mathf.Sign(bestScoreDelta)).ToString ();
		} else {
			bestScore.GetComponent<Text> ().text = PlayerPrefs.GetInt ("BestScore" + PlayerPrefs.GetString ("Mode")).ToString ();
		}

		if (int.Parse (bestScore.GetComponent<Text> ().text) < 0) {
			bestScore.GetComponent<Text> ().text = "0";
		}

		if (int.Parse (averageScore.GetComponent<Text> ().text)*averageScoreDelta > PlayerPrefs.GetInt ("AverageScore" + PlayerPrefs.GetString ("Mode"))*averageScoreDelta) {
			averageScore.GetComponent<Text> ().text = (int.Parse (averageScore.GetComponent<Text> ().text) - averageScoreDelta ).ToString ();
		} else {
			averageScore.GetComponent<Text> ().text = PlayerPrefs.GetInt ("AverageScore" + PlayerPrefs.GetString ("Mode")).ToString ();
		}

		if (int.Parse (averageScore.GetComponent<Text> ().text) < 0) {
			averageScore.GetComponent<Text> ().text = "0";
		}

		if (int.Parse (attempts.GetComponent<Text> ().text)*attemptsDelta > PlayerPrefs.GetInt ("Attempts" + PlayerPrefs.GetString ("Mode"))*attemptsDelta) {
			attempts.GetComponent<Text> ().text = (int.Parse (attempts.GetComponent<Text> ().text) - attemptsDelta).ToString ();
		} else {
			attempts.GetComponent<Text> ().text = PlayerPrefs.GetInt ("Attempts" + PlayerPrefs.GetString ("Mode")).ToString ();
		}

		if (int.Parse (attempts.GetComponent<Text> ().text) < 0) {
			attempts.GetComponent<Text> ().text = "0";
		}
	}

	public void NewGame(){
		StartCoroutine(OpenScene("Game",5/6f));
		GetComponent<Animation> ().Play ("NewGame");

		game = true;

	}

	void CheckModeLocked(string modeName, int modeNumber){
		if (PlayerPrefs.GetString (modeName) == "opened") {
			SetModeCondition (false, modeNumber);
		} else {
			SetModeCondition (true, modeNumber);
		}
	}

	void SetModeCondition(bool condition, int modeNumber){
		modeChooser.transform.GetChild (modeNumber).GetChild (1).gameObject.SetActive (condition);
		modeChooser.transform.GetChild (modeNumber).GetChild (0).GetComponent<BoxCollider2D>().enabled = !condition;
	}

	void Order(object sender, ValueChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		DataSnapshot snap = args.Snapshot;

		GetRecords (recordsTableEasy, snap.Child("easy"));
		GetRecords (recordsTableMedium, snap.Child("medium"));
		GetRecords (recordsTableHard, snap.Child("hard"));
		GetRecords (recordsTableInsane, snap.Child("insane"));
		GetRecords (recordsTableAbsolute, snap.Child("absolute"));

		PlayerPrefs.SetInt ("WorldBestScoreEasy", recordsTableEasy.Max (i => i.Value));
		PlayerPrefs.SetInt ("WorldBestScoreMedium", recordsTableEasy.Max (i => i.Value));
		PlayerPrefs.SetInt ("WorldBestScoreHard", recordsTableEasy.Max (i => i.Value));
		PlayerPrefs.SetInt ("WorldBestScoreInsane", recordsTableEasy.Max (i => i.Value));

		switch (PlayerPrefs.GetString ("Mode")) {
		case "Easy":
			modeN = 0;
			StartCoroutine(ShowRecords (recordsTableEasy));
			break;
		case "Medium":
			modeN = 1;
			StartCoroutine(ShowRecords (recordsTableMedium));
			break;
		case "Hard":
			modeN = 2;
			StartCoroutine(ShowRecords (recordsTableHard));
			break;
		case "Insane":
			modeN = 3;
			StartCoroutine(ShowRecords (recordsTableInsane));
			break;
		}

		recordsMode = modeN;
	}

	void GetRecords(Dictionary<string,int> rec, DataSnapshot snap){
		foreach (var cc in snap.Children) {
			rec [cc.Key] = int.Parse (cc.Value.ToString ());
		}
	}

	IEnumerator ShowRecords(Dictionary<string,int> rec){
		if (leaderbord.transform.GetChild (0).localScale.y !=0) {
			leaderbord.GetComponent<Animation> ().Play ("LeaderbordOdd");
		}
		if (leaderbord.transform.GetChild (9).gameObject.activeSelf) {
			leaderbord.transform.GetChild (9).gameObject.SetActive (false);
		}

		yield return new WaitForSeconds (20 / 60f);
		int k = 0;
		if (index == 0) {
			if (recordsMode == 0) {
				records.transform.GetChild (2).GetComponent<Animation> ().Play ("HideArrow");
			} else {
				records.transform.GetChild (2).GetComponent<Animation> ().Play ("AntiHideArrow");
			}
			records.transform.GetChild (4).GetComponent<Animation> ().Play ("AntiHideArrow");
		}
		foreach (var item in rec.OrderByDescending (i => i.Value)) {
			if (item.Key == PlayerPrefs.GetString("Nickname")) {
				if (k > 6) {
					leaderbord.transform.GetChild (5).gameObject.SetActive (false);
					leaderbord.transform.GetChild (7).gameObject.SetActive (true);
					leaderbord.transform.GetChild (6).GetChild (1).GetComponent<Text> ().color = new Color (0, 155 / 255f, 1, 1);
					leaderbord.transform.GetChild (6).GetChild (1).GetComponent<Text> ().text = item.Key;
					leaderbord.transform.GetChild (6).GetChild (0).GetComponent<Text> ().text = "#" + (k>998?"999+":(k + 1).ToString ());
					leaderbord.transform.GetChild (6).GetChild (2).GetComponent<Text> ().text = item.Value.ToString ();
					leaderbord.transform.GetChild (6).GetChild (2).GetComponent<Text> ().color = modeColors [recordsMode];
				} else {
					leaderbord.transform.GetChild (6).GetChild (0).GetComponent<Text> ().text = "#7";
					leaderbord.transform.GetChild (5).gameObject.SetActive (true);
					leaderbord.transform.GetChild (7).gameObject.SetActive (false);
					leaderbord.transform.GetChild (k).GetChild (1).GetComponent<Text> ().color = new Color (0, 155 / 255f, 1, 1);
				}

				break;
			}
			k++;
		}

		int kk = 0;

		foreach (var item in rec.OrderByDescending (i => i.Value)) {
			if (kk == 6 && k > 6) {					
				break;
			}
			leaderbord.transform.GetChild (kk).GetChild (1).GetComponent<Text> ().color = Color.white;
			leaderbord.transform.GetChild (kk).GetChild (2).GetComponent<Text> ().color = startColor;
			leaderbord.transform.GetChild (kk).GetChild (1).GetComponent<Text> ().color = Color.white;
			leaderbord.transform.GetChild (kk).GetChild (1).GetComponent<Text> ().text = item.Key;
			leaderbord.transform.GetChild (kk).GetChild (2).GetComponent<Text> ().text = item.Value.ToString ();
			leaderbord.transform.GetChild (kk).GetChild (2).GetComponent<Text> ().color = modeColors [recordsMode];
			kk++;
			if (kk == 7) {
				break;
			}
		}
		if (k < 7) {
			leaderbord.transform.GetChild (k).GetChild (1).GetComponent<Text> ().color = new Color (0, 155 / 255f, 1, 1);
		}
		leaderbord.GetComponent<Animation> ().Play ("Leaderbord");
		leaderbord.transform.GetChild (8).gameObject.SetActive (false);
	}

	IEnumerator OpenScene(string name, float time){
		yield return new WaitForSeconds (time);
		SceneManager.LoadScene (name);
	}

	IEnumerator ChooseMode(GameObject mode){
		
		Color startAllColor = Color.white;
		Color startTextColor = Color.white;
		Color[] startAnotherTextColor = new Color[]{
			Color.white,
			Color.white,
			Color.white,
			Color.white
		};
		Color endColor = Color.white;
		float time = Time.time;
		bool b = true;
		modeN = -1;

		switch (mode.name) {
		case "Easy":
			modeN = 0;
			StartCoroutine (ShowRecords (recordsTableEasy));
			break;
		case "Medium":
			modeN = 1;
			StartCoroutine (ShowRecords (recordsTableMedium));
			break;
		case "Hard":
			modeN = 2;
			StartCoroutine (ShowRecords (recordsTableHard));
			break;
		case "Insane":
			modeN = 3;
			StartCoroutine (ShowRecords (recordsTableInsane));
			break;
		}

		if(modeN!=-1){

			recordsMode = modeN;

			records.transform.GetChild (3).GetComponent<Text> ().color = modeColors[modeN];
			records.transform.GetChild (3).GetComponent<Text> ().text = modes [modeN];

			bestScoreDelta = PlayerPrefs.GetInt ("BestScore" + PlayerPrefs.GetString ("Mode"));
			attemptsDelta = PlayerPrefs.GetInt ("Attempts" + PlayerPrefs.GetString ("Mode"));
			averageScoreDelta = PlayerPrefs.GetInt ("AverageScore" + PlayerPrefs.GetString ("Mode"));

			//StartCoroutine (ChangeScores ());
			endColor = modeColors [modeN];
			
			PlayerPrefs.SetString ("Mode", mode.name);

			bestScoreDelta -= PlayerPrefs.GetInt ("BestScore"+PlayerPrefs.GetString("Mode"));
			attemptsDelta -= PlayerPrefs.GetInt ("Attempts"+PlayerPrefs.GetString("Mode"));
			averageScoreDelta -= PlayerPrefs.GetInt("AverageScore"+PlayerPrefs.GetString("Mode"));

			bestScoreDelta /= 10;
			attemptsDelta /= 10;
			averageScoreDelta /= 10;

			if (bestScoreDelta == 0) {
				bestScoreDelta = 1;
			}

			if (averageScoreDelta == 0) {
				averageScoreDelta = 1;
			}

			if (attemptsDelta == 0) {
				attemptsDelta = 1;
			}

			startAllColor = modeChooser.GetComponent<Image> ().color;
			startTextColor = mode.GetComponent<Text> ().color;

			for (int i = 0; i < 4; i++) {
				if (i != modeN) {
					startAnotherTextColor [i] = modeChooser.transform.GetChild (i).GetChild (0).GetComponent<Text>().color;
				}
			}

			while (b) {
				
				mode.GetComponent<Text> ().color = Color.Lerp (startTextColor, endColor, (Time.time - time)*3);

				for (int i = 0; i < 4; i++) {
					if (i != modeN) {
						modeChooser.transform.GetChild (i).GetChild (0).GetComponent<Text>().color = Color.Lerp (startAnotherTextColor[i], modeColor, (Time.time - time) * 3);
					}
				}
				
				modeChooser.GetComponent<Image> ().color = Color.Lerp (startAllColor, endColor, (Time.time - time)*3);

				bestScore.GetComponent<Text> ().color = Color.Lerp (startAllColor, endColor, (Time.time - time)*3);
				averageScore.GetComponent<Text> ().color = Color.Lerp (startAllColor, endColor, (Time.time - time)*3);
				attempts.GetComponent<Text> ().color = Color.Lerp (startAllColor, endColor, (Time.time - time)*3);

				if (mode.GetComponent<Text> ().color == endColor) {
					b = false;
				}

				yield return new WaitForEndOfFrame ();
			}	
		}


	}

	IEnumerator ChangeScores(){
		bestScore.GetComponent<Animation> ().Play ("Change");
		averageScore.GetComponent<Animation> ().Play ("Change");
		attempts.GetComponent<Animation> ().Play ("Change");

		yield return new WaitForSeconds (0.1f);

		bestScore.GetComponent<Text>().text = PlayerPrefs.GetInt ("BestScore"+PlayerPrefs.GetString("Mode")).ToString();
		attempts.GetComponent<Text>().text = PlayerPrefs.GetInt ("Attempts"+PlayerPrefs.GetString("Mode")).ToString();
		averageScore.GetComponent<Text>().text = PlayerPrefs.GetInt("AverageScore"+PlayerPrefs.GetString("Mode")).ToString();

	}

	IEnumerator UnlockMode(int modeNumber){
		yield return new WaitForSeconds (100 / 60f);
		modeChooser.transform.GetChild (modeNumber).GetChild (1).GetComponent<Animation> ().Play ("UnlockMode");
		modeChooser.transform.GetChild (modeNumber).GetChild (1).GetComponent<AudioSource> ().Play ();
		modeChooser.transform.GetChild (modeNumber).GetChild (0).GetComponent<BoxCollider2D>().enabled = true;
		yield return new WaitForSeconds (1);
		modeChooser.transform.GetChild (modeNumber).GetChild (1).gameObject.SetActive (false);
	}

	IEnumerator LateChoosedMode(){
		yield return new WaitForSeconds (101/60f);
		choosedMode.transform.parent.GetComponent<Animation> ().Play ("StartHoverMode");
	}

	IEnumerator LateNewGame(){
		choosedMode.transform.parent.GetComponent<Animation> ().Play ("EndHoverMode");
		yield return new WaitForSeconds (11 / 60f);
		NewGame ();
	}

	IEnumerator ChangeRecordsHead(string mode){
		records.transform.GetChild (3).GetComponent<Animation> ().Play ("ChangeRecordsMode");
		yield return new WaitForSeconds (0.25f);
		records.transform.GetChild (3).GetComponent<Text> ().color = modeColors [recordsMode];
		records.transform.GetChild (3).GetComponent<Text> ().text = mode;

	}

	bool CheckConnection(){
		try{

			using(var client = new WebClient())
			using(var stream = new WebClient().OpenRead("http://google.com")){
				return true;
			}

		} catch{
			return false;
		}
	}

}
