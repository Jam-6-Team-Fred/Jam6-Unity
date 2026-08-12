using UnityEngine;

public class MindSlideProjector : MonoBehaviour
{
	[SerializeField]
	private MindSlideCollection _mindSlideCollection;

	[SerializeField]
	private AnimationCurve _closingCurve;

	[SerializeField]
	private float _closingDuration;

	[SerializeField]
	private AnimationCurve _openingCurve;

	[SerializeField]
	private float _openingDuration;

	[Tooltip("offset in seconds when slide fade in should happen. Can be negative")]
	[SerializeField]
	private float _startSlideFadeCloseTimeOffset;

	[SerializeField]
	private float _slideFadeDuration;

	[Header("Deprecated (use MindSlideCollection instead)")]
	[SerializeField]
	private float _defaultSlideDuration;

	[SerializeField]
	private SlideCollectionContainer _slideCollectionItem;

	public OWEvent OnProjectionStart = new OWEvent(1);

	public OWEvent OnProjectionStop = new OWEvent(1);

	public OWEvent OnProjectionComplete = new OWEvent(1);

	private MindProjectorImageEffect _mindProjectorImageEffect;

	private Texture _slideTexture;

	private bool _isPlaying;

	private bool _isClosing;

	private bool _isOpening;

	private bool _isScrollingSlides;

	private bool _timeFrozen;

	private float _closingTime;

	private float _openingTime;

	private float _lastSlidePlayTime;

	private float _slideFadeTime;

	private float _eyeOpenness;

	public MindSlideCollection mindSlideCollection => _mindSlideCollection;

	public Texture slideTexture => _slideTexture;

	private void Awake()
	{
		if (_mindSlideCollection != null)
		{
			_defaultSlideDuration = _mindSlideCollection.defaultSlideDuration;
			_slideCollectionItem = _mindSlideCollection.slideCollectionContainer;
		}
	}

	private void Start()
	{
		_mindProjectorImageEffect = Locator.GetPlayerCamera().GetComponent<MindProjectorImageEffect>();
		_slideCollectionItem.onSlideTextureUpdated += new OWEvent.OWCallback(OnSlideTextureUpdated);
		_slideCollectionItem.onPlayBeatAudio += new OWEvent<AudioType>.OWCallback(OnPlayBeatAudio);
		_slideCollectionItem.Initialize();
		_slideCollectionItem.enabled = false;
		_eyeOpenness = 1f;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_slideCollectionItem.onSlideTextureUpdated -= new OWEvent.OWCallback(OnSlideTextureUpdated);
		_slideCollectionItem.onPlayBeatAudio -= new OWEvent<AudioType>.OWCallback(OnPlayBeatAudio);
		Finish();
	}

	public bool IsPlaying()
	{
		return _isPlaying;
	}

	public bool IsOpeningEyes()
	{
		return _isOpening;
	}

	private void Update()
	{
		if (!_isPlaying)
		{
			return;
		}
		if (!_isOpening && Locator.GetDeathManager().IsPlayerDying())
		{
			Stop();
		}
		_mindProjectorImageEffect.eyeOpenness = _eyeOpenness;
		if (_isClosing)
		{
			_eyeOpenness = Mathf.Min(_closingCurve.Evaluate((Time.unscaledTime - _closingTime) / _closingDuration), _eyeOpenness);
			_mindProjectorImageEffect.slideFade = _eyeOpenness;
			if (_eyeOpenness <= 0f)
			{
				_isClosing = false;
				_lastSlidePlayTime = Time.unscaledTime;
				_slideCollectionItem.SetCurrentRead();
				_slideCollectionItem.ForceCurrentSlideDisplayEvent(forward: true);
				OnProjectionStart.Invoke();
			}
		}
		if (_isOpening)
		{
			_eyeOpenness = Mathf.Max(_openingCurve.Evaluate((Time.unscaledTime - _openingTime) / _openingDuration), _eyeOpenness);
			if (_eyeOpenness >= 1f)
			{
				_isOpening = false;
				Finish();
				return;
			}
		}
		float num = Mathf.Clamp01((Time.unscaledTime - _slideFadeTime) / _slideFadeDuration);
		_mindProjectorImageEffect.slideFade = num;
		if (num >= 1f && !_isScrollingSlides && !_isOpening)
		{
			_isScrollingSlides = true;
		}
		float currentSlidePlayDuration = GetCurrentSlidePlayDuration();
		if (_isScrollingSlides && Time.unscaledTime >= _lastSlidePlayTime + currentSlidePlayDuration)
		{
			if (!_slideCollectionItem.isEndOfSlide)
			{
				NextSlide();
			}
			else
			{
				OnEndOfSlides();
			}
			_lastSlidePlayTime += currentSlidePlayDuration;
		}
		UpdateTimeFreeze();
	}

	private void UpdateTimeFreeze()
	{
		bool flag = _eyeOpenness <= 0f && !Locator.GetGlobalMusicController().IsEndTimesPlaying();
		if (OWInput.IsInputMode(InputMode.Character | InputMode.NomaiRemoteCam) && (OWInput.IsPressed(InputLibrary.jump) || OWInput.GetAxisValue(InputLibrary.moveXZ).magnitude > 0f || OWInput.GetValue(InputLibrary.thrustUp) > 0f || OWInput.GetValue(InputLibrary.thrustDown) > 0f))
		{
			flag = false;
		}
		if (!_timeFrozen && flag)
		{
			OWTime.Pause(OWTime.PauseType.Reading);
			_timeFrozen = true;
		}
		else if (_timeFrozen && !flag)
		{
			OWTime.Unpause(OWTime.PauseType.Reading);
			_timeFrozen = false;
		}
	}

