using UnityEngine;

public class CapsuleFogWarpVolume : FogWarpVolume
{
	[SerializeField]
	protected CapsuleFogWarpVolume _linkedCapsuleWarpVolume;

	[SerializeField]
	protected Vector3 _localExitDirection = Vector3.forward;

	[SerializeField]
	protected float _radius;

	[SerializeField]
	protected float _height;

	[SerializeField]
	protected float _minExitSpeed;

	public override float GetFogThickness()
	{
		return 20f;
	}

	public override bool IsOuterWarpVolume()
	{
		return false;
	}

	public override bool UseFastFogFade()
	{
		return true;
	}

	public override void PropagateCanvasMarkerOutwards(CanvasMarker marker, bool addMarker, float warpDist = 0f)
	{
	}

	public override float CheckWarpProximity(FogWarpDetector detector)
	{
		Vector3 vector = detector.transform.position - base.transform.position;
		float magnitude = vector.magnitude;
		if (magnitude < _height)
		{
			Vector3 worldFacing = GetWorldFacing();
			Vector3 vector2 = Vector3.Project(vector, worldFacing);
			if ((vector - vector2).magnitude < _radius && Vector3.Dot(worldFacing, vector) < 0f)
			{
				WarpDetector(detector, _linkedCapsuleWarpVolume);
				return 0f;
			}
		}
		return magnitude;
	}

	protected override void RepositionWarpedBody(OWRigidbody body, Vector3 localRelVelocity, Vector3 localPos, Quaternion localRot)
	{
		Quaternion quaternion = base.transform.rotation * localRot;
		Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.ProjectOnPlane(quaternion * Vector3.up, GetWorldFacing()), base.transform.up);
		body.WarpToPositionRotation(base.transform.position + GetWorldFacing(), quaternion2 * quaternion);
		float num = Mathf.Max(_minExitSpeed, localRelVelocity.magnitude);
		body.SetVelocity(GetWorldFacing() * num + _attachedBody.GetVelocity());
	}

	private Vector3 GetWorldFacing()
	{
		return base.transform.TransformDirection(_localExitDirection);
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new ColorHSV(300f, 0.8f, 0.8f).ToColorRGB();
			OWGizmos.DrawWireCircle(base.transform.position, GetWorldFacing(), _radius);
			Gizmos.DrawLine(base.transform.position, base.transform.position + GetWorldFacing() * _height);
		}
	}
}
