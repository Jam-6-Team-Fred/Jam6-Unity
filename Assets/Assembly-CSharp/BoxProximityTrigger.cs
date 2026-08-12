using UnityEngine;

public class BoxProximityTrigger : ProximityTrigger
{
	protected class BoxTrackedObject : TrackedObject
	{
		public BoxTrackedObject(GameObject obj)
			: base(obj)
		{
		}

		public override void Update(ProximityTrigger proximityTrigger)
		{
			BoxProximityTrigger obj = proximityTrigger as BoxProximityTrigger;
			Vector3 vector = proximityTrigger.transform.InverseTransformPoint(_gameObject.transform.position);
			Vector3 vector2 = obj._size * 0.5f;
			bool flag = vector.x < vector2.x && vector.x > 0f - vector2.x && vector.y < vector2.y && vector.y > 0f - vector2.y && vector.z < vector2.z && vector.z > 0f - vector2.z;
			_justEntered = !_inside && flag;
			_justExited = _inside && !flag;
			_inside = flag;
		}
	}

	[SerializeField]
	private Vector3 _size;

	public Vector3 size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
		}
	}

	protected override TrackedObject CreateTrackedObject(GameObject obj)
	{
		return new BoxTrackedObject(obj);
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.zero, _size);
		}
		else
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, _size);
		}
	}
}
