using UnityEngine;

public class PlayerCockpitNotificationDisplay : NotificationDisplayTextLayout
{
	protected override void Awake()
	{
		base.Awake();
		_notificationTargetType = NotificationTarget.Ship;
	}

	public override void PushNotification(NotificationData data)
	{
		NotificationTarget notificationTgt = data.notificationTgt;
		if (notificationTgt == NotificationTarget.All || (notificationTgt & _notificationTargetType) == _notificationTargetType)
		{
			int num = FindDuplicateIndex(data);
			if (num == -1)
			{
				NotificationDisplayData notificationDisplayData = new NotificationDisplayData(data);
				_listDisplayData.Add(notificationDisplayData);
				AddDisplayItem(notificationDisplayData);
				base.enabled = true;
			}
			else
			{
				NotificationDisplayData notificationDisplayData = _listDisplayData[num];
				notificationDisplayData.ResetDisplay();
				notificationDisplayData.Data = data;
				notificationDisplayData.TextScrollEffect.Init(notificationDisplayData.Data.displayMessage, _textAnchor);
			}
		}
	}

	protected override void Update()
	{
		ClearFlaggedDisplayItems();
		if (PlayerState.AtFlightConsole() && PlayerData.IsUILargeTextSize())
		{
			_displayCanvas.enabled = true;
		}
		else
		{
			_displayCanvas.enabled = false;
		}
		int num = 0;
		_ = _isAudioPlaying;
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
			_backgroundImage.gameObject.SetActive(PlayerData.IsUILargeTextSize() && PlayerState.AtFlightConsole());
		}
	}
}
