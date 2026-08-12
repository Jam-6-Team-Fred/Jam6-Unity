using UnityEngine;

public class ShipCockpitController : MonoBehaviour
{
	[SerializeField]
	private ShipLight _headlight;

	[SerializeField]
	private ShipLight _landingLight;

	[SerializeField]
	private ShipLight[] _dimmingLights = new ShipLight[0];

	[SerializeField]
	private float _dimmingLightScale = 0.5f;

	[SerializeField]
	private ProbeLauncher _shipProbeLauncher;

	[SerializeField]
	private PlayerAttachPoint _playerAttachPoint;

	[SerializeField]
	private Collider _cockpitCollider;

	[SerializeField]
	private Collider[] _fogLightOcclusionColliders;

	[SerializeField]
	private ShipCanvasGroup[] _shipCanvases = new ShipCanvasGroup[0];

	[SerializeField]
	private ShipCanvasGroup[] _dashboardCanvases = new ShipCanvasGroup[0];

	[SerializeField]
	private LandingCamera _landingCam;

	private OWRigidbody _shipBody;

	private SingleInteractionVolume _interactVolume;

	private ShipThrusterController _thrustController;

	private ThrusterModel _thrusterModel;

	private Autopilot _autopilot;

	private LandingPadManager _landingManager;

	private ReferenceFrameTracker _rfTracker;

	private ShipAudioController _shipAudioController;

	private OWCamera _playerCam;

	private PlayerCameraController _playerCamController;

	private ShipCameraComponent _landingCamComponent;

	private Vector3 _playerAttachOffset = Vector3.zero;

	private Vector3 _origAttachPointLocalPos;

	private Vector3 _raisedAttachPointLocalPos;

	private bool _playerAtFlightConsole;

	private float _enterFlightConsoleTime;

	private float _exitFlightConsoleTime;

	private bool _isLandingMode;

	private bool _usingLandingCam;

	private float _initLandingCamTime;

	private bool _enteringLandingCam;

	private bool _externalLightsOn = true;

	private bool _controlsLocked;

	private float _controlsUnlockTime;

	private bool _shipSystemFailure;

	private bool _sunExploded;

	private const float FREELOOK_CAM_TRANSITION_LENGTH = 0.5f;

	private const float LANDING_CAM_TRANSITION_LENGTH = 0.5f;

