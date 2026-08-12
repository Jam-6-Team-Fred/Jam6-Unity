using UnityEngine;

public class DreamWorldController : MonoBehaviour, SunLightController.ISunOverrider
{
	public const float SIMULATION_SPHERE_RADIUS = 20f;

	private readonly int _propID_Radius = Shader.PropertyToID("_Radius");

	private readonly int _propID_DreamSimulationCenter = Shader.PropertyToID("_DreamSimulationCenter");

	[SerializeField]
	private DreamLanternItem _debugPlayerLantern;

	[Header("Simulation")]
	[SerializeField]
	private Transform _primarySimulationRoot;

	[SerializeField]
	private Transform[] _simulationRoots = new Transform[0];

	[SerializeField]
	private SimulationCamera _simulationCamera;

	[SerializeField]
	private OWRenderer _simulationSphere;

	[Space]
	[SerializeField]
	private Sector _dreamWorldSector;

	[SerializeField]
	private OWTriggerVolume _dreamWorldVolume;

	[SerializeField]
	private Color _tempSkyboxColor;

	[Space]
	[SerializeField]
	private SarcophagusController _sarcophagusController;

	[SerializeField]
	private PrisonerDirector _prisonerDirector;

	private OWRigidbody _dreamBody;

	private OWRigidbody _planetBody;

	private RingWorldController _ringWorldController;

	private OWCamera _playerCamera;

	private PlayerCameraEffectController _playerCamEffectController;

	private HeightmapAmbientLightRenderer _playerCamAmbientLightRenderer;

	private ProxyShadowLight _proxyShadowLight;

	private DreamLanternItem _playerLantern;

	private DreamLanternSocket _playerLanternSocket;

	private DreamCampfire _dreamCampfire;

	private DreamArrivalPoint _dreamArrivalPoint;

	private RelativeLocationData _relativeSleepLocation;

	private bool _enteringDream;

	private bool _exitingDream;

	private bool _closingEyes;

	private bool _insideDream;

	private bool _outsideLanternBounds;

	private bool _waitingToLightLantern;

	private bool _suitUpOnWake;

	private float _prevPlayerCameraFarPlaneDist;

	private float _lightLanternTime;

	private float _closeEyesTime;

	private float _eyeAnimDuration;

	private float _exitDreamTime;

	private float _dreamCampfireExtinguishedTime;

	private float _cachedCamDegreesY;

	private float _simulationRadiusBuffer;

	private bool[] _achievementTracker = new bool[7];

	private DreamRaftProjector _lastUsedRaftProjector;

	private LanternZoomPoint _activeZoomPoint;

	private GhostGrabController _activeGhostGrabController;

	private DreamWakeType _wakeType = DreamWakeType.Default;

	public OWEvent OnEnterLanternBounds = new OWEvent(8);

	public OWEvent OnExitLanternBounds = new OWEvent(8);

