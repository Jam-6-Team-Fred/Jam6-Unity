using UnityEngine;

public class CylinderFluidVolume : FluidVolume
{
	[SerializeField]
	private float _verticalSpeed;

	[SerializeField]
	private float _inwardSpeed;

	[SerializeField]
	private float _falloffRadius;

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 up = base.transform.up;
		Vector3 vector = pointVelocity + up * _verticalSpeed;
		Vector3 vector2 = Vector3.ProjectOnPlane(base.transform.position - worldPosition, up);
		return vector + vector2.normalized * _inwardSpeed * Mathf.InverseLerp(0f, _falloffRadius, vector2.magnitude);
	}
}
