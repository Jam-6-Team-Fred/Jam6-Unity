using UnityEngine;

public class AirlockInterface : AbstractGhostAirlockInterface
{
	[SerializeField]
	private SingleLightSensor[] _lightSensors;

	[SerializeField]
	private Transform[] _rotatingElements;

	[Header("Front Gear")]
	[SerializeField]
	private InteractReceiver _frontInterface;

	[SerializeField]
	private GearInterfaceEffects _frontGearEffects;

	[Header("Back Gear")]
	[SerializeField]
	private InteractReceiver _backInterface;

	[SerializeField]
	private GearInterfaceEffects _backGearEffects;

	[Space]
	[SerializeField]
	private float _maxSpeed = 54f;

	[SerializeField]
	private float _acceleration = 120f;

	[SerializeField]
	private float _deceleration = 216f;

	[SerializeField]
	private float _angleAccuracy = 10f;

	[Space]
	private float _currentRotation;

	private float _rotatingSpeed;

	private int _rotatingDirection;

	private bool _calledToOpenFromOutside;

	private int _litSensors;

	private bool _frozenFromFlood;

	public OWEvent OnFirstSensorLight = new OWEvent(1);

	public OWEvent OnAllSensorsDark = new OWEvent(1);

	private void Awake()
	{
		for (int i = 0; i < _lightSensors.Length; i++)
		{
			_lightSensors[i].OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
			_lightSensors[i].OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		}
		_rotatingDirection = 0;
		_litSensors = 0;
		_calledToOpenFromOutside = false;
	}

	private void Start()
	{
		if (_frontInterface != null)
		{
			_frontInterface.SetPromptText(UITextType.RotateGearPrompt);
			_frontInterface.OnPressInteract += OnCallToOpenFront;
		}
		if (_backInterface != null)
		{
			_backInterface.SetPromptText(UITextType.RotateGearPrompt);
			_backInterface.OnPressInteract += OnCallToOpenBack;
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _lightSensors.Length; i++)
		{
			_lightSensors[i].OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
			_lightSensors[i].OnDetectLight -= new OWEvent.OWCallback(OnDetectDarkness);
		}
		if (_frontInterface != null)
		{
			_frontInterface.OnPressInteract -= OnCallToOpenFront;
		}
		if (_backInterface != null)
		{
			_backInterface.OnPressInteract -= OnCallToOpenBack;
		}
	}

	public bool AreAnySensorsLit()
	{
		return _litSensors > 0;
	}

	public float GetSpeedFraction()
	{
		if (!(Mathf.Abs(_rotatingSpeed) > 0f))
		{
			return 0f;
		}
		return Mathf.Abs(_rotatingSpeed) / _maxSpeed;
	}

	public override void SetStartingPosition(bool IsOpen)
	{
		InstantSetRotation(IsOpen);
	}

	private void InstantSetRotation(bool IsOpen)
	{
		_currentRotation = (IsOpen ? 180f : 0f);
		for (int i = 0; i < _rotatingElements.Length; i++)
		{
			if (i == 2)
			{
				_rotatingElements[i].Rotate(new Vector3(0f, _currentRotation * 0.5f - _rotatingElements[i].localRotation.eulerAngles.y, 0f));
			}
			else
			{
				_rotatingElements[i].Rotate(new Vector3(0f, _currentRotation * (float)(i + 1) - _rotatingElements[i].localRotation.eulerAngles.y, 0f));
			}
		}
	}

