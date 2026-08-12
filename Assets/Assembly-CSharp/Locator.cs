using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Locator : MonoBehaviour
{
	private static LoadManager _loadManager;

	private static PlayerCharacterController _playerController;

	private static PlayerAudioController _playerAudioController;

	private static Collider _playerCollider;

	private static PlayerSpacesuit _playerSuit;

	private static ToolModeSwapper _toolModeSwapper;

	private static PromptManager _promptManager;

	private static CenterOfTheUniverse _centerOfTheUniverse;

	private static Transform _rootTransform;

	private static Transform _sunTransform;

	private static SunController _sunController;

	private static Transform _skyboxTransform;

	private static Transform _playerTransform;

	private static Transform _shipTransform;

	private static GameObject _playerDetector;

	private static GameObject _playerCameraDetector;

	private static OWRigidbody _playerBody;

	private static OWRigidbody _shipBody;

	private static GameObject _shipDetector;

	private static SurveyorProbe _probe;

	private static Flashlight _flashlight;

	private static ReferenceFrameTracker _rfTracker;

	private static ThrusterLightTracker _thrusterLightTracker;

	private static PlayerSectorDetector _playerSectorDetector;

	private static RulesetDetector _playerRulesetDetector;

	private static AlignmentForceDetector _playerForceDetector;

	private static OWAudioMixer _audioMixer;

	private static AudioManager _audioManager;

	private static DeathManager _deathManager;

	private static SectorManager _sectorManager;

	private static CanvasMarkerManager _markerManager;

	private static SurfaceManager _surfaceManager;

	private static EventSystem _eventSystem;

	private static OWMenuInputModule _menuInputModule;

	private static PauseCommandListener _pauseCommandListener;

	private static SceneMenuManager _sceneMenuManager;

	private static GlobalMusicController _globalMusicController;

	private static TravelerAudioManager _travelerAudioManager;

	private static MenuAudioController _menuAudioController;

	private static PlayerDeathAudio _playerDeathAudio;

	private static FogLightManager _fogLightManager;

	private static ShipLogManager _shipLogDatabase;

	private static EyeStateManager _eyeStateManager;

	private static TimelineObliterationController _timelineObliterationController;

	private static UIStyleManager _uiStyleManager;

	private static OWCamera _activeCamera;

	private static OWCamera _playerCamera;

	private static PlayerCameraController _playerCameraController;

	private static MapController _mapController;

	private static UISizeManager _uiSizeManager;

	private static CloakFieldController _cloakFieldController;

	private static RingWorldController _ringWorldController;

	private static DreamWorldController _dreamWorldController;

	private static RingRiverFluidVolume _ringRiverFluidVolume;

	private static DreamWorldAudioController _dreamWorldAudioController;

	private static AlarmSequenceController _alarmSequenceController;

	private static LightSensor _playerLightSensor;

	private static SlideReelMusicManager _slideReelMusicManager;

	private static List<RaftController> _occupiedRafts = new List<RaftController>(2);

	private static AstroObject _sun;

	private static AstroObject _hourglassTwinA;

	private static AstroObject _hourglassTwinB;

	private static AstroObject _timberHearth;

	private static AstroObject _brittleHollow;

	private static AstroObject _giantsDeep;

	private static AstroObject _darkBramble;

	private static AstroObject _comet;

	private static AstroObject _whiteHole;

	private static AstroObject _whiteHoleTarget;

	private static AstroObject _quantumMoonAstroObj;

	private static QuantumMoon _quantumMoon;

	private static AstroObject _orbitalProbeCannon;

	private static AstroObject _ringWorld;

	private static AstroObject _dreamWorld;

	private static List<AstroObject> _minorAstroObjects = new List<AstroObject>();

	private static List<AudioSignal> _audioSignals = new List<AudioSignal>();

	private static List<GravityCannonController> _gravityCannons = new List<GravityCannonController>();

	private static List<NomaiShuttleController> _nomaiShuttles = new List<NomaiShuttleController>();

	private static List<OuterFogWarpVolume> _outerFogWarps = new List<OuterFogWarpVolume>();

	private static List<NomaiWarpReceiver> _warpReceivers = new List<NomaiWarpReceiver>();

	private static Dictionary<string, ShipLogEntryLocation> _entryLocationsByID = new Dictionary<string, ShipLogEntryLocation>();

	private static Dictionary<DreamArrivalPoint.Location, KeyValuePair<DreamCampfire, DreamArrivalPoint>> _dreamCampfirePairs = new Dictionary<DreamArrivalPoint.Location, KeyValuePair<DreamCampfire, DreamArrivalPoint>>(8);

	private void Awake()
	{
		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
		if (!SceneManager.GetActiveScene().name.Contains("SolarSystem"))
		{
			LocateSceneObjects();
		}
	}

	private void Start()
	{
		if (SceneManager.GetActiveScene().name.Contains("SolarSystem"))
		{
			LocateSceneObjects();
		}
	}

	private void OnDestroy()
	{
		ClearReferences();
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
	}

	private void LocateSceneObjects()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		GameObject gameObject2 = GameObject.FindWithTag("Global");
		GameObject gameObject3 = GameObject.FindWithTag("Ship");
		GameObject gameObject4 = GameObject.FindWithTag("Probe");
		GameObject gameObject5 = GameObject.FindWithTag("Sun");
		GameObject gameObject6 = GameObject.FindWithTag("Root");
		GameObject gameObject7 = GameObject.FindWithTag("Skybox");
		GameObject gameObject8 = GameObject.FindWithTag("MainCamera");
		GameObject gameObject9 = GameObject.FindWithTag("ScreenPromptUI");
		GameObject gameObject10 = GameObject.FindWithTag("MarkerManager");
		GameObject gameObject11 = GameObject.FindWithTag("MapCamera");
		if (gameObject != null)
		{
			_playerBody = gameObject.GetComponent<OWRigidbody>();
			_playerController = gameObject.GetComponent<PlayerCharacterController>();
			_playerAudioController = gameObject.GetComponentInChildren<PlayerAudioController>();
			_playerSuit = gameObject.GetComponent<PlayerSpacesuit>();
			_toolModeSwapper = gameObject.GetComponent<ToolModeSwapper>();
			_deathManager = gameObject.GetComponent<DeathManager>();
			_playerTransform = gameObject.GetComponent<Transform>();
			_rfTracker = gameObject.GetComponent<ReferenceFrameTracker>();
			_thrusterLightTracker = gameObject.GetComponentInChildren<ThrusterLightTracker>();
			_playerCollider = gameObject.GetComponent<Collider>();
			_playerDetector = GameObject.FindWithTag("PlayerDetector");
			_playerSectorDetector = _playerDetector.GetComponent<PlayerSectorDetector>();
			_playerRulesetDetector = _playerDetector.GetComponent<RulesetDetector>();
			_playerForceDetector = _playerDetector.GetComponent<AlignmentForceDetector>();
			_playerCameraDetector = GameObject.FindWithTag("PlayerCameraDetector");
			_playerLightSensor = _playerCameraDetector.GetComponent<LightSensor>();
		}
		if (gameObject8 != null)
		{
			_playerCamera = gameObject8.GetComponent<OWCamera>();
			_playerCameraController = gameObject8.GetComponent<PlayerCameraController>();
			_flashlight = gameObject8.GetComponentInChildren<Flashlight>();
		}
		if (gameObject11 != null)
		{
			_mapController = gameObject11.GetComponent<MapController>();
		}
		if (gameObject2 != null)
		{
			_loadManager = gameObject2.GetComponent<LoadManager>();
			_audioMixer = gameObject2.GetComponent<OWAudioMixer>();
			_audioManager = gameObject2.GetComponent<AudioManager>();
			_surfaceManager = gameObject2.GetComponent<SurfaceManager>();
			_eventSystem = gameObject2.GetComponent<EventSystem>();
			_menuInputModule = gameObject2.GetComponent<OWMenuInputModule>();
			_shipLogDatabase = gameObject2.GetComponent<ShipLogManager>();
			_uiStyleManager = gameObject2.GetComponent<UIStyleManager>();
			_pauseCommandListener = gameObject2.GetComponent<PauseCommandListener>();
			_sceneMenuManager = gameObject2.GetComponent<SceneMenuManager>();
			_globalMusicController = gameObject2.GetComponentInChildren<GlobalMusicController>();
			_travelerAudioManager = gameObject2.GetComponent<TravelerAudioManager>();
			_menuAudioController = gameObject2.GetComponentInChildren<MenuAudioController>();
			_playerDeathAudio = gameObject2.GetComponentInChildren<PlayerDeathAudio>();
			_fogLightManager = gameObject2.GetComponent<FogLightManager>();
			_timelineObliterationController = gameObject2.GetComponent<TimelineObliterationController>();
			_slideReelMusicManager = gameObject2.GetComponentInChildren<SlideReelMusicManager>();
			_uiSizeManager = gameObject2.GetComponentInChildren<UISizeManager>();
		}
		if (gameObject5 != null)
		{
			_sunTransform = gameObject5.GetComponent<Transform>();
			_sunController = gameObject5.GetComponent<SunController>();
		}
		if (gameObject3 != null)
		{
			_shipTransform = gameObject3.GetComponent<Transform>();
			_shipBody = gameObject3.GetComponent<OWRigidbody>();
			_shipDetector = GameObject.FindWithTag("ShipDetector");
		}
		if (gameObject4 != null)
		{
			_probe = gameObject4.GetComponent<SurveyorProbe>();
		}
		if (gameObject6 != null)
		{
			_sectorManager = gameObject6.GetComponent<SectorManager>();
			_rootTransform = gameObject6.GetComponent<Transform>();
			_centerOfTheUniverse = gameObject6.GetComponent<CenterOfTheUniverse>();
			_eyeStateManager = gameObject6.GetComponent<EyeStateManager>();
		}
		if (gameObject7 != null)
		{
			_skyboxTransform = gameObject7.GetComponent<Transform>();
		}
		if (gameObject9 != null)
		{
			_promptManager = gameObject9.GetComponent<PromptManager>();
		}
		if (gameObject10 != null)
		{
			_markerManager = gameObject10.GetComponent<CanvasMarkerManager>();
		}
		_activeCamera = base.gameObject.GetTaggedComponent<OWCamera>("MainCamera");
	}

	public static void ClearReferences()
	{
		Debug.Log("LOCATOR: References Cleared");
		_loadManager = null;
		_playerController = null;
		_playerAudioController = null;
		_playerCollider = null;
		_playerSuit = null;
		_toolModeSwapper = null;
		_promptManager = null;
		_centerOfTheUniverse = null;
		_rootTransform = null;
		_sunTransform = null;
		_sunController = null;
		_skyboxTransform = null;
		_playerTransform = null;
		_shipTransform = null;
		_playerDetector = null;
		_playerCameraDetector = null;
		_playerBody = null;
		_shipBody = null;
		_shipDetector = null;
		_probe = null;
		_flashlight = null;
		_rfTracker = null;
		_thrusterLightTracker = null;
		_playerSectorDetector = null;
		_playerRulesetDetector = null;
		_playerForceDetector = null;
		_audioMixer = null;
		_audioManager = null;
		_deathManager = null;
		_sectorManager = null;
		_markerManager = null;
		_surfaceManager = null;
		_eventSystem = null;
		_menuInputModule = null;
		_pauseCommandListener = null;
		_sceneMenuManager = null;
		_globalMusicController = null;
		_travelerAudioManager = null;
		_menuAudioController = null;
		_playerDeathAudio = null;
		_fogLightManager = null;
		_shipLogDatabase = null;
		_eyeStateManager = null;
		_timelineObliterationController = null;
		_uiStyleManager = null;
		_activeCamera = null;
		_playerCamera = null;
		_playerCameraController = null;
		_mapController = null;
		_uiSizeManager = null;
		_cloakFieldController = null;
		_ringWorldController = null;
		_dreamWorldController = null;
		_dreamWorldAudioController = null;
		_alarmSequenceController = null;
		_ringRiverFluidVolume = null;
		_ringRiverFluidVolume = null;
		_playerLightSensor = null;
		_slideReelMusicManager = null;
		_occupiedRafts.Clear();
		_sun = null;
		_hourglassTwinA = null;
		_hourglassTwinB = null;
		_timberHearth = null;
		_brittleHollow = null;
		_giantsDeep = null;
		_darkBramble = null;
		_comet = null;
		_whiteHole = null;
		_whiteHoleTarget = null;
		_quantumMoonAstroObj = null;
		_quantumMoon = null;
		_orbitalProbeCannon = null;
		_ringWorld = null;
		_dreamWorld = null;
		_minorAstroObjects.Clear();
		_audioSignals.Clear();
		_gravityCannons.Clear();
		_nomaiShuttles.Clear();
		_outerFogWarps.Clear();
		_warpReceivers.Clear();
		_entryLocationsByID.Clear();
		_dreamCampfirePairs.Clear();
	}

	public static void RegisterAstroObject(AstroObject astroObject)
	{
		switch (astroObject.GetAstroObjectName())
		{
		case AstroObject.Name.Sun:
			_sun = astroObject;
			break;
		case AstroObject.Name.CaveTwin:
			_hourglassTwinA = astroObject;
			break;
		case AstroObject.Name.TowerTwin:
			_hourglassTwinB = astroObject;
			break;
		case AstroObject.Name.TimberHearth:
			_timberHearth = astroObject;
			break;
		case AstroObject.Name.BrittleHollow:
			_brittleHollow = astroObject;
			break;
		case AstroObject.Name.GiantsDeep:
			_giantsDeep = astroObject;
			break;
		case AstroObject.Name.DarkBramble:
			_darkBramble = astroObject;
			break;
		case AstroObject.Name.Comet:
			_comet = astroObject;
			break;
		case AstroObject.Name.WhiteHole:
			_whiteHole = astroObject;
			break;
		case AstroObject.Name.WhiteHoleTarget:
			_whiteHoleTarget = astroObject;
			break;
		case AstroObject.Name.QuantumMoon:
			_quantumMoonAstroObj = astroObject;
			_quantumMoon = astroObject.GetComponent<QuantumMoon>();
			break;
		case AstroObject.Name.ProbeCannon:
			_orbitalProbeCannon = astroObject;
			break;
		case AstroObject.Name.RingWorld:
			_ringWorld = astroObject;
			break;
		case AstroObject.Name.DreamWorld:
			_dreamWorld = astroObject;
			break;
		case AstroObject.Name.CustomString:
			if (_minorAstroObjects == null)
			{
				_minorAstroObjects = new List<AstroObject>();
			}
			_minorAstroObjects.Add(astroObject);
			break;
		case AstroObject.Name.TimberMoon:
		case AstroObject.Name.VolcanicMoon:
		case AstroObject.Name.Eye:
		case AstroObject.Name.HourglassTwins:
		case AstroObject.Name.SunStation:
			break;
		}
	}

	public static AstroObject GetAstroObject(AstroObject.Name astroObjectName)
	{
		switch (astroObjectName)
		{
		case AstroObject.Name.Sun:
			return _sun;
		case AstroObject.Name.CaveTwin:
			return _hourglassTwinA;
		case AstroObject.Name.TowerTwin:
			return _hourglassTwinB;
		case AstroObject.Name.TimberHearth:
			return _timberHearth;
		case AstroObject.Name.BrittleHollow:
			return _brittleHollow;
		case AstroObject.Name.GiantsDeep:
			return _giantsDeep;
		case AstroObject.Name.DarkBramble:
			return _darkBramble;
		case AstroObject.Name.Comet:
			return _comet;
		case AstroObject.Name.WhiteHole:
			return _whiteHole;
		case AstroObject.Name.WhiteHoleTarget:
			return _whiteHoleTarget;
		case AstroObject.Name.QuantumMoon:
			return _quantumMoonAstroObj;
		case AstroObject.Name.ProbeCannon:
			return _orbitalProbeCannon;
		case AstroObject.Name.RingWorld:
			return _ringWorld;
		case AstroObject.Name.DreamWorld:
			return _dreamWorld;
		default:
			return null;
		}
	}

	public static AstroObject GetMinorAstroObject(string astroObjectCustomName)
	{
		for (int i = 0; i < _minorAstroObjects.Count; i++)
		{
			if (_minorAstroObjects[i].GetCustomName() == astroObjectCustomName)
			{
				return _minorAstroObjects[i];
			}
		}
		return null;
	}

	public static QuantumMoon GetQuantumMoon()
	{
		return _quantumMoon;
	}

	public static void RegisterOuterFogWarp(OuterFogWarpVolume outerWarp)
	{
		_outerFogWarps.SafeAdd(outerWarp);
	}

	public static void UnregisterOuterFogWarp(OuterFogWarpVolume outerWarp)
	{
		_outerFogWarps.Remove(outerWarp);
	}

	public static OuterFogWarpVolume GetOuterFogWarp(OuterFogWarpVolume.Name name)
	{
		if (name == OuterFogWarpVolume.Name.None)
		{
			return null;
		}
		for (int i = 0; i < _outerFogWarps.Count; i++)
		{
			if (_outerFogWarps[i].GetName() == name)
			{
				return _outerFogWarps[i];
			}
		}
		return null;
	}

	public static void RegisterGravityCannon(GravityCannonController cannon)
	{
		_gravityCannons.SafeAdd(cannon);
	}

	public static void UnregisterGravityCannon(GravityCannonController cannon)
	{
		_gravityCannons.Remove(cannon);
	}

	public static GravityCannonController GetGravityCannon(NomaiShuttleController.ShuttleID id)
	{
		for (int i = 0; i < _gravityCannons.Count; i++)
		{
			if (_gravityCannons[i].GetID() == id)
			{
				return _gravityCannons[i];
			}
		}
		return null;
	}

	public static void RegisterNomaiShuttle(NomaiShuttleController shuttle)
	{
		_nomaiShuttles.SafeAdd(shuttle);
	}

	public static void UnregisterNomaiShuttle(NomaiShuttleController shuttle)
	{
		_nomaiShuttles.Remove(shuttle);
	}

	public static NomaiShuttleController GetNomaiShuttle(NomaiShuttleController.ShuttleID id)
	{
		for (int i = 0; i < _nomaiShuttles.Count; i++)
		{
			if (_nomaiShuttles[i].GetID() == id)
			{
				return _nomaiShuttles[i];
			}
		}
		return null;
	}

	public static void RegisterAudioSignal(AudioSignal signal)
	{
		_audioSignals.SafeAdd(signal);
	}

	public static void UnregisterAudioSignal(AudioSignal signal)
	{
		_audioSignals.Remove(signal);
	}

	public static List<AudioSignal> GetAudioSignals()
	{
		return _audioSignals;
	}

	public static void RegisterWarpReceiver(NomaiWarpReceiver receiver)
	{
		_warpReceivers.SafeAdd(receiver);
	}

	public static void UnregisterWarpReceiver(NomaiWarpReceiver receiver)
	{
		_warpReceivers.QuickRemove(receiver);
	}

	public static void RegisterEntryLocation(ShipLogEntryLocation location)
	{
		if (!_entryLocationsByID.ContainsKey(location.GetEntryID()))
		{
			_entryLocationsByID.Add(location.GetEntryID(), location);
		}
		else
		{
			Debug.LogError("Already registered ShipLogEntryLocation with ID " + location.GetEntryID());
		}
	}

	public static void UnregisterEntryLocation(ShipLogEntryLocation location)
	{
		_entryLocationsByID.Remove(location.GetEntryID());
	}

	public static ShipLogEntryLocation GetEntryLocation(string entryID)
	{
		if (!_entryLocationsByID.ContainsKey(entryID))
		{
			return null;
		}
		return _entryLocationsByID[entryID];
	}

	public static NomaiWarpReceiver GetWarpReceiver(NomaiWarpPlatform.Frequency frequency)
	{
		for (int i = 0; i < _warpReceivers.Count; i++)
		{
			if (_warpReceivers[i].GetFrequency() == frequency)
			{
				return _warpReceivers[i];
			}
		}
		return null;
	}

	public static void RegisterDreamCampfire(DreamCampfire campfire, DreamArrivalPoint.Location location)
	{
		if (location == DreamArrivalPoint.Location.Undefined)
		{
			Debug.LogWarning("Tried to register DreamCampfire with Location Undefined; skipping", campfire);
			return;
		}
		KeyValuePair<DreamCampfire, DreamArrivalPoint> value = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(null, null);
		_dreamCampfirePairs.TryGetValue(location, out value);
		if (value.Key != null)
		{
			Debug.LogWarning(string.Concat("Tried to register DreamCampfire with duplicate Location ", location, "; skipping"), campfire);
		}
		else
		{
			_dreamCampfirePairs[location] = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(campfire, value.Value);
		}
	}

	public static void UnregisterDreamCampfire(DreamCampfire campfire, DreamArrivalPoint.Location location)
	{
		if (_dreamCampfirePairs.ContainsKey(location))
		{
			KeyValuePair<DreamCampfire, DreamArrivalPoint> value = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(null, null);
			_dreamCampfirePairs.TryGetValue(location, out value);
			if (value.Key == campfire)
			{
				_dreamCampfirePairs[location] = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(null, value.Value);
			}
		}
	}

	public static void RegisterDreamArrivalPoint(DreamArrivalPoint arrivalPoint, DreamArrivalPoint.Location location)
	{
		if (location == DreamArrivalPoint.Location.Undefined)
		{
			Debug.LogWarning("Tried to register DreamArrivalPoint with Location Undefined; skipping", arrivalPoint);
			return;
		}
		KeyValuePair<DreamCampfire, DreamArrivalPoint> value = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(null, null);
		_dreamCampfirePairs.TryGetValue(location, out value);
		if (value.Value != null)
		{
			Debug.LogWarning(string.Concat("Tried to register DreamArrivalPoint with duplicate Location ", location, "; skipping"), arrivalPoint);
		}
		else
		{
			_dreamCampfirePairs[location] = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(value.Key, arrivalPoint);
		}
	}

	public static void UnregisterDreamArrivalPoint(DreamArrivalPoint arrivalPoint, DreamArrivalPoint.Location location)
	{
		if (_dreamCampfirePairs.ContainsKey(location))
		{
			KeyValuePair<DreamCampfire, DreamArrivalPoint> value = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(null, null);
			_dreamCampfirePairs.TryGetValue(location, out value);
			if (value.Value == arrivalPoint)
			{
				_dreamCampfirePairs[location] = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(value.Key, null);
			}
		}
	}

	public static DreamCampfire GetDreamCampfire(DreamArrivalPoint.Location location)
	{
		KeyValuePair<DreamCampfire, DreamArrivalPoint> value = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(null, null);
		_dreamCampfirePairs.TryGetValue(location, out value);
		return value.Key;
	}

	public static DreamArrivalPoint GetDreamArrivalPoint(DreamArrivalPoint.Location location)
	{
		KeyValuePair<DreamCampfire, DreamArrivalPoint> value = new KeyValuePair<DreamCampfire, DreamArrivalPoint>(null, null);
		_dreamCampfirePairs.TryGetValue(location, out value);
		return value.Value;
	}

	public static ToolModeSwapper GetToolModeSwapper()
	{
		return _toolModeSwapper;
	}

	public static Collider GetPlayerCollider()
	{
		return _playerCollider;
	}

	public static PlayerCharacterController GetPlayerController()
	{
		return _playerController;
	}

	public static PlayerAudioController GetPlayerAudioController()
	{
		return _playerAudioController;
	}

	public static PlayerSpacesuit GetPlayerSuit()
	{
		return _playerSuit;
	}

	public static LoadManager GetLoadManager()
	{
		return _loadManager;
	}

	public static SectorManager GetSectorManager()
	{
		return _sectorManager;
	}

	public static DeathManager GetDeathManager()
	{
		return _deathManager;
	}

	public static OWAudioMixer GetAudioMixer()
	{
		return _audioMixer;
	}

	public static AudioManager GetAudioManager()
	{
		return _audioManager;
	}

	public static OWCamera GetActiveCamera()
	{
		return _activeCamera;
	}

	public static OWCamera GetPlayerCamera()
	{
		return _playerCamera;
	}

	public static PlayerCameraController GetPlayerCameraController()
	{
		return _playerCameraController;
	}

	public static MapController GetMapController()
	{
		return _mapController;
	}

	public static Flashlight GetFlashlight()
	{
		return _flashlight;
	}

	public static PromptManager GetPromptManager()
	{
		return _promptManager;
	}

	public static CanvasMarkerManager GetMarkerManager()
	{
		return _markerManager;
	}

	public static SurfaceManager GetSurfaceManager()
	{
		return _surfaceManager;
	}

	public static CenterOfTheUniverse GetCenterOfTheUniverse()
	{
		return _centerOfTheUniverse;
	}

	public static Transform GetRootTransform()
	{
		return _rootTransform;
	}

	public static Transform GetSunTransform()
	{
		return _sunTransform;
	}

	public static SunController GetSunController()
	{
		return _sunController;
	}

	public static Transform GetSkyboxTransform()
	{
		return _skyboxTransform;
	}

	public static Transform GetPlayerTransform()
	{
		return _playerTransform;
	}

	public static OWRigidbody GetPlayerBody()
	{
		return _playerBody;
	}

	public static GameObject GetPlayerDetector()
	{
		return _playerDetector;
	}

	public static GameObject GetPlayerCameraDetector()
	{
		return _playerCameraDetector;
	}

	public static Transform GetShipTransform()
	{
		return _shipTransform;
	}

	public static OWRigidbody GetShipBody()
	{
		return _shipBody;
	}

	public static GameObject GetShipDetector()
	{
		return _shipDetector;
	}

	public static SurveyorProbe GetProbe()
	{
		return _probe;
	}

	public static ReferenceFrame GetReferenceFrame(bool ignorePassiveFrame = true)
	{
		return _rfTracker.GetReferenceFrame(ignorePassiveFrame);
	}

	public static ThrusterLightTracker GetThrusterLightTracker()
	{
		return _thrusterLightTracker;
	}

	public static PlayerSectorDetector GetPlayerSectorDetector()
	{
		return _playerSectorDetector;
	}

	public static RulesetDetector GetPlayerRulesetDetector()
	{
		return _playerRulesetDetector;
	}

	public static AlignmentForceDetector GetPlayerForceDetector()
	{
		return _playerForceDetector;
	}

	public static EventSystem GetEventSystem()
	{
		return _eventSystem;
	}

	public static OWMenuInputModule GetMenuInputModule()
	{
		return _menuInputModule;
	}

	public static SceneMenuManager GetSceneMenuManager()
	{
		return _sceneMenuManager;
	}

	public static ShipLogManager GetShipLogManager()
	{
		return _shipLogDatabase;
	}

	public static UIStyleManager GetUIStyleManager()
	{
		return _uiStyleManager;
	}

	public static PauseCommandListener GetPauseCommandListener()
	{
		return _pauseCommandListener;
	}

	public static GlobalMusicController GetGlobalMusicController()
	{
		return _globalMusicController;
	}

	public static TravelerAudioManager GetTravelerAudioManager()
	{
		return _travelerAudioManager;
	}

	public static MenuAudioController GetMenuAudioController()
	{
		return _menuAudioController;
	}

	public static PlayerDeathAudio GetPlayerDeathAudio()
	{
		return _playerDeathAudio;
	}

	public static FogLightManager GetFogLightManager()
	{
		return _fogLightManager;
	}

	public static EyeStateManager GetEyeStateManager()
	{
		return _eyeStateManager;
	}

	public static TimelineObliterationController GetTimelineObliterationController()
	{
		return _timelineObliterationController;
	}

	public static UISizeManager GetUISizeManager()
	{
		return _uiSizeManager;
	}

	public static void RegisterCloakFieldController(CloakFieldController cloakFieldController)
	{
		_cloakFieldController = cloakFieldController;
	}

	public static CloakFieldController GetCloakFieldController()
	{
		return _cloakFieldController;
	}

	public static void RegisterRingWorldController(RingWorldController ringWorldController)
	{
		_ringWorldController = ringWorldController;
	}

	public static RingWorldController GetRingWorldController()
	{
		return _ringWorldController;
	}

	public static void RegisterDreamWorldController(DreamWorldController dreamWorldController)
	{
		_dreamWorldController = dreamWorldController;
	}

	public static DreamWorldController GetDreamWorldController()
	{
		return _dreamWorldController;
	}

	public static void RegisterDreamWorldAudioController(DreamWorldAudioController dreamWorldAudioController)
	{
		_dreamWorldAudioController = dreamWorldAudioController;
	}

	public static DreamWorldAudioController GetDreamWorldAudioController()
	{
		return _dreamWorldAudioController;
	}

	public static void RegisterAlarmSequenceController(AlarmSequenceController alarmSequenceController)
	{
		_alarmSequenceController = alarmSequenceController;
	}

	public static AlarmSequenceController GetAlarmSequenceController()
	{
		return _alarmSequenceController;
	}

	public static void RegisterRingRiverFluidVolume(RingRiverFluidVolume ringRiverFluidVolume)
	{
		_ringRiverFluidVolume = ringRiverFluidVolume;
	}

	public static RingRiverFluidVolume GetRingRiverFluidVolume()
	{
		return _ringRiverFluidVolume;
	}

	public static LightSensor GetPlayerLightSensor()
	{
		return _playerLightSensor;
	}

	public static SlideReelMusicManager GetSlideReelMusicManager()
	{
		return _slideReelMusicManager;
	}

	public static void AddOccupiedRaft(RaftController raft)
	{
		_occupiedRafts.SafeAdd(raft);
	}

	public static void RemoveOccupiedRaft(RaftController raft)
	{
		_occupiedRafts.Remove(raft);
	}

	public static RaftController GetOccupiedRaft()
	{
		if (_occupiedRafts.Count <= 0)
		{
			return null;
		}
		return _occupiedRafts[_occupiedRafts.Count - 1];
	}

	private void OnSwitchActiveCamera(OWCamera activeCamera)
	{
		_activeCamera = activeCamera;
	}
}
