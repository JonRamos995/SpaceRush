using UnityEngine;
using System.Collections;

public class DestroyOnExitScript : MonoBehaviour {

	void OnTriggerExit(Collider other)
	{
		Destroy (other.gameObject, 0.5f);
		if (other.tag != "PowerUp" ||  other.CompareTag("SideTriggers"))
			other.GetComponent<CrashingBehaviorScript> ().Destroyed ();
	}
}