	private void Awake()
	{
		_interactVolume = _playerAttachPoint.GetRequiredComponent<SingleInteractionVolume>();
		_shipBody = base.gameObject.GetAttachedOWRigidbody();
		_thrustController = _shipBody.GetRequiredComponent<ShipThrusterController>();
		_autopilot = _shipBody.GetRequiredComponent<Autopilot>();
		_thrusterModel = _shipBody.GetRequiredComponent<ThrusterModel>();
		_landingManager = _shipBody.GetRequiredComponentInChildren<LandingPadManager>();
		_shipAudioController = _shipBody.GetComponentInChildren<ShipAudioController>();
		_landingCamComponent = _shipBody.GetComponentInChildren<ShipCameraComponent>();
		_landingCamComponent.OnDamaged += OnLandingCameraDamaged;
		GlobalMessenger<ReferenceFrame>.AddListener("TargetReferenceFrame", OnTargetReferenceFrame);
		GlobalMessenger.AddListener("UntargetReferenceFrame", OnUntargetReferenceFrame);
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
		GlobalMessenger.AddListener("SunExploded", OnSunExploded);
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract += OnPressInteract;
		}
		_thrustController.enabled = false;
	}

	private void Start()
	{
		_playerCam = Locator.GetPlayerCamera();
		_playerCamController = _playerCam.GetRequiredComponent<PlayerCameraController>();
		_rfTracker = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
		_landingCam.enabled = false;
		SetEnableShipLights(enableLights: true, playAudio: false);
		for (int i = 0; i < _fogLightOcclusionColliders.Length; i++)
		{
			_fogLightOcclusionColliders[i].enabled = false;
		}
		_origAttachPointLocalPos = _playerAttachPoint.transform.localPosition;
		_raisedAttachPointLocalPos = _origAttachPointLocalPos + new Vector3(0f, 0.45f, 0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_playerAtFlightConsole)
		{
			RumbleManager.SetShipThrottleOff();
		}
		GlobalMessenger<ReferenceFrame>.RemoveListener("TargetReferenceFrame", OnTargetReferenceFrame);
		GlobalMessenger.RemoveListener("UntargetReferenceFrame", OnUntargetReferenceFrame);
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
		GlobalMessenger.RemoveListener("SunExploded", OnSunExploded);
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract -= OnPressInteract;
		}
	}

	public bool AllowFreeLook()
	{
		if (IsPlayerAtFlightConsole() && !_enteringLandingCam)
		{
			return !_usingLandingCam;
		}
		return false;
	}

	public ProbeLauncher GetShipProbeLauncher()
	{
		return _shipProbeLauncher;
	}

	public bool AreExternalLightsOn()
	{
		return _externalLightsOn;
	}

	public bool InLandingMode()
	{
		return _isLandingMode;
	}

	public bool CheckLandingModePromptConditions()
	{
		if (IsLandingModeAvailable() && !Locator.GetReferenceFrame().GetHideLandingModePrompt())
		{
			ReferenceFrame referenceFrame = Locator.GetReferenceFrame();
			float magnitude = (_shipBody.GetPosition() - referenceFrame.GetPosition()).magnitude;
			Vector3 vector = referenceFrame.GetVelocity() - _shipBody.GetVelocity();
			if (magnitude < referenceFrame.GetAutoAlignmentDistance())
			{
				return vector.magnitude < 50f;
			}
			return false;
		}
		return false;
	}

	public bool IsLandingModeAvailable()
	{
		if (_shipSystemFailure)
		{
			return false;
		}
		ReferenceFrame referenceFrame = Locator.GetReferenceFrame();
		if (referenceFrame != null && referenceFrame.GetAllowAutoAlignment())
		{
			return !_landingManager.IsLanded();
		}
		return false;
	}

	public Vector3 GetLocalVelocity()
	{
		if (Locator.GetReferenceFrame() != null)
		{
			return Locator.GetReferenceFrame().GetOWRigidBody().GetRelativeVelocity(_shipBody);
		}
		return _shipBody.GetVelocity();
	}

	public bool UsingLandingCam()
	{
		return _usingLandingCam;
	}

	public bool IsPlayerAtFlightConsole()
	{
		return _playerAtFlightConsole;
	}

	public bool IsMatchVelocityAvailable(bool ignorePassiveFrame = false)
	{
		if (_playerAtFlightConsole && Locator.GetReferenceFrame(ignorePassiveFrame) != null && !_landingManager.IsLanded() && !_autopilot.IsFlyingToDestination() && !_autopilot.IsDamaged())
		{
			return !_shipSystemFailure;
		}
		return false;
	}

	public bool IsAutopilotAvailable()
	{
		if (_shipSystemFailure || !PlayerData.GetAutopilotEnabled())
		{
			return false;
		}
		if (OWInput.UsingGamepad() && Locator.GetProbe() != null && Locator.GetProbe().IsAnchored() && Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Probe))
		{
			return false;
		}
		ReferenceFrame referenceFrame = Locator.GetReferenceFrame();
		if (_playerAtFlightConsole && referenceFrame != null && referenceFrame.GetAllowAutopilot() && !_landingManager.IsLanded() && !_autopilot.IsDamaged() && Vector3.Distance(_shipBody.GetPosition(), referenceFrame.GetPosition()) > referenceFrame.GetAutopilotArrivalDistance())
		{
			return true;
		}
		return false;
	}

	public void LockUpControls(float lockDuration)
	{
		_controlsLocked = true;
		_controlsUnlockTime = Time.time + lockDuration;
		_thrustController.enabled = false;
		if (_playerAtFlightConsole)
		{
			RumbleManager.SetShipThrottleLocked();
		}
	}

	private void ExitFlightConsole()
	{
		if (_autopilot.IsMatchingVelocity() && !_autopilot.IsFlyingToDestination())
		{
			_autopilot.StopMatchVelocity();
		}
		if (UsingLandingCam())
		{
			ExitLandingView();
		}
		else
		{
			_playerCamController.CenterCameraOverSeconds(0.2f);
		}
		_playerAtFlightConsole = false;
		_thrustController.enabled = false;
		_enteringLandingCam = false;
		_playerAttachOffset = Vector3.zero;
		_playerAttachPoint.SetAttachOffset(_playerAttachOffset);
		_exitFlightConsoleTime = Time.time;
		_shipAudioController.PlayUnbuckle();
		for (int i = 0; i < _dimmingLights.Length; i++)
		{
			_dimmingLights[i].SetIntensityScale(1f);
		}
		if (Locator.GetPlayerSuit().IsWearingSuit() && !Locator.GetPlayerSuit().IsWearingHelmet())
		{
			Locator.GetPlayerSuit().PutOnHelmet();
		}
		_cockpitCollider.gameObject.layer = LayerMask.NameToLayer("Default");
		for (int j = 0; j < _fogLightOcclusionColliders.Length; j++)
		{
			_fogLightOcclusionColliders[j].enabled = false;
		}
		for (int k = 0; k < _shipCanvases.Length; k++)
		{
			_shipCanvases[k].SetGameplayActive(active: true);
		}
		RumbleManager.SetShipThrottleOff();
		OWInput.ChangeInputMode(InputMode.Character);
		GlobalMessenger.FireEvent("ExitFlightConsole");
	}

	private void CompleteExitFlightConsole()
	{
		_playerAttachPoint.DetachPlayer();
		_interactVolume.ResetInteraction();
		_interactVolume.EnableInteraction();
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		if (!_playerAtFlightConsole)
		{
			float t = Mathf.InverseLerp(_exitFlightConsoleTime, _exitFlightConsoleTime + 0.2f, Time.time);
			_playerAttachPoint.transform.localPosition = Vector3.Lerp(_origAttachPointLocalPos, _raisedAttachPointLocalPos, t);
			if (Time.time >= _exitFlightConsoleTime + 0.2f)
			{
				CompleteExitFlightConsole();
			}
			return;
		}
		float t2 = Mathf.InverseLerp(_enterFlightConsoleTime, _enterFlightConsoleTime + 0.4f, Time.time);
		_playerAttachPoint.transform.localPosition = Vector3.Lerp(_raisedAttachPointLocalPos, _origAttachPointLocalPos, t2);
		if (!_shipSystemFailure && PlayerData.GetAutopilotEnabled() && !_sunExploded && _rfTracker.GetReferenceFrame() == null && PlayerState.CheckShipOutsideSolarSystem() && (Locator.GetCloakFieldController() == null || !Locator.GetCloakFieldController().isShipInsideCloak))
		{
			ReferenceFrame referenceFrame = Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody().GetReferenceFrame();
			_rfTracker.TargetReferenceFrame(referenceFrame);
			if (_autopilot.FlyToDestination(referenceFrame))
			{
				NotificationManager.SharedInstance.PostNotification(new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationReturnToSolarSystem)));
			}
		}
	}

	private void Update()
	{
		if (!_playerAtFlightConsole)
		{
			return;
		}
		if (_controlsLocked && Time.time >= _controlsUnlockTime)
		{
			_controlsLocked = false;
			_thrustController.enabled = !_shipSystemFailure;
			if (!_shipSystemFailure)
			{
				if (_thrustController.RequiresIgnition() && _landingManager.IsLanded())
				{
					RumbleManager.SetShipThrottleCold();
				}
				else
				{
					RumbleManager.SetShipThrottleNormal();
				}
			}
		}
		if (!OWInput.IsInputMode(InputMode.ShipCockpit | InputMode.LandingCam))
		{
			if (_autopilot.IsMatchingVelocity() && !_autopilot.IsFlyingToDestination())
			{
				_autopilot.StopMatchVelocity();
			}
			return;
		}
		UpdateShipLightInput();
		if (_autopilot.IsFlyingToDestination())
		{
			if (OWInput.IsNewlyPressed(InputLibrary.autopilot))
			{
				AbortAutopilot();
			}
		}
		else
		{
			if (IsAutopilotAvailable() && OWInput.IsNewlyPressed(InputLibrary.autopilot, InputMode.ShipCockpit))
			{
				InputLibrary.lockOn.BlockNextRelease();
				_autopilot.FlyToDestination(Locator.GetReferenceFrame());
			}
			if (IsMatchVelocityAvailable() && OWInput.IsNewlyPressed(InputLibrary.matchVelocity))
			{
				_autopilot.StartMatchVelocity(Locator.GetReferenceFrame(ignorePassiveFrame: false));
			}
			else if (_autopilot.IsMatchingVelocity() && !_autopilot.IsFlyingToDestination() && OWInput.IsNewlyReleased(InputLibrary.matchVelocity))
			{
				_autopilot.StopMatchVelocity();
			}
			if (!_enteringLandingCam)
			{
				if (!UsingLandingCam() && OWInput.IsNewlyPressed(InputLibrary.landingCamera) && !OWInput.IsPressed(InputLibrary.freeLook))
				{
					EnterLandingView();
				}
				else if (UsingLandingCam() && (OWInput.IsNewlyPressed(InputLibrary.landingCamera) || OWInput.IsNewlyPressed(InputLibrary.cancel)))
				{
					InputLibrary.cancel.ConsumeInput();
					ExitLandingView();
				}
			}
		}
		if (UsingLandingCam())
		{
			if (_enteringLandingCam)
			{
				UpdateEnterLandingCamTransition();
			}
			if (!_isLandingMode && IsLandingModeAvailable())
			{
				EnterLandingMode();
			}
			else if (_isLandingMode && !IsLandingModeAvailable())
			{
				ExitLandingMode();
			}
			_playerAttachOffset = Vector3.MoveTowards(_playerAttachOffset, Vector3.zero, Time.deltaTime);
		}
		else
		{
			_playerAttachOffset = _thrusterModel.GetLocalAcceleration() / _thrusterModel.GetMaxTranslationalThrust() * -0.2f;
			if (Locator.GetToolModeSwapper().GetToolMode() == ToolMode.None && OWInput.IsNewlyPressed(InputLibrary.cancel))
			{
				ExitFlightConsole();
			}
		}
		_playerAttachPoint.SetAttachOffset(_playerAttachOffset);
	}

	private void EnterLandingView()
	{
		_enteringLandingCam = true;
		_initLandingCamTime = Time.time;
		_playerCamController.SnapToDegreesOverSeconds(0f, -48.5f, 0.5f, smoothStep: true);
		_playerCamController.SnapToFieldOfView(24f, 0.5f, smoothStep: true);
		_usingLandingCam = true;
		if (_landingCam.mode == LandingCamera.Mode.Double)
		{
			_landingCam.enabled = true;
		}
		if (_externalLightsOn)
		{
			_headlight.SetOn(on: false);
			_landingLight.SetOn(on: true);
		}
		if (IsLandingModeAvailable())
		{
			EnterLandingMode();
			_autopilot.StartMatchVelocity(Locator.GetReferenceFrame());
		}
		if (_landingCamComponent.isDamaged)
		{
			_shipAudioController.PlayLandingCamOn(AudioType.ShipCockpitLandingCamStatic_LP);
			_shipAudioController.PlayLandingCamStatic(0.25f);
		}
		else
		{
			_shipAudioController.PlayLandingCamOn(AudioType.ShipCockpitLandingCamAmbient_LP);
			_shipAudioController.PlayLandingCamAmbient(0.25f);
		}
	}

	private void ExitLandingView()
	{
		_usingLandingCam = false;
		_landingCam.enabled = false;
		if (_landingCam.mode == LandingCamera.Mode.Swap)
		{
			_playerCam.enabled = true;
		}
		_playerCamController.CenterCameraOverSeconds(0.5f, smoothStep: true);
		_playerCamController.SnapToInitFieldOfView(0.5f, smoothStep: true);
		_thrustController.SetRollMode(isRollMode: false, 1);
		if (_externalLightsOn)
		{
			_headlight.SetOn(on: true);
			_landingLight.SetOn(on: false);
		}
		ExitLandingMode();
		_shipAudioController.PlayLandingCamOff();
		for (int i = 0; i < _dashboardCanvases.Length; i++)
		{
			_dashboardCanvases[i].SetGameplayActive(active: true);
		}
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _playerCam);
		GlobalMessenger.FireEvent("ExitLandingView");
	}

	private void EnterLandingMode()
	{
		if (!_isLandingMode)
		{
			_isLandingMode = true;
			GlobalMessenger<ReferenceFrame>.FireEvent("EnterLandingMode", Locator.GetReferenceFrame());
		}
	}

	private void ExitLandingMode()
	{
		if (_isLandingMode)
		{
			_isLandingMode = false;
			GlobalMessenger.FireEvent("ExitLandingMode");
		}
	}

	private void UpdateEnterLandingCamTransition()
	{
		if (Time.time > _initLandingCamTime + 0.45f)
		{
			_enteringLandingCam = false;
			if (_landingCam.mode == LandingCamera.Mode.Swap)
			{
				_playerCam.enabled = false;
				_landingCam.enabled = true;
			}
			_thrustController.SetRollMode(isRollMode: true, -1);
			_autopilot.StopMatchVelocity();
			for (int i = 0; i < _dashboardCanvases.Length; i++)
			{
				_dashboardCanvases[i].SetGameplayActive(active: false);
			}
			GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _landingCam.owCamera);
			GlobalMessenger.FireEvent("EnterLandingView");
		}
	}

	private void UpdateShipLightInput()
	{
		if (OWInput.IsNewlyPressed(InputLibrary.flashlight) && !_shipSystemFailure)
		{
			_externalLightsOn = !_externalLightsOn;
			SetEnableShipLights(_externalLightsOn);
			string displayMsg = (_externalLightsOn ? UITextLibrary.GetString(UITextType.NotificationShipLightsOn) : UITextLibrary.GetString(UITextType.NotificationShipLightsOff));
			NotificationData data = new NotificationData(NotificationTarget.Ship, displayMsg);
			NotificationManager.SharedInstance.PostNotification(data);
		}
	}

	private void SetEnableShipLights(bool enableLights, bool playAudio = true)
	{
		_headlight.SetOn(enableLights && !_usingLandingCam);
		_landingLight.SetOn(enableLights && _usingLandingCam);
		if (playAudio)
		{
			if (enableLights)
			{
				_shipAudioController.PlayHeadlightOn();
			}
			else
			{
				_shipAudioController.PlayHeadlightOff();
			}
		}
	}

	private void OnPressInteract()
	{
		if (!_playerAtFlightConsole)
		{
			base.enabled = true;
			_playerAtFlightConsole = true;
			_enterFlightConsoleTime = Time.time;
			_interactVolume.DisableInteraction();
			Locator.GetToolModeSwapper().UnequipTool();
			if (_controlsLocked && Time.time > _controlsUnlockTime)
			{
				_controlsLocked = false;
			}
			if (!_controlsLocked && !_shipSystemFailure)
			{
				_thrustController.enabled = true;
			}
			_thrustController.SetRollMode(isRollMode: false, 1);
			_playerAttachPoint.transform.localPosition = _raisedAttachPointLocalPos;
			_playerAttachOffset = Vector3.zero;
			_playerAttachPoint.SetAttachOffset(_playerAttachOffset);
			_playerAttachPoint.AttachPlayer();
			_shipAudioController.PlayBuckle();
			for (int i = 0; i < _dimmingLights.Length; i++)
			{
				_dimmingLights[i].SetIntensityScale(_dimmingLightScale);
			}
			if (Locator.GetPlayerSuit().IsWearingSuit() && !_shipSystemFailure)
			{
				Locator.GetPlayerSuit().RemoveHelmet();
			}
			_cockpitCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			for (int j = 0; j < _fogLightOcclusionColliders.Length; j++)
			{
				_fogLightOcclusionColliders[j].enabled = true;
			}
			for (int k = 0; k < _shipCanvases.Length; k++)
			{
				_shipCanvases[k].SetGameplayActive(active: false);
			}
			if (_controlsLocked || _shipSystemFailure)
			{
				RumbleManager.SetShipThrottleLocked();
			}
			else if (_thrustController.RequiresIgnition() && _landingManager.IsLanded())
			{
				RumbleManager.SetShipThrottleCold();
			}
			else
			{
				RumbleManager.SetShipThrottleNormal();
			}
			OWInput.ChangeInputMode(InputMode.ShipCockpit);
			GlobalMessenger<OWRigidbody>.FireEvent("EnterFlightConsole", _shipBody);
		}
	}

	private void OnTargetReferenceFrame(ReferenceFrame referenceFrame)
	{
		AbortAutopilot();
	}

	private void OnUntargetReferenceFrame()
	{
		AbortAutopilot();
	}

	private void AbortAutopilot()
	{
		_autopilot.Abort();
	}

	private void OnLandingCameraDamaged(ShipComponent component)
	{
		if (_landingCam.enabled)
		{
			_shipAudioController.PlayLandingCamStatic();
		}
	}

	private void OnShipSystemFailure()
	{
		AbortAutopilot();
		_thrustController.enabled = false;
		_shipSystemFailure = true;
		if (_playerAtFlightConsole)
		{
			RumbleManager.SetShipThrottleLocked();
		}
	}

	private void OnSunExploded()
	{
		_sunExploded = true;
	}
}
