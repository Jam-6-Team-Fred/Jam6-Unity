using UnityEngine;

public class SurveyorProbe : MonoBehaviour, ITimelineObliterator, ILightSource
{
	public delegate void SurveyorProbeEvent();

	public delegate void RetrieveEvent(float retrieveLength);

	[SerializeField]
	private OWLight2[] _illuminationCheckLights;

	[SerializeField]
	private VoidShadowEffectController _voidShadowEffect;

	private OWRigidbody _owRigidbody;

	private ProbeSeeking _seeking;

	private ProbeAnchor _anchor;

	private ProbeHUDMarker _hudMarker;

	private ProbeHorizonTracker _horizonTracker;

	private SectorDetector _sectorDetector;

	private RulesetDetector _rulesetDetector;

	private GameObject _detectorObj;

	private OWCollider _detectorCollider;

	private Shape _detectorShape;

	private SingularityWarpEffect _warpEffect;

	private LightFlickerController _lightFlickerController;

	private ProbeCamera _forwardCam;

	private ProbeCamera _reverseCam;

	private ProbeCamera _rotatingCam;

	private bool _isRetrieving;

	private bool _isTimeLoopCoreDuplicate;

	private LightSourceVolume _lightSourceVol;

	public event SurveyorProbeEvent OnLaunchProbe;

	public event SurveyorProbeEvent OnAnchorProbe;

	public event SurveyorProbeEvent OnUnanchorProbe;

	public event SurveyorProbeEvent OnRetrieveProbe;

	public event SurveyorProbeEvent OnProbeDestroyed;

	public event RetrieveEvent OnStartRetrieveProbe;

	public OWRigidbody GetOWRigidbody()
	{
		return _owRigidbody;
	}

	public ProbeSeeking GetSeeking()
	{
		return _seeking;
	}

	public ProbeAnchor GetAnchor()
	{
		return _anchor;
	}

	public ProbeHUDMarker GetProbeHUDMarker()
	{
		return _hudMarker;
	}

	public GameObject GetDetectorObject()
	{
		return _detectorObj;
	}

	public OWCollider GetDetectorCollider()
	{
		return _detectorCollider;
	}

	public SectorDetector GetSectorDetector()
	{
		return _sectorDetector;
	}

	public RulesetDetector GetRulesetDetector()
	{
		return _rulesetDetector;
	}

	public ProbeCamera GetForwardCamera()
	{
		return _forwardCam;
	}

	public ProbeCamera GetReverseCamera()
	{
		return _reverseCam;
	}

	public ProbeCamera GetRotatingCamera()
	{
		return _rotatingCam;
	}

	public bool IsLaunched()
	{
		return base.gameObject.activeSelf;
	}

	public bool IsAnchored()
	{
		if (IsLaunched())
		{
			return _anchor.IsAnchored();
		}
		return false;
	}

	public bool IsRetrieved()
	{
		return !IsLaunched();
	}

	public bool IsRetrieving()
	{
		if (IsLaunched())
		{
			return _isRetrieving;
		}
		return false;
	}

	public bool IsSeeking()
	{
		return _seeking.IsSeeking();
	}

