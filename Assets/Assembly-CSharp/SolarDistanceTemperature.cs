using UnityEngine;

public class SolarDistanceTemperature : MonoBehaviour, TemperatureReadout
{
	[SerializeField]
	private AnimationCurve _distanceCurve;

	[SerializeField]
	private float _maxTempK = 1000f;

	[SerializeField]
	private float _minTempK = 80f;

	[SerializeField]
	private float _maxDistance = 25000f;

	[SerializeField]
	private float _minDistance;

	private float _currentTemp;

	private void Start()
	{
	}

	private void Awake()
	{
	}

	private void Update()
	{
		UpdateDistanceToSun();
	}

	public float GetTemperature()
	{
		return _currentTemp;
	}

	private void UpdateDistanceToSun()
	{
		Vector3 position = base.transform.position;
		float time = ((Locator.GetSunTransform().position - position).magnitude - _minDistance) / (_maxDistance - _minDistance);
		_currentTemp = _distanceCurve.Evaluate(time) * (_maxTempK - _minTempK) + _minTempK;
	}
}
