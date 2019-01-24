using UnityEngine;
using UnityEngine.UI;
public class EnemyManager : MonoBehaviour
{
	//public Text LevelText;
	//public GameObject[] enemy;
	//public float spawnTime = 3f;
	//public float spawnDelay = 1f;
	//public Transform[] spawnPoints;
	//
	//private int LastSpawn;
	//private float TimePassed;
	//private float maxTime = 10f;
	//private int level;

	void Start ()
	{
		//InvokeRepeating ("Spawn", spawnTime, spawnDelay);
		//spawnTime = 0.5f;
		//level = 1;
		//LevelText.text = "Level 0" + level;
		//LevelText.resizeTextForBestFit = true;
		//LevelText.resizeTextMaxSize = 1;
	}

	void Update()
	{
		//TimePassed += Time.deltaTime;
		//
		//if (TimePassed >= maxTime && spawnDelay > 0.35f) {
		//	LevelText.text = "Level 0" + ++level;
		//	TimePassed = 0f;
		//	maxTime += 2f;
		//	spawnDelay -= 0.05f;
		//	CancelInvoke ("Spawn");
		//	InvokeRepeating ("Spawn", spawnTime, spawnDelay);
		//}
	}

	void Spawn ()
	{
		//int spawnPointIndex = Random.Range (0, spawnPoints.Length);
		//
		//while (LastSpawn == spawnPointIndex) {
		//	spawnPointIndex = Random.Range (0, spawnPoints.Length);
		//}
		//LastSpawn = spawnPointIndex;
		//int enemyIndex = Random.Range (0, enemy.Length);
		//Instantiate (enemy[enemyIndex], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
	}
}
