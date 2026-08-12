using UnityEngine;

public class AlignToSurfaceFluidDetector : AsymmetricFluidDetector
{
	[SerializeField]
	private Vector3[] _localAlignmentCheckPoints;

	[SerializeField]
	private FluidVolume _alignmentFluid;

	private ForceDetector _forceDetector;

	private bool _inAlignmentFluid;

	protected override void Awake()
	{
		_forceDetector = GetComponent<ForceDetector>();
		base.Awake();
	}

	public override void ManagedFixedUpdate()
	{
		base.ManagedFixedUpdate();
		if (_inAlignmentFluid)
		{
			Vector3 rhs = -_forceDetector.GetForceAcceleration();
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < _localAlignmentCheckPoints.Length; i++)
			{
				Vector3 vector = base.transform.TransformPoint(_localAlignmentCheckPoints[i]);
				float depthAtPosition = _alignmentFluid.GetDepthAtPosition(vector);
				zero += Vector3.Cross(vector - base.transform.position, rhs).normalized * depthAtPosition;
			}
			_owRigidbody.AddAngularVelocityChange(zero * Time.deltaTime);
		}
	}

	protected override void OnVolumeActivated(PriorityVolume volume)
	{
		base.OnVolumeActivated(volume);
		if (volume == _alignmentFluid)
		{
			_inAlignmentFluid = true;
		}
	}

	protected override void OnVolumeDeactivated(PriorityVolume volume)
	{
		base.OnVolumeDeactivated(volume);
		if (volume == _alignmentFluid)
		{
			_inAlignmentFluid = false;
		}
	}

	private void OnDrawGizmos()
	{
		if (_localAlignmentCheckPoints != null)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < _localAlignmentCheckPoints.Length; i++)
			{
				Gizmos.DrawSphere(base.transform.TransformPoint(_localAlignmentCheckPoints[i]), 0.1f);
			}
		}
	}
}
