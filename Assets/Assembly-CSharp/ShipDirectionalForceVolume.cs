using UnityEngine;

public class ShipDirectionalForceVolume : ForceVolume
{
	[SerializeField]
	private Vector3 _fieldDirection;

	[SerializeField]
	private float _fieldMagnitude;

	private OWRigidbody _playerBody;

	private bool _insideSpeedLimiter;

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger.AddListener("ShipEnterSpeedLimiter", OnShipEnterSpeedLimiter);
		GlobalMessenger.AddListener("ShipExitSpeedLimiter", OnShipExitSpeedLimiter);
	}

	private void Start()
	{
		_playerBody = Locator.GetPlayerTransform().GetComponent<OWRigidbody>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger.RemoveListener("ShipEnterSpeedLimiter", OnShipEnterSpeedLimiter);
		GlobalMessenger.RemoveListener("ShipExitSpeedLimiter", OnShipExitSpeedLimiter);
	}

	public override bool GetAffectsAlignment(OWRigidbody targetBody)
	{
		return true;
	}

	public override Vector3 CalculateForceAccelerationOnBody(OWRigidbody targetBody)
	{
		Vector3 vector = _attachedBody.GetAcceleration();
		if (vector.magnitude > 20f)
		{
			vector -= vector.normalized * 20f;
		}
		else
		{
			vector = Vector3.zero;
		}
		if (!_insideSpeedLimiter)
		{
			targetBody.AddAcceleration(vector);
		}
		if (targetBody == _playerBody && PlayerState.IsInsideShip() && !PlayerState.IsAttached())
		{
			Vector3 force = (base.transform.TransformDirection(-_fieldDirection.normalized * _fieldMagnitude) - vector) * targetBody.GetMass();
			_attachedBody.AddForce(force, targetBody.GetWorldCenterOfMass());
		}
		return CalculateForceAccelerationAtPoint(targetBody.GetWorldCenterOfMass());
	}

	public override Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos)
	{
		return base.transform.TransformDirection(_fieldDirection.normalized * _fieldMagnitude);
	}

	private void OnShipEnterSpeedLimiter()
	{
		_insideSpeedLimiter = true;
	}

	private void OnShipExitSpeedLimiter()
	{
		_insideSpeedLimiter = false;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.TransformDirection(_fieldDirection.normalized * _fieldMagnitude));
		Gizmos.color = Color.cyan;
	}
}
