using UnityEngine;

[AddComponentMenu("Audio/Audio Volume", 400)]
[RequireComponent(typeof(OWAudioSource))]
public class AudioVolume : PriorityVolume
{
	[Space(10f)]
	[SerializeField]
	protected float _fadeSeconds = 2f;

	[SerializeField]
	protected bool _noFadeFromBeginning;

	[SerializeField]
	protected bool _randomizePlayhead;

	[SerializeField]
	protected bool _pauseOnFadeOut;

	[SerializeField]
	protected OWTriggerVolume _triggerVolumeOverride;

	protected OWAudioSource _owAudioSrc;

	protected bool _isActive;

	protected bool _initialized;

	public OWEvent OnAudioPlay = new OWEvent(4);

	public OWEvent OnAudioStop = new OWEvent(4);

	protected override void Reset()
	{
		base.Reset();
		AudioSource component = GetComponent<AudioSource>();
		component.loop = true;
		component.playOnAwake = false;
	}

	protected override void Awake()
	{
		_owAudioSrc = GetComponent<OWAudioSource>();
		_triggerVolume = _triggerVolumeOverride;
		base.Awake();
	}

	protected virtual void Start()
	{
		if (!_initialized)
		{
			Init();
		}
		if (_isActive)
		{
			PlayAudio();
		}
	}

	protected virtual void Init()
	{
		_initialized = true;
		_owAudioSrc.Stop();
		_owAudioSrc.SetLocalVolume(0f);
		if (_owAudioSrc.GetTrack() != OWAudioMixer.TrackName.Music)
		{
			_owAudioSrc.rolloffMode = AudioRolloffMode.Custom;
			_owAudioSrc.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0f, 1f, 1f, 1f));
			_owAudioSrc.spatialBlend = 1f;
			_owAudioSrc.spread = 180f;
			_owAudioSrc.dopplerLevel = 0f;
		}
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public virtual void Activate()
	{
		if (!_initialized)
		{
			Init();
		}
		_isActive = true;
		PlayAudio();
		OnAudioPlay.Invoke();
	}

	public virtual void Deactivate()
	{
		_isActive = false;
		_owAudioSrc.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		OnAudioStop.Invoke();
	}

	public virtual void Deactivate(float fadeSeconds)
	{
		if (GetType() != typeof(AudioVolume))
		{
			Debug.LogError("Subclasses of AudioVolume need custom implementation for this method", this);
			Debug.Break();
		}
		_isActive = false;
		_owAudioSrc.FadeOut(fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		OnAudioStop.Invoke();
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		AudioDetector component = hitObj.GetComponent<AudioDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		AudioDetector component = hitObj.GetComponent<AudioDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}

	protected virtual void PlayAudio()
	{
		if (!_owAudioSrc.isPlaying && _noFadeFromBeginning && _owAudioSrc.time <= 0f)
		{
			_owAudioSrc.SetLocalVolume(1f);
			_owAudioSrc.Play();
		}
		else
		{
			_owAudioSrc.FadeIn(_fadeSeconds, fadeFromNothing: false, _randomizePlayhead);
		}
	}
}
