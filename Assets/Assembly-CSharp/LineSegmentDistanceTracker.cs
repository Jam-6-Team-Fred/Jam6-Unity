using UnityEngine;

public class LineSegmentDistanceTracker : MonoBehaviour
{
	[SerializeField]
	private Vector3 _localSegmentStartPoint;

	[SerializeField]
	private Vector3 _localSegmentEndPoint;

	private Transform _segmentRoot;

	private Transform _target;

	public void Initialize(Vector3 startPoint, Vector3 endPoint, Transform target, Transform root)
	{
		_localSegmentStartPoint = startPoint;
		_localSegmentEndPoint = endPoint;
		_target = target;
		_segmentRoot = root;
	}

	public void SetEndpoints(Vector3 startPoint, Vector3 endPoint)
	{
		_localSegmentStartPoint = startPoint;
		_localSegmentEndPoint = endPoint;
	}

	public void SetRootTransform(Transform rootTransform)
	{
		_segmentRoot = rootTransform;
	}

	public void SetPersistentTarget(Transform targetToTrack)
	{
		_target = targetToTrack;
	}

	public float GetDistanceToTarget()
	{
		if (_target == null)
		{
			Debug.LogWarning("No target set!");
			return 0f;
		}
		return OWMath.PointSegmentDistance(_target.position, _segmentRoot.TransformPoint(_localSegmentStartPoint), _segmentRoot.TransformPoint(_localSegmentEndPoint));
	}

	public Vector3 GetClosestPointOnSegment()
	{
		if (_target == null)
		{
			Debug.LogWarning("No target set!");
			return Vector3.zero;
		}
		return OWMath.ClosestPointOnSegment(_target.position, _segmentRoot.TransformPoint(_localSegmentStartPoint), _segmentRoot.TransformPoint(_localSegmentEndPoint));
	}

	public float GetDistanceToTarget(Transform target)
	{
		if (target == null)
		{
			Debug.LogWarning("Target parameter is null!");
			return 0f;
		}
		return OWMath.PointSegmentDistance(target.position, _segmentRoot.TransformPoint(_localSegmentStartPoint), _segmentRoot.TransformPoint(_localSegmentEndPoint));
	}

	public Vector3 GetClosestPointOnSegment(Transform target)
	{
		if (target == null)
		{
			Debug.LogWarning("Target parameter is null!");
			return Vector3.zero;
		}
		return OWMath.ClosestPointOnSegment(_target.position, _segmentRoot.TransformPoint(_localSegmentStartPoint), _segmentRoot.TransformPoint(_localSegmentEndPoint));
	}
}
