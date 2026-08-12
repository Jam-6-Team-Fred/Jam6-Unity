using System.Collections.Generic;
using UnityEngine;

public class NotificationManager
{
	private static NotificationManager s_instance;

	private List<INotifiable> _notifiableElements;

	private List<NotificationData> _pinnedNotifications;

	public static NotificationManager SharedInstance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new NotificationManager();
				s_instance.Initialize();
			}
			return s_instance;
		}
	}

	private void Initialize()
	{
		_notifiableElements = new List<INotifiable>();
		_pinnedNotifications = new List<NotificationData>();
		GlobalMessenger.AddListener("LoadFromMenu", ClearAllNotifications);
		GlobalMessenger.AddListener("RestartTimeLoop", ClearAllNotifications);
		GlobalMessenger.AddListener("ResetSimulation", ClearAllNotifications);
	}

	public void ClearAllNotifications()
	{
		_pinnedNotifications.Clear();
	}

	public void RepostNotifcation(NotificationData data)
	{
		for (int i = 0; i < _notifiableElements.Count; i++)
		{
			_notifiableElements[i].RemoveNotification(data);
			_notifiableElements[i].PushNotification(data);
		}
	}

	public void PostNotification(NotificationData data, bool pin = false)
	{
		bool flag = true;
		if (pin)
		{
			if (_pinnedNotifications.Contains(data))
			{
				Debug.LogWarning("NotificationManager already has this pinned notification: " + data.displayMessage);
				flag = false;
			}
			else
			{
				_pinnedNotifications.Add(data);
			}
		}
		if (flag)
		{
			for (int i = 0; i < _notifiableElements.Count; i++)
			{
				_notifiableElements[i].PushNotification(data);
			}
		}
	}

	public bool IsPinnedNotification(NotificationData data)
	{
		return _pinnedNotifications.Contains(data);
	}

	public void UnpinNotification(NotificationData data)
	{
		if (_pinnedNotifications.Contains(data))
		{
			for (int i = 0; i < _notifiableElements.Count; i++)
			{
				_notifiableElements[i].RemoveNotification(data);
			}
			_pinnedNotifications.Remove(data);
		}
		else
		{
			Debug.LogWarning("NotificationManager is not tracking this notification, cannot unpin");
		}
	}

	public void RegisterNotifiable(INotifiable notifiable)
	{
		_notifiableElements.Add(notifiable);
	}

	public void UnregisterNotifiable(INotifiable notifiable)
	{
		if (_notifiableElements.Contains(notifiable))
		{
			_notifiableElements.Remove(notifiable);
		}
	}

	public bool AreNotificationsVisible(NotificationTarget target)
	{
		for (int i = 0; i < _notifiableElements.Count; i++)
		{
			if (_notifiableElements[i].AreNotificationsVisible(target))
			{
				return true;
			}
		}
		return false;
	}
}
