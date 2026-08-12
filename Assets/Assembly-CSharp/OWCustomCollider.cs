using System.Collections.Generic;
using UnityEngine;

public abstract class OWCustomCollider : MonoBehaviour
{
	protected class TrackedTransform
	{
		public Transform transform;

		public bool inside;

		public FluidDetector fluidDetector;

		public TrackedTransform(Transform t)
		{
			transform = t;
			inside = false;
			fluidDetector = null;
		}
	}

	public delegate void TriggerEvent(GameObject hitObj);

	protected Collider _collider;

	protected OWCollider _owCollider;

	protected List<TrackedTransform> _trackedTransforms;

	public event TriggerEvent OnEntry;

	public event TriggerEvent OnExit;

	protected virtual void Awake()
	{
		_collider = GetComponent<Collider>();
		_owCollider = base.gameObject.GetAddComponent<OWCollider>();
		_trackedTransforms = new List<TrackedTransform>(8);
		_owCollider.OnColliderDisabled += OnColliderDisabled;
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		_owCollider.OnColliderDisabled -= OnColliderDisabled;
	}

	public abstract bool IsPointInCollider(Vector3 worldPoint);

	public abstract float GetDistToSurface(Vector3 worldPoint);

	public void UntrackTransform(Transform trackedTransform)
	{
		TrackedTransform trackedTransform2 = _trackedTransforms.Find((TrackedTransform i) => i.transform == trackedTransform);
		if (trackedTransform2 != null)
		{
			_trackedTransforms.Remove(trackedTransform2);
			if (_trackedTransforms.Count == 0)
			{
				base.enabled = false;
			}
		}
	}

	protected virtual bool IsTrackerInCollider(TrackedTransform tracker)
	{
		return IsPointInCollider(tracker.transform.position);
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		Transform hitTransform = hitCollider.transform;
		if (!_trackedTransforms.Exists((TrackedTransform i) => i.transform == hitTransform))
		{
			TrackedTransform trackedTransform = new TrackedTransform(hitTransform);
			FluidDetector component = hitTransform.GetComponent<FluidDetector>();
			if (component != null)
			{
				trackedTransform.fluidDetector = component;
			}
			_trackedTransforms.Add(trackedTransform);
			CheckTrackerCollision(trackedTransform);
			if (!base.enabled)
			{
				base.enabled = true;
			}
		}
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		TrackedTransform trackedTransform = _trackedTransforms.Find((TrackedTransform i) => i.transform == hitCollider.transform);
		if (trackedTransform != null)
		{
			_trackedTransforms.Remove(trackedTransform);
			if (this.OnExit != null && trackedTransform.inside)
			{
				this.OnExit(trackedTransform.transform.gameObject);
			}
			if (_trackedTransforms.Count == 0)
			{
				base.enabled = false;
			}
		}
	}

	private void OnColliderDisabled(OWCollider collider)
	{
		_trackedTransforms.Clear();
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		for (int num = _trackedTransforms.Count - 1; num >= 0; num--)
		{
			if (_trackedTransforms[num].transform == null || !_trackedTransforms[num].transform.gameObject.activeInHierarchy)
			{
				_trackedTransforms.QuickRemoveAt(num);
			}
			else
			{
				CheckTrackerCollision(_trackedTransforms[num]);
			}
		}
		if (_trackedTransforms.Count == 0)
		{
			base.enabled = false;
		}
	}

	private void CheckTrackerCollision(TrackedTransform tracker)
	{
		bool inside = tracker.inside;
		bool flag = (tracker.inside = IsTrackerInCollider(tracker));
		if (this.OnExit != null && inside && !flag)
		{
			this.OnExit(tracker.transform.gameObject);
		}
		else if (this.OnEntry != null && !inside && flag)
		{
			this.OnEntry(tracker.transform.gameObject);
		}
	}
}
