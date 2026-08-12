using UnityEngine;

public class AlignmentForceDetector : DynamicForceDetector
{
	private Vector3 _aligmentAccel;

	private Vector3 _alignmentAngularVelocity;

	public Vector3 GetAlignmentAcceleration()
	{
		return _aligmentAccel;
	}

	public Vector3 GetAlignmentAngularVelocity()
	{
		return _alignmentAngularVelocity;
	}

	protected override void AccumulateAcceleration()
	{
		_netAcceleration = Vector3.zero;
		_aligmentAccel = Vector3.zero;
		_alignmentAngularVelocity = Vector3.zero;
		int num = 0;
		for (int num2 = _activeVolumes.Count - 1; num2 >= 0; num2--)
		{
			ForceVolume forceVolume = _activeVolumes[num2] as ForceVolume;
			Vector3 vector = forceVolume.CalculateForceAccelerationOnBody(_attachedBody);
			_netAcceleration += vector * _fieldMultiplier;
			int alignmentPriority = forceVolume.GetAlignmentPriority();
			if (forceVolume.GetAffectsAlignment(_attachedBody))
			{
				if (alignmentPriority > num)
				{
					_aligmentAccel = vector;
					_alignmentAngularVelocity = forceVolume.GetAttachedOWRigidbody().GetAngularVelocity();
					num = alignmentPriority;
				}
				else if (alignmentPriority == num)
				{
					_aligmentAccel += vector;
					_alignmentAngularVelocity += forceVolume.GetAttachedOWRigidbody().GetAngularVelocity();
				}
			}
		}
		if (_activeInheritedDetector != null)
		{
			_netAcceleration += _activeInheritedDetector.GetForceAcceleration();
		}
	}
}
