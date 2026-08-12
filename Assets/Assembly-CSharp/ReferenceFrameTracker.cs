using UnityEngine;

public class ReferenceFrameTracker : MonoBehaviour
{
	private bool _playerTargetingActive;

	private bool _shipTargetingActive;

	private bool _mapTargetingActive;

	private OWCamera _activeCam;

	private OWCamera _landingCam;

	private bool _isMapView;

	private bool _isLandingView;

	private ReferenceFrame _currentReferenceFrame;

	private ReferenceFrame _possibleReferenceFrame;

	private bool _hasTarget;

	private bool _hasPossibleTarget;

	private float _zSpeed;

	private Vector3 _relativeVelocity;

	private Vector3 _relativeVelocityXY;

	private int _blockerCount;

	private CloakFieldController _cloakController;

	private PlayerAudioController _audioController;

	private void Awake()
	{
		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger.AddListener("PlayerEnterBlackHole", UntargetReferenceFrame);
		GlobalMessenger.AddListener("TeleportPlayer", UntargetReferenceFrame);
		GlobalMessenger.AddListener("PlayerFogWarp", OnPlayerFogWarp);
		GlobalMessenger.AddListener("EnterShip", OnEnterShip);
		GlobalMessenger.AddListener("SunExploded", OnSunExploded);
		GlobalMessenger<OWRigidbody>.AddListener("QuantumMoonChangeState", OnQuantumMoonChangeState);
	}

