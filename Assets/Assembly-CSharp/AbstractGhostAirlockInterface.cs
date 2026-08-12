using UnityEngine;

public abstract class AbstractGhostAirlockInterface : MonoBehaviour
{
	public event GhostInterfaceEvent OnOpen;

	public event GhostInterfaceEvent OnClose;

	public event GhostInterfaceEvent OnRotate;

	public abstract void SetStartingPosition(bool IsOpen);

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

	protected void CallOnRotateEvent()
	{
		if (this.OnRotate != null)
		{
			this.OnRotate();
		}
	}
}
