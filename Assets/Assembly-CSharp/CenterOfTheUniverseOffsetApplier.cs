using UnityEngine;

public class CenterOfTheUniverseOffsetApplier : MonoBehaviour
{
	[SerializeField]
	private OWRigidbody _body;

	public void Init(OWRigidbody body)
	{
		_body = body;
	}

	private void OnEnable()
	{
		FixedLateUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedLateUpdateManager.Unregister(this);
	}

	public void ManagedFixedLateUpdate(Vector3 cotuOffsetVelocity)
	{
		_body.AddVelocityChange(cotuOffsetVelocity);
	}
}
