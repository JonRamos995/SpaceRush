using UnityEngine;
using System.Collections;

public class PowerUpShield : MonoBehaviour {

	private PowerUpController powerUpController;

	void Start () {
		GameObject powerControllerObject = GameObject.FindGameObjectWithTag ("PowerUpController");
		if (powerControllerObject != null)
		{
			powerUpController = powerControllerObject.GetComponent <PowerUpController>();
		}
		if (powerUpController == null)
		{
			Debug.Log ("Cannot find 'PowerUpController' script");
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag != "Player")
			return;
	
		if (other.CompareTag ("Player") && !powerUpController.IsShieldActive ()) {
			powerUpController.AddPowerUpShield (1);
		}
		Destroy (gameObject);
	}
}
