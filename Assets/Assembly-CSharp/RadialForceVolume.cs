using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RadialForceVolume : ForceVolume
{
	public enum Falloff
	{
		Constant = 0,
		Linear = 1,
		InvSqr = 2
	}

	private SphereCollider _sphereCollider;

	[SerializeField]
	private Falloff _falloff;

	[SerializeField]
	private float _acceleration = 10f;

	protected override void Awake()
	{
		base.Awake();
		_sphereCollider = GetComponent<SphereCollider>();
	}

	public override bool GetAffectsAlignment(OWRigidbody targetBody)
	{
		return true;
	}

	public override Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos)
	{
		Vector3 vector = _sphereCollider.transform.TransformPoint(_sphereCollider.center);
		float num = Mathf.Max(_sphereCollider.transform.lossyScale.x, _sphereCollider.transform.lossyScale.y, _sphereCollider.transform.lossyScale.z) * _sphereCollider.radius;
		if (_falloff == Falloff.Constant)
		{
			return (worldPos - vector).normalized * _acceleration;
		}
		if (_falloff == Falloff.Linear)
		{
			Vector3 vector2 = worldPos - vector;
			float magnitude = vector2.magnitude;
			float num2 = 1f - Mathf.Clamp01(magnitude / num);
			return vector2 / magnitude * num2 * _acceleration;
		}
		Vector3 vector3 = worldPos - vector;
		float magnitude2 = vector3.magnitude;
		float num3 = 1f - Mathf.Clamp01(magnitude2 / num);
		num3 *= num3;
		return vector3 / magnitude2 * num3 * _acceleration;
	}
}
