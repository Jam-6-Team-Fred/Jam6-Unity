using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationDisplayTextLayout : NotificationDisplay
{
	[SerializeField]
	protected GameObject _textDisplayTemplate;

	[SerializeField]
	protected GameObject _backgroundImage;

	[SerializeField]
	protected RectTransform _displaySpace;

	protected Transform _textDisplayRoot;

	protected TextAnchor _textAnchor = TextAnchor.MiddleCenter;

	protected List<GameObject> _textItemPool;

	protected const int c_textItemPoolInitialCount = 16;

	protected override void Awake()
	{
		base.Awake();
		_textDisplayTemplate.SetActive(value: false);
		_textDisplayRoot = _textDisplayTemplate.transform.parent;
		_textItemPool = new List<GameObject>();
		for (int i = 0; i < 16; i++)
		{
			GameObject item = Object.Instantiate(_textDisplayTemplate, _textDisplayRoot);
			_textItemPool.Add(item);
		}
	}

	protected override void DetermineNumberOfAvailableLines()
	{
		LayoutElement component = _textDisplayTemplate.GetComponent<LayoutElement>();
		if (component != null && component.minHeight > 0f)
		{
			_numAvailableLines = (int)Mathf.Floor(_displaySpace.rect.height / component.minHeight);
		}
		else
		{
			_numAvailableLines = (int)Mathf.Floor(_displaySpace.rect.height / _textDisplayTemplate.GetRequiredComponent<RectTransform>().rect.height);
		}
	}

	protected override void Update()
	{
		ClearFlaggedDisplayItems();
		int num = 0;
		bool isAudioPlaying = _isAudioPlaying;
		_isAudioPlaying = false;
		for (int i = 0; i < _listDisplayData.Count; i++)
		{
			NotificationDisplayData notificationDisplayData = _listDisplayData[i];
			if (notificationDisplayData.GetNumberOfLines() + num > _numAvailableLines)
			{
				break;
			}
			if (notificationDisplayData.HoldDisplayTimer)
			{
				notificationDisplayData.HoldDisplayTimer = false;
			}
			if (notificationDisplayData.TextScrollEffect != null)
			{
				if (!notificationDisplayData.TextScrollEffect.IsTextEffectRunning() && !notificationDisplayData.TextScrollEffect.IsTextEffectComplete())
				{
					notificationDisplayData.TextScrollEffect.StartTextEffect();
				}
				if (notificationDisplayData.TextScrollEffect != null && notificationDisplayData.TextScrollEffect.IsTextEffectRunning())
				{
					_isAudioPlaying = true;
				}
			}
			notificationDisplayData.IncrementTimeDisplayed(Time.unscaledDeltaTime);
		}
		if (_isAudioPlaying && !isAudioPlaying)
		{
			PlayNotificationAudio();
		}
		else if (!_isAudioPlaying && isAudioPlaying)
		{
			StopNotificationAudio();
		}
		if (_listDisplayData.Count == 0)
		{
			base.enabled = false;
			if (_displayCanvas != null)
			{
				_displayCanvas.enabled = false;
			}
		}
		else if (_backgroundImage != null)
		{
			_backgroundImage.gameObject.SetActive(PlayerData.IsUILargeTextSize());
		}
	}

	public override bool AreNotificationsVisible(NotificationTarget target)
	{
		if (target == NotificationTarget.All || (target & _notificationTargetType) == target)
		{
			return _listDisplayData.Count != 0;
		}
		return false;
	}

	public override void PushNotification(NotificationData data)
	{
		NotificationTarget notificationTgt = data.notificationTgt;
		if (notificationTgt != 0 && (notificationTgt & _notificationTargetType) != _notificationTargetType)
		{
			return;
		}
		int num = FindDuplicateIndex(data);
		if (num == -1)
		{
			NotificationDisplayData notificationDisplayData = new NotificationDisplayData(data);
			_listDisplayData.Add(notificationDisplayData);
			AddDisplayItem(notificationDisplayData);
			base.enabled = true;
			if (_displayCanvas != null)
			{
				_displayCanvas.enabled = true;
			}
		}
		else
		{
			NotificationDisplayData notificationDisplayData = _listDisplayData[num];
			notificationDisplayData.ResetDisplay();
			notificationDisplayData.Data = data;
			notificationDisplayData.TextScrollEffect.Init(notificationDisplayData.Data.displayMessage, _textAnchor);
		}
	}

	protected virtual void AddDisplayItem(NotificationDisplayData ndd)
	{
		for (int i = 0; i < _textItemPool.Count; i++)
		{
			if (!_textItemPool[i].activeSelf)
			{
				GameObject gameObject = _textItemPool[i];
				gameObject.SetActive(value: true);
				gameObject.transform.SetAsFirstSibling();
				if (_backgroundImage != null)
				{
					_backgroundImage.transform.SetAsFirstSibling();
				}
				ndd.RootObject = gameObject;
				ndd.TextDisplay = gameObject.GetRequiredComponentInChildren<Text>();
				ndd.TextScrollEffect = gameObject.GetRequiredComponentInChildren<TypeEffectText>();
				ndd.TextScrollEffect.Init(ndd.Data.displayMessage, _textAnchor);
				return;
			}
		}
		ExpandPool();
		for (int j = 0; j < _textItemPool.Count; j++)
		{
			if (!_textItemPool[j].activeSelf)
			{
				GameObject gameObject2 = _textItemPool[j];
				gameObject2.transform.SetAsFirstSibling();
				gameObject2.SetActive(value: true);
				ndd.RootObject = gameObject2;
				ndd.TextDisplay = gameObject2.GetRequiredComponentInChildren<Text>();
				ndd.TextScrollEffect = gameObject2.GetRequiredComponentInChildren<TypeEffectText>();
				ndd.TextScrollEffect.Init(ndd.Data.displayMessage, _textAnchor);
				break;
			}
		}
	}

	protected virtual void ClearFlaggedDisplayItems()
	{
		for (int num = _listDisplayData.Count - 1; num >= 0; num--)
		{
			if (_listDisplayData[num].DeletionFlag)
			{
				if (_listDisplayData[num].TextDisplay != null)
				{
					_listDisplayData[num].TextDisplay.text = "";
					_listDisplayData[num].RootObject.SetActive(value: false);
				}
				_listDisplayData.RemoveAt(num);
			}
		}
	}

	protected virtual void ExpandPool()
	{
		int num = _textItemPool.Count * 2;
		while (_textItemPool.Count < num)
		{
			GameObject gameObject = Object.Instantiate(_textDisplayTemplate, _textDisplayRoot);
			_textItemPool.Add(gameObject);
			gameObject.SetActive(value: false);
		}
	}

	public virtual void BringNotificationToTop(NotificationData data)
	{
		NotificationDisplayData notificationDisplayData = null;
		bool flag = false;
		for (int i = 0; i < _listDisplayData.Count; i++)
		{
			notificationDisplayData = _listDisplayData[i];
			if (notificationDisplayData.Data == data)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < _textItemPool.Count; j++)
		{
			GameObject gameObject = _textItemPool[j];
			Text requiredComponentInChildren = gameObject.GetRequiredComponentInChildren<Text>();
			if (_textItemPool[j].activeSelf && requiredComponentInChildren == notificationDisplayData.TextDisplay)
			{
				gameObject.transform.SetAsFirstSibling();
			}
		}
	}

	protected override void PlayNotificationAudio()
	{
	}

	protected override void StopNotificationAudio()
	{
	}
}
