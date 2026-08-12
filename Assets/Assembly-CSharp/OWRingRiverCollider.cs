using UnityEngine;

public class OWRingRiverCollider : OWCapsuleCollider
{
	[SerializeField]
	private float _rampStartDegrees;

	[SerializeField]
	private float _rampEndDegrees;

	[SerializeField]
	private AnimationCurve _rampCurve;

	[Space]
	[SerializeField]
	private float _waveDeltaDegrees = 5f;

	[SerializeField]
	private AnimationCurve _waveFrontCurve;

	[SerializeField]
	private AnimationCurve _waveToDamCurve;

	[SerializeField]
	private AnimationCurve _damDrainCurve;

	[SerializeField]
	private AnimationCurve _downhillElevationToSpeedCurve;

	[Space]
	[SerializeField]
	private float _innerRadiusLow;

	[SerializeField]
	private float _innerRadiusHigh;

	[SerializeField]
	private float _innerRadiusFinal;

	[Space]
	[SerializeField]
	private float _outerRadius;

	private float _floodLerp;

	private float _cylinderHalfHeight;

	protected override void Awake()
	{
		base.Awake();
		_cylinderHalfHeight = Mathf.Max(0f, _capsule.height * 0.5f - _capsule.radius);
	}

	public Vector3 GetUpVectorAtPosition(Vector3 worldPosition)
	{
		Vector3 vector = base.transform.position - worldPosition;
		return Vector3.ProjectOnPlane(vector, base.transform.up).normalized;
	}