	private void Awake()
	{
		_owRigidbody = GetComponent<OWRigidbody>();
		_seeking = GetComponent<ProbeSeeking>();
		_anchor = GetComponent<ProbeAnchor>();
		_hudMarker = GetComponent<ProbeHUDMarker>();
		_horizonTracker = GetComponentInChildren<ProbeHorizonTracker>();
		_detectorObj = GetComponentInChildren<ForceDetector>().gameObject;
		_sectorDetector = _detectorObj.GetComponent<SectorDetector>();
		_rulesetDetector = _detectorObj.GetComponent<RulesetDetector>();
		_detectorCollider = _detectorObj.GetAddComponent<OWCollider>();
		_detectorShape = _detectorObj.GetComponent<Shape>();
		_warpEffect = GetComponentInChildren<SingularityWarpEffect>();
		_lightFlickerController = GetComponent<LightFlickerController>();
		ProbeCamera[] componentsInChildren = GetComponentsInChildren<ProbeCamera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetID() == ProbeCamera.ID.Forward)
			{
				_forwardCam = componentsInChildren[i];
			}
			else if (componentsInChildren[i].GetID() == ProbeCamera.ID.Reverse)
			{
				_reverseCam = componentsInChildren[i];
			}
			else if (componentsInChildren[i].GetID() == ProbeCamera.ID.Rotating)
			{
				_rotatingCam = componentsInChildren[i];
			}
		}
		_isRetrieving = false;
		_anchor.OnAnchorToSurface += OnAnchor;
		_anchor.OnUnanchorFromSurface += OnUnanchor;
		if (_warpEffect != null)
		{
			_warpEffect.OnWarpComplete += OnWarpComplete;
		}
	}

	private void Start()
	{
		_lightSourceVol = GetComponentInChildren<LightSourceVolume>();
		if (_lightSourceVol != null)
		{
			_lightSourceVol.LinkLightSource(this);
			_lightSourceVol.SetVolumeActivation(active: false);
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (this.OnProbeDestroyed != null)
		{
			this.OnProbeDestroyed();
		}
		_anchor.OnAnchorToSurface -= OnAnchor;
		_anchor.OnUnanchorFromSurface -= OnUnanchor;
		if (_warpEffect != null)
		{
			_warpEffect.OnWarpComplete -= OnWarpComplete;
		}
	}

	public LightSourceType GetLightSourceType()
	{
		return LightSourceType.PROBE;
	}

	public Vector3 GetLightSourcePosition()
	{
		return _lightSourceVol.transform.position;
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer = 0f, float maxDistance = float.PositiveInfinity)
	{
		for (int i = 0; i < _illuminationCheckLights.Length; i++)
		{
			if (_illuminationCheckLights[i].CheckIlluminationAtPoint(point, buffer, maxDistance))
			{
				return true;
			}
		}
		return false;
	}

	public OWLight2[] GetLights()
	{
		return _illuminationCheckLights;
	}

	public void FlickerOffAndOn(float offDuration, float onDuration)
	{
		_lightFlickerController.FlickerOffAndOn(offDuration, onDuration);
	}

	public void Launch(Transform launcherTransform, Vector3 launchVelocity, bool horizonTracking = false, float horizonOverride = 0f)
	{
		base.gameObject.SetActive(value: true);
		if (_lightSourceVol != null)
		{
			_lightSourceVol.SetVolumeActivation(active: true);
		}
		base.transform.parent = null;
		_owRigidbody.SetPosition(launcherTransform.position);
		_owRigidbody.SetRotation(launcherTransform.rotation);
		_owRigidbody.SetVelocity(launchVelocity);
		SectorDetector sectorDetector = (PlayerState.AtFlightConsole() ? Locator.GetShipDetector().GetComponent<SectorDetector>() : Locator.GetPlayerSectorDetector());
		RulesetDetector rulesetDetector = (PlayerState.AtFlightConsole() ? Locator.GetShipDetector().GetComponent<RulesetDetector>() : Locator.GetPlayerRulesetDetector());
		if (horizonTracking)
		{
			_horizonTracker.TrackHorizon(rulesetDetector.GetPlanetoidRuleset(), horizonOverride);
		}
		if (_sectorDetector != null)
		{
			sectorDetector.AddDetectorToAllOccupiedSectors(_sectorDetector);
		}
		_detectorCollider.SetActivation(active: true);
		_detectorShape.SetActivation(newActive: true);
		_hudMarker.MarkAsLaunched();
		if (this.OnLaunchProbe != null)
		{
			this.OnLaunchProbe();
		}
		_anchor.OnLaunch();
	}

	public void Deactivate()
	{
		if (this.OnRetrieveProbe != null)
		{
			this.OnRetrieveProbe();
		}
		if (_lightSourceVol != null)
		{
			_lightSourceVol.SetVolumeActivation(active: false);
		}
		_hudMarker.DestroyFogMarkersOnRetrieve();
		_hudMarker.MarkAsRetrieved();
		Locator.GetMarkerManager().RequestFogMarkerUpdate();
		base.transform.parent = null;
		base.transform.localScale = Vector3.one;
		_rotatingCam.ResetRotation();
		_forwardCam.SetSandLevelController(null);
		_reverseCam.SetSandLevelController(null);
		_rotatingCam.SetSandLevelController(null);
		_detectorCollider.SetActivation(active: false);
		_detectorShape.SetActivation(newActive: false);
		base.gameObject.SetActive(value: false);
		_isRetrieving = false;
	}

	public void SetAsTimeLoopCoreDuplicate(Transform anchorPoint)
	{
		GetProbeHUDMarker().SetAsTLCoreDuplicate(value: true);
		_anchor.AnchorToSocket(anchorPoint);
		_isTimeLoopCoreDuplicate = true;
	}

	public bool IsTimeLoopCoreDuplicate()
	{
		return _isTimeLoopCoreDuplicate;
	}

	private void OnAnchor()
	{
		if (_anchor.GetSandLevelController() != null && (_sectorDetector == null || !_sectorDetector.IsWithinSector(Sector.Name.TimeLoopDevice)))
		{
			_forwardCam.SetSandLevelController(_anchor.GetSandLevelController());
			_reverseCam.SetSandLevelController(_anchor.GetSandLevelController());
			_rotatingCam.SetSandLevelController(_anchor.GetSandLevelController());
		}
		if (this.OnAnchorProbe != null)
		{
			this.OnAnchorProbe();
		}
	}

	private void OnUnanchor()
	{
		_forwardCam.SetSandLevelController(null);
		_reverseCam.SetSandLevelController(null);
		_rotatingCam.SetSandLevelController(null);
		if (this.OnUnanchorProbe != null)
		{
			this.OnUnanchorProbe();
		}
	}

	private void OnWarpComplete()
	{
		Deactivate();
		GlobalMessenger<SurveyorProbe>.FireEvent("RetrieveProbe", this);
	}

	public void Retrieve(float retrievalLength)
	{
		if (!_isRetrieving)
		{
			_isRetrieving = true;
			if (_warpEffect != null)
			{
				_warpEffect.WarpObjectOut(retrievalLength);
			}
			if (this.OnStartRetrieveProbe != null)
			{
				this.OnStartRetrieveProbe(retrievalLength);
			}
		}
	}

	public void Unanchor()
	{
		if (_anchor.IsAnchored())
		{
			_anchor.UnanchorFromSurface();
		}
	}

	public void ExternalRetrieve(bool silent = false)
	{
		if (!_isRetrieving)
		{
			if (silent)
			{
				GlobalMessenger<SurveyorProbe>.FireEvent("ForceRetrieveProbeSilent", this);
			}
			else
			{
				GlobalMessenger<SurveyorProbe>.FireEvent("ForceRetrieveProbe", this);
			}
		}
	}

	public VoidShadowEffectController GetVoidShadowEffect()
	{
		return _voidShadowEffect;
	}
}
