using UnityEngine;
using System.Collections;

public class PowerUpController : MonoBehaviour {


	[SerializeField] private GUIText ScoreText;
	[SerializeField] private GUIText doublePointsText;

	private int powerUpShield;
	private int powerUpBolt;
	private int powerUpDoublePoints;
	private float powerUpPointsMultiplier;
	private float timeDoublePoints;

	[SerializeField]private float timeLimitDP;

	void Start () {
		powerUpBolt = 0;
		powerUpShield = 0;
		powerUpDoublePoints = 1;
		powerUpPointsMultiplier = 1f;
		timeDoublePoints = 0;
		timeLimitDP = 15f;
	}

	void Update()
	{
		if (powerUpDoublePoints > 1) {
			timeDoublePoints += Time.deltaTime;
			ScoreText.color = Color.green;
			doublePointsText.text = "Double Points x" + powerUpDoublePoints;
			if (timeDoublePoints >= timeLimitDP) {
				ResetDoublePoints ();
				ScoreText.color = Color.white;
				doublePointsText.text = "";
				timeDoublePoints = 0;
			}
		}
	}

	public void AddPowerUpShield(int amount)
	{
		powerUpShield = amount;
	}

	public void AddPowerUpBolt(int amount)
	{
		powerUpBolt = amount;
	}

	public void AddDoublePoints()
	{
		if (powerUpDoublePoints == 1) {
			++powerUpDoublePoints;
			return;
		}
		powerUpDoublePoints *= 2;
	}

	public bool IsShieldActive()
	{
		if (powerUpShield > 0) 
			return true;
		else
			return false;
	}

	public bool IsBoltActive()
	{
		if (powerUpBolt > 0)
			return true;
		else
			return false;
	}

	public void DecreaseShield()
	{
		if (powerUpShield > 0)
			--powerUpShield;
	}

	public void	DecreaseBolts()
	{
		if (powerUpBolt > 0)
			--powerUpBolt;
	}

	public int GetDoublePoints()
	{
		return powerUpDoublePoints;
	}

	public void ResetDoublePoints()
	{
		powerUpDoublePoints = 1;
	}

	public void UsePointsMultiplier()
	{
		powerUpPointsMultiplier = 1.25f;
	}

	public void ResetPointsMultiplier()
	{
		powerUpPointsMultiplier = 1f;
	}

	public float GetPointsMultiplier()
	{
		return powerUpPointsMultiplier;
	}

}
