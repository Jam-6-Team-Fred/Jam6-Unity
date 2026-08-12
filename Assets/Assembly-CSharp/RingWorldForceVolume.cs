using UnityEngine;

public class RingWorldForceVolume : ForceVolume
{
	[SerializeField]
	private AnimationCurve _radiusToAcceleration;

	[SerializeField]
	private RingWorldController _ringWorldController;

	private Transform _transform;

	protected override void Awake()
	{
		base.Awake();
		_transform = base.transform;
	}

	public override bool GetAffectsAlignment(OWRigidbody targetBody)
	{
		return true;
	}

	public Vector3 GetFieldDirection(Vector3 worldPos)
	{
		return Vector3.ProjectOnPlane(worldPos - base.transform.position, base.transform.up);
	}

	public override Vector3 CalculateForceAccelerationOnBody(OWRigidbody targetBody)
	{
		_ringWorldController.ApplyDepartureAcceleration(targetBody);
		return CalculateForceAccelerationAtPoint(targetBody.GetWorldCenterOfMass());
	}

	public override Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos)
	{
		Vector3 vector = Vector3.ProjectOnPlane(worldPos - _transform.position, _transform.up);
		return vector.normalized * _radiusToAcceleration.Evaluate(vector.magnitude);
	}
}
