using UnityEngine;

public class NotificationDetector : Detector
{
	private NotificationData _darkMatterData;

	protected override void Awake()
	{
		base.Awake();
		_darkMatterData = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationGhostMatter), 0f);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnVolumeAdded(EffectVolume effectVol)
	{
		NotificationVolume notificationVolume = effectVol as NotificationVolume;
		if (notificationVolume.GetNotificationType() == NotificationVolume.Type.None)
		{
			Debug.LogWarning("Notification Volume has no type.", notificationVolume);
		}
		else if (GetVolumeCountByType(notificationVolume.GetNotificationType()) == 1)
		{
			NotificationVolume.Type notificationType = notificationVolume.GetNotificationType();
			if (notificationType == NotificationVolume.Type.DarkMatter)
			{
				NotificationManager.SharedInstance.PostNotification(_darkMatterData, pin: true);
			}
		}
	}

	protected override void OnVolumeRemoved(EffectVolume effectVol)
	{
		NotificationVolume notificationVolume = effectVol as NotificationVolume;
		if (notificationVolume.GetNotificationType() == NotificationVolume.Type.None)
		{
			Debug.LogWarning("Notification Volume has no type.", notificationVolume);
		}
		else if (GetVolumeCountByType(notificationVolume.GetNotificationType()) == 0)
		{
			NotificationVolume.Type notificationType = notificationVolume.GetNotificationType();
			if (notificationType == NotificationVolume.Type.DarkMatter)
			{
				NotificationManager.SharedInstance.UnpinNotification(_darkMatterData);
			}
		}
	}

	private int GetVolumeCountByType(NotificationVolume.Type type)
	{
		int num = 0;
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			if (((NotificationVolume)_activeVolumes[i]).GetNotificationType() == type)
			{
				num++;
			}
		}
		return num;
	}
}
