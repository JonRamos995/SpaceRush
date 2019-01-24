using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Collections;

public class GameController : MonoBehaviour {

	public GameObject[] hazards;
	public GameObject[] powerUps;
	public Transform[] spawnPoints;

	public float startWait;
	public float spawnWait;
	public float waveWait;

	public int numHazards;
	public int hazardsCount;

	private bool gameOver;
	private bool restart;

	private float score;
	private int level;
	private int LastSpawn;
	private float timerRestart;

	private TextController textController;
	private PowerUpController powerUpController;

	void Start () {
		ApplyScript (); 		//Applies the TextController script to textController;
		gameOver = false;
		restart  = false;
		score = 0;
		level = 0;
		timerRestart = 0f;
		UpdateLevel (level);
		UpdateScore ();
		StartCoroutine (Spawn ());
	}

	void Update () {

		if (powerUpController.GetPointsMultiplier() > 1f) {
			score *= powerUpController.GetPointsMultiplier();
			powerUpController.ResetPointsMultiplier ();
		}

		UpdateScore ();
					
		if (restart == true) {
			timerRestart += Time.deltaTime;
			if (timerRestart > 3f)
				UnityEngine.SceneManagement.SceneManager.LoadScene ("Menu");
		}
	}


	IEnumerator Spawn()
	{
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			while(hazardsCount < numHazards)
			{
				++hazardsCount;
				int spawnPointIndex = Random.Range (0, spawnPoints.Length);

				while (LastSpawn == spawnPointIndex) {
					// Loops as long as new spawn position equals saved spawn position, for a random spawn every time without repeats
					spawnPointIndex = Random.Range (0, spawnPoints.Length);  
				}

				// Saves last spawn position
				LastSpawn = spawnPointIndex; 

				int enemyIndex = Random.Range (0, hazards.Length);
				int powerUpIndex = Random.Range (0, powerUps.Length);

				float Chance = 0;
				for (short i = 0; i < 10; i++) 
					Chance += Random.Range (0, 99);
				
				Chance /= 10;

				if (Chance < 2.5)
					Instantiate (powerUps [powerUpIndex], spawnPoints [spawnPointIndex].position, spawnPoints [spawnPointIndex].rotation);
				else
					Instantiate (hazards[enemyIndex], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

				if (gameOver) 
					break;
				
				yield return new WaitForSeconds (spawnWait);
			}
			hazardsCount = 0;
			yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				restart = true;
				break;
			}

			UpdateLevel (++level);
			LevelUp ();
		}
	}

	public void AddScore(int points)
	{
		if (!gameOver) 
			score += points * powerUpController.GetDoublePoints();
	}

	void UpdateScore()
	{
		textController.UpdateScore ((int)score);
	}

	void UpdateLevel(int _level)
	{
		textController.UpdateLevel (_level);
	}

	public void GameOver()
	{
		textController.GameOver ();
		gameOver = true;
		Analytics.CustomEvent ("GameOver", new System.Collections.Generic.Dictionary<string, object> {
			{ "Level", level },
			{ "Score", score },
			{ "Spawn Speed", spawnWait }
		});
	}

	void LevelUp()
	{
		numHazards += 4;
		if (spawnWait > .30f) 
			spawnWait -= .025f;
		else if (spawnWait <= .30f && spawnWait >.25f) 
			spawnWait -= .01f;
	}

	void ApplyScript()
	{
		textController = GameObject.FindGameObjectWithTag ("TextController").GetComponent<TextController> ();
		powerUpController = GameObject.FindGameObjectWithTag ("PowerUpController").GetComponent<PowerUpController> ();

		if (textController == null)
			Debug.Log ("Cannot find 'TextController' script");

		if (powerUpController == null)
			Debug.Log ("Cannot find 'PowerUpController' script");
	}

	public bool IsGameOver()
	{
		return gameOver;
	}
}
