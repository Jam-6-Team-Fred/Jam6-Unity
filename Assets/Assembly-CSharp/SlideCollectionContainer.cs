using System;
using System.Collections.Generic;
using UnityEngine;

public class SlideCollectionContainer : MonoBehaviour, IStreamingTexturesSubscriber
{
	public struct SlideMusicRange
	{
		public AudioType audioType;

		public float fadeInTime;

		public int start;

		public int end;
	}

	private struct ItemSlidePair
	{
		public SlideCollectionContainer item;

		public int slideIdx;
	}

	[SerializeField]
	private string _shipLogOnComplete;

	[SerializeField]
	private string[] _playWithShipLogFacts;

	[SerializeField]
	private bool _autoLoadStreaming;

	[SerializeField]
	private bool _invertBlackFrames;

	[SerializeField]
	private SlideCollection _slideCollection = new SlideCollection(0);

	private int _currentSlideIndex;

	private int[] _lastManualStreamRequest;

	private bool _changeSlidesAllowed;

	private SlideReelItem _owningItem;

	private List<int> _unreadSlideIndices = new List<int>(128);

	private static Dictionary<string, List<ItemSlidePair>> _shipLogPerSlideCompletion;

	private List<SlideMusicRange> _musicRanges;

	private bool _initialized;

	public OWEvent onSlideTextureUpdated = new OWEvent(1);

	public OWEvent onEndOfSlides = new OWEvent(1);

	public OWEvent<LightParameters> onNeedBounceLightUpdate = new OWEvent<LightParameters>(1);

	public OWEvent<AudioType> onPlayBeatAudio = new OWEvent<AudioType>(1);

	public SlideCollection slideCollection
	{
		get
		{
			return _slideCollection;
		}
		set
		{
			_slideCollection = value;
		}
	}

	public bool invertBlackFrames => _invertBlackFrames;

	public int slideCount
	{
		get
		{
			if (_slideCollection.slides == null)
			{
				return 0;
			}
			return _slideCollection.slides.Length;
		}
	}

	public bool isEndOfSlide => _currentSlideIndex >= slideCount - 1;

	public int slideIndex
	{
		get
		{
			return _currentSlideIndex;
		}
		set
		{
			if (_changeSlidesAllowed && _currentSlideIndex != value)
			{
				bool forward = _currentSlideIndex < value;
				if (GetCurrentSlide() != null)
				{
					GetCurrentSlide().EndDisplay(this, forward);
				}
				_currentSlideIndex = value;
				if (_currentSlideIndex > _slideCollection.slides.Length - 1)
				{
					_currentSlideIndex = 0;
				}
				if (_currentSlideIndex < 0)
				{
					_currentSlideIndex = _slideCollection.slides.Length - 1;
				}
				if (streamingTexturesAvailable && !_autoLoadStreaming)
				{
					RequestManualStreamSlides();
				}
				GetCurrentSlide().Display(this, forward);
			}
		}
	}

	public Texture firstSlideStandIn
	{
		get
		{
			if (_owningItem == null)
			{
				return null;
			}
			return _owningItem.firstSlideStandIn;
		}
	}

	public bool isLoadingStreaming
	{
		get
		{
			if (!streamingTexturesAvailable)
			{
				return false;
			}
			return StreamingManager.GetStreamingAssetBundleState(_slideCollection.streamingAssetIdentifier).isLoading;
		}
	}

	private float streamingProgress
	{
		get
		{
			if (!streamingTexturesAvailable)
			{
				return 0f;
			}
			return StreamingManager.GetStreamingAssetBundleState(_slideCollection.streamingAssetIdentifier).progress;
		}
	}

	public string streamingAssetID => _slideCollection.streamingAssetIdentifier;

	public bool streamingTexturesAvailable
	{
		get
		{
			if (!StreamingManager.isStreamingEnabled)
			{
				return false;
			}
			if (string.IsNullOrEmpty(_slideCollection.streamingAssetIdentifier))
			{
				return false;
			}
			return StreamingManager.StreamingAssetAvailable(_slideCollection.streamingAssetIdentifier);
		}
	}

