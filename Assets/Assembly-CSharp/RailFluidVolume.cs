using UnityEngine;

public class RailFluidVolume : FluidVolume
{
	[Space]
	[SerializeField]
	private float _flowSpeed;

	[SerializeField]
	private float _inwardSpeed;

	[SerializeField]
	private float _inwardFalloffRadius;

	[SerializeField]
	private bool _horizontalInwardVelocityOnly;

	[SerializeField]
	private float _verticalSpeed;

	[SerializeField]
	private Transform _railPointsRoot;

	[SerializeField]
	private bool _preventPlayerGrounded;

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private Vector3[] _railPoints;

	protected override void Awake()
	{
		base.Awake();
		if (!_prebuilt)
		{
			BuildRail();
		}
	}

	private void BuildRail()
	{
		_railPoints = new Vector3[_railPointsRoot.childCount];
		for (int i = 0; i < _railPointsRoot.childCount; i++)
		{
			_railPoints[i] = _railPointsRoot.GetChild(i).localPosition;
		}
		if (_railPoints.Length < 2)
		{
			Debug.LogError("Rail fluid requires at least two points", this);
			Debug.Break();
		}
	}

	public override bool PreventPlayerGrounded()
	{
		return _preventPlayerGrounded;
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		FindClosestPointOnRail(worldPosition, out var closestPos, out var closestDistance, out var segmentStart, out var segmentEnd);
		Vector3 normalized = (segmentEnd - segmentStart).normalized;
		Vector3 vector = Vector3.zero;
		if (Mathf.Abs(_inwardSpeed) > 0f)
		{
			Vector3 vector2 = closestPos - worldPosition;
			float num = _inwardSpeed;
			if (_inwardFalloffRadius > 0f)
			{
				num = Mathf.Lerp(t: Mathf.InverseLerp(0f, _inwardFalloffRadius, closestDistance), a: 0f, b: _inwardSpeed);
			}
			vector = vector2.normalized * num;
		}
		Vector3 vector3 = Vector3.zero;
		if (Mathf.Abs(_verticalSpeed) > 0f || _horizontalInwardVelocityOnly)
		{
			Vector3 rhs = Vector3.Cross(worldPosition - _attachedBody.GetPosition(), normalized);
			Vector3 normalized2 = Vector3.Cross(normalized, rhs).normalized;
			vector3 = normalized2 * _verticalSpeed;
			if (_horizontalInwardVelocityOnly)
			{
				vector -= Vector3.Project(vector, normalized2);
			}
		}
		if (detector.AffectsRumble())
		{
			RumbleManager.AddFluidRumble(_fluidType, 0.5f);
		}
		return pointVelocity + normalized * _flowSpeed + vector + vector3;
	}

	private void FindClosestPointOnRail(Vector3 worldPosition, out Vector3 closestPos, out float closestDistance, out Vector3 segmentStart, out Vector3 segmentEnd)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPosition);
		closestPos = Vector3.zero;
		closestDistance = float.PositiveInfinity;
		int num = -1;
		for (int i = 0; i < _railPoints.Length - 1; i++)
		{
			Vector3 vector2 = OWMath.ClosestPointOnSegment(vector, _railPoints[i], _railPoints[i + 1]);
			float num2 = Vector3.Distance(vector, vector2);
			if (num2 < closestDistance)
			{
				closestPos = vector2;
				closestDistance = num2;
				num = i;
			}
		}
		closestPos = _railPointsRoot.TransformPoint(closestPos);
		segmentStart = _railPointsRoot.TransformPoint(_railPoints[num]);
		segmentEnd = _railPointsRoot.TransformPoint(_railPoints[num + 1]);
	}

	private void OnDrawGizmosSelected()
	{
		if (_railPointsRoot == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		for (int i = 0; i < _railPointsRoot.childCount; i++)
		{
			Gizmos.DrawWireSphere(_railPointsRoot.GetChild(i).position, 1f);
			if (i > 0)
			{
				Gizmos.DrawLine(_railPointsRoot.GetChild(i - 1).position, _railPointsRoot.GetChild(i).position);
			}
		}
	}
}
