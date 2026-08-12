using UnityEngine;

public abstract class RotateToPoint : MonoBehaviour
{
	[SerializeField]
	private float _timeToMaxSpeed = 1f;

	[SerializeField]
	private float _targetLockAngle;

	[SerializeField]
	protected bool _quaternionTargetMode;

	[SerializeField]
	protected bool _setLocalRotationInQuatTargetMode;

	[SerializeField]
	private AnimationCurve _rampUpCurve;

	protected float _currentRotationSpeed;

	protected float _fractionalTimeSinceReset;

	protected bool _rampingUp;

	protected bool _hasTargetLock;

	protected Vector3 _target;

	protected Quaternion _targetRotation;

	protected Vector3 _direction;

	public bool TargetLocked => _hasTargetLock;

	public bool QuaternionTargetMode
	{
		get
		{
			return _quaternionTargetMode;
		}
		set
		{
			_quaternionTargetMode = value;
		}
	}

	public bool QuatTModeTargetLocalRotation
	{
		get
		{
			return _setLocalRotationInQuatTargetMode;
		}
		set
		{
			_setLocalRotationInQuatTargetMode = value;
		}
	}

	public Quaternion QuaternionTarget
	{
		get
		{
			return _targetRotation;
		}
		set
		{
			_targetRotation = value;
		}
	}

	protected virtual void IncrementalRotate(float dt)
	{
		if (_rampingUp)
		{
			_fractionalTimeSinceReset += dt / _timeToMaxSpeed;
			if (_fractionalTimeSinceReset > 1f)
			{
				_fractionalTimeSinceReset = 1f;
				_rampingUp = false;
			}
			_currentRotationSpeed = _rampUpCurve.Evaluate(_fractionalTimeSinceReset);
		}
		if (!_quaternionTargetMode)
		{
			_direction = (_target - base.transform.position).normalized;
			_targetRotation = Quaternion.LookRotation(_direction);
		}
		if (_setLocalRotationInQuatTargetMode)
		{
			Quaternion localRotation = Quaternion.Slerp(base.transform.localRotation, _targetRotation, dt * _currentRotationSpeed);
			base.transform.localRotation = localRotation;
		}
		else
		{
			Quaternion rotation = Quaternion.Slerp(base.transform.rotation, _targetRotation, dt * _currentRotationSpeed);
			base.transform.rotation = rotation;
		}
	}

	private void OnDrawGizmos()
	{
	}

	protected bool CheckLockedOn()
	{
		if (_targetLockAngle > 0f)
		{
			Quaternion rotation = base.transform.rotation;
			Vector3 eulerAngles = rotation.eulerAngles;
			eulerAngles.y = 0f;
			rotation.eulerAngles = eulerAngles;
			Quaternion targetRotation = _targetRotation;
			Vector3 eulerAngles2 = targetRotation.eulerAngles;
			eulerAngles2.y = 0f;
			targetRotation.eulerAngles = eulerAngles2;
			if (Quaternion.Angle(rotation, targetRotation) < _targetLockAngle)
			{
				return true;
			}
			return false;
		}
		return base.transform.rotation == _targetRotation;
	}

	public void SetRotationSpeedToMax()
	{
		_fractionalTimeSinceReset = 1f;
		_currentRotationSpeed = _rampUpCurve.Evaluate(_fractionalTimeSinceReset);
		_rampingUp = false;
	}

	public void ResetRotationSpeed(bool beginRampUp)
	{
		_fractionalTimeSinceReset = 0f;
		_currentRotationSpeed = _rampUpCurve.Evaluate(_fractionalTimeSinceReset);
		_rampingUp = beginRampUp;
	}
}
