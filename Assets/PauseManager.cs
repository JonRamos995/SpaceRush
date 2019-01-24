using UnityEngine;
using System.Collections;

public class PauseManager : MonoBehaviour {

	private bool pause;
	public GUIText PauseText;
	public GUIText PauseText2;
	public GameObject PausedCover;
	public GameObject GameController;

	// Use this for initialization
	void Start () {
		pause = false;
	}
	
	// Update is called once per frame
	void Update () 	{
		if (Input.GetKeyDown(KeyCode.Escape) && !GameController.GetComponent<GameController>().IsGameOver())
			{
			if (pause) {
				pause = false;
			//	PauseText.text = "";
			//	PauseText2.text = "";
				//PausedCover.GetComponent<MeshRenderer> ().enabled = false;
			} else {
				//PauseText.text = "Paused";
				//PauseText2.text = "(Press Back to continue)";
				//PausedCover.GetComponent<MeshRenderer> ().enabled = true;
				pause = true;
			}
		}
	}

	void LateUpdate()
	{
		if (pause) 
			Time.timeScale = 0;
		else
			Time.timeScale = 1;
	}
}
