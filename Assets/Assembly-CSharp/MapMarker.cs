using UnityEngine;

public class MapMarker : MonoBehaviour
{
	private enum MarkerType
	{
		Default = 0,
		Planet = 1,
		Moon = 2,
		Sun = 3,
		Player = 4,
		Probe = 5,
		Ship = 6,
		HourglassTwins = 7,
		ShipLog = 8
	}

	[SerializeField]
	private UITextType _labelID = UITextType.MaxNumberOfUIText;

	[SerializeField]
	private MarkerType _markerType;

	[SerializeField]
	private Color _customMarkerColor = Color.white;

	[SerializeField]
	private FogWarpDetector _fogWarpDetector;

	private bool _disableMapMarker;

	private float _maxDisplayDistance;

	private float _minDisplayDistance;

	private Transform _playerTransform;

	private Transform _shipTransform;

	private bool _shipDestroyed;

	private SurveyorProbe _probe;

	private bool _probeDeployed;

	private CanvasMapMarker _canvasMarker;

	private bool _canvasMarkerInitialized;

	private void Awake()
	{
		switch (_markerType)
		{
		case MarkerType.Planet:
			_maxDisplayDistance = 50000f;
			break;
		case MarkerType.Moon:
			_maxDisplayDistance = 5000f;
			break;
		case MarkerType.Sun:
			_maxDisplayDistance = 1E+10f;
			break;
		case MarkerType.Probe:
			_maxDisplayDistance = 50000f;
			break;
		case MarkerType.Ship:
			_maxDisplayDistance = 50000f;
			break;
		case MarkerType.HourglassTwins:
			_maxDisplayDistance = 50000f;
			_minDisplayDistance = 5000f;
			break;
		case MarkerType.ShipLog:
			_maxDisplayDistance = 50000f;
			_disableMapMarker = true;
			break;
		default:
			_maxDisplayDistance = 5000f;
			break;
		case MarkerType.Player:
			break;
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		if (_markerType == MarkerType.Planet && (_labelID == UITextType.LocationCo_Cap || _labelID == UITextType.LocationCo))
		{
			TempCometCollisionFix component = GetComponent<TempCometCollisionFix>();
			if (component != null)
			{
				component.onCometDestroyed += new OWEvent.OWCallback(DisableMarker);
			}
		}
		if (_markerType == MarkerType.Ship)
		{
			GlobalMessenger.AddListener("ShipSystemFailure", OnShipDestroyed);
			GlobalMessenger.AddListener("ShipDestroyed", OnShipDestroyed);
		}
		else if (_markerType == MarkerType.Probe)
		{
			_probe = this.GetRequiredComponent<SurveyorProbe>();
			_probe.OnLaunchProbe += OnProbeLaunch;
			_probe.OnRetrieveProbe += OnProbeRetrieve;
		}
	}

	private void Start()
	{
		base.enabled = false;
		_playerTransform = Locator.GetPlayerTransform();
		_shipTransform = Locator.GetShipTransform();
	}

	private void OnDisable()
	{
		if (_markerType == MarkerType.Planet && (_labelID == UITextType.LocationCo_Cap || _labelID == UITextType.LocationCo) && !base.gameObject.activeSelf)
		{
			DisableMarker();
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		if (_markerType == MarkerType.Ship)
		{
			GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipDestroyed);
			GlobalMessenger.RemoveListener("ShipDestroyed", OnShipDestroyed);
		}
		else if (_markerType == MarkerType.Probe && _probe != null)
		{
			_probe.OnLaunchProbe -= OnProbeLaunch;
			_probe.OnRetrieveProbe -= OnProbeRetrieve;
		}
		if (_markerType == MarkerType.Planet && (_labelID == UITextType.LocationCo_Cap || _labelID == UITextType.LocationCo))
		{
			TempCometCollisionFix component = GetComponent<TempCometCollisionFix>();
			if (component != null)
			{
				component.onCometDestroyed -= new OWEvent.OWCallback(DisableMarker);
			}
		}
	}

	public void InitMarker()
	{
		if (_canvasMarkerInitialized)
		{
			return;
		}
		MapMarkerManager markerManager = Locator.GetMapController().GetMarkerManager();
		bool flag = false;
		MarkerType markerType = _markerType;
		flag = (((uint)(markerType - 4) <= 2u || markerType == MarkerType.ShipLog) ? true : false);
		_canvasMarker = markerManager.InstantiateNewMarker(flag);
		OWRigidbody component = GetComponent<OWRigidbody>();
		if (_labelID == UITextType.MaxNumberOfUIText || _labelID == UITextType.None)
		{
			if (component != null)
			{
				markerManager.RegisterMarker(_canvasMarker, component);
			}
			else
			{
				markerManager.RegisterMarker(_canvasMarker, base.transform);
			}
		}
		else if (component != null)
		{
			markerManager.RegisterMarker(_canvasMarker, component, _labelID);
		}
		else
		{
			markerManager.RegisterMarker(_canvasMarker, base.transform, _labelID);
		}
		_canvasMarker.SetColor(_customMarkerColor);
		_canvasMarker.SetVisibility(value: false);
		_canvasMarkerInitialized = true;
	}

	public void EnableMarker()
	{
		_disableMapMarker = false;
	}

	public void DisableMarker()
	{
		base.enabled = false;
		if (_canvasMarkerInitialized)
		{
			_canvasMarker.SetVisibility(value: false);
		}
		_disableMapMarker = true;
	}

	public void ModifyMarker(Transform targetTransform, string label)
	{
		if (_canvasMarker != null)
		{
			_canvasMarker.SetLabel(label);
			_canvasMarker.SetMarkerTarget(targetTransform);
		}
	}

	public void SetOuterFogWarpVolume(OuterFogWarpVolume fogWarpVolume)
	{
		if (_canvasMarker != null)
		{
			_canvasMarker.SetOuterFogWarpVolume(fogWarpVolume);
		}
	}

	private void OnEnterMapView()
	{
		if ((_markerType != MarkerType.Ship || !_shipDestroyed) && (_markerType != MarkerType.Probe || _probeDeployed || _probe.IsTimeLoopCoreDuplicate()) && !_disableMapMarker)
		{
			base.enabled = true;
		}
	}

	private void OnExitMapView()
	{
		base.enabled = false;
	}

	private void OnShipDestroyed()
	{
		_shipDestroyed = true;
		if (_canvasMarkerInitialized)
		{
			_canvasMarker.SetVisibility(value: false);
		}
		base.enabled = false;
	}

	private void OnProbeLaunch()
	{
		_probeDeployed = true;
	}

	private void OnProbeRetrieve()
	{
		_probeDeployed = false;
		if (_canvasMarkerInitialized)
		{
			_canvasMarker.SetVisibility(value: false);
		}
		base.enabled = false;
	}

	private void LateUpdate()
	{
		if (!_canvasMarkerInitialized)
		{
			InitMarker();
		}
		bool flag = true;
		Vector3 position = base.transform.position;
		if (_fogWarpDetector != null)
		{
			if (_fogWarpDetector.GetOuterFogWarpVolume() != null)
			{
				position = Locator.GetAstroObject(AstroObject.Name.DarkBramble).GetOWRigidbody().GetPosition();
			}
			_canvasMarker.SetOuterFogWarpVolume(_fogWarpDetector.GetOuterFogWarpVolume());
		}
		Vector3 vector = Locator.GetActiveCamera().WorldToScreenPoint(position);
		Vector3 vector2 = Locator.GetActiveCamera().WorldToScreenPoint(_playerTransform.position);
		Vector3 vector3 = vector - vector2;
		vector3.z = 0f;
		if (_markerType == MarkerType.Player)
		{
			flag = vector.z > 0f;
		}
		else if (_markerType == MarkerType.Ship)
		{
			if (vector.z > 0f && vector.z < _maxDisplayDistance && vector3.magnitude > 10f)
			{
				flag = true;
			}
			else if (_shipTransform != null)
			{
				Vector3 vector4 = Locator.GetActiveCamera().WorldToScreenPoint(_shipTransform.position);
				Vector3 vector5 = vector - vector4;
				vector5.z = 0f;
				if (vector5.magnitude < 10f)
				{
					flag = false;
				}
			}
		}
		else if (_markerType == MarkerType.Probe)
		{
			flag = true;
			if (vector.z > 0f && vector.z < _maxDisplayDistance && vector3.magnitude < 10f)
			{
				flag = false;
			}
			if (_shipTransform != null)
			{
				Vector3 vector6 = Locator.GetActiveCamera().WorldToScreenPoint(_shipTransform.position);
				Vector3 vector7 = vector - vector6;
				vector7.z = 0f;
				if (vector7.magnitude < 10f)
				{
					flag = false;
				}
			}
		}
		else if (vector.z < 0f || vector.z > _maxDisplayDistance || vector.z <= _minDisplayDistance)
		{
			flag = false;
		}
		if (_disableMapMarker)
		{
			flag = false;
		}
		if (_canvasMarker.IsVisible() != flag)
		{
			_canvasMarker.SetVisibility(flag);
		}
	}
}
