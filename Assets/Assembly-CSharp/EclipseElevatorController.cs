using UnityEngine;

public class EclipseElevatorController : AbstractGhostElevatorInterface
{
	[SerializeField]
	private SingleLightSensor[] _lightSensors;

	[SerializeField]
	private Transform[] _rotatingElements;

	[Space]
	[SerializeField]
	private float _rotationSpeed = 180f;

	[SerializeField]
	private float _anglePrecision = 10f;

	[Header("Audio")]
	[SerializeField]
	private AudioLoopCrossfader _lightSensorCrossfader;

	[SerializeField]
	private OWAudioSource _rotationAudio;

	private void Awake()
	{
		if (_rotatingElements.Length < 1)
		{
			Debug.LogError("No rotating dial.");
		}
		for (int i = 0; i < _lightSensors.Length; i++)
		{
			_lightSensors[i].OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
			_lightSensors[i].OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		}
		base.enabled = false;
	}

	public override void SetStartingPosition(bool isUp)
	{
		for (int i = 0; i < _rotatingElements.Length; i++)
		{
			float z = _rotatingElements[i].localRotation.eulerAngles.z;
			_rotatingElements[i].Rotate(new Vector3(0f, 0f, (float)((!isUp) ? 180 : 0) - z));
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _lightSensors.Length; i++)
		{
			_lightSensors[i].OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
			_lightSensors[i].OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		}
	}

	private void FixedUpdate()
	{
		bool flag = false;
		float num = _rotatingElements[0].localRotation.eulerAngles.z % 360f;
		if (_lightSensors[0].IsIlluminated() && _lightSensors[1].IsIlluminated())
		{
			flag = true;
			for (int i = 0; i < _rotatingElements.Length; i++)
			{
				_rotatingElements[i].Rotate(new Vector3(0f, 0f, _rotationSpeed * Time.deltaTime));
			}
		}
		else if (_lightSensors[0].IsIlluminated() && !CheckIfUpSelected(num, 0.1f))
		{
			flag = true;
			for (int j = 0; j < _rotatingElements.Length; j++)
			{
				_rotatingElements[j].Rotate(new Vector3(0f, 0f, ((num > 180f) ? 1f : (-1f)) * _rotationSpeed * Time.deltaTime));
			}
		}
		else if (_lightSensors[1].IsIlluminated() && !CheckIfDownSelected(num, 0.1f))
		{
			flag = true;
			for (int k = 0; k < _rotatingElements.Length; k++)
			{
				_rotatingElements[k].Rotate(new Vector3(0f, 0f, ((num < 180f) ? 1f : (-1f)) * _rotationSpeed * Time.deltaTime));
			}
		}
		if (flag && (!_rotationAudio.isPlaying || _rotationAudio.IsFadingOut()))
		{
			_rotationAudio.FadeIn(0.2f);
		}
		else if (!flag && _rotationAudio.isPlaying && !_rotationAudio.IsFadingOut())
		{
			_rotationAudio.FadeOut(0.2f);
		}
	}

	private void OnDetectLight()
	{
		base.enabled = true;
		if (!_lightSensorCrossfader.shouldBePlaying)
		{
			_lightSensorCrossfader.Play();
		}
	}

	private bool CheckIfUpSelected(float currentRotation, float precision)
	{
		if (!(currentRotation < precision))
		{
			return 360f - currentRotation < precision;
		}
		return true;
	}

	private bool CheckIfDownSelected(float currentRotation, float precision)
	{
		return Mathf.Abs(currentRotation - 180f) < precision;
	}

	private void OnDetectDarkness()
	{
		for (int i = 0; i < _lightSensors.Length; i++)
		{
			if (_lightSensors[i].IsIlluminated())
			{
				return;
			}
		}
		float currentRotation = _rotatingElements[0].localRotation.eulerAngles.z % 360f;
		if (CheckIfUpSelected(currentRotation, _anglePrecision))
		{
			CallOnUpEvent();
		}
		if (CheckIfDownSelected(currentRotation, _anglePrecision))
		{
			CallOnDownEvent();
		}
		_rotationAudio.FadeOut(0.2f);
		_lightSensorCrossfader.Stop();
		base.enabled = false;
	}

	public override void OnActionComplete()
	{
	}
}
