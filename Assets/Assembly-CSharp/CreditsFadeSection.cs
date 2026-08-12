using UnityEngine;

[RequireComponent(typeof(FadeChildren))]
public class CreditsFadeSection : CreditsSection
{
	[SerializeField]
	private float _fadeInDuration = 0.5f;

	[SerializeField]
	private float _displayDuration = 1.7f;

	[SerializeField]
	private float _fadeOutDuration = 0.5f;

	[SerializeField]
	private float _waitDuration = 0.5f;

	[SerializeField]
	private FadeChildren _fadeComponent;

	[SerializeField]
	private AnimationCurve _fadeCurve;

	private float _startTime;

	private float _fadeInEndTime;

	private float _displayEndTime;

	private float _fadeOutEndTime;

	private float _waitEndTime;

	public float fadeInDuration
	{
		get
		{
			return _fadeInDuration;
		}
		set
		{
			_fadeInDuration = value;
		}
	}

	public float displayDuration
	{
		get
		{
			return _displayDuration;
		}
		set
		{
			_displayDuration = value;
		}
	}

	public float fadeOutDuration
	{
		get
		{
			return _fadeOutDuration;
		}
		set
		{
			_fadeOutDuration = value;
		}
	}

	public float waitDuration
	{
		get
		{
			return _waitDuration;
		}
		set
		{
			_waitDuration = value;
		}
	}

	public override float GetTotalTime()
	{
		return _fadeInDuration + _fadeOutDuration + _displayDuration + _waitDuration;
	}

	public override void Play()
	{
		_startTime = Time.time;
		_fadeInEndTime = _startTime + _fadeInDuration;
		_displayEndTime = _fadeInEndTime + _displayDuration;
		_fadeOutEndTime = _displayEndTime + _fadeOutDuration;
		_waitEndTime = _fadeOutEndTime + _waitDuration;
		_isPlaying = true;
	}

	private void Update()
	{
		if (_isPlaying)
		{
			if (Time.time < _fadeInEndTime)
			{
				SetFade(Mathf.InverseLerp(_startTime, _fadeInEndTime, Time.time), nullCheck: false);
			}
			else if (Time.time < _displayEndTime)
			{
				SetFade(1f, nullCheck: false);
			}
			else if (Time.time < _fadeOutEndTime)
			{
				SetFade(1f - Mathf.InverseLerp(_displayEndTime, _fadeOutEndTime, Time.time), nullCheck: false);
			}
			else if (Time.time < _waitEndTime)
			{
				SetFade(0f, nullCheck: false);
			}
			else
			{
				_isPlaying = false;
			}
		}
	}

	private void SetFade(float fadeVal, bool nullCheck = true)
	{
		if (nullCheck && _fadeComponent == null)
		{
			_fadeComponent = GetComponent<FadeChildren>();
		}
		_fadeComponent.fade = _fadeCurve.Evaluate(fadeVal);
	}

	public override bool SimulateTime(float time)
	{
		float totalTime = GetTotalTime();
		if (time > totalTime)
		{
			SetFade(0f);
			return false;
		}
		if (time < _fadeInDuration)
		{
			SetFade(Mathf.InverseLerp(0f, _fadeInDuration, time));
			return true;
		}
		if (time < _displayDuration)
		{
			SetFade(1f);
			return true;
		}
		if (time < _fadeInDuration + _displayDuration + _fadeOutDuration)
		{
			float num = _fadeInDuration + _displayDuration;
			float fadeVal = 1f - Mathf.InverseLerp(num, num + _fadeOutDuration, time);
			SetFade(fadeVal);
			return true;
		}
		SetFade(0f);
		return true;
	}

	public override void ResetSimulate()
	{
		SetFade(0f);
	}
}
