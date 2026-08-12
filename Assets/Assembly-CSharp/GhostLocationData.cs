using UnityEngine;

public class GhostLocationData
{
	public Vector3 localPosition;

	public Vector3 localVelocity;

	public Vector3 toPosition;

	public Vector3 toPositionXZ;

	public float distance;

	public float distanceXZ;

	public float degreesToPositionXZ;

	public void CopyFromOther(GhostLocationData other)
	{
		localPosition = other.localPosition;
		localVelocity = other.localVelocity;
		toPosition = other.toPosition;
		toPositionXZ = other.toPositionXZ;
		distance = other.distance;
		distanceXZ = other.distanceXZ;
		degreesToPositionXZ = other.degreesToPositionXZ;
	}

	public void Update(Vector3 worldPosition, Vector3 worldVelocity, GhostController controller)
	{
		localPosition = controller.WorldToLocalPosition(worldPosition);
		localVelocity = controller.WorldToLocalDirection(worldVelocity);
		Update(controller);
	}

	public void UpdateLocalPosition(Vector3 localPosition, GhostController controller)
	{
		this.localPosition = localPosition;
		Update(controller);
	}

	public void Update(GhostController controller)
	{
		toPosition = localPosition - controller.GetLocalFeetPosition();
		toPositionXZ = new Vector3(toPosition.x, 0f, toPosition.z);
		distance = toPosition.magnitude;
		distanceXZ = toPositionXZ.magnitude;
		degreesToPositionXZ = Vector3.Angle(toPositionXZ, controller.GetLocalForward());
	}
}
