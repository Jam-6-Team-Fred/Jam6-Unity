using UnityEngine;

public class AlignWithSingleForce : AlignWithDirection
{
	[SerializeField]
	protected float _fieldStrengthThreshold;

	protected ConstantForceDetector _forceDetector;

	protected override void Awake()
	{
		base.Awake();
		_forceDetector = this.GetRequiredComponentInChildren<ConstantForceDetector>();
	}

	protected override Vector3 GetAlignmentDirection()
	{
		return _forceDetector.GetForceAcceleration();
	}

	protected override bool CheckAlignmentRequirements()
	{
		return _forceDetector.GetForceAcceleration().magnitude > _fieldStrengthThreshold;
	}
}
