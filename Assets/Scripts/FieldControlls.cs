using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

public class FieldControlls : MonoBehaviour {
	
	public GameObject cellPrefab;
	public GameObject cellsBuffer, cellsField, newLine;

	public GameObject bottomBar;

	public GameObject timer;

	GameObject[] cells = new GameObject[42];

	GameObject[] newCells = new GameObject[6];

	GameObject[] raysHorizontal = new GameObject[8];
	GameObject[] raysVertical = new GameObject[6];

	int numbersRange;

	int score;
	int record;
	//int levelUPScore;
	//int levelUPDelta;
	int chooserCount = 0;
	int nextNumber;

	int[] levelUpScore;

	int[] numbersRemoved = new int[10];

	string stats = "amount of removed numbers: ";
	string stats2 = "game time: ";

	bool lose;
	bool newRecord;

	void Awake(){

		switch (PlayerPrefs.GetString ("Mode")) {
		case "Easy":
			//4-6
			//200 500 800
			numbersRange = 40;
			levelUpScore = new int[] { 250, 500, 750 };
			break;
		case "Medium":
			//5-7
			//300 700 1100
			numbersRange = 50;
			levelUpScore = new int[] { 350, 700, 1050 };
			break;
		case "Hard":
			//6-8
			//400 900 1400
			numbersRange = 60;
			levelUpScore = new int[] { 500, 1000, 1500 };
			break;
		case "Insane":
			//7-9
			//500 1100 1700
			numbersRange = 70;
			levelUpScore = new int[] { 700, 1400, 2100 };
			break;
		}

		nextNumber = 0;
		score = 0;
		lose = false;
		newRecord = false;

		record = PlayerPrefs.GetInt ("BestScore" + PlayerPrefs.GetString ("Mode"));

		transform.parent.GetComponent<GameUI> ().ShowScore (score,record);

		for (int i = 0; i < 48; i++) {
			(Instantiate (cellPrefab, cellsBuffer.transform) as GameObject).name = "Buffer Cell";
		}

		Transform t = transform.GetChild (1).transform;
		for (int i = 0; i < 8; i++) {
			raysHorizontal [i] = t.GetChild (i).gameObject;
		}

		t = transform.GetChild (2).transform;
		for (int i = 0; i < 6; i++) {
			raysVertical [i] = t.GetChild (i).gameObject;
		}

		NewField ();

		NewLine ();

		StartCoroutine (WaitForChoosers ());
	}

	public int GetRange(){
		return numbersRange / 10;
	}

	void NewField(){
		
		for (int i = 0; i < 18; i++) {
			cells [i] = cellsBuffer.transform.GetChild(0).gameObject;
			cells [i].name = "Cell";
			cells [i].transform.SetParent (cellsField.transform);
			cells [i].GetComponent<Cell> ().CreateCell (numbersRange, i / 6, i-6*(i/6));
			cells [i].SetActive (true);
			//cells [i].transform.localScale = Vector3.one;
		}

		while (CheckFieldRay (true)) {
			for (int i = 0; i < 18; i++) {
				cells [i].GetComponent<Cell> ().CreateCell (numbersRange, i / 6, i-6*(i/6));
			}
		}

		/*for (int i = 0; i < 18; i++) {
			cells [i].transform.localScale = Vector3.zero;
		}*/

	}

	void NewLine(){
		
		for (int i = 0; i < 6; i++) {
			newCells [i] = cellsBuffer.transform.GetChild (0).gameObject;
			newCells [i].transform.SetParent (newLine.transform);
			newCells [i].name = "New Cell";
			newCells [i].GetComponent<Cell> ().CreateNewCell (numbersRange, i);
		}

	}

	public bool CheckFieldRay(bool fast){

		bool b = false;
		if (!lose) {
			for (int i = 0; i < 7; i++) {
				RaycastHit2D[] hits = Physics2D.RaycastAll (raysHorizontal [i].transform.position, Vector2.right);
				for (int j = 0; j < hits.Length - 2; j++) {
					if (hits [j].collider.CompareTag ("Cell") && hits [j+1].collider.CompareTag ("Cell") && hits [j+2].collider.CompareTag ("Cell")) {
						if (CheckNext (hits, j, true, fast)) {
							if (fast) {
								return true;
							}
							b = true;
						}
					}
				}
			}

			for (int i = 0; i < 6; i++) {
				RaycastHit2D[] hits = Physics2D.RaycastAll (raysVertical [i].transform.position, Vector2.up);
				for (int j = 0; j < hits.Length - 2; j++) {
					if (hits [j].collider.CompareTag ("Cell") && hits [j+1].collider.CompareTag ("Cell") && hits [j+2].collider.CompareTag ("Cell")) {
						if (CheckNext (hits, j, false, fast)) {
							if (fast) {
								return true;
							}
							b = true;
						}
					}
				}
			}
		}
		return b;
	}

