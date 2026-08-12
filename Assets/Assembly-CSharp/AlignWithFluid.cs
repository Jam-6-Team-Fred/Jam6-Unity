using UnityEngine;

public class AlignWithFluid : AlignWithDirection
{
	[SerializeField]
	protected float _fluidSpeedThreshold;

	private FluidDetector _fluidDetector;

	protected override void Awake()
	{
		base.Awake();
		_fluidDetector = this.GetRequiredComponentInChildren<FluidDetector>();
	}

	protected override Vector3 GetAlignmentDirection()
	{
		return -_fluidDetector.GetAlignmentFluidVelocity().normalized;
	}

	protected override bool CheckAlignmentRequirements()
	{
		return _fluidDetector.GetAlignmentFluidVelocity().magnitude > _fluidSpeedThreshold;
	}
}
