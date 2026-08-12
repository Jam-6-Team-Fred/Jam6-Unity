using UnityEngine;

public class AutoRotator : MonoBehaviour
{
	private enum RotationAxis
	{
		X = 0,
		Y = 1,
		Z = 2
	}

	[SerializeField]
	private RotationAxis _visualRotationAxis;

	[SerializeField]
	private RotationAxis _actualRotationAxis;

	[SerializeField]
	private float _totalRotationTime = -1f;

	[SerializeField]
	private float _maxRotationSpeed;

	[SerializeField]
	private DampedSpringRadial _rotationSpringRampUp = new DampedSpringRadial();

	[SerializeField]
	private DampedSpringRadial _rotationSpringRampDown = new DampedSpringRadial();

	[SerializeField]
	private bool _startOnLoad;

	[SerializeField]
	private bool _rampUpRotation = true;

	[SerializeField]
	private bool _rampDownRotation = true;

	private bool _started;

	private bool _stopping;

	private float _rotationTimeRemaining;

	private float _currentRotationSpeed;

	private Vector3 _currentRotationUnbounded;

	private bool _rampingUp;

	private bool _rampingDown;

	private float _homeRotation;

	private float _rotationDirection = 1f;

	private void Awake()
	{
		switch (_actualRotationAxis)
		{
		case RotationAxis.X:
			Debug.LogError("Rotation about the X-Axis does not work. Set rotation so that the y or z axis lines up with the current x axis, then set Visual Rotation to X, and Actual Rotation to Y or Z.");
			base.enabled = false;
			return;
		case RotationAxis.Y:
			_homeRotation = base.transform.localRotation.eulerAngles.y;
			break;
		case RotationAxis.Z:
			_homeRotation = base.transform.localRotation.eulerAngles.z;
			break;
		}
		_currentRotationSpeed = 0f;
		if (_startOnLoad)
		{
			StartRotation();
		}
	}

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		if (_started || _stopping)
		{
			Rotate(Time.fixedDeltaTime);
		}
	}

	public void StartRotation(bool xAxis = true, bool yAxis = true, bool zAxis = true, bool setInfiniteRotationTime = false)
	{
		bool flag = false;
		switch (_visualRotationAxis)
		{
		case RotationAxis.X:
			flag = xAxis;
			break;
		case RotationAxis.Y:
			flag = yAxis;
			break;
		case RotationAxis.Z:
			flag = zAxis;
			break;
		}
		if (flag)
		{
			if (setInfiniteRotationTime)
			{
				_rotationTimeRemaining = -1f;
			}
			else
			{
				_rotationTimeRemaining = _totalRotationTime;
			}
			_rampingDown = false;
			if (_rampUpRotation)
			{
				_rampingUp = true;
			}
			else
			{
				_currentRotationSpeed = _maxRotationSpeed;
			}
			_rotationDirection = Mathf.Sign(_maxRotationSpeed);
			_started = true;
			_stopping = false;
		}
	}

	public void StopRotation(bool xAxis = true, bool yAxis = true, bool zAxis = true)
	{
		switch (_visualRotationAxis)
		{
		case RotationAxis.X:
			if (xAxis)
			{
				_rotationTimeRemaining = 0f;
			}
			break;
		case RotationAxis.Y:
			if (yAxis)
			{
				_rotationTimeRemaining = 0f;
			}
			break;
		case RotationAxis.Z:
			if (zAxis)
			{
				_rotationTimeRemaining = 0f;
			}
			break;
		}
		_started = false;
		_stopping = true;
	}

	private void Rotate(float dt)
	{
		Vector3 zero = Vector3.zero;
		if (_rotationTimeRemaining >= 0f && !_rampingUp)
		{
			_rotationTimeRemaining = Mathf.Clamp(_rotationTimeRemaining - dt, 0f, _totalRotationTime);
		}
		if (_rampUpRotation)
		{
			RampUpSpeeds(dt);
		}
		if (_rampDownRotation)
		{
			RampDownSpeeds(dt);
		}
		switch (_visualRotationAxis)
		{
		case RotationAxis.X:
			switch (_actualRotationAxis)
			{
			case RotationAxis.Y:
				zero += new Vector3(0f, _currentRotationSpeed * dt, 0f);
				break;
			case RotationAxis.Z:
				zero += new Vector3(0f, 0f, _currentRotationSpeed * dt);
				break;
			}
			break;
		case RotationAxis.Y:
			zero += new Vector3(0f, _currentRotationSpeed * dt, 0f);
			break;
		case RotationAxis.Z:
			zero += new Vector3(0f, 0f, _currentRotationSpeed * dt);
			break;
		}
		_currentRotationUnbounded += zero;
		base.transform.Rotate(zero);
	}

	private void RampUpSpeeds(float dt)
	{
		if (_rampingUp)
		{
			float currentValue;
			float num;
			switch (_actualRotationAxis)
			{
			case RotationAxis.Y:
				currentValue = _currentRotationUnbounded.y;
				num = base.transform.eulerAngles.y;
				break;
			case RotationAxis.Z:
				currentValue = _currentRotationUnbounded.z;
				num = base.transform.eulerAngles.z;
				break;
			default:
				num = 0f;
				currentValue = 0f;
				Debug.LogError("Invalid Rotation Axis specified");
				break;
			}
			num += 180f;
			_rotationSpringRampUp.Update(currentValue, num, dt);
			_currentRotationSpeed = _rotationSpringRampUp.velocity;
			if (Mathf.Abs(_currentRotationSpeed) >= Mathf.Abs(_maxRotationSpeed))
			{
				_currentRotationSpeed = _maxRotationSpeed;
				_rampingUp = false;
			}
		}
	}

	private void RampDownSpeeds(float dt)
	{
		Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
		float num;
		switch (_actualRotationAxis)
		{
		case RotationAxis.Y:
			num = eulerAngles.y;
			break;
		case RotationAxis.Z:
			num = eulerAngles.z;
			break;
		default:
			num = 0f;
			Debug.LogError("Invalid Rotation Axis specified");
			break;
		}
		if (_rotationTimeRemaining == 0f && !_rampingDown)
		{
			float num2;
			for (num2 = _homeRotation + 180f; num2 > 360f; num2 -= 360f)
			{
			}
			if (_rotationDirection > 0f)
			{
				if (num >= num2)
				{
					_rampingDown = true;
				}
			}
			else if (num <= num2)
			{
				_rampingDown = true;
			}
			if (_rampingDown)
			{
				switch (_actualRotationAxis)
				{
				case RotationAxis.Y:
					_currentRotationUnbounded.y = eulerAngles.y;
					break;
				case RotationAxis.Z:
					_currentRotationUnbounded.z = eulerAngles.z;
					break;
				}
				_rotationSpringRampDown.velocity = _currentRotationSpeed;
			}
		}
		if (_rampingDown)
		{
			switch (_actualRotationAxis)
			{
			case RotationAxis.Y:
				_rotationSpringRampDown.Update(_currentRotationUnbounded.y, _homeRotation, dt);
				break;
			case RotationAxis.Z:
				_rotationSpringRampDown.Update(_currentRotationUnbounded.z, _homeRotation, dt);
				break;
			}
			_currentRotationSpeed = _rotationSpringRampDown.velocity;
			if (Mathf.Abs(_currentRotationSpeed) > Mathf.Abs(_maxRotationSpeed))
			{
				_currentRotationSpeed = _maxRotationSpeed;
			}
		}
	}
}
