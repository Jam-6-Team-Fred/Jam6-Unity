using System.Collections.Generic;
using UnityEngine;

public abstract class ProximityTrigger : MonoBehaviour
{
	protected abstract class TrackedObject
	{
		public GameObject _gameObject;

		public bool _inside;

		public bool _justEntered;

		public bool _justExited;

		public TrackedObject(GameObject obj)
		{
			_gameObject = obj;
			_inside = false;
			_justEntered = false;
			_justExited = false;
		}

		public abstract void Update(ProximityTrigger proximityTrigger);
	}

	public delegate void EnterProximityHandler(GameObject obj);

	public delegate void ExitProximityHandler(GameObject obj);

	protected List<TrackedObject> _trackedObjects;

	private int _frameCount;

	public event EnterProximityHandler OnEnterProximity;

	public event ExitProximityHandler OnExitProximity;

	protected abstract TrackedObject CreateTrackedObject(GameObject obj);

	protected virtual void Awake()
	{
		_trackedObjects = new List<TrackedObject>(8);
	}

	public virtual void AddListeners(EnterProximityHandler entryHandler, ExitProximityHandler exitHandler)
	{
		OnEnterProximity += entryHandler;
		OnExitProximity += exitHandler;
	}

	public virtual void RemoveListeners(EnterProximityHandler entryHandler, ExitProximityHandler exitHandler)
	{
		OnEnterProximity -= entryHandler;
		OnExitProximity -= exitHandler;
	}

	public virtual void TrackObject(GameObject obj)
	{
		if (_trackedObjects.Exists((TrackedObject x) => x._gameObject == obj))
		{
			Debug.LogError(obj.name + " is already being tracked!", this);
			Debug.Break();
		}
		else
		{
			TrackedObject item = CreateTrackedObject(obj);
			_trackedObjects.Add(item);
		}
		base.enabled = true;
	}

	public virtual void UntrackObject(GameObject obj)
	{
		_trackedObjects.RemoveAll((TrackedObject x) => x._gameObject == obj);
		if (_trackedObjects.Count == 0)
		{
			base.enabled = false;
		}
	}

	public virtual void UntrackAll()
	{
		_trackedObjects.Clear();
		base.enabled = false;
	}

	protected virtual void FixedUpdate()
	{
		if (_frameCount < 2)
		{
			_frameCount++;
			return;
		}
		for (int i = 0; i < _trackedObjects.Count; i++)
		{
			if (_trackedObjects[i]._gameObject == null)
			{
				_trackedObjects.RemoveAt(i);
				i--;
				continue;
			}
			_trackedObjects[i].Update(this);
			if (_trackedObjects[i]._justEntered && this.OnEnterProximity != null)
			{
				this.OnEnterProximity(_trackedObjects[i]._gameObject);
			}
			else if (_trackedObjects[i]._justExited && this.OnExitProximity != null)
			{
				this.OnExitProximity(_trackedObjects[i]._gameObject);
			}
		}
		if (_trackedObjects.Count == 0)
		{
			base.enabled = false;
		}
	}
}
