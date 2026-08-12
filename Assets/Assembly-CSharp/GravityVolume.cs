using UnityEngine;

public class GravityVolume : ForceVolume
{
	private enum FalloffType
	{
		linear = 0,
		inverseSquared = 1,
		constant = 2
	}

	public const float GRAVITATIONAL_CONSTANT = 0.001f;

	private const bool DRAW_DEBUG_LINES = true;

	[SerializeField]
	private bool _isPlanetGravityVolume = true;

	[SerializeField]
	protected float _surfaceAcceleration = 10f;

	[SerializeField]
	private float _cutoffAcceleration;

	[SerializeField]
	private FalloffType _falloffType;

	[SerializeField]
	private float _alignmentRadius = 0.1f;

	[SerializeField]
	private float _upperSurfaceRadius = 0.1f;

	[SerializeField]
	private float _lowerSurfaceRadius = 0.1f;

	[SerializeField]
	private float _cutoffRadius = 0.1f;

	[SerializeField]
	private bool _setMass = true;

	private float _gravitationalMass;

	private float _falloffExponent;

	private Transform _fieldTransform;

	protected override void Awake()
	{
		base.Awake();
		_fieldTransform = base.transform;
		if (_isPlanetGravityVolume)
		{
			_attachedBody.RegisterAttachedGravityVolume(this);
		}
		if (_falloffType == FalloffType.linear)
		{
			_falloffExponent = 1f;
		}
		else
		{
			_falloffExponent = 2f;
		}
		_gravitationalMass = _surfaceAcceleration * Mathf.Pow(_upperSurfaceRadius, _falloffExponent) / 0.001f;
		if (_setMass)
		{
			_attachedBody.SetMass(_gravitationalMass);
		}
		_lowerSurfaceRadius = Mathf.Min(_lowerSurfaceRadius, _upperSurfaceRadius);
		_cutoffRadius = Mathf.Min(_cutoffRadius, _lowerSurfaceRadius);
	}

	public float GetOrbitRadiusWithAcceleration(float gravityAccel)
	{
		if (gravityAccel < _surfaceAcceleration)
		{
			return Mathf.Pow(_gravitationalMass * 0.001f / gravityAccel, 1f / _falloffExponent);
		}
		Debug.LogWarning("This gravity well cannot produce an acceleration that strong.");
		return _surfaceAcceleration;
	}

	public float GetAlignmentRadius()
	{
		return _alignmentRadius;
	}

	public float GetStandardGravitationalParameter()
	{
		return _gravitationalMass * 0.001f;
	}

	public Vector3 GetCenterPosition()
	{
		return _attachedBody.GetWorldCenterOfMass();
	}

	public override bool GetAffectsAlignment(OWRigidbody targetBody)
	{
		if (Vector3.Distance(targetBody.GetPosition(), _fieldTransform.position) < _alignmentRadius)
		{
			return true;
		}
		return false;
	}

	public override Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos)
	{
		Vector3 vector = _fieldTransform.position - worldPos;
		float magnitude = vector.magnitude;
		float num = CalculateGravityMagnitude(magnitude);
		return vector / magnitude * num;
	}

	public override Vector3 CalculateForceAccelerationOnBody(OWRigidbody targetBody)
	{
		Vector3 result = CalculateForceAccelerationAtPoint(targetBody.GetWorldCenterOfMass());
		float num = Mathf.Clamp(result.magnitude / 20f, -1f, 1f);
		float h = ((num >= 0f) ? (260f + 100f * num) : (160f + 100f * num));
		ColorHSV colorHSV = new ColorHSV(h, 1f, 1f);
		Debug.DrawLine(_fieldTransform.position, targetBody.GetWorldCenterOfMass(), colorHSV.ToColorRGB());
		return result;
	}

	public float CalculateGravityMagnitude(float distanceToCenter)
	{
		float result = _cutoffAcceleration;
		if (distanceToCenter > _upperSurfaceRadius)
		{
			result = _gravitationalMass * 0.001f / Mathf.Pow(distanceToCenter, _falloffExponent);
		}
		else if (distanceToCenter > _lowerSurfaceRadius)
		{
			result = _surfaceAcceleration;
		}
		else if (distanceToCenter > _cutoffRadius)
		{
			float t = 1f - (distanceToCenter - _cutoffRadius) / (_lowerSurfaceRadius - _cutoffRadius);
			result = Mathf.Lerp(_surfaceAcceleration, _cutoffAcceleration, t);
		}
		return result;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new ColorHSV(224f, 0.82f, 0.5f).ToColorRGB();
			OWGizmos.DrawWireCircle(base.transform.position, Vector3.up, _cutoffRadius);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _cutoffRadius);
			Gizmos.color = new ColorHSV(224f, 0.68f, 0.95f).ToColorRGB();
			OWGizmos.DrawWireCircle(base.transform.position, Vector3.up, _lowerSurfaceRadius);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _lowerSurfaceRadius);
			Gizmos.color = new ColorHSV(224f, 0.54f, 1f).ToColorRGB();
			OWGizmos.DrawWireCircle(base.transform.position, Vector3.up, _upperSurfaceRadius);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _upperSurfaceRadius);
			Gizmos.color = Color.yellow;
			OWGizmos.DrawWireCircle(base.transform.position, Vector3.up, _alignmentRadius);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _alignmentRadius);
		}
	}
}
