using UnityEngine;

public class RiverZoneTriggerVolume : OWTriggerVolume
{
	[Space]
	[SerializeField]
	private OWRingRiverCollider _riverCollider;

	private OWTriggerVolume _riverTriggerVolume;

	protected override void Reset()
	{
		base.Reset();
		_riverCollider = Object.FindObjectOfType<OWRingRiverCollider>();
	}

	protected override void Awake()
	{
		base.Awake();
		_riverTriggerVolume = _riverCollider.gameObject.GetAddComponent<OWTriggerVolume>();
		_riverTriggerVolume.OnEntry += OnObjectEnteredRiver;
		_riverTriggerVolume.OnExit += OnObjectExitedRiver;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_riverTriggerVolume.OnEntry -= OnObjectEnteredRiver;
		_riverTriggerVolume.OnExit -= OnObjectExitedRiver;
	}

	public override void AddObjectToVolume(GameObject hitObj)
	{
		if (!_active)
		{
			return;
		}
		if (_trackedObjects.SafeAdd(hitObj))
		{
			AddHitObjectListeners(hitObj);
			if (_riverTriggerVolume.IsTrackingObject(hitObj))
			{
				FireEntryEvent(hitObj);
			}
		}
		else
		{
			Debug.LogWarning("OWTriggerVolume " + base.gameObject.name + " already contains " + hitObj.name, this);
		}
	}

	public override void RemoveObjectFromVolume(GameObject hitObj)
	{
		if (_trackedObjects.Remove(hitObj))
		{
			RemoveHitObjectListeners(hitObj);
			if (_riverTriggerVolume.IsTrackingObject(hitObj))
			{
				FireExitEvent(hitObj);
			}
		}
	}

	private void OnObjectEnteredRiver(GameObject hitObj)
	{
		if (IsTrackingObject(hitObj))
		{
			FireEntryEvent(hitObj);
		}
	}

	private void OnObjectExitedRiver(GameObject hitObj)
	{
		if (IsTrackingObject(hitObj))
		{
			FireExitEvent(hitObj);
		}
	}
}
