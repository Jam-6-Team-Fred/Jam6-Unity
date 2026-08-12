using UnityEngine;

[RequireComponent(typeof(BoxShape))]
public class FlatFluidVolume_Old : FluidVolume
{
	[Space]
	[SerializeField]
	private float _buoyancyDensity = 1.1f;

	protected BoxShape _boxShape;

	protected override void Awake()
	{
		base.Awake();
		_boxShape = GetComponent<BoxShape>();
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		return _attachedBody.GetPointVelocity(worldPosition);
	}

	public override float GetFractionSubmerged(FluidDetector detector)
	{
		Vector3 vector = base.transform.InverseTransformPoint(detector.transform.position);
		float localSurfaceYPos = GetLocalSurfaceYPos(heads: true);
		return detector.GetBuoyancyData().CalculateSubmergedFraction(vector.y, localSurfaceYPos);
	}

	public override Vector3 GetBuoyancy(FluidDetector detector, float fractionSubmerged)
	{
		if (detector.GetAttachedOWRigidbody().GetAttachedForceDetector() != null)
		{
			return Vector3.Project(-(detector.GetAttachedOWRigidbody().GetAttachedForceDetector().GetForceAcceleration() - _attachedBody.GetAttachedForceDetector().GetForceAcceleration()), base.transform.up) * fractionSubmerged * _buoyancyDensity / detector.GetBuoyancyData().density;
		}
		return Vector3.zero;
	}

	public float GetLocalSurfaceYPos(bool heads)
	{
		return _boxShape.center.y + _boxShape.size.y * 0.5f * (float)(heads ? 1 : (-1));
	}
}
