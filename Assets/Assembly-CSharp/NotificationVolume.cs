using UnityEngine;

public class NotificationVolume : EffectVolume
{
	public enum Type
	{
		None = 0,
		DarkMatter = 1
	}

	[SerializeField]
	private Type _type;

	public Type GetNotificationType()
	{
		return _type;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		NotificationDetector component = hitObj.GetComponent<NotificationDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		NotificationDetector component = hitObj.GetComponent<NotificationDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
