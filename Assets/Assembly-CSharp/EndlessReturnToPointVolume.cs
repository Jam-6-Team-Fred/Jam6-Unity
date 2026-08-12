using UnityEngine;

public class EndlessReturnToPointVolume : EndlessTriggerVolume
{
	[SerializeField]
	private Transform _returnPoint;

	[SerializeField]
	private Vector3 _localReturnVelocity = Vector3.zero;

	private OWRigidbody _returnPointBody;

	protected override void Awake()
	{
		base.Awake();
		_returnPointBody = _returnPoint.GetAttachedOWRigidbody();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void WarpBody(OWRigidbody body)
	{
		body.WarpToPositionRotation(_returnPoint.position, _returnPoint.rotation);
		body.SetVelocity(_returnPointBody.GetPointVelocity(_returnPoint.position) + base.transform.TransformDirection(_localReturnVelocity));
	}
}
