using UnityEngine;

public class SuitNotificationDisplay : NotificationDisplayTextLayout
{
	private PlayerAudioController _playerAudioController;

	protected override void Awake()
	{
		base.Awake();
		_notificationTargetType = NotificationTarget.Player;
	}

	protected override void Start()
	{
		base.Start();
		_playerAudioController = Locator.GetPlayerAudioController();
	}

	protected override void ExpandPool()
	{
		int num = _textItemPool.Count * 2;
		while (_textItemPool.Count < num)
		{
			GameObject gameObject = Object.Instantiate(_textDisplayTemplate, _textDisplayRoot);
			_textItemPool.Add(gameObject);
			gameObject.SetActive(value: false);
		}
	}

	public override void PushNotification(NotificationData data)
	{
		if (Locator.GetPlayerSuit().IsWearingHelmet())
		{
			base.PushNotification(data);
		}
	}

	protected override void PlayNotificationAudio()
	{
		if (Locator.GetPlayerSuit().IsWearingHelmet())
		{
			_playerAudioController.PlayNotificationTextScrolling();
		}
	}

	protected override void StopNotificationAudio()
	{
		if (_playerAudioController != null)
		{
			_playerAudioController.StopNotificationTextScrolling();
		}
	}
}
