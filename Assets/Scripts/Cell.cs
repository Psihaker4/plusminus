using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour {

	static Color[] colors = new Color[] {
		new Color (156 / 255f, 39 / 255f, 176 / 255f),
		new Color (103 / 255f, 58 / 255f, 183 / 255f),
		new Color (63 / 255f, 81 / 255f, 181 / 255f),
		new Color (3 / 255f, 169 / 255f, 244 / 255f),
		new Color (0, 150 / 255f, 136 / 255f),
		new Color (139 / 255f, 195 / 255f, 74 / 255f),
		new Color (205 / 255f, 220 / 255f, 57 / 255f),
		new Color (1, 193 / 255f, 7 / 255f),
		new Color (1, 112 / 255f, 67 / 255f),
		new Color (244 / 255f, 67 / 255f, 54 / 255f),
	};

	RectTransform rectTransform;
	Image image;
	Text text;

	int number;
	int delta;

	bool row;
	bool removable;
	bool colorAnimation;
	bool remove1Animation,
		 remove2Animation;
	public bool moveAnimation;
	bool newAnimation;
	bool fade;
	bool choose;

	Vector3 startPosition, endPosition;
	Vector3 endScale;
	Color startColor, endColor;

	Vector3 velocity;
	Vector3 scaleVelocity;

	float animationTime, animationSpeed, colorAnimationTime;

	void Awake () {

		rectTransform = gameObject.GetComponent<RectTransform>();
		image = gameObject.GetComponent<Image>();
		text = gameObject.GetComponentInChildren<Text> ();

		startPosition = rectTransform.localPosition;
		endPosition = startPosition;

		gameObject.transform.localScale = Vector3.one;
		gameObject.SetActive (false);
		removable = false;
		moveAnimation = false;
		fade = false;
		choose = false;

	}

	void Update () {

		/*if (newAnimation) {
			rectTransform.localScale = Vector3.SmoothDamp (rectTransform.localScale, endScale, ref scaleVelocity,animationTime);
		}*/

		if (moveAnimation) {
			rectTransform.localPosition = Vector3.SmoothDamp (rectTransform.localPosition, endPosition, ref velocity, animationTime*Mathf.Pow(delta,1/3f));
			if (Mathf.Abs (rectTransform.localPosition.y - endPosition.y) < 0.1f) {
				/*if (newAnimation) {
					newAnimation = false;
					rectTransform.localScale = Vector3.one;
				}*/
				delta = 0;
				rectTransform.localPosition = endPosition;
				moveAnimation = false;
				GetComponent<BoxCollider2D> ().enabled = true;
				transform.parent.parent.GetComponent<FieldControlls> ().CheckFieldRay (false);
				StartCoroutine (DelayAnimations ());
			}
		}

		if (colorAnimation) {
			image.color = Color.Lerp (startColor, endColor, (Time.time - colorAnimationTime) * 4);
			if (image.color == endColor) {
				colorAnimation = false;
				transform.parent.parent.GetComponent<FieldControlls> ().CheckFieldRay (false);
				StartCoroutine (DelayAnimations ());				
			}
		}

		if (remove1Animation || remove2Animation) {
			
			rectTransform.localPosition = Vector3.SmoothDamp (rectTransform.localPosition, endPosition, ref velocity, animationTime);
			rectTransform.localScale = Vector3.SmoothDamp (rectTransform.localScale, endScale, ref scaleVelocity, animationTime);

			if (remove1Animation) {
				
				if (Mathf.Abs (rectTransform.localPosition.y - endPosition.y) < 15) { 
					endScale = Vector3.zero;
					velocity = Vector3.zero;
					scaleVelocity = Vector3.zero;
					remove1Animation = false;
					remove2Animation = true;
					endPosition -= Vector3.up * 90;
				} 
			}

			if (remove2Animation && Mathf.Abs (rectTransform.localScale.y - endScale.y) < 0.01f) {
				gameObject.SetActive (false);
				velocity = Vector3.zero;
				remove2Animation = false;
				gameObject.transform.GetChild (0).GetComponent<Image> ().color = new Color (1, 1, 1, 0);
				gameObject.transform.GetChild (1).GetComponent<Text> ().color = new Color (1, 1, 1, 0);
			}

		}
	}

	public void CreateCell(int range, int i, int j){	
		
		SetNumber (RandomizeNumber (range));
		SetPosition (i, j);

		removable = false;

		gameObject.SetActive (true);

		endPosition = rectTransform.localPosition;	
	}

	public void CreateNewCell(int range, int j){	

		SetNumber (RandomizeNumber (range));
		rectTransform.localPosition = new Vector3 (-350 + 140 * j, 0);

		removable = false;

		fade = false;
		choose = false;

		transform.GetChild (3).GetComponent<Image> ().enabled = false;

		transform.GetChild (4).GetComponent<Image> ().enabled = false;

		transform.localScale = Vector3.one;
		gameObject.SetActive (true);

		GetComponent<BoxCollider2D> ().enabled = false;

	}

	int RandomizeNumber(int range){

		int r = Random.Range (0, range+10);
		int n = (int)(r/10);
		return n;

	}

	public void SetNumber(int number){
		this.number = number;
		image.color = new Color (colors [number].r, colors [number].g, colors [number].b, 0);
		text.text = number.ToString ();

	}

	public void SetPreNumber(int number){
		this.number = number;
	}

	public void DeleteNumber(){
		number = -10;
	}

	void SetPosition(int i, int j){
		rectTransform.localPosition = new Vector3 (-350+140*j, -560+80+160*i);
	}

	public int GetNumber(){
		return number;
	}

	public void Remove(){
		if (!removable) {
			
			StartCoroutine (DelayRemove ());

			removable = true;
			remove1Animation = true;
			moveAnimation = false;

			endPosition = rectTransform.localPosition + Vector3.up * 50;

			endScale = Vector3.one * 0.7f;
			animationTime = 0.07f;

			gameObject.name = "Buffer Cell";

			scaleVelocity = Vector3.zero;
			velocity = Vector3.zero;

		}

	}

	public void SlowRemove(){
		Remove ();
		animationTime = 0.15f;
	}

	public bool IsAnimated(){
		return moveAnimation;
	}

	public bool IsRemovable(){
		return removable;
	}

	public bool ChangeNumber(int n, int range){
		if (number + n < range+1 && number+n>-1) {
			number += n;
			ChangeColor (number);
			return true;
		} else {
			return false;
		}
	}

	void ChangeColor(int n){

		colorAnimation = true;
		startColor = image.color;
		endColor = colors [n];
		colorAnimationTime = Time.time;
		text.text = number.ToString ();

	}

	public void Move(int d){
		moveAnimation = true;
		endPosition -= Vector3.up * d * 160;
		delta += d;

		animationTime = 0.09f;

		velocity = Vector3.zero;
	}

	public void NewMove(int d, int j){
		
		moveAnimation = true;
		endPosition = new Vector3 (-350 + 140 * j, -560+80 + 160 * (d + 1));
		delta = 1;

		animationTime = 0.11f;

		//newAnimation = true;
		//endScale = Vector3.one;

		scaleVelocity = Vector3.zero;
		velocity = Vector3.zero;
	}

	public int GetDelta(){
		return delta;
	}

	public int GetEndIndex(){
		return Mathf.RoundToInt ((560-80 + endPosition.y) / 160);
	}

	public void Fade(){
		fade = true;
	}

	public void AntiFade(){
		fade = false;
	}

	public bool IsFade(){
		return fade;
	}

	public void Choose(){
		choose = true;
	}

	public void AntiChoose(){
		choose = false;
	}

	public bool IsChoose(){
		return choose;
	}

	IEnumerator DelayRemove(){
		yield return new WaitForEndOfFrame();
		gameObject.GetComponent<BoxCollider2D> ().enabled = false;
		transform.SetParent (transform.parent.parent.GetChild(4));
	}

	IEnumerator DelayAnimations(){
		yield return new WaitForEndOfFrame ();
		yield return new WaitForEndOfFrame ();
		transform.parent.parent.GetComponent<FieldControlls>().AnimateCells ();
	}

}
