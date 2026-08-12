using UnityEngine;

public class AutoSlideProjector : SectoredMonoBehaviour
{
	[SerializeField]
	private float _defaultSlideDuration;

	[SerializeField]
	private float _endPauseDuration;

	[SerializeField]
	private SlideCollectionContainer _slideCollectionItem;

	[SerializeField]
	private OWLight2 _light;

	[SerializeField]
	private OWLight2 _bounceLight;

	[SerializeField]
	private OWRenderer[] _lightShaftRenderers = new OWRenderer[0];

	[Space]
	[SerializeField]
	private OWAudioSource _oneShotAudio;

	private float _lastSlidePlayTime;

	private float _startPausingEndTime;

	private bool _isPlaying;

	private bool _isPausingEnd;

	protected override void Awake()
	{
		base.Awake();
		if (_slideCollectionItem != null)
		{
			_slideCollectionItem.onSlideTextureUpdated += new OWEvent.OWCallback(OnSlideTextureUpdated);
			_slideCollectionItem.Initialize();
			_slideCollectionItem.enabled = false;
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_slideCollectionItem != null)
		{
			_slideCollectionItem.onSlideTextureUpdated -= new OWEvent.OWCallback(OnSlideTextureUpdated);
		}
	}

	public bool IsPlaying()
	{
		return _isPlaying;
	}

	public void Play(bool reset)
	{
		if (!_isPlaying)
		{
			_light.SetActivation(active: true);
			if (_bounceLight != null)
			{
				_bounceLight.SetActivation(active: true);
			}
			for (int i = 0; i < _lightShaftRenderers.Length; i++)
			{
				_lightShaftRenderers[i].SetActivation(active: true);
			}
			if (reset)
			{
				_slideCollectionItem.ResetSlideIndex();
			}
			if (_slideCollectionItem.streamingTexturesAvailable)
			{
				_slideCollectionItem.LoadStreamingTextures();
			}
			UpdateSlideTexture();
			_lastSlidePlayTime = Time.time;
			_isPlaying = true;
			base.enabled = true;
		}
	}

	public void Stop()
	{
		if (_isPlaying)
		{
			_isPlaying = false;
			base.enabled = false;
			_slideCollectionItem.enabled = false;
			if (_slideCollectionItem.streamingTexturesAvailable)
			{
				_slideCollectionItem.UnloadStreamingTextures();
			}
		}
	}

	public void TurnOff()
	{
		Stop();
		_oneShotAudio.PlayOneShot(AudioType.Lantern_Remove);
		_light.SetActivation(active: false);
		if (_bounceLight != null)
		{
			_bounceLight.SetActivation(active: false);
		}
		for (int i = 0; i < _lightShaftRenderers.Length; i++)
		{
			_lightShaftRenderers[i].SetActivation(active: false);
		}
	}

	protected void SetSlideCollection(SlideCollectionContainer collection)
	{
		if (_slideCollectionItem != null)
		{
			if (_isPlaying)
			{
				_slideCollectionItem.enabled = false;
				if (_slideCollectionItem.streamingTexturesAvailable)
				{
					_slideCollectionItem.UnloadStreamingTextures();
				}
			}
			_slideCollectionItem.onSlideTextureUpdated -= new OWEvent.OWCallback(OnSlideTextureUpdated);
		}
		_slideCollectionItem = collection;
		_slideCollectionItem.onSlideTextureUpdated += new OWEvent.OWCallback(OnSlideTextureUpdated);
		_slideCollectionItem.Initialize();
		if (_isPlaying)
		{
			if (_slideCollectionItem.streamingTexturesAvailable)
			{
				_slideCollectionItem.LoadStreamingTextures();
			}
			UpdateSlideTexture();
		}
	}

	protected virtual void Update()
	{
		if (!_isPlaying)
		{
			return;
		}
		if (_isPausingEnd)
		{
			if (Time.time >= _endPauseDuration + _startPausingEndTime)
			{
				_isPausingEnd = false;
				FirstSlide();
			}
		}
		else if (Time.time >= GetCurrentSlidePlayDuration() + _lastSlidePlayTime)
		{
			if (!_slideCollectionItem.isEndOfSlide)
			{
				NextSlide();
			}
			else if (_endPauseDuration > 0f)
			{
				_isPausingEnd = true;
				_startPausingEndTime = Time.time;
			}
			else
			{
				FirstSlide();
			}
		}
	}

	private void OnSlideTextureUpdated()
	{
		UpdateSlideTexture();
	}

	private void UpdateSlideTexture()
	{
		_light.GetLight().cookie = _slideCollectionItem.GetCurrentSlideTexture();
	}

	private void FirstSlide()
	{
		_slideCollectionItem.ResetSlideIndex();
		_lastSlidePlayTime = Time.time;
		if (_oneShotAudio != null)
		{
			_oneShotAudio.PlayOneShot(AudioType.Projector_Next);
		}
	}

	private void NextSlide()
	{
		_slideCollectionItem.IncreaseSlideIndex();
		_slideCollectionItem.SetCurrentRead();
		_lastSlidePlayTime = Time.time;
		if (_oneShotAudio != null)
		{
			_oneShotAudio.PlayOneShot(AudioType.Projector_Next);
		}
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

	protected override void OnSectorOccupantAdded(SectorDetector detector)
	{
		if (!(_slideCollectionItem == null) && !_isPlaying && _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			Play(reset: false);
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector detector)
	{
		if (!(_slideCollectionItem == null) && _isPlaying && !_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			Stop();
		}
	}
}
