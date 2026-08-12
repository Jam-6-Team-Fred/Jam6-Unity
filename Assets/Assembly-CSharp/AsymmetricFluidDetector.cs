using UnityEngine;

public class AsymmetricFluidDetector : FluidDetector
{
	[SerializeField]
	protected Vector3 _dragFactor = Vector3.one;

	[SerializeField]
	private Vector3 _angularDragVector = Vector3.one;

	[SerializeField]
	private float _angularDragFactor = 1f;

	private Vector3 _worldDragFactor;

	private Vector3 _worldAngularDragFactor;

	protected override void Awake()
	{
		base.Awake();
		if (!OWMath.ApproxEquals(_angularDragFactor, 1f))
		{
			Debug.LogError("Need to convert angular drag factor from float to vector", this);
			Debug.Break();
		}
	}

	public void SetDragFactor(Vector3 dragFactor)
	{
		_dragFactor = dragFactor;
	}

	public Vector3 GetDragFactor()
	{
		return _dragFactor;
	}

	public override void ManagedFixedUpdate()
	{
		_worldDragFactor = _owRigidbody.transform.TransformDirection(_dragFactor);
		_worldAngularDragFactor = _owRigidbody.transform.TransformDirection(_angularDragVector);
		base.ManagedFixedUpdate();
	}

	protected override float CalculateDragFactor(Vector3 relativeFluidVelocity)
	{
		return Vector3.Project(_worldDragFactor, -relativeFluidVelocity.normalized).magnitude;
	}

	protected override float CalculateAngularDragFactor(Vector3 relativeAngularVelocity)
	{
		return Vector3.Project(_worldAngularDragFactor, -relativeAngularVelocity.normalized).magnitude;
	}
}
