using UnityEngine;

public class GearInterfaceEffects : MonoBehaviour
{
	[Tooltip("Degrees per second")]
	[SerializeField]
	private float _speed = 360f;

	[SerializeField]
	private bool _useUnscaledTime;

	[SerializeField]
	private Transform _transformOverride;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[Header("Failure Animation")]
	[SerializeField]
	private AnimationCurve _failureAnimation;

	[SerializeField]
	private float _failureAnimMaxAngle;

	[SerializeField]
	private float _failureAnimDuration;

	private float _currentTarget;

	private Transform _targetTransform;

	private bool _isPlayingFailure;

	private bool _isPlayingFailureReverse;

	private float _failureAnimStartTime;

	private float _failureAnimStartAngle;

	private void Awake()
	{
		base.enabled = false;
		_targetTransform = ((_transformOverride != null) ? _transformOverride : base.transform);
	}

	private void Update()
	{
		if (_isPlayingFailure)
		{
			float num = ((_useUnscaledTime ? Time.unscaledTime : Time.time) - _failureAnimStartTime) / _failureAnimDuration;
			if (num < 1f)
			{
				_currentTarget = _failureAnimStartAngle + _failureAnimation.Evaluate(num) * _failureAnimMaxAngle * (float)((!_isPlayingFailureReverse) ? 1 : (-1));
				_currentTarget = OWMath.SetAnglePositive(_currentTarget);
			}
			else
			{
				EndFailure();
			}
		}
		UpdateAngle();
	}

	private void UpdateAngle()
	{
		Vector3 localEulerAngles = _targetTransform.localEulerAngles;
		if (Mathf.Abs(localEulerAngles.z - _currentTarget) <= 0.001f)
		{
			Vector3 localEulerAngles2 = _targetTransform.localEulerAngles;
			_targetTransform.localRotation = Quaternion.Euler(localEulerAngles2.x, localEulerAngles2.y, _currentTarget);
			base.enabled = _isPlayingFailure;
		}
		else
		{
			float num = (_useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
			float value = ShortestAngle(localEulerAngles.z);
			float num2 = _speed * num;
			_targetTransform.Rotate(new Vector3(0f, 0f, Mathf.Clamp(value, 0f - num2, num2)));
		}
	}

	private float ShortestAngle(float localAngle)
	{
		float num = OWMath.WrapAngle(localAngle - _currentTarget);
		float num2 = OWMath.WrapAngle(_currentTarget - localAngle);
		if (!(Mathf.Abs(num) < Mathf.Abs(num2)))
		{
			return num2;
		}
		return num;
	}

	public void PlayFailure(bool forward = true, float audioVolume = 1f)
	{
		if (!_isPlayingFailure)
		{
			_isPlayingFailure = true;
			_isPlayingFailureReverse = !forward;
			_failureAnimStartTime = (_useUnscaledTime ? Time.unscaledTime : Time.time);
			_failureAnimStartAngle = _currentTarget;
			if (_oneShotSource != null && audioVolume > 0f)
			{
				_oneShotSource.PlayOneShot(AudioType.GearRotate_Fail, audioVolume);
			}
			base.enabled = true;
		}
	}

	private void EndFailure()
	{
		_isPlayingFailure = false;
		_currentTarget = _failureAnimStartAngle;
	}

	public bool IsRotating()
	{
		return base.enabled;
	}

	public void AddRotation(float angle, float audioVolume = 1f)
	{
		if (_isPlayingFailure)
		{
			EndFailure();
		}
		_currentTarget += angle;
		_currentTarget = OWMath.SetAnglePositive(_currentTarget);
		if (_oneShotSource != null && audioVolume > 0f)
		{
			_oneShotSource.PlayOneShot(AudioType.GearRotate_Heavy, audioVolume);
		}
		base.enabled = true;
	}
}
