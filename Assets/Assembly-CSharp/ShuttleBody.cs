using UnityEngine;

public class ShuttleBody : OWRigidbody
{
	private OWRigidbody _playerBody;

	protected override void Start()
	{
		base.Start();
		_playerBody = Locator.GetPlayerTransform().GetRequiredComponent<OWRigidbody>();
	}

	public override void SetPosition(Vector3 worldPosition)
	{
		if (PlayerState.IsInsideShuttle())
		{
			Vector3 position = base.transform.InverseTransformPoint(_playerBody.transform.position);
			base.SetPosition(worldPosition);
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			_playerBody.SetPosition(base.transform.TransformPoint(position));
		}
		else
		{
			base.SetPosition(worldPosition);
		}
	}

	public override void SetRotation(Quaternion rotation)
	{
		if (PlayerState.IsInsideShuttle())
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
		if (PlayerState.IsInsideShuttle())
		{
			Vector3 direction = base.transform.InverseTransformDirection(_playerBody.GetVelocity() - GetPointVelocity(_playerBody.transform.position));
			base.SetVelocity(newVelocity);
			_playerBody.SetVelocity(GetPointVelocity(_playerBody.transform.position) + base.transform.TransformDirection(direction));
			_lastVelocity = (_currentVelocity = newVelocity);
		}
		else
		{
			base.SetVelocity(newVelocity);
		}
	}
}
