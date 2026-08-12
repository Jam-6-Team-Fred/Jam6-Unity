using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class OWCapsuleCollider : OWCustomCollider
{
	[SerializeField]
	private bool _useTopCap = true;

	[SerializeField]
	private bool _useBottomCap = true;

	[SerializeField]
	private bool _drawWireframe;

	protected CapsuleCollider _capsule;

	protected override void Awake()
	{
		base.Awake();
		_capsule = GetComponent<CapsuleCollider>();
	}

	public float GetRadius()
	{
		return _capsule.radius;
	}

	public float GetHeight()
	{
		return _capsule.height;
	}

	public Vector3 GetCenter()
	{
		return _capsule.center;
	}

	public bool UsingTopCap()
	{
		return _useTopCap;
	}

	public bool UsingBottomCap()
	{
		return _useBottomCap;
	}

	public override bool IsPointInCollider(Vector3 worldPoint)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPoint);
		Vector3 vector2 = vector - _capsule.center;
		Vector3 vector3 = vector2 - Vector3.up * vector2.y;
		float radius = _capsule.radius;
		float num = radius * radius;
		float num2 = Mathf.Max(0f, _capsule.height * 0.5f - radius);
		float num3 = num2 + radius;
		if (vector2.y < 0f - num3 || vector2.y > num3 || vector3.sqrMagnitude > num)
		{
			return false;
		}
		if (vector2.y > num2)
		{
			if (!_useTopCap)
			{
				return false;
			}
			Vector3 vector4 = _capsule.center + Vector3.up * num2;
			return (vector - vector4).sqrMagnitude < num;
		}
		if (vector2.y < 0f - num2)
		{
			if (!_useBottomCap)
			{
				return false;
			}
			Vector3 vector5 = _capsule.center - Vector3.up * num2;
			return (vector - vector5).sqrMagnitude < num;
		}
		return true;
	}

	public override float GetDistToSurface(Vector3 worldPoint)
	{
		if (!IsPointInCollider(worldPoint))
		{
			return Vector3.Distance(worldPoint, base.transform.position) - GetRadius();
		}
		return 0f;
	}

	private void OnDrawGizmos()
	{
		if (_drawWireframe && OWGizmos.SelectionContainsComponentOfType<OWCapsuleCollider>())
		{
			if (_capsule == null) _capsule = GetComponent<CapsuleCollider>(); // CHANGED
			
			float num = Mathf.Max(0f, _capsule.height * 0.5f - _capsule.radius);
			Vector3 vector = base.transform.TransformPoint(_capsule.center + Vector3.up * num);
			Vector3 vector2 = base.transform.TransformPoint(_capsule.center - Vector3.up * num);
			Gizmos.color = Color.green;
			OWGizmos.DrawWireCircle(vector, base.transform.up, _capsule.radius);
			OWGizmos.DrawWireCircle(vector2, base.transform.up, _capsule.radius);
			Gizmos.DrawLine(vector2 + base.transform.right * _capsule.radius, vector + base.transform.right * _capsule.radius);
			Gizmos.DrawLine(vector2 + -base.transform.right * _capsule.radius, vector + -base.transform.right * _capsule.radius);
			Gizmos.DrawLine(vector2 + base.transform.forward * _capsule.radius, vector + base.transform.forward * _capsule.radius);
			Gizmos.DrawLine(vector2 + -base.transform.forward * _capsule.radius, vector + -base.transform.forward * _capsule.radius);
			if (_useTopCap)
			{
				OWGizmos.DrawWireArc(vector, base.transform.right, -base.transform.forward, 180f, _capsule.radius);
				OWGizmos.DrawWireArc(vector, base.transform.forward, base.transform.right, 180f, _capsule.radius);
			}
			if (_useBottomCap)
			{
				OWGizmos.DrawWireArc(vector2, base.transform.right, base.transform.forward, 180f, _capsule.radius);
				OWGizmos.DrawWireArc(vector2, base.transform.forward, -base.transform.right, 180f, _capsule.radius);
			}
		}
	}
}
