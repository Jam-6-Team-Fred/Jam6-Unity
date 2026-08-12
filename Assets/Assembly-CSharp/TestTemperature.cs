using UnityEngine;

public class TestTemperature : MonoBehaviour, TemperatureReadout
{
	[SerializeField]
	private float temperature;

	public float GetTemperature()
	{
		return temperature;
	}
}
