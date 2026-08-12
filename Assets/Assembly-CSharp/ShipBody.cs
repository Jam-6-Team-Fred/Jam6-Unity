using UnityEngine;

public class ShipBody : OWRigidbody
{
	private OWRigidbody _playerBody;

	protected override void Start()
	{
		base.Start();
		_playerBody = Locator.GetPlayerTransform().GetRequiredComponent<OWRigidbody>();
	}

	public override void SetPosition(Vector3 worldPosition)
	{
		if (PlayerState.IsInsideShip() && !PlayerState.IsAttached())
		{
			Vector3 position = base.transform.InverseTransformPoint(_playerBody.transform.position);
			base.SetPosition(worldPosition);
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			_playerBody.SetPosition(base.transform.TransformPoint(position));
		}
		else if (PlayerState.IsAttached())
		{
			base.SetPosition(worldPosition);
			GlobalMessenger.FireEvent("PlayerRepositioned");
		}
		else
		{
			base.SetPosition(worldPosition);
		}
	}

	public override void SetRotation(Quaternion rotation)
	{
		if (PlayerState.IsInsideShip() && !PlayerState.IsAttached())
		{
			Vector3 position = base.transform.InverseTransformPoint(_playerBody.transform.position);
			Quaternion quaternion = Quaternion.Inverse(base.transform.rotation) * _playerBody.transform.rotation;
			base.SetRotation(rotation);
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			_playerBody.transform.position = base.transform.TransformPoint(position);
			_playerBody.transform.rotation = base.transform.rotation * quaternion;
		}
		else
		{
			base.SetRotation(rotation);
		}
	}

	public override void SetVelocity(Vector3 newVelocity)
	{
		base.SetVelocity(newVelocity);
		if (PlayerState.IsInsideShip() && !PlayerState.IsAttached())
		{
			_playerBody.SetVelocity(newVelocity);
			_lastVelocity = (_currentVelocity = newVelocity);
		}
	}
}
