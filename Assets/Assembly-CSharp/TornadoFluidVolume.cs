using UnityEngine;

public class TornadoFluidVolume : FluidVolume
{
	[SerializeField]
	private Transform _tornadoPivot;

	[SerializeField]
	private float _verticalSpeed = 300f;

	[SerializeField]
	private float _angularSpeed = 10f;

	[SerializeField]
	private float _inwardSpeed = 100f;

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 vector = Vector3.ProjectOnPlane(_tornadoPivot.position - worldPosition, _tornadoPivot.up);
		float magnitude = vector.magnitude;
		float num = Mathf.Lerp(t: Mathf.InverseLerp(20f, 0f, magnitude), a: _inwardSpeed, b: 0f);
		return pointVelocity + _tornadoPivot.up * _verticalSpeed + vector.normalized * num;
	}

	public override Vector3 GetPointFluidAngularVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		if (detector.CompareTag("ShipDetector"))
		{
			return _tornadoPivot.up * _angularSpeed;
		}
		return Vector3.zero;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
	}
}
