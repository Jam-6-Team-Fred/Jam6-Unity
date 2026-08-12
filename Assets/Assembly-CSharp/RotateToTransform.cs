using UnityEngine;

public class RotateToTransform : RotateToPoint
{
	[SerializeField]
	private Transform _targetTransform;

	private void FixedUpdate()
	{
		_target = _targetTransform.position;
		IncrementalRotate(Time.fixedDeltaTime);
	}

	public Transform GetTargetTransform()
	{
		return _targetTransform;
	}
}
