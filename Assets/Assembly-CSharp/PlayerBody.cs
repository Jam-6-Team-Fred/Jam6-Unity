using UnityEngine;

public class PlayerBody : OWRigidbody
{
	private Rigidbody _activeRigidbody;

	protected override void Awake()
	{
		base.Awake();
		_activeRigidbody = _rigidbody;
		GlobalMessenger<OWRigidbody>.AddListener("AttachPlayerToPoint", OnAttachPlayerToPoint);
		GlobalMessenger.AddListener("DetachPlayerFromPoint", OnDetachPlayerFromPoint);
		GlobalMessenger<OWRigidbody>.AddListener("ShipCockpitDetached", OnShipCockpitDetached);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger<OWRigidbody>.RemoveListener("AttachPlayerToPoint", OnAttachPlayerToPoint);
		GlobalMessenger.RemoveListener("DetachPlayerFromPoint", OnDetachPlayerFromPoint);
		GlobalMessenger<OWRigidbody>.RemoveListener("ShipCockpitDetached", OnShipCockpitDetached);
	}

	public override void WarpToPositionRotation(Vector3 worldPosition, Quaternion worldRotation)
	{
		base.WarpToPositionRotation(worldPosition, worldRotation);
		GlobalMessenger.FireEvent("WarpPlayer");
	}

	public override void SetPosition(Vector3 worldPosition)
	{
		base.SetPosition(worldPosition);
		GlobalMessenger.FireEvent("PlayerRepositioned");
	}

	public override Vector3 GetVelocity_Internal()
	{
		return _activeRigidbody.GetPointVelocity(base.transform.position);
	}

	public override void ManagedFixedUpdate(float invFixedDeltaTime, Vector3 cotuStaticFrameVel)
	{
		_lastVelocity = _currentVelocity;
		_currentVelocity = _activeRigidbody.GetPointVelocity(_activeRigidbody.transform.TransformPoint(_rigidbody.centerOfMass)) - cotuStaticFrameVel;
		_lastAngularVelocity = _currentAngularVelocity;
		_currentAngularVelocity = _activeRigidbody.angularVelocity;
		_lastAccel = _currentAccel;
		_currentAccel = (_currentVelocity - _lastVelocity) * invFixedDeltaTime;
		_kinematicStateNewlyChanged = false;
	}

	private void OnAttachPlayerToPoint(OWRigidbody attachBody)
	{
		_activeRigidbody = attachBody.GetRigidbody();
	}

	private void OnDetachPlayerFromPoint()
	{
		_activeRigidbody = _rigidbody;
	}

	private void OnShipCockpitDetached(OWRigidbody cockpitBody)
	{
		if (PlayerState.AtFlightConsole())
		{
			_activeRigidbody = cockpitBody.GetRigidbody();
		}
	}
}
