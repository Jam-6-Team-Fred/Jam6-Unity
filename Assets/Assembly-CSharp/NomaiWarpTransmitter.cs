using UnityEngine;

public class NomaiWarpTransmitter : NomaiWarpPlatform
{
	[SerializeField]
	private float _alignmentWindow = 5f;

	[SerializeField]
	private bool _upsideDown;

	private NomaiWarpReceiver _targetReceiver;

	protected override void Start()
	{
		base.Start();
		_targetReceiver = Locator.GetWarpReceiver(GetFrequency());
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_objectsOnPlatform.Count > 0 && !IsBlackHoleOpen() && _targetReceiver != null && _targetReceiver.gameObject.activeInHierarchy)
		{
			float viewAngleToTarget = GetViewAngleToTarget();
			float num = 0.5f * ((TimeLoop.GetSecondsRemaining() < 30f) ? 5f : _alignmentWindow);
			if (viewAngleToTarget <= num)
			{
				OpenBlackHole(_targetReceiver);
			}
		}
	}

	protected override void ReceiveWarpedBody(OWRigidbody body, NomaiWarpPlatform startPlatform)
	{
		base.ReceiveWarpedBody(body, startPlatform);
	}

	public float GetViewAngleToTarget()
	{
		Vector3 from = _targetReceiver.GetAlignmentTarget().position - base.transform.position;
		Vector3 to = (_upsideDown ? (-base.transform.up) : base.transform.up);
		return Vector3.Angle(from, to);
	}
}
