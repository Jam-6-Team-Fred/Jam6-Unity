using UnityEngine;

public abstract class PriorityVolume : EffectVolume
{
	[SerializeField]
	private int _layer;

	[SerializeField]
	private int _priority;

	public int GetLayer()
	{
		return _layer;
	}

	public int GetPriority()
	{
		return _priority;
	}

	public void SetPriority(int priority)
	{
		_priority = priority;
	}
}