	private void Start()
	{
		_activeCam = Locator.GetActiveCamera();
		_audioController = Locator.GetPlayerAudioController();
		_cloakController = Locator.GetCloakFieldController();
		if (_cloakController != null)
		{
			_cloakController.OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnterCloak);
			_cloakController.OnPlayerExit += new OWEvent.OWCallback(OnPlayerExitCloak);
		}
	}

	private void OnDisable()
	{
		UntargetReferenceFrame();
	}

	private void OnDestroy()
	{
		if (_cloakController != null)
		{
			_cloakController.OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnterCloak);
			_cloakController.OnPlayerExit -= new OWEvent.OWCallback(OnPlayerExitCloak);
		}
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger.RemoveListener("PlayerEnterBlackHole", UntargetReferenceFrame);
		GlobalMessenger.RemoveListener("TeleportPlayer", UntargetReferenceFrame);
		GlobalMessenger.RemoveListener("PlayerFogWarp", OnPlayerFogWarp);
		GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
		GlobalMessenger.RemoveListener("SunExploded", OnSunExploded);
		GlobalMessenger<OWRigidbody>.RemoveListener("QuantumMoonChangeState", OnQuantumMoonChangeState);
	}

	public void AddBlocker()
	{
		_blockerCount++;
		UntargetReferenceFrame(playAudio: false);
	}

	public void RemoveBlocker()
	{
		_blockerCount = Mathf.Max(_blockerCount - 1, 0);
	}

	public ReferenceFrame GetReferenceFrame(bool ignorePassiveFrame = true)
	{
		if (_hasTarget)
		{
			return _currentReferenceFrame;
		}
		if (!ignorePassiveFrame)
		{
			return Locator.GetPlayerSectorDetector().GetPassiveReferenceFrame();
		}
		return null;
	}

	public bool IsPlayerTargetingActive()
	{
		return _playerTargetingActive;
	}

	public bool IsShipTargetingActive()
	{
		return _shipTargetingActive;
	}

	public bool IsMapTargetingActive()
	{
		return _mapTargetingActive;
	}

	public ReferenceFrame GetPossibleReferenceFrame()
	{
		if (_hasPossibleTarget)
		{
			return _possibleReferenceFrame;
		}
		return null;
	}

	public void TargetReferenceFrame(ReferenceFrame frame)
	{
		if (_hasTarget)
		{
			_currentReferenceFrame.OnReferenceFrameDestroyed -= OnReferenceFrameDestroyed;
			_currentReferenceFrame.GetOWRigidBody().OnWarpOWRigidbody -= OnWarpOWRigidbody;
		}
		_hasTarget = true;
		_currentReferenceFrame = frame;
		_currentReferenceFrame.OnReferenceFrameDestroyed += OnReferenceFrameDestroyed;
		_currentReferenceFrame.GetOWRigidBody().OnWarpOWRigidbody += OnWarpOWRigidbody;
		GlobalMessenger<ReferenceFrame>.FireEvent("TargetReferenceFrame", _currentReferenceFrame);
		_audioController.PlayLockOn();
	}

	public void UntargetReferenceFrame()
	{
		UntargetReferenceFrame(playAudio: true);
	}

	public void UntargetReferenceFrame(bool playAudio)
	{
		if (_hasTarget)
		{
			_currentReferenceFrame.OnReferenceFrameDestroyed -= OnReferenceFrameDestroyed;
			_currentReferenceFrame.GetOWRigidBody().OnWarpOWRigidbody -= OnWarpOWRigidbody;
			_currentReferenceFrame = null;
			_hasTarget = false;
			GlobalMessenger.FireEvent("UntargetReferenceFrame");
			if (playAudio && _audioController != null)
			{
				_audioController.PlayLockOff();
			}
		}
	}

	private void Update()
	{
		if (!(_activeCam == null))
		{
			if (_cloakController != null && _hasTarget && !_currentReferenceFrame.GetOWRigidBody().IsKinematic() && _cloakController.CheckBodyInsideCloak(_currentReferenceFrame.GetOWRigidBody()) != _cloakController.isPlayerInsideCloak)
			{
				UntargetReferenceFrame();
			}
			_playerTargetingActive = Locator.GetPlayerSuit().IsWearingHelmet() && PlayerState.InZeroG() && _blockerCount <= 0;
			_shipTargetingActive = PlayerState.AtFlightConsole();
			_mapTargetingActive = _isMapView && (_playerTargetingActive || PlayerState.IsInsideShip());
			if (_playerTargetingActive || _shipTargetingActive || _mapTargetingActive)
			{
				UpdateTargeting();
			}
		}
	}

	private void UpdateTargeting()
	{
		ReferenceFrame referenceFrame = (_isMapView ? FindReferenceFrameInMapView() : FindReferenceFrameInLineOfSight());
		if (_cloakController != null && referenceFrame != null)
		{
			bool flag = false;
			if (referenceFrame.GetAstroObject() != null && referenceFrame.GetAstroObject().GetAstroObjectName() == AstroObject.Name.RingWorld)
			{
				ReferenceFrame referenceFrame2 = referenceFrame.GetAstroObject().GetOWRigidbody().GetReferenceFrame();
				if (referenceFrame != referenceFrame2)
				{
					flag = true;
				}
			}
			else if (!referenceFrame.GetOWRigidBody().IsKinematic())
			{
				flag = _cloakController.CheckBodyInsideCloak(referenceFrame.GetOWRigidBody());
			}
			if (_cloakController.isPlayerInsideCloak != flag)
			{
				referenceFrame = null;
			}
		}
		InputMode mask = InputMode.Character | InputMode.Map | InputMode.ScopeZoom | InputMode.ShipCockpit | InputMode.LandingCam;
		if (OWInput.IsNewlyPressed(InputLibrary.lockOn, mask))
		{
			if (referenceFrame == null || referenceFrame.Equals(_currentReferenceFrame))
			{
				if (!PlayerState.AtFlightConsole() || _currentReferenceFrame == null || !(_currentReferenceFrame.GetAstroObject() != null) || _currentReferenceFrame.GetAstroObject().GetAstroObjectName() != AstroObject.Name.Sun || !PlayerState.CheckShipOutsideSolarSystem())
				{
					UntargetReferenceFrame();
				}
			}
			else
			{
				if (_isMapView && !PlayerData.GetPersistentCondition("HAS_PLAYER_LOCKED_ON_MAP"))
				{
					PlayerData.SetPersistentCondition("HAS_PLAYER_LOCKED_ON_MAP", state: true);
				}
				else if (!Locator.GetPlayerSuit().IsTrainingSuit() && !PlayerData.GetPersistentCondition("HAS_PLAYER_LOCKED_ON"))
				{
					PlayerData.SetPersistentCondition("HAS_PLAYER_LOCKED_ON", state: true);
				}
				TargetReferenceFrame(referenceFrame);
			}
		}
		_hasPossibleTarget = false;
		if (referenceFrame != null && !referenceFrame.Equals(_currentReferenceFrame))
		{
			_possibleReferenceFrame = referenceFrame;
			_hasPossibleTarget = true;
		}
	}

	private ReferenceFrame FindReferenceFrameInMapView()
	{
		OWCamera activeCam = _activeCam;
		ReferenceFrame result = null;
		Ray ray = new Ray(activeCam.transform.position, activeCam.transform.forward);
		float maxDistance = 100000f;
		RaycastHit[] array = Physics.RaycastAll(ray, maxDistance, OWLayerMask.longRangeRFMask);
		float num = 180f;
		for (int i = 0; i < array.Length; i++)
		{
			ReferenceFrameVolume component = array[i].collider.GetComponent<ReferenceFrameVolume>();
			float maxTargetDistance = component.GetReferenceFrame().GetMaxTargetDistance();
			Vector3 to = array[i].collider.transform.position - activeCam.transform.position;
			float num2 = Vector3.Angle(activeCam.transform.forward, to);
			bool flag = maxTargetDistance <= 0f || to.sqrMagnitude < maxTargetDistance * maxTargetDistance;
			if (num2 < num && flag)
			{
				num = num2;
				result = component.GetReferenceFrame();
			}
		}
		return result;
	}

	private ReferenceFrame FindReferenceFrameInLineOfSight()
	{
		OWCamera oWCamera = (_isLandingView ? _landingCam : _activeCam);
		ReferenceFrame result = null;
		float maxDistance = (PlayerState.InBrambleDimension() ? 120 : 1000);
		Vector3 position = oWCamera.transform.position;
		if (!_isMapView && Physics.Raycast(position, oWCamera.transform.forward, out var hitInfo, maxDistance, OWLayerMask.closeRangeRFMask))
		{
			ReferenceFrameVolume component = hitInfo.collider.GetComponent<ReferenceFrameVolume>();
			if (component != null)
			{
				result = component.GetReferenceFrame();
			}
			else if (hitInfo.rigidbody != null)
			{
				OWRigidbody component2 = hitInfo.rigidbody.GetComponent<OWRigidbody>();
				if (component2 != null && component2.IsTargetable() && (PlayerState.AtFlightConsole() || Vector3.Distance(position, component2.GetPosition()) >= component2.GetReferenceFrame().GetMinSuitTargetDistance()))
				{
					result = component2.GetReferenceFrame();
				}
			}
		}
		else if (!PlayerState.InBrambleDimension() && !PlayerState.InGiantsDeep())
		{
			float maxDistance2 = 100000f;
			RaycastHit[] array = Physics.RaycastAll(oWCamera.transform.position, oWCamera.transform.forward, maxDistance2, OWLayerMask.longRangeRFMask);
			float num = 180f;
			for (int i = 0; i < array.Length; i++)
			{
				ReferenceFrameVolume component3 = array[i].collider.GetComponent<ReferenceFrameVolume>();
				float maxTargetDistance = component3.GetReferenceFrame().GetMaxTargetDistance();
				Vector3 to = array[i].collider.transform.position - oWCamera.transform.position;
				float num2 = Vector3.Angle(oWCamera.transform.forward, to);
				bool flag = maxTargetDistance <= 0f || to.sqrMagnitude < maxTargetDistance * maxTargetDistance;
				if (num2 < num && flag)
				{
					num = num2;
					result = component3.GetReferenceFrame();
				}
			}
		}
		return result;
	}

	private void OnSwitchActiveCamera(OWCamera activeCam)
	{
		if (activeCam.CompareTag("MapCamera"))
		{
			_isMapView = true;
		}
		else
		{
			_isMapView = false;
		}
		if (activeCam.CompareTag("LandingCamera"))
		{
			_isLandingView = true;
			_landingCam = activeCam;
		}
		else
		{
			_isLandingView = false;
			_activeCam = activeCam;
		}
	}

	private void OnReferenceFrameDestroyed()
	{
		UntargetReferenceFrame();
	}

	private void OnWarpOWRigidbody(OWRigidbody body)
	{
		UntargetReferenceFrame();
	}

	private void OnEnterShip()
	{
		if (_currentReferenceFrame == Locator.GetShipBody().GetReferenceFrame())
		{
			UntargetReferenceFrame();
		}
	}

	private void OnSunExploded()
	{
		if (_currentReferenceFrame != null && _currentReferenceFrame.GetAstroObject() != null && _currentReferenceFrame.GetAstroObject().GetAstroObjectName() == AstroObject.Name.Sun)
		{
			UntargetReferenceFrame();
		}
	}

	private void OnQuantumMoonChangeState(OWRigidbody moonBody)
	{
		if (_hasTarget && _currentReferenceFrame.GetOWRigidBody() == moonBody)
		{
			UntargetReferenceFrame();
		}
	}

	private void OnPlayerFogWarp()
	{
		UntargetReferenceFrame(playAudio: false);
	}

	private void OnPlayerEnterCloak()
	{
		UntargetReferenceFrame(playAudio: false);
	}

	private void OnPlayerExitCloak()
	{
		UntargetReferenceFrame(playAudio: false);
	}
}
