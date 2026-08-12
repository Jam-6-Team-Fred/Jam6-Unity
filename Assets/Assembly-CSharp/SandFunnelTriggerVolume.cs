using System.Collections.Generic;
using UnityEngine;

public class SandFunnelTriggerVolume : OWTriggerVolume
{
	private struct SandFunnelObj
	{
		public GameObject trackedGameObject;

		public bool isObjectExposed;

		public bool flagForRemoval;

		public SandFunnelObj(GameObject go)
		{
			trackedGameObject = go;
			isObjectExposed = false;
			flagForRemoval = false;
		}
	}

	[SerializeField]
	private DirectionalForceVolume[] _alignmentOverrideVolumes = new DirectionalForceVolume[0];

	private List<SandFunnelObj> _listObjByExposure = new List<SandFunnelObj>(8);

	private Queue<GameObject> _objectsReadyToTrack = new Queue<GameObject>(8);

	private LayerMask _raycastLayerMask;

	protected override void Awake()
	{
		base.Awake();
		base.enabled = false;
		_raycastLayerMask = (int)OWLayerMask.physicalMask & ~(1 << LayerMask.NameToLayer("IgnoreOrbRaycast"));
	}

	public override void AddObjectToVolume(GameObject hitObj)
	{
		if (_active)
		{
			_objectsReadyToTrack.Enqueue(hitObj);
			base.enabled = true;
		}
	}

	public override void RemoveObjectFromVolume(GameObject hitObj)
	{
		if (!_trackedObjects.Remove(hitObj))
		{
			return;
		}
		RemoveHitObjectListeners(hitObj);
		for (int num = _listObjByExposure.Count - 1; num >= 0; num--)
		{
			SandFunnelObj value = _listObjByExposure[num];
			if (value.trackedGameObject == hitObj)
			{
				value.flagForRemoval = true;
				if (value.isObjectExposed)
				{
					FireExitEvent(value.trackedGameObject);
				}
				_listObjByExposure[num] = value;
				break;
			}
		}
	}

	public bool IsObjectExposed(GameObject obj)
	{
		if (obj.CompareTag("ShuttleDetector"))
		{
			return true;
		}
		Vector3 vector = obj.transform.position - base.transform.position;
		Vector3 zero = Vector3.zero;
		if (vector.sqrMagnitude > 62500f)
		{
			zero = -base.transform.forward;
		}
		else
		{
			zero = vector;
			if (obj.CompareTag("PlayerDetector") || obj.CompareTag("ProbeDetector"))
			{
				for (int i = 0; i < _alignmentOverrideVolumes.Length; i++)
				{
					if (_alignmentOverrideVolumes[i].GetOWTriggerVolume().IsTrackingObject(obj))
					{
						zero = -_alignmentOverrideVolumes[i].GetFieldDirection();
						break;
					}
				}
			}
		}
		float num = (obj.CompareTag("ShipDetector") ? 10f : 0f);
		return !Physics.Raycast(obj.transform.position + zero * num, zero, 20f, _raycastLayerMask);
	}

	private void FixedUpdate()
	{
		for (int num = _listObjByExposure.Count - 1; num >= 0; num--)
		{
			if (_listObjByExposure[num].flagForRemoval)
			{
				_listObjByExposure.RemoveAt(num);
			}
		}
		while (_objectsReadyToTrack.Count > 0)
		{
			GameObject gameObject = _objectsReadyToTrack.Dequeue();
			if (_trackedObjects.Contains(gameObject))
			{
				Debug.LogWarning("OWTriggerVolume " + base.gameObject.name + " already contains " + gameObject.name, this);
				Debug.Break();
			}
			else
			{
				_trackedObjects.SafeAdd(gameObject);
				_listObjByExposure.Add(new SandFunnelObj(gameObject));
				AddHitObjectListeners(gameObject);
			}
		}
		for (int num2 = _listObjByExposure.Count - 1; num2 >= 0; num2--)
		{
			SandFunnelObj value = _listObjByExposure[num2];
			if (value.isObjectExposed != IsObjectExposed(value.trackedGameObject))
			{
				value.isObjectExposed = !value.isObjectExposed;
				if (value.isObjectExposed)
				{
					FireEntryEvent(value.trackedGameObject);
				}
				else
				{
					FireExitEvent(value.trackedGameObject);
				}
				_listObjByExposure[num2] = value;
			}
		}
		base.enabled = _trackedObjects.Count > 0;
	}
}
