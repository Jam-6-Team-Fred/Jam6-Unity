using UnityEngine;

public class RingRiverRuleset : RulesetVolume
{
	[SerializeField]
	private float _lowerFlowSpeed = 5f;

	[SerializeField]
	private float _upperFlowSpeed = 10f;

	[SerializeField]
	private RingRiverFluidVolume _fluidVolume;

	public Vector3 CalculateJetpackCounterAcceleration(Vector3 localAcceleration, Transform jetpackTransform, OWRigidbody playerBody)
	{
		Vector3 pointFlowOnlyVelocity = _fluidVolume.GetPointFlowOnlyVelocity(jetpackTransform.position);
		float magnitude = pointFlowOnlyVelocity.magnitude;
		if (PlayerState.InUndertowVolume() && !_fluidVolume.IsInCalmVolume(Detector.Name.Player))
		{
			if (!_fluidVolume.IsPlayerPinnedByUndertow())
			{
				return -localAcceleration;
			}
			return Vector3.zero;
		}
		if (magnitude > _lowerFlowSpeed)
		{
			Vector3 vector = jetpackTransform.TransformDirection(localAcceleration);
			if (Vector3.Dot(vector, pointFlowOnlyVelocity) > 0f)
			{
				float num = Mathf.InverseLerp(_lowerFlowSpeed, _upperFlowSpeed, magnitude);
				Vector3 vector2 = Vector3.Project(vector, pointFlowOnlyVelocity.normalized);
				return jetpackTransform.InverseTransformDirection(-vector2 * num);
			}
		}
		return Vector3.zero;
	}
}