	public void SetMindSlideCollection(MindSlideCollection mindSlideCollection)
	{
		if (!(_mindSlideCollection == mindSlideCollection))
		{
			_slideCollectionItem.onSlideTextureUpdated -= new OWEvent.OWCallback(OnSlideTextureUpdated);
			_slideCollectionItem.onPlayBeatAudio -= new OWEvent<AudioType>.OWCallback(OnPlayBeatAudio);
			_mindSlideCollection = mindSlideCollection;
			_defaultSlideDuration = _mindSlideCollection.defaultSlideDuration;
			_slideCollectionItem = _mindSlideCollection.slideCollectionContainer;
			_slideCollectionItem.onSlideTextureUpdated += new OWEvent.OWCallback(OnSlideTextureUpdated);
			_slideCollectionItem.onPlayBeatAudio += new OWEvent<AudioType>.OWCallback(OnPlayBeatAudio);
			_slideCollectionItem.Initialize();
			_slideCollectionItem.enabled = false;
		}
	}

	public void Play(bool reset)
	{
		_mindProjectorImageEffect.enabled = true;
		_mindProjectorImageEffect.slideFade = 0f;
		_mindProjectorImageEffect.eyeOpenness = 1f;
		base.enabled = true;
		_isPlaying = true;
		_isClosing = true;
		_isOpening = false;
		_isScrollingSlides = false;
		_closingTime = Time.unscaledTime;
		_openingTime = float.PositiveInfinity;
		_slideFadeTime = _closingTime + _closingDuration + _startSlideFadeCloseTimeOffset;
		_slideCollectionItem.enabled = true;
		if (reset)
		{
			_slideCollectionItem.ResetSlideIndex();
		}
		if (_slideCollectionItem.streamingTexturesAvailable)
		{
			_slideCollectionItem.LoadStreamingTextures();
		}
		UpdateSlideTexture();
		_slideCollectionItem.TryPlayMusicForCurrentSlideInclusive();
		_lastSlidePlayTime = float.MaxValue;
		Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.VisionTorch_EnterVision);
		Locator.GetAudioMixer().OnEnterMindSlideProjector();
		Locator.GetPauseCommandListener().AddPauseCommandLock();
		GlobalMessenger.FireEvent("StartViewingProjector");
	}

	public void Stop(bool projectionComplete = false)
	{
		if (!_isOpening && _isPlaying)
		{
			_isScrollingSlides = false;
			_isOpening = true;
			_isClosing = false;
			_openingTime = Time.unscaledTime;
			if (_timeFrozen)
			{
				_timeFrozen = false;
				OWTime.Unpause(OWTime.PauseType.Reading);
			}
			Locator.GetSlideReelMusicManager().OnExitSlideProjector(projectionComplete);
			Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.VisionTorch_ExitVision);
			Locator.GetAudioMixer().OnExitMindSlideProjector();
			Locator.GetPauseCommandListener().RemovePauseCommandLock();
			OnProjectionStop.Invoke();
		}
	}

	private void NextSlide()
	{
		_slideCollectionItem.IncreaseSlideIndex();
		_slideCollectionItem.SetCurrentRead();
		_slideCollectionItem.TryPlayMusicForCurrentSlideTransition(forward: true);
	}

	private void OnSlideTextureUpdated()
	{
		UpdateSlideTexture();
	}

	private void UpdateSlideTexture()
	{
		_slideTexture = _slideCollectionItem.GetCurrentSlideTexture();
		_mindProjectorImageEffect.slideTexture = _slideTexture;
	}

	private void OnPlayBeatAudio(AudioType audioType)
	{
		Locator.GetSlideReelMusicManager().PlayBeat(audioType, allowOverlap: true);
	}

	private void OnEndOfSlides()
	{
		Stop(projectionComplete: true);
		OnProjectionComplete.Invoke();
	}

	private float GetCurrentSlidePlayDuration()
	{
		float num = 0f;
		Slide currentSlide = _slideCollectionItem.GetCurrentSlide();
		SlideBlackFrameModule module = currentSlide.GetModule<SlideBlackFrameModule>();
		if (module != null)
		{
			num = module._duration;
		}
		SlidePlayTimeModule module2 = currentSlide.GetModule<SlidePlayTimeModule>();
		if (module2 != null)
		{
			return module2._duration + num;
		}
		return _defaultSlideDuration + num;
	}

	private void Finish()
	{
		base.enabled = false;
		_isPlaying = false;
		_isOpening = false;
		GlobalMessenger.FireEvent("EndViewingProjector");
		if (_mindProjectorImageEffect != null)
		{
			_mindProjectorImageEffect.enabled = false;
			_mindProjectorImageEffect.slideTexture = null;
		}
		_slideCollectionItem.enabled = false;
		if (_slideCollectionItem.streamingTexturesAvailable)
		{
			_slideCollectionItem.UnloadStreamingTextures();
		}
	}
}
