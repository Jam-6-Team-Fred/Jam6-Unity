using UnityEngine;

public class SimpleFluidVolume : FluidVolume
{
	private enum FlowType
	{
		Linear = 0,
		Attractive = 1,
		Repulsive = 2
	}

	[SerializeField]
	private FlowType _flowType;

	[SerializeField]
	private float _flowSpeed;

	[SerializeField]
	private Vector3 _localLinearFlow = Vector3.zero;

	[Space]
	[SerializeField]
	private bool _rumble;

	[SerializeField]
	private float _rumbleScale;

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		if (_flowType == FlowType.Linear)
		{
			pointVelocity += base.transform.TransformDirection(_localLinearFlow.normalized * _flowSpeed);
		}
		else
		{
			Vector3 vector = worldPosition - base.transform.position;
			if (_flowType == FlowType.Attractive)
			{
				pointVelocity -= vector.normalized * _flowSpeed;
			}
			else if (_flowType == FlowType.Repulsive)
			{
				pointVelocity += vector.normalized * _flowSpeed;
			}
		}
		if (_rumble && detector.AffectsRumble())
		{
			RumbleManager.AddFluidRumble(_fluidType, _rumbleScale);
		}
		return pointVelocity;
	}

	private void OnDrawGizmosSelected()
	{
		if (_flowType == FlowType.Linear)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.TransformDirection(_localLinearFlow) * _flowSpeed);
		}
	}
}
