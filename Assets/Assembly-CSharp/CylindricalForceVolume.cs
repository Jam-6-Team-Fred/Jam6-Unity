using UnityEngine;

public class CylindricalForceVolume : ForceVolume
{
	[SerializeField]
	private float _acceleration = 10f;

	[SerializeField]
	private Vector3 _localAxis = Vector3.up;

	[SerializeField]
	private bool _playGravityCrystalAudio;

	private Transform _transform;

	public override bool GetPlayGravityCrystalAudio()
	{
		return _playGravityCrystalAudio;
	}

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
		return Vector3.ProjectOnPlane(base.transform.position - worldPos, base.transform.TransformDirection(_localAxis)) * Mathf.Sign(_acceleration);
	}

	public override Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos)
	{
		return Vector3.ProjectOnPlane(_transform.position - worldPos, _transform.TransformDirection(_localAxis)).normalized * _acceleration;
	}
}
