using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class DistanceTrigger : SectoredMonoBehaviour
{
	[Serializable]
	public class TriggerEnterEvent : UnityEvent
	{
	}

	[Serializable]
	public class TriggerExitEvent : UnityEvent
	{
	}

	[SerializeField]
	protected float _triggerRadius;

	[SerializeField]
	public TriggerEnterEvent OnTriggerEnter;

	[SerializeField]
	public TriggerExitEvent OnTriggerExit;

	public abstract void TriggerEnter();

	public abstract void TriggerExit();

	public virtual float GetTriggerRadius()
	{
		return _triggerRadius;
	}

	public virtual void SetTriggerRadius(float value)
	{
		_triggerRadius = value;
	}

	protected void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, _triggerRadius);
	}
}
