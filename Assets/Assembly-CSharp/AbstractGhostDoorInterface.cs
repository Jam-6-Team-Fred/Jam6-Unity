using UnityEngine;

public abstract class AbstractGhostDoorInterface : MonoBehaviour
{
	public event GhostInterfaceEvent OnOpen;

	public event GhostInterfaceEvent OnClose;

	public abstract void SetStartingPosition(bool IsActivated);

	protected void CallOpenEvent()
	{
		if (this.OnOpen != null)
		{
			this.OnOpen();
		}
	}

	protected void CallCloseEvent()
	{
		if (this.OnClose != null)
		{
			this.OnClose();
		}
	}
}