	public float WorldPositionToDegrees(Vector3 worldPosition)
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(worldPosition);
		return LocalPositionToDegrees(localPosition);
	}

	public float LocalPositionToDegrees(Vector3 localPosition)
	{
		float num = 57.29578f * Mathf.Atan2(localPosition.x, localPosition.z);
		if (!(num < 0f))
		{
			return num;
		}
		return num + 360f;
	}

	public void SnapTransformToInnerRadius(Transform transformToSnap, float floodLerp)
	{
		float floodLerp2 = _floodLerp;
		_floodLerp = floodLerp;
		Vector3 vector = base.transform.InverseTransformPoint(transformToSnap.position);
		float innerRadiusAtLocalPosition = GetInnerRadiusAtLocalPosition(vector);
		float num = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z) - innerRadiusAtLocalPosition;
		Vector3 direction = new Vector3(0f - vector.x, 0f, 0f - vector.z);
		vector += direction.normalized * num;
		transformToSnap.position = base.transform.TransformPoint(vector);
		Vector3 toDirection = base.transform.TransformDirection(direction);
		Quaternion quaternion = Quaternion.FromToRotation(transformToSnap.up, toDirection);
		transformToSnap.rotation = quaternion * transformToSnap.rotation;
		_floodLerp = floodLerp2;
	}

	public void SetFloodLerp(float floodLerp)
	{
		_floodLerp = floodLerp;
	}

	public float GetFloodLerp()
	{
		return _floodLerp;
	}

	public bool HasFloodReachedPosition(Vector3 worldPosition, float reservoirFloodThreshold = 0f)
	{
		if (_floodLerp > 0.999f)
		{
			return true;
		}
		if (_floodLerp > 0.001f)
		{
			Vector3 localPosition = base.transform.InverseTransformPoint(worldPosition);
			float num = LocalPositionToDegrees(localPosition);
			if (num > _rampStartDegrees && _floodLerp > reservoirFloodThreshold)
			{
				return true;
			}
			float num2 = _floodLerp * 360f;
			return num < num2;
		}
		return false;
	}

	public float GetFloodWaveDegree()
	{
		return _floodLerp * 360f;
	}

	public float WorldPositionToRiverLerp(Vector3 worldPosition)
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(worldPosition);
		float value = LocalPositionToDegrees(localPosition);
		return Mathf.InverseLerp(0f, 360f, value);
	}

	public float GetWaveSpeedFraction(Vector3 localPosition, bool debug = false)
	{
		if (_floodLerp > 0f && _floodLerp < 1f)
		{
			float num = 10f;
			float num2 = _floodLerp * 360f;
			float num3 = Mathf.Max(0f, num2 - _waveDeltaDegrees - num);
			float num4 = LocalPositionToDegrees(localPosition);
			if (num4 < _rampStartDegrees && num4 < num2 && num4 > num3)
			{
				if (num4 > num2 - _waveDeltaDegrees)
				{
					return 1f;
				}
				return Mathf.InverseLerp(num3, num2 - _waveDeltaDegrees, num4);
			}
		}
		return 0f;
	}

	public float GetDownhillSpeed(Vector3 localPosition, bool debug = false)
	{
		if (_floodLerp > 0f && _floodLerp < 0.5f)
		{
			float num = _floodLerp * 360f;
			float num2 = Mathf.Max(0f, num - _waveDeltaDegrees);
			if (LocalPositionToDegrees(localPosition) < num2)
			{
				float num3 = Mathf.Sqrt(localPosition.x * localPosition.x + localPosition.z * localPosition.z);
				float time = _innerRadiusFinal - num3;
				return _downhillElevationToSpeedCurve.Evaluate(time);
			}
		}
		return 0f;
	}

	public float GetInnerRadiusAtLocalPosition(Vector3 localPosition)
	{
		if (_floodLerp >= 1f)
		{
			return _innerRadiusFinal;
		}
		float degrees = LocalPositionToDegrees(localPosition);
		return GetInnerRadiusAtDegrees(degrees);
	}

	public float GetInnerRadiusAtDegrees(float degrees)
	{
		if (_floodLerp >= 1f)
		{
			return _innerRadiusFinal;
		}
		float num = _innerRadiusLow;
		float num2 = Mathf.LerpUnclamped(_innerRadiusHigh, _innerRadiusFinal, _damDrainCurve.Evaluate(_floodLerp));
		if (degrees > _rampEndDegrees)
		{
			num = num2;
		}
		else if (degrees > _rampStartDegrees)
		{
			float time = Mathf.InverseLerp(_rampStartDegrees, _rampEndDegrees, degrees);
			num = Mathf.LerpUnclamped(_innerRadiusLow, num2, _rampCurve.Evaluate(time));
		}
		if (_floodLerp <= 0f)
		{
			return num;
		}
		float num3 = _floodLerp * 360f;
		if (degrees < num3)
		{
			float num4 = Mathf.Max(0f, num3 - _waveDeltaDegrees);
			if (degrees > num4)
			{
				float time2 = Mathf.InverseLerp(num3, num4, degrees);
				return Mathf.LerpUnclamped(num, _innerRadiusFinal, _waveFrontCurve.Evaluate(time2));
			}
			float time3 = Mathf.InverseLerp(num4, 0f, degrees);
			return Mathf.LerpUnclamped(_innerRadiusFinal, num2, _waveToDamCurve.Evaluate(time3));
		}
		return num;
	}

	public float GetPreFloodInnerRadiusAtLocalPosition(Vector3 localPosition)
	{
		float num = LocalPositionToDegrees(localPosition);
		float result = _innerRadiusLow;
		if (num > _rampEndDegrees)
		{
			result = _innerRadiusHigh;
		}
		else if (num > _rampStartDegrees)
		{
			float time = Mathf.InverseLerp(_rampStartDegrees, _rampEndDegrees, num);
			result = Mathf.LerpUnclamped(_innerRadiusLow, _innerRadiusHigh, _rampCurve.Evaluate(time));
		}
		return result;
	}

	public float GetPostFloodInnerRadiusAtLocalPosition(Vector3 localPosition)
	{
		return _innerRadiusFinal;
	}

	public override float GetDistToSurface(Vector3 worldPoint)
	{
		Debug.LogError("WHY IS THIS BEING CALLED???");
		Debug.Break();
		if (!IsPointInCollider(worldPoint))
		{
			return Mathf.Max(Vector3.Distance(worldPoint, base.transform.position) - GetInnerRadiusAtLocalPosition(base.transform.InverseTransformPoint(worldPoint)), 0f);
		}
		return 0f;
	}

	public override bool IsPointInCollider(Vector3 worldPoint)
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(worldPoint);
		if (localPosition.y > _cylinderHalfHeight || localPosition.y < 0f - _cylinderHalfHeight)
		{
			return false;
		}
		float num = Mathf.Sqrt(localPosition.x * localPosition.x + localPosition.z * localPosition.z);
		if (num > GetInnerRadiusAtLocalPosition(localPosition))
		{
			return num < _outerRadius;
		}
		return false;
	}

	protected override bool IsTrackerInCollider(TrackedTransform tracker)
	{
		if (tracker.fluidDetector == null)
		{
			return false;
		}
		SectorDetector sectorDetector = null;
		sectorDetector = ((!tracker.fluidDetector.CompareTag("PlayerCameraDetector")) ? tracker.fluidDetector.GetComponent<SectorDetector>() : Locator.GetPlayerSectorDetector());
		if (sectorDetector != null && !sectorDetector.IsWithinSector("RingWorldInterior"))
		{
			return false;
		}
		Shape shape = tracker.fluidDetector.GetShape();
		Collider collider = tracker.fluidDetector.GetCollider();
		float num = 0f;
		if (shape != null)
		{
			if (shape.GetType() == typeof(CapsuleShape))
			{
				num = ((CapsuleShape)shape).height * 0.5f;
			}
			else if (shape.GetType() == typeof(BoxShape))
			{
				num = ((BoxShape)shape).size.y * 0.5f;
			}
			else if (shape.GetType() == typeof(SphereShape))
			{
				num = ((SphereShape)shape).radius * 0.5f;
			}
		}
		else if (collider != null)
		{
			if (collider.GetType() == typeof(CapsuleCollider))
			{
				num = ((CapsuleCollider)collider).height * 0.5f;
			}
			else if (collider.GetType() == typeof(BoxCollider))
			{
				num = ((BoxCollider)collider).size.y * 0.5f;
			}
			else if (collider.GetType() == typeof(SphereCollider))
			{
				num = ((SphereCollider)collider).radius * 0.5f;
			}
		}
		Vector3 localPosition = base.transform.InverseTransformPoint(tracker.transform.position);
		if (localPosition.y > _cylinderHalfHeight || localPosition.y < 0f - _cylinderHalfHeight)
		{
			return false;
		}
		float num2 = Mathf.Sqrt(localPosition.x * localPosition.x + localPosition.z * localPosition.z);
		if (num2 + num > GetInnerRadiusAtLocalPosition(localPosition))
		{
			return num2 - num < _outerRadius;
		}
		return false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Quaternion quaternion = Quaternion.AngleAxis(_rampStartDegrees, base.transform.up);
		Vector3 vector = base.transform.position + base.transform.forward * 300f;
		Gizmos.DrawLine(base.transform.position, quaternion * vector);
		Quaternion quaternion2 = Quaternion.AngleAxis(_rampEndDegrees, base.transform.up);
		Vector3 vector2 = base.transform.position + base.transform.forward * 300f;
		Gizmos.DrawLine(base.transform.position, quaternion2 * vector2);
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward * 300f);
	}
}
