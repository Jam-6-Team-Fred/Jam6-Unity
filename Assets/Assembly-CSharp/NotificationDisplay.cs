using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class NotificationDisplay : MonoBehaviour, INotifiable
{
	protected class NotificationDisplayData
	{
		public NotificationData Data;

		public string TextToDisplay;

		public bool DeletionFlag;

		public bool HoldDisplayTimer;

		public float FractionDisplayed;

		public Text TextDisplay;

		public TypeEffectText TextScrollEffect;

		private float _timeFullyVisible;

		public GameObject RootObject;

		public NotificationDisplayData(NotificationData data, GameObject rootObject = null)
		{
			Data = data;
			TextToDisplay = string.Empty;
			DeletionFlag = false;
			HoldDisplayTimer = true;
			RootObject = rootObject;
			_timeFullyVisible = 0f;
		}

		public void IncrementTimeDisplayed(float t)
		{
			if (!HoldDisplayTimer && (!(TextScrollEffect != null) || TextScrollEffect.IsTextEffectComplete()))
			{
				_timeFullyVisible += t;
				if (_timeFullyVisible > Data.minDuration && !NotificationManager.SharedInstance.IsPinnedNotification(Data))
				{
					DeletionFlag = true;
				}
			}
		}

		public void ResetDisplay()
		{
			TextToDisplay = string.Empty;
			DeletionFlag = false;
			HoldDisplayTimer = true;
			_timeFullyVisible = 0f;
			TextScrollEffect.StopAndClearText();
		}

		public int GetNumberOfLines()
		{
			return Data.displayMessage.Split('\n').Length;
		}

		public float GetTimeFullyVisible()
		{
			return _timeFullyVisible;
		}
	}

	[SerializeField]
	protected Text _displayText;

	[SerializeField]
	protected Canvas _displayCanvas;

	protected NotificationTarget _notificationTargetType;

	protected List<NotificationDisplayData> _listDisplayData;

	protected int _numAvailableLines;

	protected bool _isAudioPlaying;

	protected virtual void Awake()
	{
		if (_displayCanvas != null)
		{
			_displayCanvas.enabled = false;
		}
	}

	protected virtual void Start()
	{
		_listDisplayData = new List<NotificationDisplayData>();
		if (_displayText != null)
		{
			_displayText.text = string.Empty;
		}
		DetermineNumberOfAvailableLines();
		NotificationManager.SharedInstance.RegisterNotifiable(this);
		base.enabled = false;
	}

	private void OnDisable()
	{
		StopNotificationAudio();
	}

	protected virtual void OnDestroy()
	{
		NotificationManager.SharedInstance.UnregisterNotifiable(this);
	}

	protected virtual void DetermineNumberOfAvailableLines()
	{
		int fontSize = _displayText.fontSize;
		float height = _displayText.rectTransform.rect.height;
		_numAvailableLines = (int)Mathf.Floor(height / (float)fontSize);
	}

	public virtual void PushNotification(NotificationData data)
	{
		NotificationTarget notificationTgt = data.notificationTgt;
		if (notificationTgt != 0 && (notificationTgt & _notificationTargetType) != _notificationTargetType)
		{
			return;
		}
		int num = FindDuplicateIndex(data);
		if (num == -1)
		{
			NotificationDisplayData item = new NotificationDisplayData(data);
			_listDisplayData.Add(item);
			base.enabled = true;
			if (_displayCanvas != null)
			{
				_displayCanvas.enabled = true;
			}
		}
		else
		{
			NotificationDisplayData item = _listDisplayData[num];
			item.Data = data;
			item.ResetDisplay();
		}
	}

	public virtual void RemoveNotification(NotificationData data)
	{
		NotificationTarget notificationTgt = data.notificationTgt;
		if (notificationTgt != 0 && (notificationTgt & _notificationTargetType) != _notificationTargetType)
		{
			return;
		}
		for (int i = 0; i < _listDisplayData.Count; i++)
		{
			if (_listDisplayData[i].Data == data)
			{
				_listDisplayData[i].DeletionFlag = true;
				base.enabled = true;
				break;
			}
		}
	}

	protected virtual int FindDuplicateIndex(NotificationData data)
	{
		for (int i = 0; i < _listDisplayData.Count; i++)
		{
			if (_listDisplayData[i].Data.Equals(data))
			{
				return i;
			}
		}
		return -1;
	}

	public virtual bool AreNotificationsVisible(NotificationTarget target)
	{
		if (target == NotificationTarget.All || (target & _notificationTargetType) == target)
		{
			return _displayText.text != string.Empty;
		}
		return false;
	}

	protected abstract void PlayNotificationAudio();

	protected abstract void StopNotificationAudio();

	protected virtual void Update()
	{
		for (int num = _listDisplayData.Count - 1; num >= 0; num--)
		{
			if (_listDisplayData[num].DeletionFlag)
			{
				_listDisplayData.RemoveAt(num);
			}
		}
		string text = string.Empty;
		int num2 = 0;
		bool isAudioPlaying = _isAudioPlaying;
		_isAudioPlaying = false;
		for (int i = 0; i < _listDisplayData.Count; i++)
		{
			NotificationDisplayData notificationDisplayData = _listDisplayData[i];
			if (notificationDisplayData.GetNumberOfLines() + num2 > _numAvailableLines)
			{
				break;
			}
			if (notificationDisplayData.HoldDisplayTimer)
			{
				notificationDisplayData.HoldDisplayTimer = false;
			}
			text = ((!(text == string.Empty)) ? (text + "\n" + notificationDisplayData.TextToDisplay) : notificationDisplayData.TextToDisplay);
			if (notificationDisplayData.TextScrollEffect != null && notificationDisplayData.TextScrollEffect.IsTextEffectRunning())
			{
				_isAudioPlaying = true;
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
		if (_displayText.text != text)
		{
			_displayText.text = text;
		}
		if (_displayText.text == string.Empty && _listDisplayData.Count == 0)
		{
			base.enabled = false;
			if (_displayCanvas != null)
			{
				_displayCanvas.enabled = false;
			}
		}
	}
}
