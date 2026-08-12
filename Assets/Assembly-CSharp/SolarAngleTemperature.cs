using UnityEngine;

public class SolarAngleTemperature : MonoBehaviour, TemperatureReadout
{
	[SerializeField]
	private AnimationCurve _temperatureCurve;

	[SerializeField]
	private float _maxTempK = 720f;

	[SerializeField]
	private float _minTempK = 100f;

	private OWRigidbody _parentBody;

	private float _currentTemp;

	private void Start()
	{
	}

	private void Awake()
	{
		_parentBody = this.GetAttachedOWRigidbody();
	}

	private void Update()
	{
		UpdateAngleToSun();
	}

	public float GetTemperature()
	{
		return _currentTemp;
	}

	private void UpdateAngleToSun()
	{
		Vector3 position = _parentBody.GetPosition();
		Vector3 from = Locator.GetSunTransform().position - position;
		Vector3 to = base.transform.position - position;
		float time = Vector3.Angle(from, to) / 180f;
		_currentTemp = _temperatureCurve.Evaluate(time) * (_maxTempK - _minTempK) + _minTempK;
	}
}