	bool CheckNext(RaycastHit2D[] rs, int i, bool row, bool fast){
		if (GetCellIndex (rs [i+1].collider.gameObject,row) - GetCellIndex (rs [i].collider.gameObject,row) == 1 && GetCellIndex (rs [i + 2].collider.gameObject,row) - GetCellIndex (rs [i + 1].collider.gameObject,row) == 1) {
			if (rs [i].collider.GetComponent<Cell> ().GetNumber () == rs [i + 1].collider.GetComponent<Cell> ().GetNumber () && rs [i+1].collider.GetComponent<Cell> ().GetNumber () == rs [i + 2].collider.GetComponent<Cell> ().GetNumber ()){
				if (!fast) {
					if (PlayerPrefs.GetString ("Sound") == "on") {
						cellsField.GetComponent<AudioSource> ().Play ();
					}
					for (int j = i; j < i + 3; j++) {
						if (!rs [j].collider.gameObject.GetComponent<Cell> ().IsRemovable ()) {
							
							rs [j].collider.gameObject.GetComponent<Cell> ().Remove ();
							score += rs [j].collider.gameObject.GetComponent<Cell> ().GetNumber ();
							numbersRemoved [rs [j].collider.gameObject.GetComponent<Cell> ().GetNumber ()]++;
							PlayerPrefs.SetInt ("SumScore"+PlayerPrefs.GetString ("Mode"), PlayerPrefs.GetInt ("SumScore"+PlayerPrefs.GetString ("Mode")) + rs [j].collider.gameObject.GetComponent<Cell> ().GetNumber ());

							if (score >= levelUpScore [nextNumber] && nextNumber < 2) {
								numbersRange += 10;

								transform.parent.GetComponent<GameUI> ().NewNumber (numbersRange / 10);

								nextNumber++;

							}

							if (score > record) {
								PlayerPrefs.SetInt ("BestScore" + PlayerPrefs.GetString ("Mode"), score);

								record = score;

								if (score > levelUpScore [2]) {
									switch (PlayerPrefs.GetString ("Mode")) {
									case "Easy":
										if (PlayerPrefs.GetString ("Medium") == "locked") {
											PlayerPrefs.SetString ("Medium", "openNow");

											StartCoroutine (transform.parent.GetComponent<GameUI> ().NewMode ("medium", 1));
										}
										break;
									case "Medium":
										if (PlayerPrefs.GetString ("Hard") == "locked") {
											PlayerPrefs.SetString ("Hard", "openNow");
											StartCoroutine (transform.parent.GetComponent<GameUI> ().NewMode ("hard", 2));
										}
										break;
									case "Hard":
										if (PlayerPrefs.GetString ("Insane") == "locked") {
											PlayerPrefs.SetString ("Insane", "openNow");
											StartCoroutine (transform.parent.GetComponent<GameUI> ().NewMode ("insane", 3));
										}
										break;
									case "Insane":
										StartCoroutine (transform.parent.GetComponent<GameUI> ().NewMode ("awesome", 4));
										break;
									}

								}

								if (!newRecord) {
									newRecord = true;
									transform.parent.GetComponent<Animation> ().Play ("NewRecord");
									transform.parent.GetChild (3).GetChild (0).gameObject.SetActive (true);
									if (PlayerPrefs.GetString ("Sound") == "on") {
										transform.parent.GetChild (3).GetChild (0).GetComponent<AudioSource> ().Play ();
									}
								}

							}

							transform.parent.GetComponent<GameUI> ().ShowScore (score,record);
						}
					}
				}
				return true;
			}
		}	
		return false;
	}

	int GetCellIndex(GameObject g,bool row){
		if (row) {
			return Mathf.RoundToInt ((350 + g.transform.localPosition.x) / 140);
		} else {
			return Mathf.RoundToInt ((560-80 + g.transform.localPosition.y) / 160);
		}
	}

