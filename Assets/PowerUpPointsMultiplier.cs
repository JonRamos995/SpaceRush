using UnityEngine;
using System.Collections;

public class PowerUpPointsMultiplier : MonoBehaviour {


	private PowerUpController powerUpController;

	void Start () {
		powerUpController = GameObject.FindGameObjectWithTag ("PowerUpController").GetComponent<PowerUpController> ();

		if (powerUpController == null)
			Debug.Log ("Cannot find 'PowerUpController' script");
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag ("Player"))
			return;

		if (other.CompareTag ("Player"))
			powerUpController.UsePointsMultiplier();

		Destroy (gameObject);
	}
}
