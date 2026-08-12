using UnityEngine;

public class ShipCockpitUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _shipBodyRoot;

	[SerializeField]
	private ShipCockpitController _shipSystemsCtrlr;

	[SerializeField]
	private ShipNotificationDisplay _shipConsole;

	private Autopilot _autopilot;

	private ShipDamageController _shipDamageCtrlr;

	private ShipAudioController _shipAudioController;

	private bool _shipDestroyed;

	[Header("Probe Launcher")]
	[SerializeField]
	private Transform _probeLauncherDisplay;

	[SerializeField]
	private Transform _probeLauncherStowRotation;

	[SerializeField]
	private Transform _probeLauncherDisplayRotation;

	[SerializeField]
	private ShipLight _probeLauncherScreenLight;

	[SerializeField]
	private float _probeRotateTime;

	private bool _displayProbeLauncherScreen;

	private float _probeScreenT;

	private OWCamera _playerCam;

	[Header("Signalscope")]
	[SerializeField]
	private SignalscopeUI _signalscopeUI;

	[SerializeField]
	private float _screenEmissionOff;

	[SerializeField]
	private ShipLight _sigScopeScreenLight;

	[SerializeField]
	private Transform _sigScopeDisplay;

	[SerializeField]
	private Transform _sigScopeStowRotation;

	[SerializeField]
	private Transform _sigScopeDisplayRotation;

	[SerializeField]
	private Transform _sigScopeDish;

	[SerializeField]
	private float _scopeRotateTime;

	private bool _displaySignalscopeScreen;

	private float _signalscopeScreenT;

	private Signalscope _signalscopeTool;

	[SerializeField]
	private MeshRenderer _scopeMeshRenderer;

	private Material _scopeScreenMaterial;

	private Color _scopeScreenColor;

	private SignalscopeReticleController _reticuleController;

	[Header("Console Screen")]
	[SerializeField]
	private float _screenEmissionConsole = 0.5f;

	[SerializeField]
	private MeshRenderer _consoleScreenMeshRndr;

	private Material _consoleScreenMaterial;

	private Color _consoleScreenColor;

	[Header("Damage Screen")]
	[SerializeField]
	private Light _damageLight;

	[SerializeField]
	private ShipLight _damageScreenLight;

	[SerializeField]
	private MeshRenderer _damageLightMeshRndr;

	[SerializeField]
	private int _damageLightMatIndex;

	private Material _damageLightMaterial;

	private Color _damageLightColor;

	private bool _damageLightOn;

	private bool _hullCritical;

	private bool _reactorCritical;

	[Header("Dashboard Lights")]
	[SerializeField]
	private Light _flashlightlightFLL;

	[SerializeField]
	private MeshRenderer _fllMeshRndr;

	private Material _fllMaterial;

	private Color _fllColor;

	[SerializeField]
	private Light _autopilotLight;

	[SerializeField]
	private MeshRenderer _autopilotLightMeshRndr;

	private Material _autopilotLightMaterial;

	private Color _autopilotLightColor;

	[SerializeField]
	private Light _matchingVelocityLight;

	[SerializeField]
	private MeshRenderer _matchVMeshRndr;

	private Material _matchVLightMaterial;

	private Color _matchVLightColor;

	[Header("Lock-On")]
	[SerializeField]
	private ReferenceFrameGUI _cockpitLockOnGui;

	[SerializeField]
	private Canvas _offScreenIndicatorGui;

	[Header("Landing Cam Panel")]
	[SerializeField]
	private ShipLight _landingCamScreenLight;

	[SerializeField]
	private ShipLight _altimeterLight;

	[SerializeField]
	private ShipLight _minimapLight;

	[SerializeField]
	private ShipLight _minimapNorthPoleLight;

	[SerializeField]
	private ShipLight _minimapSouthPoleLight;

	[SerializeField]
	private ShipLight _minimapProbeLight;

	[SerializeField]
	private ShipLight _minimapShipLight;

	[SerializeField]
	private Canvas _landingCamOffScreenIndicatorGui;

	private int _propID_EmissionColor;

	private NotificationData _exitToRepairMessage;

	private bool _checkPostToRepairMessage;

	private void Awake()
	{
		_exitToRepairMessage = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationExitShip), 0f);
		_probeScreenT = 0f;
		_signalscopeScreenT = 0f;
		_autopilot = _shipBodyRoot.GetRequiredComponent<Autopilot>();
		_shipDamageCtrlr = _shipBodyRoot.GetRequiredComponent<ShipDamageController>();
		_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
		_landingCamScreenLight.SetOn(on: false);
		_altimeterLight.SetOn(on: false);
		_minimapLight.SetOn(on: false);
		_minimapNorthPoleLight.SetOn(on: false);
		_minimapSouthPoleLight.SetOn(on: false);
		_minimapProbeLight.SetOn(on: false);
		_minimapShipLight.SetOn(on: false);
		_landingCamOffScreenIndicatorGui.gameObject.SetActive(value: false);
		_consoleScreenMaterial = new Material(_consoleScreenMeshRndr.sharedMaterial);
		_consoleScreenMeshRndr.sharedMaterial = _consoleScreenMaterial;
		_consoleScreenColor = _consoleScreenMaterial.GetColor(_propID_EmissionColor);
		_scopeScreenMaterial = new Material(_scopeMeshRenderer.sharedMaterial);
		_scopeMeshRenderer.sharedMaterial = _scopeScreenMaterial;
		_scopeScreenColor = _scopeScreenMaterial.GetColor(_propID_EmissionColor);
		Material[] sharedMaterials = _damageLightMeshRndr.sharedMaterials;
		_damageLightMaterial = new Material(sharedMaterials[_damageLightMatIndex]);
		sharedMaterials[_damageLightMatIndex] = _damageLightMaterial;
		_damageLightMeshRndr.sharedMaterials = sharedMaterials;
		_damageLightColor = _damageLightMaterial.GetColor(_propID_EmissionColor);
		_fllMaterial = new Material(_fllMeshRndr.sharedMaterial);
		_fllMeshRndr.sharedMaterial = _fllMaterial;
		_fllColor = _fllMaterial.GetColor(_propID_EmissionColor);
		_autopilotLightMaterial = new Material(_autopilotLightMeshRndr.sharedMaterial);
		_autopilotLightMeshRndr.sharedMaterial = _autopilotLightMaterial;
		_autopilotLightColor = _autopilotLightMaterial.GetColor(_propID_EmissionColor);
		_matchVLightMaterial = new Material(_matchVMeshRndr.sharedMaterial);
		_matchVMeshRndr.sharedMaterial = _matchVLightMaterial;
		_matchVLightColor = _matchVLightMaterial.GetColor(_propID_EmissionColor);
		_damageScreenLight.SetOn(on: false);
		_damageLight.enabled = false;
		_damageLightMaterial.SetColor(_propID_EmissionColor, 0f * _damageLightColor);
		_flashlightlightFLL.enabled = false;
		_fllMaterial.SetColor(_propID_EmissionColor, 0f * _fllColor);
		_autopilotLight.enabled = false;
		_autopilotLightMaterial.SetColor(_propID_EmissionColor, 0f * _autopilotLightColor);
		_matchingVelocityLight.enabled = false;
		_matchVLightMaterial.SetColor(_propID_EmissionColor, 0f * _matchVLightColor);
		_consoleScreenMaterial.SetColor(_propID_EmissionColor, _screenEmissionOff * _consoleScreenColor);
		_scopeScreenMaterial.SetColor(_propID_EmissionColor, _screenEmissionOff * _scopeScreenColor);
		_sigScopeScreenLight.SetOn(on: false);
		_probeLauncherDisplay.localRotation = _probeLauncherStowRotation.localRotation;
		_probeLauncherScreenLight.SetOn(on: false);
		_shipDamageCtrlr.OnShipHullDamaged += OnShipHullDamaged;
		_shipDamageCtrlr.OnShipHullRepaired += OnShipHullRepaired;
		_shipDamageCtrlr.OnShipComponentDamaged += OnShipComponentDamaged;
		_shipDamageCtrlr.OnShipComponentRepaired += OnShipComponentRepaired;
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", OnSignalscopeEquipped);
		GlobalMessenger.AddListener("UnequipSignalscope", OnSignalscopeUnequipped);
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
		GlobalMessenger<ProbeCamera>.AddListener("ProbeSnapshot", OnProbeSnapshot);
		GlobalMessenger.AddListener("Probe Snapshot Removed", OnProbeSnapshotRemoved);
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
	}

	private void Start()
	{
		_playerCam = Locator.GetPlayerCamera();
		_cockpitLockOnGui.SetActiveCamera(_playerCam);
		_cockpitLockOnGui.gameObject.SetActive(value: false);
		_offScreenIndicatorGui.gameObject.SetActive(value: false);
		_signalscopeTool = _playerCam.GetComponentInChildren<Signalscope>();
		_shipAudioController = Locator.GetShipTransform().GetComponentInChildren<ShipAudioController>();
	}

	private void OnDestroy()
	{
		Object.Destroy(_consoleScreenMaterial);
		Object.Destroy(_scopeScreenMaterial);
		Object.Destroy(_damageLightMaterial);
		Object.Destroy(_fllMaterial);
		Object.Destroy(_autopilotLightMaterial);
		Object.Destroy(_matchVLightMaterial);
		_consoleScreenMaterial = null;
		_scopeScreenMaterial = null;
		_damageLightMaterial = null;
		_fllMaterial = null;
		_autopilotLightMaterial = null;
		_matchVLightMaterial = null;
		_shipDamageCtrlr.OnShipComponentDamaged -= OnShipComponentDamaged;
		_shipDamageCtrlr.OnShipComponentRepaired -= OnShipComponentRepaired;
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger<Signalscope>.RemoveListener("EquipSignalscope", OnSignalscopeEquipped);
		GlobalMessenger.RemoveListener("UnequipSignalscope", OnSignalscopeUnequipped);
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
		GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", OnProbeSnapshot);
		GlobalMessenger.RemoveListener("Probe Snapshot Removed", OnProbeSnapshotRemoved);
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
	}

	public void RegisterSignalscopeReticuleController(SignalscopeReticleController scopeReticuleCtrlr)
	{
		_reticuleController = scopeReticuleCtrlr;
	}

	private void Update()
	{
		if (_shipSystemsCtrlr.AreExternalLightsOn())
		{
			if (!_flashlightlightFLL.enabled)
			{
				_flashlightlightFLL.enabled = true;
				_fllMaterial.SetColor(_propID_EmissionColor, _fllColor);
			}
		}
		else if (_flashlightlightFLL.enabled)
		{
			_flashlightlightFLL.enabled = false;
			_fllMaterial.SetColor(_propID_EmissionColor, 0f * _fllColor);
		}
		if (_autopilot.IsFlyingToDestination() || _autopilot.IsLiningUpDestination() || _autopilot.IsApproachingDestination())
		{
			if (!_autopilotLight.enabled)
			{
				_autopilotLight.enabled = true;
				_autopilotLightMaterial.SetColor(_propID_EmissionColor, _autopilotLightColor);
			}
		}
		else if (_autopilotLight.enabled)
		{
			_autopilotLight.enabled = false;
			_autopilotLightMaterial.SetColor(_propID_EmissionColor, 0f * _autopilotLightColor);
		}
		if (_autopilot.IsMatchingVelocity())
		{
			if (!_matchingVelocityLight.enabled)
			{
				_matchingVelocityLight.enabled = true;
				_matchVLightMaterial.SetColor(_propID_EmissionColor, _matchVLightColor);
			}
		}
		else if (_matchingVelocityLight.enabled)
		{
			_matchingVelocityLight.enabled = false;
			_matchVLightMaterial.SetColor(_propID_EmissionColor, 0f * _matchVLightColor);
		}
		if (_shipSystemsCtrlr.UsingLandingCam() && !_landingCamScreenLight.IsOn())
		{
			_landingCamScreenLight.SetOn(on: true);
			_altimeterLight.SetOn(on: true);
			_minimapLight.SetOn(on: true);
			_minimapNorthPoleLight.SetOn(on: true);
			_minimapSouthPoleLight.SetOn(on: true);
			_minimapProbeLight.SetOn(on: true);
			_minimapShipLight.SetOn(on: true);
			_landingCamOffScreenIndicatorGui.gameObject.SetActive(value: true);
		}
		else if (!_shipSystemsCtrlr.UsingLandingCam() && _landingCamScreenLight.IsOn())
		{
			_landingCamScreenLight.SetOn(on: false);
			_altimeterLight.SetOn(on: false);
			_minimapLight.SetOn(on: false);
			_minimapNorthPoleLight.SetOn(on: false);
			_minimapSouthPoleLight.SetOn(on: false);
			_minimapProbeLight.SetOn(on: false);
			_minimapShipLight.SetOn(on: false);
			_landingCamOffScreenIndicatorGui.gameObject.SetActive(value: false);
		}
		if (!_checkPostToRepairMessage)
		{
			return;
		}
		bool flag = NotificationManager.SharedInstance.IsPinnedNotification(_exitToRepairMessage);
		if (_shipDamageCtrlr.ShouldExitShipToRepair())
		{
			if (flag)
			{
				_shipConsole.BringNotificationToTop(_exitToRepairMessage);
			}
			else
			{
				NotificationManager.SharedInstance.PostNotification(_exitToRepairMessage, pin: true);
			}
		}
		else if (flag)
		{
			NotificationManager.SharedInstance.UnpinNotification(_exitToRepairMessage);
		}
		_checkPostToRepairMessage = false;
	}

	private void FixedUpdate()
	{
		UpdateShipConsole();
		UpdateSignalscopeCanvas();
		if (_displayProbeLauncherScreen && _probeScreenT < 1f)
		{
			_probeScreenT = Mathf.Clamp01(_probeScreenT + Time.deltaTime / _probeRotateTime);
			_probeLauncherDisplay.rotation = Quaternion.Lerp(_probeLauncherStowRotation.rotation, _probeLauncherDisplayRotation.rotation, Mathf.SmoothStep(0f, 1f, _probeScreenT));
			if (_probeScreenT >= 1f)
			{
				_shipAudioController.StopProbeScreenMotor();
			}
		}
		else if (!_displayProbeLauncherScreen && _probeScreenT > 0f)
		{
			_probeScreenT = Mathf.Clamp01(_probeScreenT - Time.deltaTime / _probeRotateTime);
			_probeLauncherDisplay.rotation = Quaternion.Lerp(_probeLauncherStowRotation.rotation, _probeLauncherDisplayRotation.rotation, Mathf.SmoothStep(0f, 1f, _probeScreenT));
			if (_probeScreenT <= 0f)
			{
				_shipAudioController.StopProbeScreenMotor();
			}
		}
		if (_displaySignalscopeScreen && _signalscopeScreenT < 1f)
		{
			_signalscopeScreenT = Mathf.Clamp01(_signalscopeScreenT + Time.deltaTime / _scopeRotateTime);
			_sigScopeDisplay.rotation = Quaternion.Lerp(_sigScopeStowRotation.rotation, _sigScopeDisplayRotation.rotation, Mathf.SmoothStep(0f, 1f, _signalscopeScreenT));
			_sigScopeDish.localEulerAngles = new Vector3(Mathf.SmoothStep(0f, 90f, _signalscopeScreenT), 0f, 0f);
			if (_signalscopeScreenT >= 1f)
			{
				_shipAudioController.StopSigScopeSlide();
				_shipAudioController.PlaySignalscopeInPosition();
			}
		}
		else if (!_displaySignalscopeScreen && _signalscopeScreenT > 0f)
		{
			_signalscopeScreenT = Mathf.Clamp01(_signalscopeScreenT - Time.deltaTime / _scopeRotateTime);
			_sigScopeDisplay.rotation = Quaternion.Lerp(_sigScopeStowRotation.rotation, _sigScopeDisplayRotation.rotation, Mathf.SmoothStep(0f, 1f, _signalscopeScreenT));
			_sigScopeDish.localEulerAngles = new Vector3(Mathf.SmoothStep(0f, 90f, _signalscopeScreenT), 0f, 0f);
			if (_signalscopeScreenT <= 0f)
			{
				_shipAudioController.StopSigScopeSlide();
				_shipAudioController.PlaySignalscopeInPosition();
			}
		}
	}

	private void OnShipHullDamaged(ShipHull shipHull)
	{
		_hullCritical = _shipDamageCtrlr.GetLowestHullIntegrity() < 0.3f;
		if ((_hullCritical || _reactorCritical) && !_damageLightOn)
		{
			_damageLight.enabled = true;
			_damageLightMaterial.SetColor(_propID_EmissionColor, _damageLightColor);
			_damageLightOn = true;
		}
		_damageScreenLight.SetOn(on: true);
		NotificationData data = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(shipHull.hullName) + UITextLibrary.GetString(UITextType.NotificationDamaged), 3f);
		if (NotificationManager.SharedInstance.IsPinnedNotification(data))
		{
			_shipConsole.BringNotificationToTop(data);
		}
		else
		{
			NotificationManager.SharedInstance.PostNotification(data, pin: true);
		}
		_checkPostToRepairMessage = true;
	}

	private void OnShipHullRepaired(ShipHull shipHull)
	{
		_hullCritical = _shipDamageCtrlr.GetLowestHullIntegrity() < 0.3f;
		if (!_hullCritical && !_reactorCritical && _damageLightOn)
		{
			_damageLight.enabled = false;
			_damageLightMaterial.SetColor(_propID_EmissionColor, 0f * _damageLightColor);
			_damageLightOn = false;
		}
		if (!_shipDamageCtrlr.IsDamaged())
		{
			_damageScreenLight.SetOn(on: false);
		}
		NotificationData data = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(shipHull.hullName) + UITextLibrary.GetString(UITextType.NotificationDamaged), 3f);
		NotificationManager.SharedInstance.UnpinNotification(data);
		_checkPostToRepairMessage = true;
	}

	private void OnShipComponentDamaged(ShipComponent shipComponent)
	{
		if (shipComponent is ShipReactorComponent)
		{
			_reactorCritical = true;
		}
		if ((_hullCritical || _reactorCritical) && !_damageLightOn)
		{
			_damageLight.enabled = true;
			_damageLightMaterial.SetColor(_propID_EmissionColor, _damageLightColor);
			_damageLightOn = true;
		}
		_damageScreenLight.SetOn(on: true);
		NotificationData data = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(shipComponent.componentName) + UITextLibrary.GetString(UITextType.NotificationDamaged), 3f);
		if (NotificationManager.SharedInstance.IsPinnedNotification(data))
		{
			_shipConsole.BringNotificationToTop(data);
		}
		else
		{
			NotificationManager.SharedInstance.PostNotification(data, pin: true);
		}
		_checkPostToRepairMessage = true;
	}

	private void OnShipComponentRepaired(ShipComponent shipComponent)
	{
		if (shipComponent is ShipReactorComponent)
		{
			_reactorCritical = false;
		}
		if (!_hullCritical && !_reactorCritical && _damageLightOn)
		{
			_damageLight.enabled = false;
			_damageLightMaterial.SetColor(_propID_EmissionColor, 0f * _damageLightColor);
			_damageLightOn = false;
		}
		if (!_shipDamageCtrlr.IsDamaged())
		{
			_damageScreenLight.SetOn(on: false);
		}
		NotificationData data = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(shipComponent.componentName) + UITextLibrary.GetString(UITextType.NotificationDamaged), 3f);
		NotificationManager.SharedInstance.UnpinNotification(data);
		_checkPostToRepairMessage = true;
	}

	private void UpdateSignalscopeCanvas()
	{
		bool flag = false;
		if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.SignalScope)
		{
			if (_signalscopeUI.IsActivated())
			{
				_signalscopeUI.Deactivate();
			}
		}
		else if (PlayerState.AtFlightConsole())
		{
			flag = true;
			if (!_signalscopeUI.IsActivated())
			{
				if (_reticuleController == null)
				{
					Debug.LogError("ReticuleController cannot be null!");
				}
				_signalscopeUI.Activate(_signalscopeTool, _reticuleController);
			}
		}
		_scopeScreenMaterial.SetColor(_propID_EmissionColor, _scopeScreenColor * (flag ? 1f : 0f));
		_sigScopeScreenLight.SetOn(flag);
	}

	private void UpdateShipConsole()
	{
		float num = _screenEmissionOff;
		if (_shipConsole.AreNotificationsVisible(NotificationTarget.Ship))
		{
			num = _screenEmissionConsole;
		}
		_consoleScreenMaterial.SetColor(_propID_EmissionColor, num * _consoleScreenColor);
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		if (!_shipDestroyed)
		{
			_cockpitLockOnGui.gameObject.SetActive(value: true);
			_offScreenIndicatorGui.gameObject.SetActive(value: true);
		}
	}

	private void OnExitFlightConsole()
	{
		if (!_shipDestroyed)
		{
			_cockpitLockOnGui.gameObject.SetActive(value: false);
			_offScreenIndicatorGui.gameObject.SetActive(value: false);
			_displayProbeLauncherScreen = false;
		}
	}

	private void OnProbeLauncherEquipped(ProbeLauncher launcher)
	{
		if (!_shipDestroyed && launcher == _shipSystemsCtrlr.GetShipProbeLauncher())
		{
			_displayProbeLauncherScreen = true;
			_shipAudioController.PlayProbeScreenMotor();
		}
	}

	private void OnProbeLauncherUnequipped(ProbeLauncher launcher)
	{
		if (!_shipDestroyed && launcher == _shipSystemsCtrlr.GetShipProbeLauncher())
		{
			_displayProbeLauncherScreen = false;
			_probeLauncherScreenLight.SetOn(on: false);
			_shipAudioController.PlayProbeScreenMotor();
		}
	}

	private void OnProbeSnapshot(ProbeCamera camera)
	{
		if (!_shipDestroyed && _displayProbeLauncherScreen)
		{
			_probeLauncherScreenLight.SetOn(on: true);
		}
	}

	private void OnProbeSnapshotRemoved()
	{
		if (!_shipDestroyed && _displayProbeLauncherScreen)
		{
			_probeLauncherScreenLight.SetOn(on: false);
		}
	}

	private void OnSignalscopeEquipped(Signalscope scope)
	{
		if (!_shipDestroyed && PlayerState.AtFlightConsole())
		{
			_displaySignalscopeScreen = true;
			_sigScopeScreenLight.SetOn(on: true);
			_shipAudioController.PlaySigScopeSlide();
		}
	}

	private void OnSignalscopeUnequipped()
	{
		if (!_shipDestroyed && PlayerState.AtFlightConsole())
		{
			_displaySignalscopeScreen = false;
			_sigScopeScreenLight.SetOn(on: false);
			_shipAudioController.PlaySigScopeSlide();
		}
	}

	private void OnShipSystemFailure()
	{
		_shipDestroyed = true;
		if (_flashlightlightFLL.enabled)
		{
			_flashlightlightFLL.enabled = false;
			_fllMaterial.SetColor(_propID_EmissionColor, 0f * _fllColor);
		}
		if (_autopilotLight.enabled)
		{
			_autopilotLight.enabled = false;
			_autopilotLightMaterial.SetColor(_propID_EmissionColor, 0f * _autopilotLightColor);
		}
		if (_matchingVelocityLight.enabled)
		{
			_matchingVelocityLight.enabled = false;
			_matchVLightMaterial.SetColor(_propID_EmissionColor, 0f * _matchVLightColor);
		}
		if (_landingCamScreenLight.IsOn())
		{
			_landingCamScreenLight.SetOn(on: false);
			_altimeterLight.SetOn(on: false);
			_minimapLight.SetOn(on: false);
			_minimapNorthPoleLight.SetOn(on: false);
			_minimapSouthPoleLight.SetOn(on: false);
			_minimapProbeLight.SetOn(on: false);
			_minimapShipLight.SetOn(on: false);
			_landingCamOffScreenIndicatorGui.gameObject.SetActive(value: false);
		}
		_consoleScreenMaterial.SetColor(_propID_EmissionColor, 0f * _consoleScreenColor);
		if (_signalscopeUI.IsActivated())
		{
			_signalscopeUI.Deactivate();
		}
		_scopeScreenMaterial.SetColor(_propID_EmissionColor, 0f * _scopeScreenColor);
		_sigScopeScreenLight.SetOn(on: false);
		_shipAudioController.StopSigScopeSlide();
		_shipAudioController.StopProbeScreenMotor();
		if (_damageLightOn)
		{
			_damageLight.enabled = false;
			_damageLightMaterial.SetColor(_propID_EmissionColor, 0f * _damageLightColor);
			_damageLightOn = false;
		}
		_damageScreenLight.SetOn(on: false);
		_cockpitLockOnGui.gameObject.SetActive(value: false);
		_offScreenIndicatorGui.gameObject.SetActive(value: false);
		base.enabled = false;
	}
}
