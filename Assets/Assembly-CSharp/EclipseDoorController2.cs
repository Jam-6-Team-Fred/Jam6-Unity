using UnityEngine;

public class EclipseDoorController2 : MonoBehaviour
{
	[SerializeField]
	private SingleLightSensor _frontLightSensor;

	[SerializeField]
	private SingleLightSensor _backLightSensor;

	[SerializeField]
	private Transform[] _rotatingElements;

	[SerializeField]
	private AbstractDoor[] _backDoors;

	[SerializeField]
	private AbstractDoor _frontDoor;

	[Space]
	[SerializeField]
	private float _rotationSpeed = 180f;

	[SerializeField]
	private float _startingRotation = 270f;

	[SerializeField]
	private float _anglePrecision = 10f;

	private bool _frontSensorActive;

	private bool _backSensorActive;

	private void Awake()
	{
		if (_rotatingElements.Length < 1)
		{
			Debug.LogError("No rotating dial.");
		}
		for (int i = 0; i < _rotatingElements.Length; i++)
		{
			float y = _rotatingElements[i].localRotation.eulerAngles.y;
			_rotatingElements[i].Rotate(new Vector3(0f, _startingRotation - y, 0f));
		}
		_frontSensorActive = false;
		_frontLightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_frontLightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		_backSensorActive = false;
		_backLightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_backLightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_frontLightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_frontLightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		_backLightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_backLightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < _rotatingElements.Length; i++)
		{
			_rotatingElements[i].Rotate(new Vector3(0f, _rotationSpeed * Time.deltaTime, 0f));
		}
	}

	private void OnDetectLight()
	{
		if (_frontDoor.IsOpen())
		{
			_frontDoor.Close();
		}
		for (int i = 0; i < _backDoors.Length; i++)
		{
			if (_backDoors[i].IsOpen())
			{
				_backDoors[i].Close();
			}
		}
		if (_frontLightSensor.IsIlluminated())
		{
			_frontSensorActive = true;
		}
		if (_backLightSensor.IsIlluminated())
		{
			_backSensorActive = true;
		}
		base.enabled = true;
	}

	private void OnDetectDarkness()
	{
		float num = _rotatingElements[0].localRotation.eulerAngles.y % 360f;
		if (_frontSensorActive && (num < _anglePrecision || 360f - num < _anglePrecision))
		{
			_frontDoor.Open();
		}
		if (_backSensorActive && Mathf.Abs(num - 180f) < _anglePrecision)
		{
			_frontDoor.Open();
		}
		if (!_frontLightSensor.IsIlluminated())
		{
			_frontSensorActive = false;
		}
		if (!_backLightSensor.IsIlluminated())
		{
			_backSensorActive = false;
		}
		base.enabled = false;
	}
}
