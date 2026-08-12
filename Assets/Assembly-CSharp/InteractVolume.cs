using UnityEngine;

public abstract class InteractVolume : MonoBehaviour
{
	protected bool _focused;

	protected OWCamera _playerCam;

	protected virtual void Start()
	{
		_playerCam = Locator.GetPlayerCamera();
	}

	public void SetInteractionEnabled(bool enable)
	{
		if (enable)
		{
			EnableInteraction();
		}
		else
		{
			DisableInteraction();
		}
	}

	public virtual void EnableInteraction()
	{
	}

	public virtual void DisableInteraction()
	{
	}

	public virtual void ResetInteraction()
	{
	}

	public abstract void UpdateInteractVolume();

	public virtual bool IsFocused()
	{
		return _focused;
	}

	public abstract void LoseFocus();

	protected virtual void GainFocus()
	{
	}
}
