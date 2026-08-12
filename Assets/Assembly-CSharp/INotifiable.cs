public interface INotifiable
{
	void PushNotification(NotificationData data);

	void RemoveNotification(NotificationData data);

	bool AreNotificationsVisible(NotificationTarget target);
}
