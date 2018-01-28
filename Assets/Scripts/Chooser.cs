using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class Chooser : MonoBehaviour {


	public bool plus;
	public GameObject bottomBar,field;

	RectTransform rectTransform;
	Text text;
	int number;
	int autoCellNumber;
	Vector3 originalPosition;
	bool animated;

	public bool deleteAnimation;
	public bool takeAnimation;
	public bool chooseAnimation;
	public bool backAnimation;
	public bool createAnimation;
//	bool autoChooseAnimation;
	public bool autoAnimation;
	public bool selfDeleteAnimation;

	Vector3 startPosition, endPosition, autoPosition;
	float animationStartTime,
			animationSpeed;

	Vector3 startScale, endScale;
	Vector3 velocity, scaleVelocity;

	GameObject autoCell;

	void Start () {

		rectTransform = gameObject.GetComponent<RectTransform> ();
		text = gameObject.GetComponentInChildren<Text> ();

		originalPosition = rectTransform.localPosition;

		startScale = Vector3.one;
		endScale = startScale * 0.8f;

		startPosition = rectTransform.localPosition;
		endPosition = startPosition;

		RandomizeNumber ();
	}
	
	void Update () {
		
		if (createAnimation) {
			rectTransform.localScale = Vector3.SmoothDamp (rectTransform.localScale, Vector3.one, ref scaleVelocity, 0.2f);
		}

		if (backAnimation) {
			rectTransform.localPosition = Vector3.SmoothDamp (rectTransform.localPosition, endPosition, ref velocity, 0.1f);
			if (deleteAnimation) {
				rectTransform.localScale = Vector3.SmoothDamp (rectTransform.localScale, Vector3.zero, ref scaleVelocity, 0.1f);
			} else {
				if (Mathf.Abs (rectTransform.localPosition.y - endPosition.y) < 1f) {
					gameObject.GetComponent<BoxCollider2D> ().enabled = true;
				}
			}

			if (Mathf.Abs (rectTransform.localPosition.y - endPosition.y) < 0.1f) {
				rectTransform.localPosition = endPosition;
				rectTransform.localScale = Vector3.one;
				backAnimation = false;
				//rectTransform.localScale = Vector3.one;

				/*if (gameObject.activeSelf) {
					AutoChoose ();
				}*/

				if (deleteAnimation) {
					
					gameObject.SetActive (false);
					transform.SetParent (bottomBar.transform);
					rectTransform.localScale = Vector3.zero;
					rectTransform.localPosition = originalPosition;
					deleteAnimation = false;

					/*bottomBar.transform.GetChild (1).GetComponent<Chooser> ().AutoChoose ();

					k++;
					if (k == 4) {
						k = 0;
						if (field.GetComponent<FieldControlls>().CreateNewLine ()) {
							for (int i = 1; i < 5; i++) {
								bottomBar.transform.GetChild (i).gameObject.GetComponent<Chooser> ().Create ();
							}
						}
					}*/
				}

			}
		}
			
		if (takeAnimation) {
			rectTransform.localScale = Vector3.Lerp (startScale, endScale, (Time.time - animationStartTime) / animationSpeed);
			if (rectTransform.localScale == endScale) {
				takeAnimation = false;
			}
		}

		if (chooseAnimation) {
			rectTransform.position = Vector3.SmoothDamp (rectTransform.position, new Vector3 (Camera.main.ScreenToWorldPoint (Input.mousePosition).x, Camera.main.ScreenToWorldPoint (Input.mousePosition).y), ref velocity, 0.02f);
		}

		if (autoAnimation) {
			rectTransform.localPosition = Vector3.SmoothDamp (rectTransform.localPosition, endPosition, ref velocity, 0.1f);
			if (Mathf.Abs (rectTransform.localPosition.y - endPosition.y) < 50f) {

				autoCell.GetComponent<Cell> ().ChangeNumber (0, field.GetComponent<FieldControlls> ().GetRange ());
				Delete (endPosition);
				autoAnimation = false;

			}
		}

		if (selfDeleteAnimation) {

			rectTransform.localScale = Vector3.SmoothDamp (rectTransform.localScale, Vector3.zero, ref scaleVelocity, 0.1f);
			if (rectTransform.localScale.y < 0.01f) {
				gameObject.SetActive (false);
				transform.SetParent (bottomBar.transform);
				rectTransform.localScale = Vector3.zero;
				rectTransform.localPosition = originalPosition;
				selfDeleteAnimation = false;
			}
		}

		/*if (autoChooseAnimation) {
			rectTransform.localPosition = Vector3.SmoothDamp (rectTransform.localPosition, autoPosition, ref velocity, 0.01f);
			if (Mathf.Abs (rectTransform.localPosition.x - autoPosition.x) < 1) {
				autoChooseAnimation = false;
				RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector2.zero);
				if (hit.collider != null && hit.collider.gameObject.CompareTag ("Cell") && hit.collider.gameObject.GetComponent<Cell> ().ChangeNumber (GetNumber (), field.GetComponent<FieldControlls> ().GetRange ())) {
					Delete (hit.collider.transform.localPosition);
				} else {
					GetBack ();
				}
			}			
		}*/
	}

	public void RandomizeNumber(){

		int r = Random.Range (10, 40);
		int n = (int)(r/10);

		SetNumber (n);
	}

	public void SetNumber(int n){

		if (plus) {
			number = n;
		} else {
			number = -n;
		}

		text.text = number.ToString ();

	}

	public int GetNumber(){
		return number;
	}

	public void GetBack(){
		if (!deleteAnimation) {
			transform.SetParent (bottomBar.transform);

			SetMovingAnimation (originalPosition, 2000);
			startScale = rectTransform.localScale;
			endScale = Vector3.one;
			takeAnimation = true;

			velocity = Vector3.zero;
		}
	}

	public void Delete(Vector3 pos){
		transform.SetParent (field.transform);

		SetMovingAnimation (pos, 100f);
		startScale = rectTransform.localScale;
		deleteAnimation = true;
		takeAnimation = false;

		scaleVelocity = Vector3.zero;
		velocity = Vector3.zero;
	}

	public void SelfDelete(){

		transform.SetParent (field.transform);
		chooseAnimation = false;
		createAnimation = false;
		startScale = rectTransform.localScale;
		deleteAnimation = false;
		takeAnimation = false;
		backAnimation = false;
		selfDeleteAnimation = true;
		scaleVelocity = Vector3.zero;
		velocity = Vector3.zero;
	}

	void SetMovingAnimation(Vector3 pos, float speed){
		chooseAnimation = false;
		if (rectTransform.localPosition != pos) {
			backAnimation = true;
			startPosition = rectTransform.localPosition;
			endPosition = pos;
			animationStartTime = Time.time;
			animationSpeed = Vector3.Distance (startPosition, endPosition) / speed;
		} else {
			GetComponent<BoxCollider2D> ().enabled = true;
		}
	}

	public void Choose(){
		transform.SetParent (field.transform);
		transform.SetParent (bottomBar.transform);
		gameObject.GetComponent<BoxCollider2D> ().enabled = false;
		takeAnimation = true;
		chooseAnimation = true;
		createAnimation = false;
		backAnimation = false;
		animationStartTime = Time.time;
		animationSpeed = 0.1f;
		startScale = Vector3.one;
		endScale = startScale * 0.8f;

		velocity = Vector3.zero;
	}

	public void Create(){
		createAnimation = true;
		RandomizeNumber ();
		gameObject.SetActive (true);
		gameObject.GetComponent<BoxCollider2D> ().enabled = true;

		scaleVelocity = Vector3.zero;

	}

	public void GoToCell(Vector3 pos, int r){
		Choose ();
		chooseAnimation = false;
		autoAnimation = true;
		transform.SetParent (field.transform);
		endPosition = pos;

		autoCellNumber = r;

		autoCell = field.transform.GetChild (3).GetChild (r).gameObject;
	}

	/*public void AutoChoose(){
		Choose ();
		chooseAnimation = false;
		autoChooseAnimation = true;
		autoPosition = new Vector3 (Random.Range (-450f, 450f), Random.Range (0, 1200f));
	}*/

}
