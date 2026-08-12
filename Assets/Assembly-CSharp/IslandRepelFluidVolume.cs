using UnityEngine;

public class IslandRepelFluidVolume : FluidVolume
{
	[SerializeField]
	private SphereOceanFluidVolume _oceanFluidVolume;

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		return _attachedBody.GetPointVelocity(worldPosition) + _oceanFluidVolume.GetPointBarrierVelocity(worldPosition, detector);
	}
}
