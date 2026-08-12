using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class OWShellCollider : OWCustomCollider
{
	private SphereCollider _sphereCollider;

	[SerializeField]
	private float _innerRadius = 0.25f;

	public float innerRadius
	{
		get
		{
			return _innerRadius;
		}
		set
		{
			_innerRadius = Mathf.Clamp(value, 0f, _sphereCollider.radius);
		}
	}

	private void OnValidate()
	{
		float radius = GetComponent<SphereCollider>().radius;
		if (_innerRadius < 0f || _innerRadius > radius)
		{
			_innerRadius = Mathf.Clamp(_innerRadius, 0f, radius);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_sphereCollider = GetComponent<SphereCollider>();
	}

	public override bool IsPointInCollider(Vector3 worldPoint)
	{
		float num = Mathf.Max(Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y), base.transform.lossyScale.z);
		float num2 = _innerRadius * num;
		float num3 = _sphereCollider.radius * num;
		Vector3 vector = base.transform.TransformPoint(_sphereCollider.center);
		float num4 = Vector3.SqrMagnitude(worldPoint - vector);
		if (num4 < num3 * num3)
		{
			return num4 > num2 * num2;
		}
		return false;
	}

	public override float GetDistToSurface(Vector3 worldPoint)
	{
		float num = Mathf.Max(Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y), base.transform.lossyScale.z);
		float num2 = _innerRadius * num;
		float num3 = _sphereCollider.radius * num;
		Vector3 b = base.transform.TransformPoint(_sphereCollider.center);
		float num4 = Vector3.Distance(worldPoint, b);
		if (num4 < num3)
		{
			return Mathf.Max(num2 - num4, 0f);
		}
		return Mathf.Max(num4 - num3, 0f);
	}

	private void OnDrawGizmosSelected()
	{
		SphereCollider component = GetComponent<SphereCollider>();
		Gizmos.color = new Color(0.5f, 1f, 0.5f, component.enabled ? 0.33f : 0.1f);
		float num = Mathf.Max(Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y), base.transform.lossyScale.z);
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one * num);
		Gizmos.DrawWireSphere(component.center, _innerRadius);
	}
}
