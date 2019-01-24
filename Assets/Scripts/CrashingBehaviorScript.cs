using UnityEngine;
using System.Collections;

public class CrashingBehaviorScript : MonoBehaviour {

	public GameObject explosion;
	public GameObject playerExplosion;
	public int scoreValue;

	private GameController gameController;
	private PowerUpController powerUpController;

	void Start ()
	{
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
		{
			gameController = gameControllerObject.GetComponent <GameController>();
		}
		if (gameController == null)
		{
			Debug.Log ("Cannot find 'GameController' script");
		}

		GameObject powerUpControllerObject = GameObject.FindGameObjectWithTag ("PowerUpController");
		if (powerUpControllerObject != null)
		{
		 	powerUpController = powerUpControllerObject.GetComponent <PowerUpController>();
		}
		if (powerUpController == null)
		{
			Debug.Log ("Cannot find 'powerUpController' script");
		}
	}
		
	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Boundary" || other.tag == "Enemy" || other.tag == "SideTriggers")
		{
			return;
		}

		if (explosion != null)
		{
			Instantiate(explosion, transform.position, transform.rotation);
		}

		if (other.tag == "Player")
		{
			if (!powerUpController.IsShieldActive ()) {
				Instantiate (playerExplosion, other.transform.position, other.transform.rotation);
				gameController.GameOver ();
				Destroy (other.gameObject);
			} else
				powerUpController.DecreaseShield ();
		}

		Destroy (gameObject);
	}

	public void Destroyed()
	{
		gameController.AddScore (scoreValue);
	}
}
