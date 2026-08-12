using UnityEngine;

public abstract class SphericalFogWarpVolume : FogWarpVolume
{
	[Space]
	[SerializeField]
	protected float _warpRadius;

	[SerializeField]
	protected float _exitRadius;

	private SphericalFogWarpExit[] _exits;

	protected override void OnAwake()
	{
		_exits = GetComponentsInChildren<SphericalFogWarpExit>();
	}

	public override bool IsProbeOnly()
	{
		return _warpRadius < 10f;
	}

	public float GetWarpRadius()
	{
		return _warpRadius;
	}

	public float GetExitRadius()
	{
		return _exitRadius;
	}

	public override float CheckWarpProximity(FogWarpDetector detector)
	{
		float num = (detector.transform.position - base.transform.position).magnitude - _warpRadius;
		bool flag = _exitRadius < _warpRadius;
		if ((flag && num > 0f) || (!flag && num < 0f))
		{
			WarpDetector(detector, GetLinkedFogWarpVolume());
			return 0f;
		}
		return num;
	}

	public Vector3 FindClosestWarpExitPosition(Vector3 worldPos)
	{
		SphericalFogWarpExit sphericalFogWarpExit = FindClosestWarpExit(worldPos);
		if (!(sphericalFogWarpExit != null))
		{
			return worldPos;
		}
		return GetExitPosition(sphericalFogWarpExit);
	}

	public SphericalFogWarpExit FindClosestWarpExit(Vector3 worldPos)
	{
		float num = float.PositiveInfinity;
		SphericalFogWarpExit result = null;
		for (int i = 0; i < _exits.Length; i++)
		{
			float num2 = Vector3.Distance(base.transform.position + _exits[i].transform.up * _exitRadius, worldPos);
			if (num2 < num)
			{
				result = _exits[i];
				num = num2;
			}
		}
		return result;
	}

	protected override void RepositionWarpedBody(OWRigidbody body, Vector3 localRelVelocity, Vector3 localPos, Quaternion localRot)
	{
		Vector3 vector = base.transform.TransformDirection(localRelVelocity);
		Vector3 vector2 = base.transform.TransformPoint(localPos.normalized * _exitRadius);
		Quaternion quaternion = base.transform.rotation * localRot;
		SphericalFogWarpExit sphericalFogWarpExit = FindClosestWarpExit(vector2);
		Vector3 vector3 = vector2;
		if (sphericalFogWarpExit != null)
		{
			vector3 = GetExitPosition(sphericalFogWarpExit);
			Vector3 fromDirection = vector2 - base.transform.position;
			Vector3 vector4 = vector3 - base.transform.position;
			Quaternion quaternion2 = Quaternion.FromToRotation(fromDirection, vector4);
			vector = quaternion2 * vector;
			quaternion = quaternion2 * quaternion;
			vector = sphericalFogWarpExit.GetRelativeExitVelocity(vector);
			if (body.CompareTag("Probe"))
			{
				quaternion = Quaternion.FromToRotation(body.transform.forward, -vector4) * body.transform.rotation;
				vector = -vector4.normalized * Mathf.Max(localRelVelocity.magnitude, 40f);
			}
		}
		else
		{
			Debug.LogError("Failed to find fog warp exit point.", this);
			Debug.Break();
		}
		body.WarpToPositionRotation(vector3, quaternion);
		body.SetVelocity(vector + _attachedBody.GetVelocity());
	}

	public abstract FogWarpVolume GetLinkedFogWarpVolume();

	public void DrawExitMarkers()
	{
		SphericalFogWarpExit[] componentsInChildren = GetComponentsInChildren<SphericalFogWarpExit>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(base.transform.position + componentsInChildren[i].transform.up * _exitRadius, 7.5f);
		}
	}

	private Vector3 GetExitPosition(SphericalFogWarpExit exit)
	{
		return base.transform.position + exit.transform.up * _exitRadius;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new ColorHSV(300f, 0.8f, 0.8f).ToColorRGB();
			Gizmos.DrawWireSphere(base.transform.position, _warpRadius);
			Gizmos.color = new ColorHSV(300f, 0.4f, 0.4f).ToColorRGB();
			Gizmos.DrawWireSphere(base.transform.position, _exitRadius);
			DrawExitMarkers();
		}
	}
}
