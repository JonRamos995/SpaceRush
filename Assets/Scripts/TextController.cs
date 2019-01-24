using UnityEngine;
using System.Collections;

public class TextController : MonoBehaviour {

	public GUIText scoreText;
	public GUIText levelText;
	public GUIText gameOverText;
	public GUIText doublePointsText;
	public GUIText PausedText;
	public GUIText PausedText2;


	public int ratio = 10;

	void Start()
	{
		float finalSize = (float)Screen.width/ratio;

		scoreText.fontSize = (int)finalSize;
		levelText.fontSize = (int)finalSize;
		gameOverText.fontSize = (int)finalSize;
		doublePointsText.fontSize = (int)finalSize - 4;
		PausedText.fontSize = (int)finalSize;
		PausedText2.fontSize = (int)finalSize - 6;
	}

	public void UpdateLevel(int level)
	{
		levelText.text = "Level 0" + level;
	}

	public void UpdateScore(int score)
	{
		scoreText.text = score.ToString();
	}

	public void GameOver()
	{
		gameOverText.text = "Game Over!";
	}


}
