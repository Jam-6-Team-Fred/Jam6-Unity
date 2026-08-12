using UnityEngine;

public class ZeroGVolume : ForceVolume
{
	public override Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos)
	{
		return Vector3.zero;
	}

	public override bool GetAffectsAlignment(OWRigidbody targetBody)
	{
		return false;
	}
}
