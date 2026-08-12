using UnityEngine;

public abstract class ElectricalComponent : MonoBehaviour
{
	protected bool _powered;

	public virtual bool IsPowered()
	{
		return _powered;
	}

	public virtual void SetPowered(bool powered)
	{
		_powered = powered;
	}

	protected virtual void Awake()
	{
		_powered = false;
	}
}
