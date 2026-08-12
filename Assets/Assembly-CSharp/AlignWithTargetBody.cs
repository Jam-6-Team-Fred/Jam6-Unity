using UnityEngine;

public class AlignWithTargetBody : AlignWithDirection
{
	[SerializeField]
	private OWRigidbody _targetBody;

	public void SetTargetBody(OWRigidbody targetBody)
	{
		_targetBody = targetBody;
	}

	protected override Vector3 GetAlignmentDirection()
	{
		return _targetBody.GetWorldCenterOfMass() - _owRigidbody.GetWorldCenterOfMass();
	}

	protected override bool CheckAlignmentRequirements()
	{
		return true;
	}
}
