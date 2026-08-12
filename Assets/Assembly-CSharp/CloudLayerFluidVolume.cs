using UnityEngine;

public class CloudLayerFluidVolume : FluidVolume
{
	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		return _attachedBody.GetPointVelocity(worldPosition);
	}

	public override bool IsSpherical()
	{
		return true;
	}
}
