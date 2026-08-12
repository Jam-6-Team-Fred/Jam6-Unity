using UnityEngine;

public class ShipFluidDetector : DynamicFluidDetector
{
	public bool InOceanBarrierZone()
	{
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			if (_activeVolumes[i] is SphereOceanFluidVolume)
			{
				return ((SphereOceanFluidVolume)_activeVolumes[i]).IsPointInBarrierZone(base.transform.position);
			}
		}
		return false;
	}

	protected override void AddDrag(FluidVolume fluidVolume, float fractionSubmerged)
	{
		if (fluidVolume.AllowShipAutoroll())
		{
			Vector3 vector = fluidVolume.GetAttachedOWRigidbody().GetPosition() - _owRigidbody.GetPosition();
			Vector3 forward = _owRigidbody.transform.forward;
			float num = Vector3.Angle(vector, forward);
			num = 1f - Mathf.Abs(num - 90f) / 90f;
			Vector3 vector2 = Vector3.Cross(forward, vector);
			Vector3 right = _owRigidbody.transform.right;
			float a = Vector3.Angle(vector2, right) * Mathf.Sign(Vector3.Dot(_owRigidbody.transform.up, vector2));
			a = Mathf.Min(a, 90f) * 0.015f * num;
			_netAngularAcceleration += forward * a;
		}
		base.AddDrag(fluidVolume, fractionSubmerged);
	}

	protected override void OnEnterFluidType_Internal(FluidVolume fluid)
	{
		base.OnEnterFluidType_Internal(fluid);
		if (fluid.GetFluidType() == FluidVolume.Type.WATER)
		{
			GlobalMessenger<float>.FireEvent("ShipEnterWater", (_owRigidbody.GetVelocity() - fluid.GetAttachedOWRigidbody().GetPointVelocity(base.transform.position)).magnitude);
		}
	}
}
