using UnityEngine;

public class FlatFluidVolume : FluidVolume
{
	[Space]
	[SerializeField]
	private float _buoyancyDensity = 1.1f;

	public override bool PreventPlayerGrounded()
	{
		return PlayerState.IsCameraUnderwater();
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		return _attachedBody.GetPointVelocity(worldPosition);
	}

	public override float GetFractionSubmerged(FluidDetector detector)
	{
		Vector3 vector = base.transform.InverseTransformPoint(detector.transform.position);
		return detector.GetBuoyancyData().CalculateSubmergedFraction(vector.y, 0f);
	}

	public override Vector3 GetBuoyancy(FluidDetector detector, float fractionSubmerged)
	{
		if (detector.GetAttachedOWRigidbody().GetAttachedForceDetector() != null)
		{
			return Vector3.Project(-(detector.GetAttachedOWRigidbody().GetAttachedForceDetector().GetForceAcceleration() - _attachedBody.GetAttachedForceDetector().GetForceAcceleration()), base.transform.up) * fractionSubmerged * _buoyancyDensity / detector.GetBuoyancyData().density;
		}
		return Vector3.zero;
	}

	public override float GetDepthAtPosition(Vector3 worldPosition)
	{
		return 0f - base.transform.InverseTransformPoint(worldPosition).y;
	}

	public override Vector3 GetSplashAlignment(Vector3 impactPos, Vector3 impactDir)
	{
		return base.transform.up;
	}
}
