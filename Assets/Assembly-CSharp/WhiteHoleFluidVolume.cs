using UnityEngine;

public class WhiteHoleFluidVolume : FluidVolume
{
	[SerializeField]
	private float _outerRadius;

	[SerializeField]
	private float _innerRadius;

	[SerializeField]
	private AnimationCurve _densityCurve;

	[SerializeField]
	private float _flowSpeed;

	[SerializeField]
	private float _massiveFlowSpeed;

	protected override void Awake()
	{
		base.enabled = false;
		base.Awake();
	}

	public override bool CheckTriggerProbeDragIncrease(FluidDetector probeDetector)
	{
		return true;
	}

	public float GetMassiveFlowSpeed()
	{
		return _massiveFlowSpeed;
	}

	public override float GetPointDensity(Vector3 worldPosition, FluidDetector detector)
	{
		float value = Vector3.Distance(worldPosition, base.transform.position);
		float time = Mathf.InverseLerp(_innerRadius, _outerRadius, value);
		return _density * _densityCurve.Evaluate(time);
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		if (detector.CompareName(Detector.Name.Ship))
		{
			worldPosition = Locator.GetShipBody().GetPosition();
		}
		float num = ((detector.GetAttachedOWRigidbody().GetMass() > 10f) ? _massiveFlowSpeed : _flowSpeed);
		return _attachedBody.GetPointVelocity(worldPosition) + (worldPosition - base.transform.position).normalized * num;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(Vector3.zero, _outerRadius);
			Gizmos.DrawWireSphere(Vector3.zero, _innerRadius);
		}
	}
}
