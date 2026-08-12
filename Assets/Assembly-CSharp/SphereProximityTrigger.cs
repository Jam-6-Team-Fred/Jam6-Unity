using UnityEngine;

public class SphereProximityTrigger : ProximityTrigger
{
	protected class RadialTrackedObject : TrackedObject
	{
		public RadialTrackedObject(GameObject obj)
			: base(obj)
		{
		}

		public override void Update(ProximityTrigger proximityTrigger)
		{
			SphereProximityTrigger sphereProximityTrigger = proximityTrigger as SphereProximityTrigger;
			bool flag = (sphereProximityTrigger.transform.position - _gameObject.transform.position).sqrMagnitude < sphereProximityTrigger._radius * sphereProximityTrigger._radius;
			_justEntered = !_inside && flag;
			_justExited = _inside && !flag;
			_inside = flag;
		}
	}

	[SerializeField]
	private float _radius;

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

	protected override TrackedObject CreateTrackedObject(GameObject obj)
	{
		return new RadialTrackedObject(obj);
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
			Gizmos.DrawSphere(Vector3.zero, _radius);
		}
		else
		{
			Gizmos.color = Color.green;
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _radius);
		}
	}
}
