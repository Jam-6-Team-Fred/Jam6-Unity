using UnityEngine;

public class RadialFluidVolume : FluidVolume
{
	[SerializeField]
	protected float _buoyancyDensity = 1f;

	[SerializeField]
	protected float _radius;

	protected virtual void OnValidate()
	{
		SphereCollider component = GetComponent<SphereCollider>();
		if (component != null)
		{
			component.radius = _radius;
		}
	}

	public override Vector3 GetBuoyancy(FluidDetector detector, float fractionSubmerged)
	{
		if (detector.GetAttachedOWRigidbody().GetAttachedForceDetector() != null)
		{
			Vector3 vector = detector.GetAttachedOWRigidbody().GetAttachedForceDetector().GetForceAcceleration() - _attachedBody.GetAttachedForceDetector().GetForceAcceleration();
			return Vector3.Project(onNormal: detector.transform.position - base.transform.position, vector: -vector) * fractionSubmerged * _buoyancyDensity / detector.GetBuoyancyData().density;
		}
		return Vector3.zero;
	}

	public override bool IsSpherical()
	{
		return true;
	}

	public override float GetFractionSubmerged(FluidDetector detector)
	{
		Vector3 vector = detector.transform.position - base.transform.position;
		return detector.GetBuoyancyData().CalculateSubmergedFraction(vector.magnitude, _radius);
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(base.transform.position, _radius);
	}
}
