using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class PlaneOffsetMarker : MonoBehaviour
{
	private MeshRenderer _meshRenderer;

	private LineRenderer _lineRenderer;

	[SerializeField]
	private Transform _trackedObject;

	[Space]
	[SerializeField]
	private Color _lineColor = Color.white;

	[SerializeField]
	private float _lineWidth = 10f;

	[SerializeField]
	private float _maxLineWidth = 100f;

	[SerializeField]
	private float _dotFrequency = 100f;

	[Space]
	[SerializeField]
	private Color _gridColor = Color.white;

	[SerializeField]
	private float _gridSize = 100f;

	[SerializeField]
	private float _maxGridSize = 1000f;

	[Space]
	[SerializeField]
	private bool _fade;

	[SerializeField]
	private float _fadeStartDist = 10000f;

	[SerializeField]
	private float _fadeEndDist = 20000f;

	[Space]
	[SerializeField]
	private bool _lockOnFade;

	[SerializeField]
	private float _lockOnFadeLength = 1f;

	private RulesetDetector _rulesetDetector;

	private ReferenceFrameVolume _rfVolume;

	private Transform _sunTransform;

	private float _planetFade;

	private bool _isLockedOn;

	private float _lockOnFadeFactor;

	private bool _isShip;

	private bool _shipDestroyed;

	private void Reset()
	{
		LineRenderer component = GetComponent<LineRenderer>();
		component.positionCount = 2;
		component.SetPositions(new Vector3[2]
		{
			Vector3.zero,
			Vector3.up
		});
	}

	private void Awake()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
		_lineRenderer = GetComponent<LineRenderer>();
		_rulesetDetector = ((_trackedObject != null) ? _trackedObject.GetComponentInChildren<RulesetDetector>() : null);
		_rfVolume = ((_lockOnFade && _trackedObject != null) ? _trackedObject.GetComponentInChildren<ReferenceFrameVolume>() : null);
		_isShip = _trackedObject != null && _trackedObject.CompareTag("Ship");
		_lineRenderer.startWidth = _lineWidth;
		_lineRenderer.endWidth = _lineWidth;
		_lineRenderer.startColor = _lineColor;
		_lineRenderer.endColor = new Color(_lineColor.r, _lineColor.g, _lineColor.b, 0f);
	}

	private void Start()
	{
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		if (_lockOnFade)
		{
			GlobalMessenger<ReferenceFrame>.AddListener("TargetReferenceFrame", OnTargetReferenceFrame);
			GlobalMessenger.AddListener("UntargetReferenceFrame", OnUntargetReferenceFrame);
		}
		if (_isShip)
		{
			GlobalMessenger.AddListener("ShipSystemFailure", OnShipDestroyed);
			GlobalMessenger.AddListener("ShipDestroyed", OnShipDestroyed);
		}
		_sunTransform = Locator.GetSunTransform();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		if (_lockOnFade)
		{
			GlobalMessenger<ReferenceFrame>.RemoveListener("TargetReferenceFrame", OnTargetReferenceFrame);
			GlobalMessenger.RemoveListener("UntargetReferenceFrame", OnUntargetReferenceFrame);
		}
		if (_isShip)
		{
			GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipDestroyed);
			GlobalMessenger.RemoveListener("ShipDestroyed", OnShipDestroyed);
		}
	}

	private void OnEnable()
	{
		if (_trackedObject == null)
		{
			return;
		}
		_meshRenderer.enabled = true;
		_lineRenderer.enabled = true;
		_planetFade = 1f;
		if (!(_rulesetDetector != null))
		{
			return;
		}
		PlanetoidRuleset planetoidRuleset = _rulesetDetector.GetPlanetoidRuleset();
		if (planetoidRuleset != null)
		{
			Vector3 position = planetoidRuleset.GetAttachedOWRigidbody().GetPosition();
			float sqrMagnitude = (_trackedObject.position - position).sqrMagnitude;
			float altitudeCeiling = planetoidRuleset.GetAltitudeCeiling();
			if (sqrMagnitude < altitudeCeiling * altitudeCeiling)
			{
				_planetFade = 0f;
			}
		}
	}

	private void OnDisable()
	{
		_meshRenderer.enabled = false;
		_lineRenderer.enabled = false;
	}

	private void OnEnterMapView()
	{
		if (!_isShip || !_shipDestroyed)
		{
			base.enabled = true;
		}
	}

	private void OnExitMapView()
	{
		base.enabled = false;
	}

	private void OnTargetReferenceFrame(ReferenceFrame referenceFrame)
	{
		_isLockedOn = referenceFrame == _rfVolume.GetReferenceFrame();
	}

	private void OnUntargetReferenceFrame()
	{
		_isLockedOn = false;
	}

	private void OnShipDestroyed()
	{
		_shipDestroyed = true;
		base.enabled = false;
	}

	private void Update()
	{
		if (_trackedObject == null || _sunTransform == null)
		{
			base.enabled = false;
			return;
		}
		if (_rulesetDetector != null)
		{
			PlanetoidRuleset planetoidRuleset = _rulesetDetector.GetPlanetoidRuleset();
			if (planetoidRuleset != null)
			{
				Vector3 position = planetoidRuleset.GetAttachedOWRigidbody().GetPosition();
				float sqrMagnitude = (_trackedObject.position - position).sqrMagnitude;
				float altitudeCeiling = planetoidRuleset.GetAltitudeCeiling();
				if (sqrMagnitude < altitudeCeiling * altitudeCeiling)
				{
					_planetFade = Mathf.MoveTowards(_planetFade, 0f, Time.deltaTime);
				}
				else
				{
					_planetFade = Mathf.MoveTowards(_planetFade, 1f, Time.deltaTime);
				}
			}
		}
		if (_lockOnFade)
		{
			if (_isLockedOn)
			{
				_lockOnFadeFactor = Mathf.MoveTowards(_lockOnFadeFactor, 1f, Time.deltaTime / _lockOnFadeLength);
			}
			else
			{
				_lockOnFadeFactor = Mathf.MoveTowards(_lockOnFadeFactor, 0f, Time.deltaTime / _lockOnFadeLength);
			}
		}
		Vector3 vector = Vector3.ProjectOnPlane(_trackedObject.position - _sunTransform.position, _sunTransform.up);
		Vector3 vector2 = _sunTransform.position + vector;
		float num = Vector3.Dot(_trackedObject.position - vector2, _sunTransform.up);
		float num2 = OWMath.PointSegmentDistance(Locator.GetActiveCamera().transform.position, vector2, _trackedObject.position);
		float num3 = Vector3.Distance(Locator.GetActiveCamera().transform.position, vector2);
		float widthMultiplier = Mathf.Min(num2 * (_lineWidth / 1000f), _maxLineWidth);
		float x = num / _dotFrequency;
		float num4 = Mathf.Min(num3 * (_gridSize / 1000f), _maxGridSize);
		float num5 = (_fade ? (1f - Mathf.Clamp01((num2 - _fadeStartDist) / (_fadeEndDist - _fadeStartDist))) : 1f);
		num5 *= _planetFade;
		if (_lockOnFade)
		{
			num5 *= _lockOnFadeFactor;
		}
		base.transform.position = vector2;
		base.transform.rotation = Quaternion.LookRotation(vector, _sunTransform.up);
		base.transform.localScale = new Vector3(num4, num, num4);
		_lineRenderer.widthMultiplier = widthMultiplier;
		_lineRenderer.startColor = new Color(_lineColor.r, _lineColor.g, _lineColor.b, _lineColor.a * num5 * num5);
		_lineRenderer.material.mainTextureScale = new Vector2(x, 1f);
		_meshRenderer.material.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, _gridColor.a * num5 * num5);
		_meshRenderer.material.SetMatrix("_GridCenterMatrix", _sunTransform.worldToLocalMatrix);
	}
}
