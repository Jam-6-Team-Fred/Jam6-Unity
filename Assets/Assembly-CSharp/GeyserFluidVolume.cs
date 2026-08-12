using UnityEngine;

[RequireComponent(typeof(CapsuleShape))]
public class GeyserFluidVolume : FluidVolume
{
	private CapsuleShape _shape;

	[SerializeField]
	private float _directionalFlowSpeed;

	[SerializeField]
	private float _attractionalFlowSpeed;

	[SerializeField]
	private float _climbSpeed = 10f;

	[SerializeField]
	private float _attractionalRadiusFallOff = 0.5f;

	private bool _firing;

	private float _maxHeight;

	private void OnValidate()
	{
		if (_fluidType != Type.GEYSER)
		{
			_fluidType = Type.GEYSER;
		}
		if (_alignmentFluid)
		{
			_alignmentFluid = false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_shape = GetComponent<CapsuleShape>();
		_firing = false;
		_maxHeight = _shape.height;
		_shape.height = 0.001f;
		_shape.center = new Vector3(_shape.center.x, _shape.height * 0.5f, _shape.center.z);
		base.enabled = false;
	}

	public override bool CheckTriggerProbeDragIncrease(FluidDetector probeDetector)
	{
		return true;
	}

	public void SetFiring(bool firing, bool resetHeight = true)
	{
		if (resetHeight)
		{
			_shape.height = 0.001f;
			_shape.center = new Vector3(_shape.center.x, _shape.height * 0.5f, _shape.center.z);
		}
		_firing = firing;
		SetVolumeActivation(_firing);
		base.enabled = _firing;
	}

	public override float GetPointDensity(Vector3 worldPosition, FluidDetector detector)
	{
		if (detector.CompareName(Detector.Name.Probe))
		{
			return _density * 50f;
		}
		return _density;
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		CalculateCapsuleEndpoints(out var capsuleStart, out var capsuleEnd);
		Vector3 vector = OWMath.ClosestPointOnSegment(worldPosition, capsuleStart, capsuleEnd);
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		pointVelocity += Vector3.Normalize(capsuleEnd - capsuleStart) * _directionalFlowSpeed;
		Vector3 vector2 = vector - worldPosition;
		float num = Mathf.InverseLerp(0f, _attractionalRadiusFallOff, vector2.magnitude);
		bool flag = detector.CompareName(Detector.Name.Probe);
		pointVelocity += vector2.normalized * num * (flag ? (_attractionalFlowSpeed / 10f) : _attractionalFlowSpeed);
		if (detector.AffectsRumble())
		{
			RumbleManager.AddFluidRumble(Type.WATER, 1f);
		}
		return pointVelocity;
	}

	public float GetMaximumHeight()
	{
		return base.transform.lossyScale.y * _maxHeight;
	}

	public float GetHeight()
	{
		return _shape.height * base.transform.lossyScale.y;
	}

	protected void FixedUpdate()
	{
		_shape.height = Mathf.MoveTowards(_shape.height, _maxHeight, _climbSpeed * Time.deltaTime / base.transform.lossyScale.y);
		_shape.center = new Vector3(_shape.center.x, _shape.height * 0.5f, _shape.center.z);
	}

	protected void CalculateCapsuleEndpoints(out Vector3 capsuleStart, out Vector3 capsuleEnd)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		Vector3 vector = Vector3.zero;
		if (_shape.direction == 0)
		{
			vector = Vector3.right * _shape.height * 0.5f;
		}
		if (_shape.direction == 1)
		{
			vector = Vector3.up * _shape.height * 0.5f;
		}
		if (_shape.direction == 2)
		{
			vector = Vector3.forward * _shape.height * 0.5f;
		}
		capsuleStart = matrix4x.MultiplyPoint3x4(_shape.center - vector);
		capsuleEnd = matrix4x.MultiplyPoint3x4(_shape.center + vector);
	}

	protected void OnDrawGizmosSelected()
	{
		if (_shape == null) _shape = GetComponent<CapsuleShape>(); // CHANGED
		CalculateCapsuleEndpoints(out var capsuleStart, out var capsuleEnd);
		Gizmos.color = Color.red;
		Gizmos.DrawRay(capsuleStart, Vector3.Normalize(capsuleEnd - capsuleStart) * _shape.height);
	}
}
