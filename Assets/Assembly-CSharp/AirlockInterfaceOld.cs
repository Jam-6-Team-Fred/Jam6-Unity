using UnityEngine;

public class AirlockInterfaceOld : AbstractGhostAirlockInterface
{
	[SerializeField]
	private SingleLightSensor[] _lightSensorClockwise;

	[SerializeField]
	private SingleLightSensor[] _lightSensorCounterClock;

	[SerializeField]
	private Transform[] _rotatingElements;

	[SerializeField]
	private AbstractGhostDoorInterface _frontInterface;

	[SerializeField]
	private AbstractGhostDoorInterface _backInterface;

	[Space]
	[SerializeField]
	private float _maxSpeed;

	[SerializeField]
	private float _acceleration;

	[SerializeField]
	private float _maxRotation;

	[SerializeField]
	private float _slowDownSpeed;

	[SerializeField]
	private float _litUpSlowDownSpeed;

	private float _currentRotation;

	private float _rotatingSpeed;

	private int _rotatingDirection;

	private bool _calledToOpenFromOutside;

	private int _litSensors;

	private void Awake()
	{
		for (int i = 0; i < _lightSensorClockwise.Length; i++)
		{
			_lightSensorClockwise[i].OnDetectLight += new OWEvent.OWCallback(OnDetectLightClock);
			_lightSensorClockwise[i].OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarknessClock);
		}
		for (int j = 0; j < _lightSensorCounterClock.Length; j++)
		{
			_lightSensorCounterClock[j].OnDetectLight += new OWEvent.OWCallback(OnDetectLightCounter);
			_lightSensorCounterClock[j].OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarknessCounter);
		}
		if (_frontInterface != null)
		{
			_frontInterface.OnOpen += OnCallToOpenFront;
		}
		if (_backInterface != null)
		{
			_backInterface.OnOpen += OnCallToOpenBack;
		}
		_rotatingDirection = 0;
		_litSensors = 0;
		base.enabled = false;
		_calledToOpenFromOutside = false;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _lightSensorClockwise.Length; i++)
		{
			_lightSensorClockwise[i].OnDetectLight -= new OWEvent.OWCallback(OnDetectLightClock);
			_lightSensorClockwise[i].OnDetectLight -= new OWEvent.OWCallback(OnDetectDarknessClock);
		}
		for (int j = 0; j < _lightSensorCounterClock.Length; j++)
		{
			_lightSensorCounterClock[j].OnDetectLight -= new OWEvent.OWCallback(OnDetectLightCounter);
			_lightSensorCounterClock[j].OnDetectLight -= new OWEvent.OWCallback(OnDetectDarknessCounter);
		}
		if (_frontInterface != null)
		{
			_frontInterface.OnOpen -= OnCallToOpenFront;
		}
		if (_backInterface != null)
		{
			_backInterface.OnOpen -= OnCallToOpenBack;
		}
	}

	public override void SetStartingPosition(bool IsOpen)
	{
		InstantSetRotation(IsOpen);
	}

	private void InstantSetRotation(bool IsOpen)
	{
		_currentRotation = (IsOpen ? _maxRotation : 0f);
		for (int i = 0; i < _rotatingElements.Length; i++)
		{
			_rotatingElements[i].Rotate(new Vector3(_currentRotation / Mathf.Pow(2f, i) - _rotatingElements[i].localRotation.eulerAngles.y, 0f, 0f));
		}
	}

	private void FixedUpdate()
	{
		if ((float)_rotatingDirection == 0f && _litSensors == 0)
		{
			if (Mathf.Abs(_rotatingSpeed) < _slowDownSpeed)
			{
				_rotatingSpeed = 0f;
				base.enabled = false;
			}
			else
			{
				_rotatingSpeed -= ((_rotatingSpeed > 0f) ? _slowDownSpeed : (0f - _slowDownSpeed));
			}
		}
		else if ((float)_rotatingDirection == 0f)
		{
			if (Mathf.Abs(_rotatingSpeed) < _litUpSlowDownSpeed)
			{
				_rotatingSpeed = 0f;
				base.enabled = false;
			}
			else
			{
				_rotatingSpeed -= ((_rotatingSpeed > 0f) ? _litUpSlowDownSpeed : (0f - _litUpSlowDownSpeed));
			}
		}
		else
		{
			_rotatingSpeed += (float)_rotatingDirection * _acceleration * Time.deltaTime;
			if (_rotatingSpeed > _maxSpeed)
			{
				_rotatingSpeed = _maxSpeed;
			}
			if (_rotatingSpeed < 0f - _maxSpeed)
			{
				_rotatingSpeed = 0f - _maxSpeed;
			}
		}
		if (_currentRotation + _rotatingSpeed >= _maxRotation)
		{
			CallOpenEvent();
			InstantSetRotation(IsOpen: true);
			_rotatingSpeed = 0f;
			base.enabled = false;
			if (_calledToOpenFromOutside)
			{
				_rotatingDirection--;
				_calledToOpenFromOutside = false;
			}
		}
		else if (_currentRotation + _rotatingSpeed <= 0f)
		{
			CallCloseEvent();
			InstantSetRotation(IsOpen: false);
			_rotatingSpeed = 0f;
			base.enabled = false;
			if (_calledToOpenFromOutside)
			{
				_rotatingDirection++;
				_calledToOpenFromOutside = false;
			}
		}
		else
		{
			CallOnRotateEvent();
			_currentRotation += _rotatingSpeed;
			for (int i = 0; i < _rotatingElements.Length; i++)
			{
				_rotatingElements[i].Rotate(new Vector3(_rotatingSpeed / Mathf.Pow(2f, i), 0f, 0f));
			}
		}
	}

	private void OnDetectLightClock()
	{
		_rotatingDirection++;
		_litSensors++;
		base.enabled = true;
	}

	private void OnDetectLightCounter()
	{
		_rotatingDirection--;
		_litSensors++;
		base.enabled = true;
	}

	private void OnDetectDarknessClock()
	{
		_rotatingDirection--;
		_litSensors--;
		base.enabled = true;
	}

	private void OnDetectDarknessCounter()
	{
		_rotatingDirection++;
		_litSensors--;
		base.enabled = true;
	}

	private void OnCallToOpenFront()
	{
		if (!_calledToOpenFromOutside)
		{
			_rotatingDirection--;
			base.enabled = true;
			_calledToOpenFromOutside = true;
		}
	}

	private void OnCallToOpenBack()
	{
		if (!_calledToOpenFromOutside)
		{
			_rotatingDirection++;
			base.enabled = true;
			_calledToOpenFromOutside = true;
		}
	}
}
