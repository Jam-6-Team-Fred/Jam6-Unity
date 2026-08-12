using UnityEngine;

public class CompoundProximityTrigger : ProximityTrigger
{
	protected class CompoundTrackedObject : TrackedObject
	{
		public int _enterCount;

		public CompoundTrackedObject(GameObject obj)
			: base(obj)
		{
			_enterCount = 0;
		}

		public override void Update(ProximityTrigger proximityTrigger)
		{
			bool flag = _enterCount > 0;
			_justEntered = !_inside && flag;
			_justExited = _inside && !flag;
			_inside = flag;
		}
	}

	private ProximityTrigger[] _childProxTriggers;

	protected override TrackedObject CreateTrackedObject(GameObject obj)
	{
		return new CompoundTrackedObject(obj);
	}

	protected override void Awake()
	{
		base.Awake();
		_childProxTriggers = GetComponentsInChildren<ProximityTrigger>();
		for (int i = 0; i < _childProxTriggers.Length; i++)
		{
			if (_childProxTriggers[i] != this)
			{
				_childProxTriggers[i].AddListeners(OnChildEnterProximity, OnChildExitProximity);
			}
		}
	}

	protected void OnDestroy()
	{
		if (_childProxTriggers == null)
		{
			return;
		}
		for (int i = 0; i < _childProxTriggers.Length; i++)
		{
			if (_childProxTriggers[i] != this && _childProxTriggers[i] != null)
			{
				_childProxTriggers[i].RemoveListeners(OnChildEnterProximity, OnChildExitProximity);
			}
		}
	}

	public override void TrackObject(GameObject obj)
	{
		base.TrackObject(obj);
		if (_childProxTriggers == null)
		{
			return;
		}
		for (int i = 0; i < _childProxTriggers.Length; i++)
		{
			if (_childProxTriggers[i] != this)
			{
				_childProxTriggers[i].TrackObject(obj);
			}
		}
	}

	public override void UntrackObject(GameObject obj)
	{
		base.UntrackObject(obj);
		if (_childProxTriggers == null)
		{
			return;
		}
		for (int i = 0; i < _childProxTriggers.Length; i++)
		{
			if (_childProxTriggers[i] != this)
			{
				_childProxTriggers[i].UntrackObject(obj);
			}
		}
	}

	public override void UntrackAll()
	{
		base.UntrackAll();
		if (_childProxTriggers == null)
		{
			return;
		}
		for (int i = 0; i < _childProxTriggers.Length; i++)
		{
			if (_childProxTriggers[i] != this)
			{
				_childProxTriggers[i].UntrackAll();
			}
		}
	}

	protected void OnChildEnterProximity(GameObject obj)
	{
		(_trackedObjects.Find((TrackedObject x) => x._gameObject == obj) as CompoundTrackedObject)._enterCount++;
	}

	protected void OnChildExitProximity(GameObject obj)
	{
		(_trackedObjects.Find((TrackedObject x) => x._gameObject == obj) as CompoundTrackedObject)._enterCount--;
	}
}
