using UnityEngine;

public class PlayerCameraFluidDetector : FluidDetector
{
	private int _atmosphereFluidCount;

	private void OnValidate()
	{
		if (!_dontApplyForces)
		{
			_dontApplyForces = true;
		}
	}

	protected override float CalculateDragFactor(Vector3 relativeFluidVelocity)
	{
		return 0f;
	}

	protected override float CalculateAngularDragFactor(Vector3 relativeAngularVelocity)
	{
		return 0f;
	}

	public override void ManagedFixedUpdate()
	{
	}

	protected override void OnEnterFluidType_Internal(FluidVolume fluid)
	{
		if (fluid.GetFluidType() == FluidVolume.Type.WATER)
		{
			Vector3 vector = _owRigidbody.GetVelocity() - fluid.GetAttachedOWRigidbody().GetPointVelocity(base.transform.position);
			if (fluid.IsSpherical())
			{
				Vector3 vector2 = base.transform.position - fluid.transform.position;
				vector = Vector3.Project(vector, vector2.normalized);
			}
			GlobalMessenger<float>.FireEvent("PlayerCameraEnterWater", vector.magnitude);
		}
		if (fluid.IsAtmosphereFluid())
		{
			if (_atmosphereFluidCount == 0)
			{
				Locator.GetAudioMixer().OnEnterAtmosphere();
			}
			_atmosphereFluidCount++;
		}
	}

	protected override void OnExitFluidType_Internal(FluidVolume fluid)
	{
		if (fluid.GetFluidType() == FluidVolume.Type.WATER)
		{
			GlobalMessenger.FireEvent("PlayerCameraExitWater");
		}
		if (fluid.IsAtmosphereFluid())
		{
			if (_atmosphereFluidCount == 1 && Locator.GetAudioMixer() != null)
			{
				Locator.GetAudioMixer().OnExitAtmosphere();
			}
			_atmosphereFluidCount--;
		}
	}
}
