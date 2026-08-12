using UnityEngine;

public class DayNightAudioVolume : AudioVolume
{
	[SerializeField]
	private AudioType _dayLibraryClip;

	[SerializeField]
	private AudioType _nightLibraryClip;

	[SerializeField]
	private float _dayWindow = 180f;

	[SerializeField]
	private OWAudioSource _daySource;

	[SerializeField]
	private OWAudioSource _nightSource;

	[SerializeField]
	private bool _usePlayerPosition;

	[SerializeField]
	private Transform _dayPointTransform;

	private Transform _planetTransform;

	private bool _wasDay;

	protected override void Awake()
	{
		base.Awake();
		_planetTransform = base.gameObject.GetAttachedOWRigidbody().transform;
		if (_dayPointTransform == null)
		{
			_dayPointTransform = base.transform;
		}
		if (_nightSource == null)
		{
			Debug.LogWarning("No Night source found! Instantiating new object");
			GameObject gameObject = new GameObject("NightAudioSource", typeof(AudioSource));
			gameObject.transform.SetParent(base.transform);
			_nightSource = gameObject.AddComponent<OWAudioSource>();
		}
	}

	protected override void Start()
	{
		if (_usePlayerPosition)
		{
			_dayPointTransform = Locator.GetPlayerTransform();
		}
		base.enabled = false;
	}

	protected override void Init()
	{
		base.Init();
		if (_daySource == null)
		{
			if (_owAudioSrc == null)
			{
				Debug.LogError("No OWAudioSource Found!", this);
			}
			else
			{
				_daySource = _owAudioSrc;
				Debug.LogWarning("_daySource is null! Using default AudioVolume Source.", this);
			}
		}
		_daySource.Stop();
		_daySource.SetLocalVolume(0f);
		_nightSource.Stop();
		_nightSource.SetLocalVolume(0f);
		if (_owAudioSrc.GetTrack() != OWAudioMixer.TrackName.Music)
		{
			_daySource.rolloffMode = AudioRolloffMode.Custom;
			_daySource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0f, 1f, 1f, 1f));
			_daySource.spatialBlend = 1f;
			_daySource.spread = 180f;
			_daySource.dopplerLevel = 0f;
			_nightSource.rolloffMode = AudioRolloffMode.Custom;
			_nightSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0f, 1f, 1f, 1f));
			_nightSource.spatialBlend = 1f;
			_nightSource.spread = 180f;
			_nightSource.dopplerLevel = 0f;
		}
		if (_dayLibraryClip != 0)
		{
			_daySource.AssignAudioLibraryClip(_dayLibraryClip);
		}
		if (_nightLibraryClip != 0)
		{
			_nightSource.AssignAudioLibraryClip(_nightLibraryClip);
		}
	}

	public override void Activate()
	{
		base.enabled = true;
		_isActive = true;
		UpdatePlayState(IsDay());
	}

	public override void Deactivate()
	{
		base.enabled = false;
		_isActive = false;
		_daySource.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		if (_nightSource != null)
		{
			_nightSource.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		}
	}

	private void Update()
	{
		bool flag = IsDay();
		if (flag)
		{
			if (!_wasDay)
			{
				UpdatePlayState(flag);
			}
		}
		else if (_wasDay)
		{
			UpdatePlayState(flag);
		}
		_wasDay = flag;
	}

	private void UpdatePlayState(bool isDay)
	{
		if (!_initialized)
		{
			Init();
		}
		if (isDay)
		{
			_daySource.FadeIn(_fadeSeconds, fadeFromNothing: false, _randomizePlayhead);
			if (_nightSource != null)
			{
				_nightSource.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
			}
		}
		else
		{
			_daySource.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
			if (_nightSource != null)
			{
				_nightSource.FadeIn(_fadeSeconds, fadeFromNothing: false, _randomizePlayhead);
			}
		}
	}

	private bool IsDay()
	{
		return Vector3.Angle(_planetTransform.position - _dayPointTransform.position, _dayPointTransform.position - Locator.GetSunTransform().position) < _dayWindow * 0.5f;
	}
}
