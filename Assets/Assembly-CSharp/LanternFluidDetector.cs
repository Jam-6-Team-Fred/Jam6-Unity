using UnityEngine;

public class LanternFluidDetector : FluidDetector
{
	private void OnValidate()
	{
		if (!_dontApplyForces)
		{
			_dontApplyForces = true;
		}
	}

	protected override void Awake()
	{
		_dontApplyForces = true;
		base.Awake();
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
}
