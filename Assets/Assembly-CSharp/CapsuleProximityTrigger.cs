using UnityEngine;

public class CapsuleProximityTrigger : ProximityTrigger
{
	protected class CapsuleTrackedObject : TrackedObject
	{
		public CapsuleTrackedObject(GameObject obj)
			: base(obj)
		{
		}

		public override void Update(ProximityTrigger proximityTrigger)
		{
			CapsuleProximityTrigger capsuleProximityTrigger = proximityTrigger as CapsuleProximityTrigger;
			Vector3 segmentStart = proximityTrigger.transform.position + proximityTrigger.transform.up * capsuleProximityTrigger._length * 0.5f;
			Vector3 segmentEnd = proximityTrigger.transform.position - proximityTrigger.transform.up * capsuleProximityTrigger._length * 0.5f;
			bool flag = (OWMath.ClosestPointOnSegment(_gameObject.transform.position, segmentStart, segmentEnd) - _gameObject.transform.position).sqrMagnitude < capsuleProximityTrigger._radius * capsuleProximityTrigger._radius;
			_justEntered = !_inside && flag;
			_justExited = _inside && !flag;
			_inside = flag;
		}
	}

	[SerializeField]
	private float _radius;

	[SerializeField]
	private float _length;

	public float radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	protected override TrackedObject CreateTrackedObject(GameObject obj)
	{
		return new CapsuleTrackedObject(obj);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			OWGizmos.DrawCapsule(base.transform.position, base.transform.rotation, _length, _radius);
		}
		else
		{
			OWGizmos.DrawWireCapsuleOutline(base.transform.position, base.transform.rotation, _length, _radius);
		}
	}
}
