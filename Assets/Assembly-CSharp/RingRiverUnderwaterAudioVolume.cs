using UnityEngine;

public class RingRiverUnderwaterAudioVolume : AudioVolume
{
	[SerializeField]
	private OWAudioSource _undertowSource;

	private bool _wasInUndertow;

	protected override void Reset()
	{
		base.Reset();
		if (_undertowSource != null)
		{
			_undertowSource.loop = true;
			_undertowSource.playOnAwake = false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		base.enabled = false;
	}

	protected override void Init()
	{
		base.Init();
		_undertowSource.Stop();
		_undertowSource.SetLocalVolume(0f);
		_undertowSource.rolloffMode = AudioRolloffMode.Custom;
		_undertowSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0f, 1f, 1f, 1f));
		_undertowSource.spatialBlend = 1f;
		_undertowSource.spread = 180f;
		_undertowSource.dopplerLevel = 0f;
	}

	public override void Activate()
	{
		if (!_initialized)
		{
			Init();
		}
		_isActive = true;
		base.enabled = true;
		UpdatePlayState(PlayerState.InUndertowVolume());
		Locator.GetRingWorldController().GetRiverPathAudioController().AddAudioModifier(AudioModifier.Muffle);
	}

	public override void Deactivate()
	{
		base.enabled = false;
		_isActive = false;
		_wasInUndertow = false;
		_owAudioSrc.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		_undertowSource.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		Locator.GetRingWorldController().GetRiverPathAudioController().RemoveAudioModifier(AudioModifier.Muffle);
	}

	private void Update()
	{
		bool flag = PlayerState.InUndertowVolume();
		if (flag != _wasInUndertow)
		{
			UpdatePlayState(flag);
			_wasInUndertow = flag;
		}
	}

	private void UpdatePlayState(bool inUndertow)
	{
		if (inUndertow)
		{
			_undertowSource.FadeIn(_fadeSeconds, fadeFromNothing: false, _randomizePlayhead);
			_owAudioSrc.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		}
		else
		{
			_owAudioSrc.FadeIn(_fadeSeconds, fadeFromNothing: false, _randomizePlayhead);
			_undertowSource.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		}
	}
}
