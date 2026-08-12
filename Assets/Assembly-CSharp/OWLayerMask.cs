using UnityEngine;

public static class OWLayerMask
{
	public static readonly LayerMask dreamSimulationMask = 1 << LayerMask.NameToLayer("DreamSimulation");

	public static readonly LayerMask physicalMask = BuildPhysicalMask();

	public static readonly LayerMask detectorMask = BuildDetectorMask();

	public static readonly LayerMask quantumOcclusionMask = physicalMask;

	public static readonly LayerMask groundMask = physicalMask;

	public static readonly LayerMask advancedDetectorMask = BuildAdvancedDetectorMask();

	public static readonly LayerMask effectVolumeMask = BuildEffectVolumeMask();

	public static readonly LayerMask interactMask = 1 << LayerMask.NameToLayer("Interactible");

	public static readonly LayerMask blockableInteractMask = ((int)physicalMask | (int)interactMask) & ~(1 << LayerMask.NameToLayer("IgnoreOrbRaycast"));

	public static readonly LayerMask closeRangeRFMask = (int)physicalMask | (1 << LayerMask.NameToLayer("CloseRangeRFVolume"));

	public static readonly LayerMask longRangeRFMask = 1 << LayerMask.NameToLayer("ReferenceFrameVolume");

	public static readonly int playerSafetyCollider = LayerMask.NameToLayer("PlayerSafetyCollider");

	public static bool IsLayerInMask(int layer, LayerMask layerMask)
	{
		return (layerMask.value & (1 << layer)) > 0;
	}

	public static LayerMask BuildDetectorMask()
	{
		return (1 << LayerMask.NameToLayer("BasicDetector")) | (1 << LayerMask.NameToLayer("AdvancedDetector")) | (1 << LayerMask.NameToLayer("PhysicalDetector"));
	}

	public static LayerMask BuildEffectVolumeMask()
	{
		return (1 << LayerMask.NameToLayer("BasicEffectVolume")) | (1 << LayerMask.NameToLayer("AdvancedEffectVolume"));
	}

	public static LayerMask BuildAdvancedDetectorMask()
	{
		return (1 << LayerMask.NameToLayer("AdvancedDetector")) | (1 << LayerMask.NameToLayer("PhysicalDetector"));
	}

	public static LayerMask BuildPhysicalMask()
	{
		return (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Primitive")) | (1 << LayerMask.NameToLayer("IgnoreSun")) | (1 << LayerMask.NameToLayer("PhysicalDetector")) | (1 << LayerMask.NameToLayer("ShipInterior")) | (1 << LayerMask.NameToLayer("IgnoreOrbRaycast"));
	}
}
