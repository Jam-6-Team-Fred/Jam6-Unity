using UnityEngine;

public class AlignWithForce : AlignWithDirection
{
	[SerializeField]
	protected float _fieldStrengthThreshold;

	protected AlignmentForceDetector _forceDetector;

	protected override void Awake()
	{
		base.Awake();
		_forceDetector = this.GetRequiredComponentInChildren<AlignmentForceDetector>();
	}

	protected override Vector3 GetAlignmentDirection()
	{
		return _forceDetector.GetAlignmentAcceleration();
	}

	protected override bool CheckAlignmentRequirements()
	{
		return _forceDetector.GetAlignmentAcceleration().magnitude > _fieldStrengthThreshold;
	}
}
