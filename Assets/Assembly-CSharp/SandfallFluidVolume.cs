using UnityEngine;

[RequireComponent(typeof(CapsuleShape))]
public class SandfallFluidVolume : FluidVolume
{
	[SerializeField]
	private float _verticalSpeed;

	[SerializeField]
	private float _inwardSpeed;

	[SerializeField]
	private float _falloffLength = 2f;

	private CapsuleShape _capsule;

	private SandLevelController _sandLevelController;

	private float _sandfallBottomAltitude;

	private void OnValidate()
	{
		if (_fluidType != Type.SAND)
		{
			_fluidType = Type.SAND;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_capsule = GetComponent<CapsuleShape>();
	}

	protected override void Start()
	{
		base.Start();
		_sandLevelController = GetComponentInParent<AstroObject>().GetSandLevelController();
		if (_sandLevelController != null)
		{
			_sandfallBottomAltitude = Vector3.Distance(base.transform.position, _sandLevelController.transform.position) - _capsule.height;
		}
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 vector = base.transform.InverseTransformPoint(worldPosition);
		Vector3 direction = new Vector3(vector.x, 0f, vector.z);
		float num = ((_sandLevelController == null) ? 0f : Mathf.Max(0f, _sandLevelController.GetRadius() - _sandfallBottomAltitude));
		float num2 = 0f - _capsule.height + num;
		float num3 = ((_falloffLength > 0f) ? Mathf.InverseLerp(num2, num2 + _falloffLength, vector.y) : 1f);
		float num4 = ((num3 < 1f) ? 0f : Mathf.InverseLerp(0f, _capsule.radius, direction.magnitude));
		Vector3 result = pointVelocity + base.transform.TransformDirection(direction).normalized * num4 * (0f - _inwardSpeed) + base.transform.up * _verticalSpeed * num3;
		if (detector.AffectsRumble())
		{
			RumbleManager.AddFluidRumble(_fluidType, 0.5f);
		}
		return result;
	}

	private void OnDrawGizmosSelected()
	{
		_capsule = GetComponent<CapsuleShape>();
		Vector3 vector = base.transform.TransformPoint(new Vector3(0f, 0f - _capsule.height, 0f));
		Gizmos.color = Color.red;
		Gizmos.DrawLine(vector, vector + base.transform.up * _falloffLength);
	}
}