	public void AnimateCells(){
		if (!lose) {
			GameObject cell;

			for (int i = 0; i < cellsField.transform.childCount; i++) {
				cell = cellsField.transform.GetChild (i).gameObject;
				if (!cell.GetComponent<Cell> ().IsRemovable ()) {
					
					cell.GetComponent<BoxCollider2D> ().enabled = false;
					int delta = 0;

					if (!cell.GetComponent<Cell> ().IsAnimated ()) {
						delta = GetCellIndex (cell, false);
					}

					RaycastHit2D hit = Physics2D.Raycast (cell.transform.position, -Vector2.up);
								
					if (hit.collider != null && hit.collider.CompareTag ("Cell")) {
					
						if (hit.collider.GetComponent<Cell> ().IsAnimated ()) {
							if (!cell.GetComponent<Cell> ().IsAnimated ()) {		
								delta -= hit.collider.GetComponent<Cell> ().GetEndIndex () + 1;
							} else {
								delta = cell.GetComponent<Cell> ().GetEndIndex () - hit.collider.GetComponent<Cell> ().GetEndIndex () - 1;
							}
						} else {
							if (!cell.GetComponent<Cell> ().IsAnimated ()) {		
								delta -= GetCellIndex (hit.collider.gameObject, false) + 1;
							} else {
								delta = cell.GetComponent<Cell> ().GetEndIndex () - hit.collider.GetComponent<Cell> ().GetEndIndex () - 1;
							}
						}

					} else {
						if (cell.GetComponent<Cell> ().IsAnimated ()) {
							delta = cell.GetComponent<Cell> ().GetEndIndex ();
						}
					}

					if (delta > 0) {
						cell.GetComponent<Cell> ().Move (delta);
					}

					cell.GetComponent<BoxCollider2D> ().enabled = true;
					
				}
			}
		}
	}

	public void IncreaseChooserCount(bool checkingChoosers){
		chooserCount++;
		if (chooserCount == 4) {
			if (!checkingChoosers) {
				timer.GetComponent<Timer> ().Stop ();
			}
			StartCoroutine (CheckNewLine ());
			
			chooserCount = 0;

		}
	}

	public IEnumerator CheckNewLine(){

		bool b = true;

		while (bottomBar.transform.childCount != 5) {
			yield return new WaitForEndOfFrame ();
		}

		for (int j = 0; j < 2; j++) {
			b = true;
			RaycastHit2D[] checkHits = Physics2D.RaycastAll (raysHorizontal [6].transform.position, Vector2.right);
			for (int i = 0; i < checkHits.Length; i++) {
				if (checkHits[i].collider.CompareTag("Cell") && !checkHits [i].collider.GetComponent<Cell> ().IsAnimated ()) {
					b = false;
					break;
				}
			}

			if (b) {

				for (int i = 1; i < 5; i++) {
					bottomBar.transform.GetChild (i).gameObject.GetComponent<Chooser> ().Create ();
				}	

				if (cellsField.transform.childCount != 0) {
					while (HaveTurns () == 0) {
						for (int i = 1; i < 5; i++) {
							bottomBar.transform.GetChild (i).gameObject.GetComponent<Chooser> ().RandomizeNumber ();
						}	
					}
				}

				CreateNewLine ();
				break;
			} 
			yield return new WaitForSeconds (0.1f);
		}

		if (!b) {			

			for (int i = 0; i < cellsField.transform.childCount; i++) {
				cellsField.transform.GetChild (i).GetComponent<Cell> ().SlowRemove ();
			}

			int h = (int)Time.timeSinceLevelLoad / 3600;
			int m = ((int)Time.timeSinceLevelLoad - h * 3600) / 60;
			int s = ((int)Time.timeSinceLevelLoad - h * 3600) % 60;

			stats2 += (h != 0 ? h + "h " : "") + (m != 0 ? m + "m " : "") + s + "s";

			for (int i = 0; i < 10; i++) {
				stats += i + " - " + numbersRemoved [i] + ((numbersRemoved [i]%10==1 && numbersRemoved [i]%100!=11)?" item":" items");
				if (i != 9) {
					stats += ", ";
				} else {
					stats += "";
				}
			}

			transform.parent.GetComponent<GameUI> ().Lose (score, stats,stats2);

			lose = true;
		}

	}

	public void CreateNewLine(){
					
		transform.parent.GetComponent<GameUI> ().StartNewLineAnimations ();

		GameObject cell;

		for (int j = 0; j < 6; j++) {
			cell = newLine.transform.GetChild (0).gameObject;
			cell.name = "Cell";
			cell.transform.SetParent (cellsField.transform);
			int delta = -1;	
			RaycastHit2D[] hits = Physics2D.RaycastAll (raysVertical [j].transform.position, Vector2.up);
			if (hits.Length > 0) {			

				if (hits [hits.Length - 1].collider != null) {
					int n = hits.Length - 1;
					while (!hits [n].collider.CompareTag ("Cell")) {
						n--;
					}
					if (hits [n].collider.GetComponent<Cell> ().IsAnimated ()) {
						delta = hits [n].collider.GetComponent<Cell> ().GetEndIndex ();
					} else {
						delta = GetCellIndex (hits [n].collider.gameObject, false);
					}
				}
			
			} 

			cell.GetComponent<Cell> ().NewMove (delta, j);
		}
		if (PlayerPrefs.GetString ("Sound") == "on") {
			newLine.GetComponent<AudioSource> ().Play ();
		}
		NewLine ();
	}

