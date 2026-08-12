using UnityEngine;

public class PolarForceVolume : ForceVolume
{
	private enum ForceMode
	{
		Polar = 0,
		Tangential = 1
	}

	[SerializeField]
	private ForceMode _fieldMode;

	[SerializeField]
	private float _acceleration = 10f;

	[SerializeField]
	private Vector3 _localAxis = Vector3.up;

	private Transform _fieldTransform;

	protected override void Awake()
	{
		base.Awake();
		_fieldTransform = base.transform;
	}

	public override bool GetAffectsAlignment(OWRigidbody targetBody)
	{
		return true;
	}

	public override Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos)
	{
		Vector3 lhs = _fieldTransform.position - worldPos;
		if (_fieldMode == ForceMode.Polar)
		{
			return lhs.normalized * _acceleration;
		}
		return Vector3.Cross(lhs, base.transform.TransformDirection(_localAxis)).normalized * _acceleration;
	}
}
