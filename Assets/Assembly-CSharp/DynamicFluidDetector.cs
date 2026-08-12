using UnityEngine;

public class DynamicFluidDetector : FluidDetector
{
	[SerializeField]
	protected float _dragFactor = 1f;

	[SerializeField]
	protected float _angularDragFactor = 1f;

	public void SetDragFactor(float dragFactor)
	{
		_dragFactor = dragFactor;
	}

	protected override float CalculateDragFactor(Vector3 relativeFluidVelocity)
	{
		return _dragFactor;
	}

	protected override float CalculateAngularDragFactor(Vector3 relativeAngularVelocity)
	{
		return _angularDragFactor;
	}
}