	private void FixedUpdate()
	{
		if ((float)_rotatingDirection == 0f)
		{
			if (Mathf.Abs(_rotatingSpeed) < 0.1f)
			{
				base.enabled = false;
				_rotatingSpeed = 0f;
				return;
			}
			if (_currentRotation % 360f <= _angleAccuracy || _currentRotation % 360f >= 360f - _angleAccuracy)
			{
				_rotatingDirection += ((_currentRotation % 360f <= _angleAccuracy) ? 1 : (-1));
			}
			else if (Mathf.Abs(180f - _currentRotation % 360f) <= _angleAccuracy)
			{
				_rotatingDirection += ((180f - Mathf.Abs(_currentRotation % 360f) <= 0f) ? 1 : (-1));
			}
			else
			{
				_rotatingSpeed = Mathf.MoveTowards(_rotatingSpeed, 0f, _deceleration * Time.deltaTime);
			}
		}
		else
		{
			_rotatingSpeed += (float)_rotatingDirection % 1.1f * _acceleration * Time.deltaTime;
			if (Mathf.Abs(_rotatingSpeed) > _maxSpeed)
			{
				_rotatingSpeed = ((_rotatingSpeed > 0f) ? _maxSpeed : (0f - _maxSpeed));
			}
			float num = _maxSpeed * Time.deltaTime;
			if (_calledToOpenFromOutside && _rotatingDirection > 0 && Mathf.Abs(180f - _currentRotation % 360f) <= num)
			{
				SetOpen(openFront: true);
				_rotatingDirection--;
				_calledToOpenFromOutside = false;
				return;
			}
			if (_calledToOpenFromOutside && _rotatingDirection < 0 && _currentRotation % 360f <= num)
			{
				SetOpen(openFront: false);
				_rotatingDirection++;
				_calledToOpenFromOutside = false;
				return;
			}
			if (!_calledToOpenFromOutside && _litSensors == 0)
			{
				if (_currentRotation % 360f <= num || _currentRotation % 360f >= 360f - num)
				{
					SetOpen(openFront: false);
					_rotatingDirection = 0;
					return;
				}
				if (Mathf.Abs(180f - _currentRotation % 360f) <= num)
				{
					SetOpen(openFront: true);
					_rotatingDirection = 0;
					return;
				}
			}
		}
		float num2 = _rotatingSpeed * Time.deltaTime;
		_currentRotation -= num2;
		while (_currentRotation < 0f)
		{
			_currentRotation += 360f;
		}
		for (int i = 0; i < _rotatingElements.Length; i++)
		{
			if (i == 2)
			{
				_rotatingElements[i].Rotate(new Vector3(0f, (0f - num2) * 0.5f, 0f));
			}
			else
			{
				_rotatingElements[i].Rotate(new Vector3(0f, (0f - num2) * (float)(i + 1), 0f));
			}
		}
	}

	private void SetOpen(bool openFront)
	{
		if (openFront)
		{
			CallOpenEvent();
			InstantSetRotation(IsOpen: true);
		}
		else
		{
			CallCloseEvent();
			InstantSetRotation(IsOpen: false);
		}
		_rotatingSpeed = 0f;
		base.enabled = false;
	}

	private void OnDetectLight()
	{
		if (!_frozenFromFlood)
		{
			_rotatingDirection = -1;
			_litSensors++;
			base.enabled = true;
			CallOnRotateEvent();
			if (_litSensors == 1)
			{
				OnFirstSensorLight.Invoke();
			}
		}
	}

	private void OnDetectDarkness()
	{
		if (!_frozenFromFlood)
		{
			_litSensors--;
			_rotatingDirection = ((_litSensors != 0 || _calledToOpenFromOutside) ? _rotatingDirection : 0);
			base.enabled = true;
			if (_litSensors == 0)
			{
				OnAllSensorsDark.Invoke();
			}
		}
	}

	private void OnCallToOpenFront()
	{
		if (_frozenFromFlood)
		{
			_backGearEffects.PlayFailure();
			return;
		}
		if (_calledToOpenFromOutside)
		{
			_frontGearEffects.AddRotation(90f);
			return;
		}
		_frontGearEffects.AddRotation(90f);
		_rotatingDirection--;
		base.enabled = true;
		_calledToOpenFromOutside = true;
		CallOnRotateEvent();
	}

	private void OnCallToOpenBack()
	{
		if (_frozenFromFlood)
		{
			_backGearEffects.PlayFailure();
			return;
		}
		if (_calledToOpenFromOutside)
		{
			_backGearEffects.AddRotation(90f);
			return;
		}
		_backGearEffects.AddRotation(90f);
		_rotatingDirection++;
		base.enabled = true;
		_calledToOpenFromOutside = true;
		CallOnRotateEvent();
	}

	public void OnFloodDeactivate()
	{
		_rotatingDirection = 0;
		_litSensors = 0;
		OnCallToOpenFront();
		_frozenFromFlood = true;
		OnAllSensorsDark.Invoke();
	}
}
