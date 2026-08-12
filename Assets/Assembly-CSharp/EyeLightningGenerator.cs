using System;
using UnityEngine;

public class EyeLightningGenerator : CloudLightningGenerator
{
	[Space]
	[SerializeField]
	protected float _radius = 100f;

	[SerializeField]
	protected Range _branchAngle = new Range(-45f, 45f);

	protected override Vector3 GetLightningStartPosition()
	{
		float f = UnityEngine.Random.value * (float)Math.PI * 2f;
		return new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * _radius;
	}

	protected override Vector3 GetLightningBranchPosition()
	{
		Vector3 normalized = _lastLightningPosition.normalized;
		Vector3 vector = Quaternion.AngleAxis(_branchAngle.random, Vector3.up) * normalized * _branchDistance.random;
		return _lastLightningPosition + vector;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		OWGizmos.DrawWireCircle(Vector3.zero, Vector3.up, _radius);
	}
}
