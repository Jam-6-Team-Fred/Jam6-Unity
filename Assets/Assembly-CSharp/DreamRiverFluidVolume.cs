using UnityEngine;

[RequireComponent(typeof(BoxShape))]
public class DreamRiverFluidVolume : FlatFluidVolume
{
	[Space]
	[SerializeField]
	private float _defaultSpeed;

	[SerializeField]
	private float _densityForRaft = 100f;

	[SerializeField]
	private AnimationCurve _attractCurve;

	[SerializeField]
	private RiverFlowRail _rail;

	public Vector3 GetPointFlowOnlyVelocity(Vector3 worldPosition)
	{
		RiverFlowRail.RiverFlowSegment nearestSegment = _rail.GetNearestSegment(worldPosition);
		Vector3 vector = nearestSegment.pt2.GetPosition() - nearestSegment.pt1.GetPosition();
		float t = Vector3.Project(worldPosition - nearestSegment.pt1.GetPosition(), vector).magnitude / vector.magnitude;
		vector = Vector3.ProjectOnPlane(vector, base.transform.up);
		float num = Mathf.Lerp(nearestSegment.pt1.magnitude, nearestSegment.pt2.magnitude, t);
		return (nearestSegment.pt2.GetPosition() - nearestSegment.pt1.GetPosition()).normalized * num;
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		RiverFlowRail.RiverFlowSegment nearestSegment = _rail.GetNearestSegment(worldPosition);
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 vector = nearestSegment.pt2.GetPosition() - nearestSegment.pt1.GetPosition();
		float t = Vector3.Project(worldPosition - nearestSegment.pt1.GetPosition(), vector).magnitude / vector.magnitude;
		vector = Vector3.ProjectOnPlane(vector, base.transform.up);
		float num = Mathf.Lerp(nearestSegment.pt1.magnitude, nearestSegment.pt2.magnitude, t);
		if (detector.CompareName(Detector.Name.Raft))
		{
			DreamRaftController dreamRaftController = (detector as DreamRaftFluidDetector).GetDreamRaftController();
			if (dreamRaftController.IsBoosting())
			{
				num = dreamRaftController.GetTurboSpeed();
			}
		}
		Vector3 vector2 = (nearestSegment.pt2.GetPosition() - nearestSegment.pt1.GetPosition()).normalized * num;
		float num2 = Mathf.Lerp(nearestSegment.pt1.curveDistance, nearestSegment.pt2.curveDistance, t);
		Vector3 vector3 = nearestSegment.closestPoint - worldPosition;
		vector3 = Vector3.ProjectOnPlane(vector3, base.transform.up);
		Vector3 vector4 = vector3.normalized * _attractCurve.Evaluate(vector3.magnitude / num2) * Mathf.Lerp(nearestSegment.pt1.attractMagnitude, nearestSegment.pt2.attractMagnitude, t);
		return pointVelocity + vector2 + vector4;
	}

	public override float GetPointDensity(Vector3 worldPosition, FluidDetector detector)
	{
		if (!detector.CompareName(Detector.Name.Raft))
		{
			return _density;
		}
		return _densityForRaft;
	}
}