	private void Awake()
	{
		_owningItem = GetComponent<SlideReelItem>();
	}

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
		GetCurrentSlide().Update(this);
	}

	private void OnDestroy()
	{
		if (_shipLogPerSlideCompletion != null)
		{
			_shipLogPerSlideCompletion = null;
		}
	}

	public void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		SetupReadFlags();
		RegisterPerSlideCompletion();
		if (streamingTexturesAvailable)
		{
			SetupStreaming();
		}
		BuildMusicRangesIndex();
		_changeSlidesAllowed = true;
		_initialized = true;
		_slideCollection.isVision = _owningItem == null;
		for (int i = 0; i < _playWithShipLogFacts.Length; i++)
		{
			ShipLogFact fact = Locator.GetShipLogManager().GetFact(_playWithShipLogFacts[i]);
			if (fact != null)
			{
				fact.RegisterSlideCollection(_slideCollection);
			}
			else
			{
				Debug.LogError("Failed to locate ship log fact " + _playWithShipLogFacts[i], this);
			}
		}
	}

	public Slide GetCurrentSlide()
	{
		if (_slideCollection.slides.Length == 0)
		{
			return null;
		}
		return _slideCollection.slides[_currentSlideIndex];
	}

	public Texture GetCurrentSlideTexture()
	{
		if (_slideCollection.slides.Length == 0)
		{
			return null;
		}
		return GetCurrentSlide().GetTexture();
	}

	public Texture GetStreamingTexture(int id)
	{
		return _slideCollection.GetStreamingTexture(id);
	}

	public bool NextSlideAvailable()
	{
		if (!streamingTexturesAvailable)
		{
			return true;
		}
		int num = slideIndex + 1;
		num = ((num < _slideCollection.slides.Length) ? num : 0);
		return _slideCollection.IsStreamedSlideIndexLoaded(num);
	}

	public bool PrevSlideAvailable()
	{
		if (!streamingTexturesAvailable)
		{
			return true;
		}
		int num = slideIndex - 1;
		num = ((num < 0) ? (_slideCollection.slides.Length - 1) : num);
		return _slideCollection.IsStreamedSlideIndexLoaded(num);
	}

	public void SetChangeSlidesAllowed(bool allowed)
	{
		_changeSlidesAllowed = allowed;
	}

	public void ForceCurrentSlideDisplayEvent(bool forward)
	{
		GetCurrentSlide()?.Display(this, forward);
	}

	public void ResetSlideIndex()
	{
		if (_owningItem != null)
		{
			_owningItem.RotateToAngle(0f);
		}
		slideIndex = 0;
		GetCurrentSlide().SetOwner(this);
	}

	public bool IncreaseSlideIndex()
	{
		if (!_changeSlidesAllowed)
		{
			return false;
		}
		slideIndex++;
		if (slideIndex == 0)
		{
			onEndOfSlides.Invoke();
		}
		return true;
	}

	public bool DecreaseSlideIndex()
	{
		if (!_changeSlidesAllowed)
		{
			return false;
		}
		slideIndex--;
		return true;
	}

	private void BuildMusicRangesIndex()
	{
		_musicRanges = new List<SlideMusicRange>();
		SlideMusicRange slideMusicRange = default(SlideMusicRange);
		slideMusicRange.audioType = AudioType.None;
		slideMusicRange.fadeInTime = 1f;
		slideMusicRange.start = 0;
		SlideMusicRange item = slideMusicRange;
		for (int i = 0; i < _slideCollection.slides.Length; i++)
		{
			Slide slide = _slideCollection.slides[i];
			if (slide.HasModule(typeof(SlideBackdropAudioModule)))
			{
				if (item.audioType != 0)
				{
					item.end = i - 1;
					_musicRanges.Add(item);
				}
				SlideBackdropAudioModule module = slide.GetModule<SlideBackdropAudioModule>();
				slideMusicRange = default(SlideMusicRange);
				slideMusicRange.audioType = module._audioType;
				slideMusicRange.fadeInTime = module._fadeTime;
				slideMusicRange.start = i;
				item = slideMusicRange;
			}
		}
		if (item.audioType != 0)
		{
			item.end = _slideCollection.slides.Length - 1;
			_musicRanges.Add(item);
		}
	}

	public void TryPlayMusicForCurrentSlideTransition(bool forward)
	{
		if (_currentSlideIndex == 0)
		{
			Locator.GetSlideReelMusicManager().OnTransitionToFirstSlide();
		}
		for (int i = 0; i < _musicRanges.Count; i++)
		{
			if ((forward ? _musicRanges[i].start : _musicRanges[i].end) == _currentSlideIndex)
			{
				Locator.GetSlideReelMusicManager().PlayBackdrop(_musicRanges[i].audioType, _musicRanges[i].fadeInTime);
			}
		}
	}

	public void TryPlayMusicForCurrentSlideInclusive()
	{
		for (int i = 0; i < _musicRanges.Count; i++)
		{
			if (_musicRanges[i].start <= _currentSlideIndex && _musicRanges[i].end >= _currentSlideIndex)
			{
				Locator.GetSlideReelMusicManager().PlayBackdrop(_musicRanges[i].audioType, _musicRanges[i].fadeInTime);
			}
		}
	}

	private void SetupReadFlags()
	{
		_unreadSlideIndices.Clear();
		for (int i = 0; i < _slideCollection.slides.Length; i++)
		{
			_unreadSlideIndices.Add(i);
		}
	}

	private void RegisterPerSlideCompletion()
	{
		if (_shipLogPerSlideCompletion == null)
		{
			_shipLogPerSlideCompletion = new Dictionary<string, List<ItemSlidePair>>();
		}
		int num = 0;
		Slide[] slides = _slideCollection.slides;
		for (int i = 0; i < slides.Length; i++)
		{
			SlideShipLogEntryModule module = slides[i].GetModule<SlideShipLogEntryModule>();
			if (module != null)
			{
				string[] array = module._entryKey.Split(',');
				foreach (string key in array)
				{
					if (!_shipLogPerSlideCompletion.ContainsKey(key))
					{
						_shipLogPerSlideCompletion.Add(key, new List<ItemSlidePair>());
					}
					_shipLogPerSlideCompletion[key].Add(new ItemSlidePair
					{
						item = this,
						slideIdx = num
					});
				}
			}
			num++;
		}
	}

	public void SetCurrentRead()
	{
		SetReadFlag(slideIndex);
	}

	private void SetReadFlag(int index)
	{
		Mathf.FloorToInt((float)index / 64f);
		_ = index % 64;
		_unreadSlideIndices.Remove(index);
		SlideShipLogEntryModule module = GetCurrentSlide().GetModule<SlideShipLogEntryModule>();
		string[] array;
		if (module != null)
		{
			array = module._entryKey.Split(',');
			foreach (string key in array)
			{
				CheckSlidesCompletionForLog(key);
			}
		}
		if (string.IsNullOrEmpty(_shipLogOnComplete))
		{
			return;
		}
		array = _shipLogOnComplete.Split(',');
		foreach (string id in array)
		{
			if (!Locator.GetShipLogManager().IsFactRevealed(id) && _unreadSlideIndices.Count == 0)
			{
				Locator.GetShipLogManager().RevealFact(id);
			}
		}
	}

	public bool IsSlideRead(int slideIdx)
	{
		return !_unreadSlideIndices.Contains(slideIdx);
	}

	private void CheckSlidesCompletionForLog(string key)
	{
		if (Locator.GetShipLogManager().IsFactRevealed(key) || !_shipLogPerSlideCompletion.ContainsKey(key))
		{
			return;
		}
		List<ItemSlidePair> list = _shipLogPerSlideCompletion[key];
		bool flag = true;
		foreach (ItemSlidePair item in list)
		{
			if (!item.item.IsSlideRead(item.slideIdx))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Locator.GetShipLogManager().RevealFact(key);
		}
	}

	private void SetupStreaming()
	{
		StreamingManager.ConvertTextureAssetBundleToIterable(_slideCollection.streamingAssetIdentifier);
	}

	public bool IsAutoLoadStreamingSlides()
	{
		return _autoLoadStreaming;
	}

	public bool IsSlideStreamingTextureAvailable(int slideIdx)
	{
		return _slideCollection.IsStreamedSlideIndexLoaded(slideIdx);
	}

	public bool IsStreamingTextureIDAvailable(int streamIdx)
	{
		return _slideCollection.IsStreamedTextureIndexLoaded(streamIdx);
	}

	public void RequestManualStreamSlides()
	{
		int num = 5;
		if (num <= 1)
		{
			_lastManualStreamRequest = new int[1] { slideIndex };
			_slideCollection.RequestStreamSlides(slideIndex);
			return;
		}
		int num2 = num;
		num2 = ((num2 % 2 > 0) ? (num2 - 1) : num2);
		num2 /= 2;
		int num3 = _currentSlideIndex - num2;
		int num4 = _currentSlideIndex + num2;
		num3 = ((num % 2 == 0) ? (num3 + 1) : num3);
		int[] array = new int[num];
		int num5 = 0;
		for (int i = num3; i <= num4; i++)
		{
			int num6 = ((i < 0) ? (slideCount + i) : i);
			num6 = ((num6 >= slideCount) ? (num6 - slideCount) : num6);
			array[num5] = num6;
			num5++;
		}
		if (_lastManualStreamRequest != null)
		{
			List<int> list = new List<int>();
			for (int j = 0; j < _lastManualStreamRequest.Length; j++)
			{
				bool flag = false;
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k] == _lastManualStreamRequest[j])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(_lastManualStreamRequest[j]);
				}
			}
			if (list.Count > 0)
			{
				_slideCollection.RequestRelease(list.ToArray());
			}
		}
		_lastManualStreamRequest = array;
		_slideCollection.RequestStreamSlides(array);
	}

	public void LoadStreamingTextures()
	{
		StreamingManager.RegisterStreamingTextureSubscriber(_slideCollection.streamingAssetIdentifier, this);
		if (!_autoLoadStreaming)
		{
			RequestManualStreamSlides();
		}
		StreamingManager.LoadStreamingAssets(_slideCollection.streamingAssetIdentifier);
	}

	public void UnloadStreamingTextures()
	{
		StreamingManager.UnregisterStreamingTextureSubscriber(_slideCollection.streamingAssetIdentifier, this);
		if (_slideCollection.GetAssetBundle() == null || _slideCollection.GetAssetBundle().subscriberCount <= 0)
		{
			StreamingManager.UnloadStreamingAssets(_slideCollection.streamingAssetIdentifier);
		}
	}

	public void OnTexturesLoaded(StreamingTextureAssetBundle textureAssetBundle)
	{
	}

	public void OnTexturesUnloaded()
	{
	}

	public void OnTextureLoaded(int index, Texture texture)
	{
		if (index == GetCurrentSlide().GetStreamingIndex())
		{
			GetCurrentSlide().InvokeTextureUpdate();
		}
	}

	public void OnTextureUnloaded(int index)
	{
	}

	public void OnBeginSubscription(StreamingIteratedTextureAssetBundle streamingAssetBundle)
	{
		_slideCollection.SetAssetBundle(streamingAssetBundle);
		streamingAssetBundle.SetAutoLoad(_autoLoadStreaming);
	}

	public void OnAssetBundleBeginLoad(StreamingIteratedTextureAssetBundle streamingAssetBundle)
	{
	}

	public Slide GetSlideAt(int index)
	{
		return _slideCollection.slides[index];
	}

	public void RotateToSection(int streamingIndex)
	{
		if (_owningItem != null)
		{
			_owningItem.RotateToSection(streamingIndex);
		}
	}

	public void RotateToPrevSection(int streamingIndex)
	{
		if (_owningItem != null)
		{
			_owningItem.RotateToPrevSection(streamingIndex);
		}
	}

	public void AddSlide(Slide slide)
	{
		if (_slideCollection == null)
		{
			_slideCollection = new SlideCollection(0);
		}
		Array.Resize(ref _slideCollection.slides, _slideCollection.slides.Length + 1);
		_slideCollection.slides[_slideCollection.slides.Length - 1] = slide;
	}

	public void SetTextureAt(int idx, Texture2D tex)
	{
		if (_slideCollection.slides != null && idx >= 0 && idx < _slideCollection.slides.Length)
		{
			_slideCollection.slides[idx]._image = tex;
		}
	}

	public void ClearSlides()
	{
		_slideCollection.slides = new Slide[0];
	}
}