	[ContextMenu("Scrub Simulation Root", false)]
	private void ScrubSimulationRoot()
	{
		Transform[] simulationRoots = _simulationRoots;
		foreach (Transform transform in simulationRoots)
		{
			if (transform == null)
			{
				continue;
			}
			SectorCullGroup[] componentsInChildren = transform.GetComponentsInChildren<SectorCullGroup>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (componentsInChildren[j].GetSector() == null)
				{
					Debug.Log("Destroyed Cull Group: " + componentsInChildren[j], componentsInChildren[j].gameObject);
					Object.DestroyImmediate(componentsInChildren[j]);
				}
			}
			SectorLightsCullGroup[] componentsInChildren2 = transform.GetComponentsInChildren<SectorLightsCullGroup>();
			for (int k = 0; k < componentsInChildren2.Length; k++)
			{
				if (componentsInChildren2[k].GetSector() == null)
				{
					Debug.Log("Destroyed Lights Cull Group: " + componentsInChildren2[k], componentsInChildren2[k].gameObject);
					Object.DestroyImmediate(componentsInChildren2[k]);
				}
			}
			SectorCollisionGroup[] componentsInChildren3 = transform.GetComponentsInChildren<SectorCollisionGroup>();
			for (int l = 0; l < componentsInChildren3.Length; l++)
			{
				Debug.Log("Destroyed Collision Group: " + componentsInChildren3[l], componentsInChildren3[l].gameObject);
				Object.DestroyImmediate(componentsInChildren3[l]);
			}
			OWCollider[] componentsInChildren4 = transform.GetComponentsInChildren<OWCollider>();
			for (int m = 0; m < componentsInChildren4.Length; m++)
			{
				Debug.Log("Destroyed OWCollider: " + componentsInChildren4[m], componentsInChildren4[m].gameObject);
				Object.DestroyImmediate(componentsInChildren4[m]);
			}
			Collider[] componentsInChildren5 = transform.GetComponentsInChildren<Collider>();
			for (int n = 0; n < componentsInChildren5.Length; n++)
			{
				Debug.Log("Destroyed Collider: " + componentsInChildren5[n], componentsInChildren5[n].gameObject);
				Object.DestroyImmediate(componentsInChildren5[n]);
			}
			Shape[] componentsInChildren6 = transform.GetComponentsInChildren<Shape>();
			for (int num = 0; num < componentsInChildren6.Length; num++)
			{
				Debug.Log("Destroyed Shape: " + componentsInChildren6[num], componentsInChildren6[num].gameObject);
				Object.DestroyImmediate(componentsInChildren6[num]);
			}
		}
	}

	private void Awake()
	{
		Locator.RegisterDreamWorldController(this);
		if (_simulationCamera != null)
		{
			_simulationCamera.enabled = false;
		}
		for (int i = 0; i < _achievementTracker.Length; i++)
		{
			_achievementTracker[i] = false;
		}
	}

	private void Start()
	{
		UpdateSimulationSphereRadius(0f);
		_dreamBody = base.gameObject.GetAttachedOWRigidbody();
		AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.RingWorld);
		if (astroObject != null)
		{
			_planetBody = astroObject.GetOWRigidbody();
			_ringWorldController = _planetBody.GetComponent<RingWorldController>();
		}
		_playerCamera = Locator.GetPlayerCamera();
		_playerCamEffectController = _playerCamera.GetComponent<PlayerCameraEffectController>();
		_playerCamAmbientLightRenderer = _playerCamera.GetComponent<HeightmapAmbientLightRenderer>();
		_proxyShadowLight = Locator.GetSunTransform().GetComponentInChildren<ProxyShadowLight>();
		_simulationCamera.SetTargetCamera(_playerCamera);
		CheckDreamZone2Completion();
	}

	private void OnDestroy()
	{
		if (_dreamCampfire != null)
		{
			_dreamCampfire.OnDreamCampfireExtinguished -= new OWEvent.OWCallback(OnDreamCampfireExtinguished);
		}
	}

	public SunLightController.SunOverrideSettings ApplySunOverrides(OWCamera owCamera, SunLightController.SunOverrideSettings settings)
	{
		if (_insideDream)
		{
			settings.sunIntensity = 0f;
			settings.ambientIntensity = 0f;
			settings.sunShadowStrength = 0f;
		}
		return settings;
	}

	public void UpdateSimulationSphereRadius(float buffer)
	{
		_simulationRadiusBuffer = buffer;
		float num = 20f + _simulationRadiusBuffer;
		_simulationSphere.transform.localScale = Vector3.one * (num * 2f);
		_simulationSphere.SetMaterialProperty(_propID_Radius, num - 0.5f);
	}

	public void SetPlayerLanternSocket(DreamLanternSocket socket)
	{
		_playerLanternSocket = socket;
	}

	public void RegisterLastUsedRaftProjector(DreamRaftProjector raftProjector)
	{
		_lastUsedRaftProjector = raftProjector;
	}

	public void ExtinguishDreamRaft()
	{
		if (_lastUsedRaftProjector != null)
		{
			_lastUsedRaftProjector.ExtinguishImmediately();
			_lastUsedRaftProjector = null;
		}
	}

	public void SetActiveZoomPoint(LanternZoomPoint zoomPoint)
	{
		_activeZoomPoint = zoomPoint;
	}

	public void SetActiveGhostGrabController(GhostGrabController activeGrabController)
	{
		_activeGhostGrabController = activeGrabController;
	}

	public bool IsPlayerGrabbedByGhost()
	{
		return _activeGhostGrabController != null;
	}

	public bool IsPlayerSleepingAtLocation(DreamArrivalPoint.Location location)
	{
		if (_dreamCampfire != null)
		{
			return _dreamCampfire.GetLocation() == location;
		}
		return false;
	}

	public DreamCampfire GetDreamCampfire()
	{
		return _dreamCampfire;
	}

	public DreamLanternItem GetPlayerLantern()
	{
		return _playerLantern;
	}

	public bool IsLanternConcealed()
	{
		return _playerLantern.GetLanternController().IsConcealed();
	}

	public bool IsInDream()
	{
		return _insideDream;
	}

	public bool IsExitingDream()
	{
		return _exitingDream;
	}

	public DreamWakeType GetLastDreamWakeType()
	{
		return _wakeType;
	}

	public SarcophagusController GetSarcophagusController()
	{
		return _sarcophagusController;
	}

	public PrisonerDirector GetPrisonerDirector()
	{
		return _prisonerDirector;
	}

	public void SpawnInDreamWorld(DreamArrivalPoint.Location ringWorldSleepLocation, DreamArrivalPoint zoneArrivalPoint)
	{
		_insideDream = true;
		_dreamArrivalPoint = zoneArrivalPoint;
		_playerCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Sun"));
		_prevPlayerCameraFarPlaneDist = _playerCamera.farClipPlane;
		_playerCamera.farClipPlane = 4000f;
		_playerCamera.mainCamera.backgroundColor = _tempSkyboxColor;
		_playerCamera.planetaryFog.enabled = false;
		_playerCamera.postProcessingSettings.ambientOcclusionAvailable = false;
		_playerCamera.postProcessingSettings.screenSpaceReflectionAvailable = true;
		_playerLantern = _debugPlayerLantern;
		Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(_playerLantern);
		_playerLantern.SetLit(lit: true);
		if (_playerCamAmbientLightRenderer != null)
		{
			_playerCamAmbientLightRenderer.enabled = true;
		}
		_simulationCamera.OnEnterDreamWorld();
		_dreamCampfire = Locator.GetDreamCampfire(ringWorldSleepLocation);
		if (_dreamCampfire != null)
		{
			_dreamCampfire.OnDreamCampfireExtinguished += new OWEvent.OWCallback(OnDreamCampfireExtinguished);
			_relativeSleepLocation = new RelativeLocationData(new Vector3(0f, 1f, -2f), Quaternion.identity, Vector3.zero);
		}
		_dreamWorldSector.GetTriggerVolume().AddObjectToVolume(Locator.GetPlayerDetector());
		_dreamWorldVolume.AddObjectToVolume(Locator.GetPlayerDetector());
		SunLightController.RegisterSunOverrider(this, 1000);
		if (_proxyShadowLight != null)
		{
			_proxyShadowLight.enabled = false;
		}
		Locator.GetAudioMixer().MixDreamWorld();
		_playerLantern.OnEnterDreamWorld();
		GlobalMessenger.FireEvent("EnterDreamWorld");
	}

	public void EnterDreamWorld(DreamCampfire dreamCampfire, DreamArrivalPoint arrivalPoint, RelativeLocationData relativeLocation)
	{
		if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern)
		{
			_dreamCampfire = dreamCampfire;
			_dreamCampfire.OnDreamCampfireExtinguished += new OWEvent.OWCallback(OnDreamCampfireExtinguished);
			_dreamArrivalPoint = arrivalPoint;
			_relativeSleepLocation = relativeLocation;
			_cachedCamDegreesY = _playerCamera.GetComponent<PlayerCameraController>().GetDegreesY();
			_playerLantern = (DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
			_enteringDream = true;
			ReticleController.Hide();
			Locator.GetPromptManager().SetPromptsVisible(visible: false);
			Locator.GetPlayerCameraDetector().GetComponent<AudioDetector>().DeactivateAllVolumes(0f);
			Locator.GetAudioMixer().MixDreamWorld();
		}
		else
		{
			Debug.LogError("Attempting to enter dream world without lantern!");
			Debug.Break();
		}
	}

	public void ExitDreamWorld(DeathType deathType)
	{
		switch (deathType)
		{
		case DeathType.Asphyxiation:
			ExitDreamWorld(DreamWakeType.Asphyxiation);
			break;
		case DeathType.Impact:
			ExitDreamWorld(DreamWakeType.Impact);
			break;
		case DeathType.CrushedByElevator:
			Locator.GetPlayerDeathAudio().PlayCrushedByElevator();
			ExitDreamWorld(DreamWakeType.CrushedByElevator);
			break;
		case DeathType.Default:
			ExitDreamWorld(DreamWakeType.BurnedByFire);
			break;
		default:
			ExitDreamWorld();
			break;
		}
	}

	public void ExitDreamWorld(DreamWakeType wakeType = DreamWakeType.Default)
	{
		if (PlayerState.IsResurrected())
		{
			_playerLantern.SetLit(lit: false);
			Locator.GetDeathManager().KillPlayer(DeathType.Dream);
		}
		else if (_insideDream && !_exitingDream)
		{
			_wakeType = wakeType;
			CheckDreamZone2Completion();
			CheckSleepWakeDieAchievement(wakeType);
			if (wakeType == DreamWakeType.Alarm)
			{
				_cachedCamDegreesY = 50f;
			}
			_playerLantern.SetLit(lit: false);
			_closingEyes = false;
			_closeEyesTime = Time.time;
			if (wakeType == DreamWakeType.LanternBlownOut || wakeType == DreamWakeType.LanternSubmerged || wakeType == DreamWakeType.CampfireExtinguished)
			{
				_closeEyesTime += 0.4f;
			}
			_eyeAnimDuration = 0.5f;
			if (wakeType == DreamWakeType.LanternBlownOut || wakeType == DreamWakeType.CampfireExtinguished || wakeType == DreamWakeType.Alarm)
			{
				_eyeAnimDuration = 0.1f;
			}
			ReticleController.Hide();
			Locator.GetPromptManager().SetPromptsVisible(visible: false);
			_exitingDream = true;
		}
	}

	private void Update()
	{
		if (_enteringDream)
		{
			return;
		}
		if (_exitingDream)
		{
			if (!_closingEyes && Time.time > _closeEyesTime)
			{
				_closingEyes = true;
				float num = ((_wakeType == DreamWakeType.Alarm) ? 0.1f : 0.5f);
				_exitDreamTime = Time.time + _eyeAnimDuration + num;
				_playerCamEffectController.CloseEyes(_eyeAnimDuration);
				Locator.GetAudioMixer().MixSleepAtCampfire(_eyeAnimDuration);
			}
			return;
		}
		if (_outsideLanternBounds)
		{
			UpdateSimulationCamera();
		}
		if (_insideDream)
		{
			if (_waitingToLightLantern && Time.time > _lightLanternTime)
			{
				_waitingToLightLantern = false;
				_playerLantern.SetLit(lit: true);
				ReticleController.Show();
				Locator.GetPromptManager().SetPromptsVisible(visible: true);
			}
			if (_dreamCampfire != null && _dreamCampfire.GetState() == Campfire.State.UNLIT && Time.time > _dreamCampfireExtinguishedTime + 1f)
			{
				ExitDreamWorld(DreamWakeType.CampfireExtinguished);
			}
			if (!TimeLoopCoreController.ParadoxExists() && !TimeLoop.IsTimeLoopEnabled() && PlayerState.IsResurrected() && _dreamArrivalPoint.GetLocation() != DreamArrivalPoint.Location.Zone1 && _dreamArrivalPoint.GetLocation() != DreamArrivalPoint.Location.Zone2 && Time.time > _lightLanternTime + 10f)
			{
				Locator.GetDeathManager().BeginEscapedTimeLoopSequence(TimeloopEscapeType.Dreamworld);
			}
		}
	}

	private void UpdateSimulationCamera()
	{
		if (_primarySimulationRoot != null)
		{
			Vector3 position = _primarySimulationRoot.position;
			Shader.SetGlobalVector(_propID_DreamSimulationCenter, new Vector4(position.x, position.y, position.z, 0f));
		}
		_simulationSphere.transform.position = _playerLantern.transform.position;
		_simulationCamera.UpdateCamera();
	}

	private void ExitLanternBounds()
	{
		_outsideLanternBounds = true;
		_simulationCamera.enabled = true;
		UpdateSimulationCamera();
		Locator.GetAudioMixer().MixSimulation();
		OnExitLanternBounds.Invoke();
	}

	private void EnterLanternBounds()
	{
		_outsideLanternBounds = false;
		_simulationCamera.enabled = false;
		Locator.GetAudioMixer().UnmixSimulation();
		OnEnterLanternBounds.Invoke();
	}

	private void FixedUpdate()
	{
		if (_enteringDream)
		{
			_enteringDream = false;
			_insideDream = true;
			_waitingToLightLantern = true;
			_lightLanternTime = Time.time + 1f;
			Locator.GetFlashlight().TurnOff(playAudio: false);
			_suitUpOnWake = false;
			if (Locator.GetPlayerSuit().IsWearingSuit())
			{
				_suitUpOnWake = true;
				Locator.GetPlayerSuit().RemoveSuit(instantRemoveSuit: true);
			}
			OWInput.ChangeInputMode(InputMode.Character);
			_playerCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Sun"));
			_prevPlayerCameraFarPlaneDist = _playerCamera.farClipPlane;
			_playerCamera.farClipPlane = 4000f;
			_playerCamera.mainCamera.backgroundColor = _tempSkyboxColor;
			_playerCamera.planetaryFog.enabled = false;
			_playerCamera.postProcessingSettings.ambientOcclusionAvailable = false;
			_playerCamera.postProcessingSettings.screenSpaceReflectionAvailable = true;
			if (_playerCamAmbientLightRenderer != null)
			{
				_playerCamAmbientLightRenderer.enabled = true;
			}
			_simulationCamera.OnEnterDreamWorld();
			if (PlayerState.IsResurrected())
			{
				Vector3 vector = new Vector3(_relativeSleepLocation.localPosition.x, 0f, _relativeSleepLocation.localPosition.z);
				if (vector.magnitude - 1.7f < 0f)
				{
					_relativeSleepLocation.localPosition = Vector3.up * _relativeSleepLocation.localPosition.y + vector.normalized * 2.2f;
				}
			}
			if (Locator.GetProbe().IsLaunched())
			{
				Locator.GetProbe().ExternalRetrieve(silent: true);
			}
			Locator.GetPlayerBody().MoveToRelativeLocation(_relativeSleepLocation, _dreamBody, _dreamArrivalPoint.transform);
			GlobalMessenger.FireEvent("WarpPlayer");
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			_dreamWorldVolume.AddObjectToVolume(Locator.GetPlayerDetector());
			PlayerSectorDetector playerSectorDetector = Locator.GetPlayerSectorDetector();
			playerSectorDetector.RemoveFromAllSectors();
			Sector sector = _dreamArrivalPoint.GetSector();
			while (sector != null)
			{
				sector.GetTriggerVolume().AddObjectToVolume(playerSectorDetector.gameObject);
				sector = sector.GetParentSector();
			}
			SunLightController.RegisterSunOverrider(this, 1000);
			if (_proxyShadowLight != null)
			{
				_proxyShadowLight.enabled = false;
			}
			_dreamCampfire.OnEnterDreamWorld();
			_dreamArrivalPoint.OnEnterDreamWorld(_playerLantern);
			_playerLantern.OnEnterDreamWorld();
			GlobalMessenger.FireEvent("EnterDreamWorld");
		}
		else if (_exitingDream && _closingEyes && Time.time > _exitDreamTime)
		{
			Locator.GetPlayerCameraDetector().GetComponent<AudioDetector>().DeactivateAllVolumes(0f);
			if (_activeGhostGrabController != null)
			{
				_activeGhostGrabController.ReleasePlayer();
			}
			if (_activeZoomPoint != null)
			{
				_activeZoomPoint.CancelZoom();
			}
			if (_outsideLanternBounds)
			{
				EnterLanternBounds();
			}
			_simulationCamera.OnExitDreamWorld();
			SunLightController.UnregisterSunOverrider(this);
			if (_proxyShadowLight != null)
			{
				_proxyShadowLight.enabled = true;
			}
			_exitingDream = false;
			_closingEyes = false;
			_insideDream = false;
			_waitingToLightLantern = false;
			if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() != ItemType.DreamLantern)
			{
				Locator.GetToolModeSwapper().GetItemCarryTool().DropItemInstantly(_dreamWorldSector, base.transform);
				if (_playerLanternSocket != null)
				{
					Locator.GetToolModeSwapper().GetItemCarryTool().UnsocketItemInstantly(_playerLanternSocket);
				}
				else
				{
					Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(_playerLantern);
				}
			}
			_playerLantern.OnExitDreamWorld();
			if (_dreamCampfire != null)
			{
				Locator.GetPlayerBody().MoveToRelativeLocation(_relativeSleepLocation, _planetBody, _dreamCampfire.transform);
				Locator.GetPlayerBody().SetAngularVelocity(_planetBody.GetAngularVelocity());
				GlobalMessenger.FireEvent("WarpPlayer");
				if (!Physics.autoSyncTransforms)
				{
					Physics.SyncTransforms();
				}
				Locator.GetPlayerCameraController().SetDegreesY(_cachedCamDegreesY);
				PlayerSectorDetector playerSectorDetector2 = Locator.GetPlayerSectorDetector();
				playerSectorDetector2.RemoveFromAllSectors();
				Sector sector2 = _dreamCampfire.GetSector();
				while (sector2 != null)
				{
					sector2.GetTriggerVolume().AddObjectToVolume(playerSectorDetector2.gameObject);
					sector2 = sector2.GetParentSector();
				}
				if (_ringWorldController != null)
				{
					_ringWorldController.OnExitDreamWorld();
				}
				_dreamArrivalPoint.OnExitDreamWorld();
				_dreamCampfire.OnExitDreamWorld();
				_dreamCampfire.OnDreamCampfireExtinguished -= new OWEvent.OWCallback(OnDreamCampfireExtinguished);
				_dreamCampfire = null;
				Locator.GetPlayerDetector().GetComponent<ForceApplier>().SkipNextFrame();
				Locator.GetPlayerBody().GetComponent<AlignPlayerWithForce>().SkipNextFrame();
			}
			else
			{
				Debug.LogError("No DreamCampfire to move player to!");
				Debug.Break();
			}
			ExtinguishDreamRaft();
			Locator.GetAudioMixer().UnmixDreamWorld();
			Locator.GetAudioMixer().UnmixSleepAtCampfire(1f);
			if (_suitUpOnWake)
			{
				Locator.GetPlayerSuit().SuitUp(isTrainingSuit: false, instantSuitUp: true, putOnHelmet: false);
				Locator.GetPlayerSuit().PutOnHelmetAfterDelay(2f);
			}
			_playerCamEffectController.OpenEyes(0.5f);
			ReticleController.Show();
			Locator.GetPromptManager().SetPromptsVisible(visible: true);
			if (_playerCamAmbientLightRenderer != null)
			{
				_playerCamAmbientLightRenderer.enabled = false;
			}
			_playerCamera.cullingMask |= 1 << LayerMask.NameToLayer("Sun");
			_playerCamera.farClipPlane = _prevPlayerCameraFarPlaneDist;
			_prevPlayerCameraFarPlaneDist = 0f;
			_playerCamera.mainCamera.backgroundColor = Color.black;
			_playerCamera.planetaryFog.enabled = true;
			_playerCamera.postProcessingSettings.screenSpaceReflectionAvailable = false;
			_playerCamera.postProcessingSettings.ambientOcclusionAvailable = true;
			if (_wakeType != DreamWakeType.CampfireExtinguished && _wakeType != DreamWakeType.Asphyxiation)
			{
				Locator.GetPlayerAudioController().OnExitDreamWorld(AudioType.PlayerGasp_Medium);
			}
			GlobalMessenger.FireEvent("ExitDreamWorld");
		}
		else if (_insideDream && !_exitingDream && _playerLantern != null)
		{
			float num = Vector3.Distance(_playerLantern.transform.position, _playerCamera.transform.position);
			float num2 = 20f + _simulationRadiusBuffer;
			if (!_outsideLanternBounds && num > num2 && !PlayerState.IsCameraUnderwater())
			{
				ExitLanternBounds();
			}
			else if (_outsideLanternBounds && num <= num2)
			{
				EnterLanternBounds();
			}
		}
	}

	private void OnDreamCampfireExtinguished()
	{
		_dreamCampfire.OnDreamCampfireExtinguished -= new OWEvent.OWCallback(OnDreamCampfireExtinguished);
		_dreamCampfireExtinguishedTime = Time.time;
	}

	private void CheckSleepWakeDieAchievement(DreamWakeType wakeType)
	{
		switch (wakeType)
		{
		case DreamWakeType.Alarm:
			_achievementTracker[0] = true;
			break;
		case DreamWakeType.LanternSubmerged:
		case DreamWakeType.Asphyxiation:
			_achievementTracker[1] = true;
			break;
		case DreamWakeType.CrushedByElevator:
			_achievementTracker[2] = true;
			break;
		case DreamWakeType.NeckSnapped:
			_achievementTracker[3] = true;
			break;
		case DreamWakeType.LanternBlownOut:
			_achievementTracker[4] = true;
			break;
		case DreamWakeType.BurnedByFire:
			_achievementTracker[5] = true;
			break;
		case DreamWakeType.Impact:
			_achievementTracker[6] = true;
			break;
		}
		int num = 0;
		for (int i = 0; i < _achievementTracker.Length; i++)
		{
			if (_achievementTracker[i])
			{
				num++;
			}
		}
		if (num >= 5)
		{
			Achievements.Earn(Achievements.Type.SLEEP_WAKE_REPEAT);
		}
		if (wakeType == DreamWakeType.NeckSnapped)
		{
			Achievements.Earn(Achievements.Type.OOFMYBONES);
		}
	}

	private void CheckDreamZone2Completion()
	{
		if (!PlayerData.GetPersistentCondition("HAS_COMPLETED_DREAM_ZONE_2") && Locator.GetShipLogManager().IsFactRevealed("IP_DREAM_ZONE_2_X4") && Locator.GetShipLogManager().IsFactRevealed("IP_DREAM_LIBRARY_2_X1"))
		{
			PlayerData.SetPersistentCondition("HAS_COMPLETED_DREAM_ZONE_2", state: true);
		}
	}
}
