using System;
using UnityEngine;

public abstract class MovingPlatform : SectoredMonoBehaviour
{
	private Vector3 _angularVelocity;

	private OWRigidbody _parentBody;

	private Quaternion _lastRot;

	private Quaternion _rot;

	protected override void Awake()
	{
		base.Awake();
		_parentBody = this.GetAttachedOWRigidbody();
	}

	private void Start()
	{
		base.enabled = false;
	}

	public Vector3 GetAngularVelocity()
	{
		return _angularVelocity;
	}

	public Vector3 GetPointVelocity(Vector3 worldPoint)
	{
		Vector3 pointVelocity = _parentBody.GetPointVelocity(worldPoint);
		if (base.enabled)
		{
			Vector3 rhs = worldPoint - base.transform.position;
			Vector3 vector = Vector3.Cross(_angularVelocity, rhs);
			return pointVelocity + vector;
		}
		Debug.LogWarning("Moving platform not enabled (sector issue?)", this);
		return pointVelocity;
	}

	private void FixedUpdate()
	{
		_lastRot = _rot;
		_rot = base.transform.rotation;
		Quaternion quaternion = _rot * Quaternion.Inverse(_lastRot);
		float angle = 0f;
		Vector3 axis = Vector3.zero;
		quaternion.ToAngleAxis(out angle, out axis);
		_angularVelocity = axis * (angle * ((float)Math.PI / 180f)) / Time.fixedDeltaTime;
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			base.enabled = true;
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			base.enabled = false;
		}
	}
}
