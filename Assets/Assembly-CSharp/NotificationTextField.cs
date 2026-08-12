using UnityEngine;
using UnityEngine.UI;

public class NotificationTextField : MonoBehaviour, INotifiable
{
	[SerializeField]
	private NotificationTarget _notificationFilter;

	[SerializeField]
	private Text _textField;

	private string _message;

	private string _markupMessage;

	private float _visibleChars;

	private float _startDisplayTime;

	private float _displayDuration;

	private void Start()
	{
		SetVisible(visible: false);
		NotificationManager.SharedInstance.RegisterNotifiable(this);
	}

	private void OnDestroy()
	{
		NotificationManager.SharedInstance.UnregisterNotifiable(this);
	}

	private void SetVisible(bool visible)
	{
		base.enabled = visible;
		_textField.enabled = visible;
	}

	public void DisplayMessage(string message, float duration = -1f, bool useScrolling = true)
	{
		SetVisible(visible: true);
		_startDisplayTime = Time.time;
		_message = (_markupMessage = message);
		if (duration == -1f)
		{
			_displayDuration = Mathf.Max((float)message.Length * 0.2f, 3f);
		}
		else
		{
			_displayDuration = duration;
		}
		_visibleChars = ((!useScrolling) ? _message.Length : 0);
		Update();
	}

	public void SetMarkupMessage(string markupMessage)
	{
		_markupMessage = markupMessage;
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - _startDisplayTime) / _displayDuration);
		_visibleChars += Time.deltaTime * 50f;
		if (_visibleChars < (float)_message.Length)
		{
			_textField.text = _message.Substring(0, (int)_visibleChars);
		}
		else
		{
			_textField.text = _markupMessage;
		}
		if (num >= 1f)
		{
			SetVisible(visible: false);
		}
	}

	public void PushNotification(NotificationData data)
	{
		NotificationTarget notificationTgt = data.notificationTgt;
		if (notificationTgt == NotificationTarget.All || (notificationTgt & _notificationFilter) == _notificationFilter)
		{
			DisplayMessage(data.displayMessage, data.minDuration);
			if (data.markupMessage != string.Empty)
			{
				SetMarkupMessage(data.markupMessage);
			}
		}
	}

	public void RemoveNotification(NotificationData data)
	{
	}

	public bool AreNotificationsVisible(NotificationTarget target)
	{
		if (target == NotificationTarget.All || (target & _notificationFilter) == target)
		{
			return _textField.enabled;
		}
		return false;
	}
}
