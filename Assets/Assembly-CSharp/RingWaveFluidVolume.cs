using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class RingWaveFluidVolume : FluidVolume
{
	[Space]
	[SerializeField]
	private RingRiverFluidVolume _riverFluid;

	[SerializeField]
	private float _radius;

	[SerializeField]
	private float _buoyancyDensity = 1.1f;

	private CapsuleCollider _capsule;

	private void OnValidate()
	{
		_capsule = GetComponent<CapsuleCollider>();
		if (_capsule.radius != 0.5f)
		{
			_capsule.radius = 0.5f;
		}
		Vector3 localScale = base.transform.localScale;
		if (localScale.x != _radius * 2f)
		{
			localScale.x = _radius * 2f;
			base.transform.localScale = localScale;
		}
		if (localScale.z != _radius * 2f)
		{
			localScale.z = _radius * 2f;
			base.transform.localScale = localScale;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_capsule = GetComponent<CapsuleCollider>();
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		return _riverFluid.GetPointFluidVelocity(worldPosition, detector);
	}

	public override float GetFractionSubmerged(FluidDetector detector)
	{
		Vector3 vector = Vector3.ProjectOnPlane(detector.transform.position - base.transform.position, base.transform.up);
		return detector.GetBuoyancyData().CalculateSubmergedFraction(vector.magnitude, _radius);
	}

	public override Vector3 GetBuoyancy(FluidDetector detector, float fractionSubmerged)
	{
		if (detector.GetAttachedOWRigidbody().GetAttachedForceDetector() != null)
		{
			Vector3 onNormal = Vector3.ProjectOnPlane(_riverFluid.transform.position - detector.transform.position, _riverFluid.transform.up);
			return Vector3.Project(-(detector.GetAttachedOWRigidbody().GetAttachedForceDetector().GetForceAcceleration() - _attachedBody.GetAttachedForceDetector().GetForceAcceleration()), onNormal) * fractionSubmerged * _buoyancyDensity / detector.GetBuoyancyData().density;
		}
		return Vector3.zero;
	}
}
