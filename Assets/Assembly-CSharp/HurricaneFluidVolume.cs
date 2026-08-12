using UnityEngine;

[RequireComponent(typeof(ConeShape))]
public class HurricaneFluidVolume : FluidVolume
{
	[SerializeField]
	private float _flowSpeed = 200f;

	[SerializeField]
	private float _wallApproachThickness = 100f;

	[SerializeField]
	private float _wallThickness = 100f;

	private ConeShape _coneShape;

	private void OnValidate()
	{
		ConeShape component = GetComponent<ConeShape>();
		if (component.center != Vector3.zero)
		{
			component.center = Vector3.zero;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_coneShape = GetComponent<ConeShape>();
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 vector = Vector3.ProjectOnPlane(worldPosition - base.transform.position, base.transform.up);
		float magnitude = vector.magnitude;
		Vector3 vector2 = base.transform.InverseTransformPoint(worldPosition);
		float t = Mathf.InverseLerp((0f - _coneShape.height) * 0.5f, _coneShape.height * 0.5f, vector2.y);
		float num = Mathf.Lerp(_coneShape.bottomRadius, _coneShape.topRadius, t);
		if (magnitude > num - (_wallApproachThickness + _wallThickness))
		{
			float num2 = Mathf.InverseLerp(num, num - _wallApproachThickness, magnitude);
			pointVelocity += vector.normalized * _flowSpeed * num2;
			if (detector.GetType() == typeof(ProbeFluidDetector))
			{
				((ProbeFluidDetector)detector).SetHurricaneDrag();
			}
		}
		return pointVelocity;
	}
}
