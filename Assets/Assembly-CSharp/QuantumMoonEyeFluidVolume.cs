using UnityEngine;

public class QuantumMoonEyeFluidVolume : FluidVolume
{
	[SerializeField]
	private float _upperRadius;

	[SerializeField]
	private float _lowerRadius;

	[SerializeField]
	private float _upperDensity;

	[SerializeField]
	private float _poleAttractSpeed = 20f;

	[SerializeField]
	private float _inwardSpeed = 10f;

	public override float GetPointDensity(Vector3 worldPosition, FluidDetector detector)
	{
		float magnitude = (worldPosition - base.transform.position).magnitude;
		float num = Mathf.InverseLerp(_lowerRadius, _upperRadius, magnitude);
		return Mathf.Lerp(_density, _upperDensity, num * num);
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 planeNormal = worldPosition - base.transform.position;
		float magnitude = planeNormal.magnitude;
		float num = Mathf.InverseLerp(_lowerRadius, _upperRadius, magnitude);
		num *= num;
		Vector3 normalized = Vector3.ProjectOnPlane(base.transform.position + base.transform.up * magnitude - worldPosition, planeNormal).normalized;
		return _attachedBody.GetPointVelocity(worldPosition) + normalized * _poleAttractSpeed * num - planeNormal.normalized * _inwardSpeed * num;
	}

	public override bool CheckTriggerProbeDragIncrease(FluidDetector probeDetector)
	{
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(base.transform.position, _lowerRadius);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, _upperRadius);
	}
}
