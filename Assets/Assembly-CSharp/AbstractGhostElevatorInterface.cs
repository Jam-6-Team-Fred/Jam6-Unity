using UnityEngine;

public abstract class AbstractGhostElevatorInterface : MonoBehaviour
{
	public event GhostInterfaceEvent OnUpSelected;

	public event GhostInterfaceEvent OnDownSelected;

	public event GhostInterfaceEvent OnActivate;

	public abstract void SetStartingPosition(bool IsActivated);

	public abstract void OnActionComplete();

	protected void CallOnUpEvent()
	{
		if (this.OnUpSelected != null)
		{
			this.OnUpSelected();
		}
	}

	protected void CallOnDownEvent()
	{
		if (this.OnDownSelected != null)
		{
			this.OnDownSelected();
		}
	}

	protected void CallOnActivateEvent()
	{
		if (this.OnActivate != null)
		{
			this.OnActivate();
		}
	}
}
