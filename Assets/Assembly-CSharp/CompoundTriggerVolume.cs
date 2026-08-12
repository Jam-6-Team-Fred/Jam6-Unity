using System.Collections.Generic;
using UnityEngine;

public class CompoundTriggerVolume : MonoBehaviour
{
	public delegate void EntryEvent(GameObject hitObj);

	public delegate void ExitEvent(GameObject hitObj);

	private IDictionary<Collider, int> _trackedColliders;

	private ChildTriggerVolume[] _childTriggers;

	public event EntryEvent OnEntry;

	public event ExitEvent OnExit;

	private void Awake()
	{
		_childTriggers = GetComponentsInChildren<ChildTriggerVolume>();
		_trackedColliders = new Dictionary<Collider, int>(_childTriggers.Length);
		for (int i = 0; i < _childTriggers.Length; i++)
		{
			ChildTriggerVolume childTriggerVolume = _childTriggers[i];
			if (childTriggerVolume.IsEntryTrigger())
			{
				childTriggerVolume.OnEnterChildTrigger += OnEnterChildTrigger;
			}
			if (childTriggerVolume.IsExitTrigger())
			{
				childTriggerVolume.OnExitChildTrigger += OnExitChildTrigger;
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _childTriggers.Length; i++)
		{
			ChildTriggerVolume obj = _childTriggers[i];
			obj.OnEnterChildTrigger -= OnEnterChildTrigger;
			obj.OnExitChildTrigger -= OnExitChildTrigger;
		}
	}

	public void SetActivation(bool active)
	{
		_trackedColliders.Clear();
		for (int i = 0; i < _childTriggers.Length; i++)
		{
			_childTriggers[i].enabled = active;
		}
		base.enabled = active;
	}

	public void UntrackCollider(Collider collider)
	{
		_trackedColliders.Remove(collider);
	}

	private void OnEnterChildTrigger(Collider hitCollider)
	{
		if (!_trackedColliders.ContainsKey(hitCollider))
		{
			_trackedColliders.Add(hitCollider, 0);
			if (this.OnEntry != null)
			{
				this.OnEntry(hitCollider.gameObject);
			}
		}
		_trackedColliders[hitCollider]++;
	}

	private void OnExitChildTrigger(Collider hitCollider)
	{
		_trackedColliders[hitCollider]--;
		if (_trackedColliders[hitCollider] == 0)
		{
			_trackedColliders.Remove(hitCollider);
			if (this.OnExit != null)
			{
				this.OnExit(hitCollider.gameObject);
			}
		}
	}
}
