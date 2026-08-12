using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipLogSlideProjector : MonoBehaviour, IStreamingTexturesSubscriber
{
	[SerializeField]
	private Image _slideImage;

	[SerializeField]
	private CanvasGroupAnimator _animator;

	[SerializeField]
	private GameObject _promptRoot;

	[SerializeField]
	private ScreenPromptList _promptList;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private Text _reelCounter;

	private List<SlideCollection> _slideCollections;

	private ShipLogEntry _lastDisplayedEntry;

	private int _collectionIndex = -1;

	private float _defaultSlideDuration;

	private float _nextSlidePlayTime;

	private bool _visible;

	private bool _closing;

	private bool _slideDirty;

	private int _slideIndex;

	private RectTransform _rectTransform;

	private float _enterModeTime;

	private float _targetXPos;

	private ScreenPrompt _nextReelPrompt;

	private void Awake()
	{
		_slideCollections = new List<SlideCollection>();
		_animator = GetComponent<CanvasGroupAnimator>();
		_rectTransform = GetComponent<RectTransform>();
		GlobalMessenger.AddListener("DeathSequenceComplete", OnDeathSequenceComplete);
	}

	private void Start()
	{
		_reelCounter.text = string.Empty;
		_promptList.Init();
		_promptList.SetMinElementDimensionsAndFontSize(30f, 0f, 16);
		_nextReelPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.LogNextReelPrompt));
		Locator.GetPromptManager().AddScreenPrompt(_nextReelPrompt, _promptList, TextAnchor.MiddleCenter, -1, makeVisible: true);
		_animator.SetImmediate(0f, new Vector3(1f, 0f, 1f));
		base.gameObject.SetActive(value: false);
		_promptRoot.SetActive(value: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		UnloadCurrentStreamingTextures();
		GlobalMessenger.RemoveListener("DeathSequenceComplete", OnDeathSequenceComplete);
	}

	private void OnDeathSequenceComplete()
	{
		UnloadCurrentStreamingTextures();
	}

	private void OnEnable()
	{
		Canvas.willRenderCanvases += OnWillRenderCanvases;
	}

	private void OnDisable()
	{
		Canvas.willRenderCanvases -= OnWillRenderCanvases;
	}

	public void OnEnterMapMode()
	{
		_targetXPos = -371.3f;
		_enterModeTime = Time.unscaledTime;
		if (!_visible)
		{
			Vector2 anchoredPosition = _rectTransform.anchoredPosition;
			anchoredPosition.x = _targetXPos;
			_rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public void OnEnterDetectiveMode()
	{
		_targetXPos = -675f;
		_enterModeTime = Time.unscaledTime;
		if (!_visible)
		{
			Vector2 anchoredPosition = _rectTransform.anchoredPosition;
			anchoredPosition.x = _targetXPos;
			_rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public void CheckFactsForSlides(List<ShipLogFact> factList, ShipLogEntry entry = null)
	{
		if (entry != null && _lastDisplayedEntry != null && _lastDisplayedEntry.GetID() == entry.GetID())
		{
			return;
		}
		_lastDisplayedEntry = entry;
		UnloadCurrentStreamingTextures();
		_slideCollections.Clear();
		_collectionIndex = -1;
		_slideImage.enabled = false;
		for (int i = 0; i < factList.Count; i++)
		{
			if (factList[i].GetSlideCollection() != null)
			{
				_slideCollections.Add(factList[i].GetSlideCollection());
			}
		}
		_promptRoot.SetActive(_slideCollections.Count > 1);
		if (_visible)
		{
			if (_slideCollections.Count > 0)
			{
				PlaySlideCollection(0);
			}
			else
			{
				AttemptSetVisible(visible: false);
			}
		}
	}

	public void AttemptSetVisible(bool visible)
	{
		if (visible == _visible)
		{
			return;
		}
		if (visible && _slideCollections.Count > 0)
		{
			_visible = true;
			_closing = false;
			base.enabled = true;
			base.gameObject.SetActive(value: true);
			_animator.AnimateTo(1f, Vector3.one, 0.2f);
			_promptRoot.SetActive(_slideCollections.Count > 1);
			if (!HasActiveSlideCollection())
			{
				PlaySlideCollection(0);
			}
		}
		else if (!visible)
		{
			_visible = false;
			_closing = true;
			_animator.AnimateTo(0f, new Vector3(1f, 0f, 1f), 0.1f);
		}
	}

	private bool HasActiveSlideCollection()
	{
		if (_collectionIndex >= 0)
		{
			return _collectionIndex < _slideCollections.Count;
		}
		return false;
	}

	private void PlaySlideCollection(int index, float defaultSlideDuration = 0.5f)
	{
		UnloadCurrentStreamingTextures();
		_collectionIndex = index;
		SlideCollection slideCollection = _slideCollections[_collectionIndex];
		_defaultSlideDuration = defaultSlideDuration;
		_slideIndex = 0;
		_nextSlidePlayTime = Time.unscaledTime + GetCurrentSlidePlayDuration() + 0.2f;
		_slideDirty = true;
		bool flag = slideCollection.streamingAssetIdentifier.Equals("invisibleplanet/textures/towervision", StringComparison.OrdinalIgnoreCase);
		_slideImage.material.SetFloat("_Exposure", flag ? 2f : 1f);
		_slideImage.material.SetFloat("_InvertColor", slideCollection.isVision ? 0f : 1f);
		_slideImage.material.SetFloat("_ApplyVignette", slideCollection.isVision ? 1f : 0f);
		_reelCounter.text = ((_slideCollections.Count > 1) ? (index + 1 + "/" + _slideCollections.Count) : string.Empty);
		if (CheckStreamingTexturesAvailable())
		{
			MonoBehaviour.print("Ship log slide streaming initialized");
			StreamingManager.ConvertTextureAssetBundleToIterable(slideCollection.streamingAssetIdentifier);
			StreamingManager.RegisterStreamingTextureSubscriber(slideCollection.streamingAssetIdentifier, this);
			StreamingManager.LoadStreamingAssets(slideCollection.streamingAssetIdentifier);
			for (int i = 0; i < 3; i++)
			{
				slideCollection.RequestStreamSlides(i);
			}
		}
	}

	private void Update()
	{
		if (HasActiveSlideCollection())
		{
			UpdateSlidePlayback(_slideCollections[_collectionIndex]);
		}
		if (_slideCollections.Count > 1)
		{
			if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
			{
				int num = _collectionIndex + 1;
				if (num > _slideCollections.Count - 1)
				{
					num = 0;
				}
				_promptRoot.SetActive(value: false);
				_oneShotSource.PlayOneShot(AudioType.ShipLogMoveBetweenEntries);
				PlaySlideCollection(num);
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary))
			{
				int num2 = _collectionIndex - 1;
				if (num2 < 0)
				{
					num2 = _slideCollections.Count - 1;
				}
				_promptRoot.SetActive(value: false);
				_oneShotSource.PlayOneShot(AudioType.ShipLogMoveBetweenEntries);
				PlaySlideCollection(num2);
			}
		}
		if (_closing && _animator.IsComplete())
		{
			UnloadCurrentStreamingTextures();
			_collectionIndex = -1;
			base.gameObject.SetActive(value: false);
			base.enabled = false;
		}
	}

	private void OnWillRenderCanvases()
	{
		float t = (Time.unscaledTime - _enterModeTime) / 1f;
		Vector2 anchoredPosition = _rectTransform.anchoredPosition;
		anchoredPosition.x = Mathf.Lerp(anchoredPosition.x, _targetXPos, t);
		_rectTransform.anchoredPosition = anchoredPosition;
	}

	private void UpdateSlidePlayback(SlideCollection collection)
	{
		bool flag = CheckStreamingTexturesAvailable();
		if (Time.unscaledTime >= _nextSlidePlayTime)
		{
			_slideIndex++;
			if (_slideIndex > collection.slides.Length - 1)
			{
				_slideIndex = 0;
			}
			_slideDirty = true;
			_nextSlidePlayTime = Time.unscaledTime + GetCurrentSlidePlayDuration();
			if (flag)
			{
				int num = _slideIndex + 2;
				if (num > collection.slides.Length - 1)
				{
					num -= collection.slides.Length;
				}
				collection.RequestStreamSlides(num);
				int num2 = _slideIndex - 2;
				if (num2 < 0)
				{
					num2 += collection.slides.Length;
				}
				collection.RequestRelease(num2);
			}
		}
		if (!_slideDirty)
		{
			return;
		}
		Texture texture = null;
		if (flag)
		{
			int streamingIndex = collection[_slideIndex].GetStreamingIndex();
			if (collection.IsStreamedTextureIndexLoaded(streamingIndex))
			{
				texture = collection.GetStreamingTexture(streamingIndex) as Texture2D;
			}
		}
		else
		{
			texture = collection[_slideIndex]._image;
		}
		if (texture != null)
		{
			_slideImage.enabled = false;
			_slideImage.material.SetTexture("_SlideTex", texture);
			_slideImage.enabled = true;
			_slideDirty = false;
		}
	}

	private float GetCurrentSlidePlayDuration()
	{
		if (_slideIndex == 0)
		{
			return _defaultSlideDuration * 2f;
		}
		SlidePlayTimeModule module = _slideCollections[_collectionIndex][_slideIndex].GetModule<SlidePlayTimeModule>();
		if (module != null)
		{
			if (module._duration >= 1.5f)
			{
				return _defaultSlideDuration * 3f;
			}
			return _defaultSlideDuration * 2f;
		}
		return _defaultSlideDuration;
	}

	private bool CheckStreamingTexturesAvailable()
	{
		if (!HasActiveSlideCollection() || !StreamingManager.isStreamingEnabled || string.IsNullOrEmpty(_slideCollections[_collectionIndex].streamingAssetIdentifier))
		{
			return false;
		}
		return StreamingManager.StreamingAssetAvailable(_slideCollections[_collectionIndex].streamingAssetIdentifier);
	}

	private void UnloadCurrentStreamingTextures()
	{
		if (CheckStreamingTexturesAvailable())
		{
			StreamingManager.UnregisterStreamingTextureSubscriber(_slideCollections[_collectionIndex].streamingAssetIdentifier, this);
			if (_slideCollections[_collectionIndex].GetAssetBundle() == null || _slideCollections[_collectionIndex].GetAssetBundle().subscriberCount <= 0)
			{
				StreamingManager.UnloadStreamingAssets(_slideCollections[_collectionIndex].streamingAssetIdentifier);
			}
		}
	}

	public void OnTexturesLoaded(StreamingTextureAssetBundle streamingTextureAssetBundle)
	{
	}

	public void OnTexturesUnloaded()
	{
	}

	public void OnBeginSubscription(StreamingIteratedTextureAssetBundle streamingAssetBundle)
	{
		_slideCollections[_collectionIndex].SetAssetBundle(streamingAssetBundle);
		streamingAssetBundle.SetAutoLoad(auto: false);
	}

	public void OnAssetBundleBeginLoad(StreamingIteratedTextureAssetBundle streamingTextureAssetBundle)
	{
	}

	public void OnTextureLoaded(int index, Texture texture)
	{
	}

	public void OnTextureUnloaded(int index)
	{
	}
}
