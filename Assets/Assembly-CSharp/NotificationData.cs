using System.Linq;

public class NotificationData
{
	public NotificationTarget notificationTgt;

	public string displayMessage;

	public string markupMessage;

	public float minDuration;

	public NotificationData(string displayMsg)
		: this(NotificationTarget.All, displayMsg)
	{
	}

	public NotificationData(NotificationTarget target, string displayMsg, float fDuration = 5f, bool playSound = true)
	{
		notificationTgt = target;
		displayMessage = displayMsg;
		markupMessage = string.Empty;
		minDuration = fDuration;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NotificationData))
		{
			return false;
		}
		return Equals((NotificationData)obj);
	}

	public bool Equals(NotificationData data)
	{
		if ((object)data == null)
		{
			return false;
		}
		string text = new string(Enumerable.ToArray(Enumerable.Where(Enumerable.Cast<char>(data.displayMessage), (char c) => !char.IsWhiteSpace(c))));
		string text2 = new string(Enumerable.ToArray(Enumerable.Where(Enumerable.Cast<char>(displayMessage), (char c) => !char.IsWhiteSpace(c))));
		if (data.notificationTgt == notificationTgt && text == text2)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return notificationTgt.GetHashCode() + displayMessage.GetHashCode();
	}

	public static bool operator ==(NotificationData dataOne, NotificationData dataTwo)
	{
		return dataOne?.Equals(dataTwo) ?? ((object)dataTwo == null);
	}

	public static bool operator !=(NotificationData dataOne, NotificationData dataTwo)
	{
		if ((object)dataOne == null)
		{
			return (object)dataTwo != null;
		}
		return !dataOne.Equals(dataTwo);
	}
}
