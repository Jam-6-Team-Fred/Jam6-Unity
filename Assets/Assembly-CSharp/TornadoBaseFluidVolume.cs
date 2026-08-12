using UnityEngine;

public class TornadoBaseFluidVolume : FluidVolume
{
	[SerializeField]
	private float _flowSpeed;

	[SerializeField]
	private float _innerRadius;

	[SerializeField]
	private float _outerRadius;

	private void OnValidate()
	{
		if (_innerRadius > _outerRadius)
		{
			_innerRadius = _outerRadius;
		}
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 vector = worldPosition - base.transform.position;
		float magnitude = Vector3.ProjectOnPlane(vector, base.transform.up).magnitude;
		if (_flowSpeed > 0f && magnitude >= _innerRadius)
		{
			pointVelocity += vector.normalized * _flowSpeed * Mathf.InverseLerp(_outerRadius, _innerRadius, magnitude);
		}
		return pointVelocity;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		OWGizmos.DrawWireCircle(base.transform.position, base.transform.up, _innerRadius);
		OWGizmos.DrawWireCircle(base.transform.position, base.transform.up, _outerRadius);
	}
}