	public int GetScore(){
		return score;
	}

	public void FadeCells(int n){
		for (int i = 0; i < cellsField.transform.childCount; i++) {
			if (cellsField.transform.GetChild (i).GetComponent<Cell> ().GetNumber () + n > numbersRange / 10 || cellsField.transform.GetChild (i).GetComponent<Cell> ().GetNumber () + n < 0) {
				StartCoroutine(Fade(cellsField.transform.GetChild (i).GetComponent<Animation> (),"CellFade"));
				cellsField.transform.GetChild (i).GetComponent<Cell> ().Fade ();
			}
		}
	}

	public void AntiFadeCells(){
		for (int i = 0; i < cellsField.transform.childCount; i++) {
			if (cellsField.transform.GetChild (i).GetComponent<Cell> ().IsFade()){
				StartCoroutine(Fade(cellsField.transform.GetChild (i).GetComponent<Animation> (),"CellAntiFade"));
				cellsField.transform.GetChild (i).GetComponent<Cell> ().AntiFade ();
			}
		}
	}

	IEnumerator Fade(Animation a, string name){
		while (a.isPlaying) {
			yield return new WaitForEndOfFrame ();
		}
		a.Play (name);
	}

	public void CheckChoosers(){
		
		int k = 0;

		for (int i = 1; i < bottomBar.transform.childCount; i++) {
			if (k != 4) {
				if (bottomBar.transform.GetChild (i).gameObject.activeSelf) {

					if (!CheckChooserTurn (i)) {
						bottomBar.transform.GetChild (i).GetComponent<Chooser> ().SelfDelete ();
						IncreaseChooserCount (false);
						i--;
						k++;
					} else {

						bool b = true;
						do {
							int r = Random.Range (0, cellsField.transform.childCount);
							int n = cellsField.transform.GetChild (r).GetComponent<Cell> ().GetNumber () + bottomBar.transform.GetChild (i).GetComponent<Chooser> ().GetNumber ();
							if (n > -1 && n < numbersRange / 10 + 1) {
								b = false;
								cellsField.transform.GetChild (r).GetComponent<Cell> ().SetPreNumber (n);
								bottomBar.transform.GetChild (i).GetComponent<Chooser> ().GoToCell (cellsField.transform.GetChild (r).localPosition, r);
								i--;
								k++;
								IncreaseChooserCount (true);
								if (PlayerPrefs.GetString ("Sound") == "on") {
									cellsField.transform.GetChild (r).GetComponent<AudioSource>().Play();
								}
							}

						} while(b);
					}
				} else {
					k++;
				}
			}
		}
	}

	public int HaveTurns(){

		int b = 0;

		for (int j = 1; j < bottomBar.transform.childCount; j++) {
			if (bottomBar.transform.GetChild (j).gameObject.activeSelf) {
				if (CheckChooserTurn (j)) {
					b++;
				}
			}
		}
		return b;
	}

	public bool CheckChooserTurn(int j){
		if (cellsField.transform.childCount == 0) {
			return false;
		}
		bool b = false;

		for (int i = 0; i < cellsField.transform.childCount; i++) {			
			int n = cellsField.transform.GetChild (i).GetComponent<Cell> ().GetNumber () + bottomBar.transform.GetChild (j).GetComponent<Chooser> ().GetNumber ();
			if (n < numbersRange / 10 + 1 && n > -1) {
				b = true;
				break;
			}
		}

		return b;

	}

	IEnumerator WaitForChoosers(){
		yield return new WaitForEndOfFrame ();
		while (HaveTurns () == 0) {
			print ("newNumbers");
			for (int i = 1; i < 5; i++) {
				bottomBar.transform.GetChild (i).gameObject.GetComponent<Chooser> ().RandomizeNumber ();
			}	
		}
	}

	public int ChooserCount(){
		return chooserCount;
	}

	public void ChangeGameState(bool b){
		lose = b;
	}

	public int GetFullScore(){
		return levelUpScore [2];
	}
}
