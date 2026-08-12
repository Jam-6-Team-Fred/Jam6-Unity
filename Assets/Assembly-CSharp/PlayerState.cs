using UnityEngine;

public class PlayerState : MonoBehaviour
{
	private static bool _hasPlayerEnteredShip;

	private static bool _isWearingSuit;

	private static bool _alignedWithField;

	private static bool _insideShip;

	private static bool _insideShuttle;

	private static bool _atFlightConsole;

	private static bool _isCameraUnderwater;

	private static bool _isDead;

	private static bool _isResurrected;

	private static bool _isAttached;

	private static bool _usingTelescope;

	private static bool _usingShipComputer;

	private static bool _inLandingView;

	private static bool _mapView;

	private static bool _inBrambleDimension;

	private static bool _inGiantsDeep;

	private static bool _isFlashlightOn;

	private static bool _inDarkZone;

	private static bool _inConversation;

	private static bool _inZeroGTraining;

	private static bool _isCameraLockedOn;

	private static bool _isHullBreached;

	private static bool _usingNomaiRemoteCamera;

	private static bool _insideTheEye;

	private static bool _isSleepingAtCampfire;

	private static bool _isSleepingAtDreamCampfire;

	private static bool _isFastForwarding;

	private static bool _inCloakingField;

	private static bool _inDreamWorld;

	private static bool _hasPlayerhadLanternBlownOut;

	private static bool _viewingProjector;

	private static bool _isPeeping;

	private static bool _isGrabbedByGhost;

	private static float _lastDetachTime;

	private static int _undertowVolumeCount;

	private static PlayerFogWarpDetector _playerFogWarpDetector;

	private static QuantumMoon _quantumMoon;

	private void Awake()
	{
		Reset();
		GlobalMessenger.AddListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
		GlobalMessenger.AddListener("EnterDarkZone", OnEnterDarkZone);
		GlobalMessenger.AddListener("ExitDarkZone", OnExitDarkZone);
		GlobalMessenger.AddListener("TurnOnFlashlight", OnFlashlightOn);
		GlobalMessenger.AddListener("TurnOffFlashlight", OnFlashlightOff);
		GlobalMessenger.AddListener("InitPlayerForceAlignment", OnInitPlayerForceAlignment);
		GlobalMessenger.AddListener("BreakPlayerForceAlignment", OnBreakPlayerForceAlignment);
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.AddListener("EnterShipComputer", OnEnterShipComputer);
		GlobalMessenger.AddListener("ExitShipComputer", OnExitShipComputer);
		GlobalMessenger.AddListener("EnterLandingView", OnEnterLandingView);
		GlobalMessenger.AddListener("ExitLandingView", OnExitLandingView);
		GlobalMessenger.AddListener("EnterShip", OnEnterShip);
		GlobalMessenger.AddListener("ExitShip", OnExitShip);
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger.AddListener("EnterShuttle", OnEnterShuttle);
		GlobalMessenger.AddListener("ExitShuttle", OnExitShuttle);
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger<float>.AddListener("PlayerCameraEnterWater", OnCameraEnterWater);
		GlobalMessenger.AddListener("PlayerCameraExitWater", OnCameraExitWater);
		GlobalMessenger<OWRigidbody>.AddListener("AttachPlayerToPoint", OnAttachPlayerToPoint);
		GlobalMessenger.AddListener("DetachPlayerFromPoint", OnDetachPlayerFromPoint);
		GlobalMessenger.AddListener("PlayerEnterBrambleDimension", OnPlayerEnterBrambleDimension);
		GlobalMessenger.AddListener("PlayerExitBrambleDimension", OnPlayerExitBrambleDimension);
		GlobalMessenger.AddListener("PlayerEnterGiantsDeep", OnPlayerEnterGiantsDeep);
		GlobalMessenger.AddListener("PlayerExitGiantsDeep", OnPlayerExitGiantsDeep);
		GlobalMessenger.AddListener("EnterZeroGTraining", OnEnterZeroGTraining);
		GlobalMessenger.AddListener("ExitZeroGTraining", OnExitZeroGTraining);
		GlobalMessenger.AddListener("PlayerCameraLockOn", OnPlayerCameraLockOn);
		GlobalMessenger.AddListener("PlayerCameraBreakLock", OnPlayerCameraBreakLock);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("PlayerResurrection", OnPlayerResurrection);
		GlobalMessenger.AddListener("ShipHullBreach", OnShipHullBreach);
		GlobalMessenger.AddListener("EnterNomaiRemoteCamera", OnEnterNomaiRemoteCamera);
		GlobalMessenger.AddListener("ExitNomaiRemoteCamera", OnExitNomaiRemoteCamera);
		GlobalMessenger<EyeState>.AddListener("EyeStateChanged", OnEyeStateChanged);
		GlobalMessenger<bool>.AddListener("StartSleepingAtCampfire", OnStartSleepingAtCampfire);
		GlobalMessenger.AddListener("StopSleepingAtCampfire", OnStopSleepingAtCampfire);
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
		GlobalMessenger.AddListener("EnterCloak", OnEnterCloak);
		GlobalMessenger.AddListener("ExitCloak", OnExitCloak);
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.AddListener("StartViewingProjector", OnStartViewingProjector);
		GlobalMessenger.AddListener("EndViewingProjector", OnEndViewingProjector);
		GlobalMessenger<Peephole>.AddListener("StartPeeping", OnStartPeeping);
		GlobalMessenger<Peephole>.AddListener("StopPeeping", OnStopPeeping);
		GlobalMessenger.AddListener("PlayerEnterUndertowVolume", OnEnterUndertowVolume);
		GlobalMessenger.AddListener("PlayerExitUndertowVolume", OnExitUndertowVolume);
		GlobalMessenger.AddListener("PlayerGrabbedByGhost", OnGrabbedByGhost);
		GlobalMessenger.AddListener("PlayerReleasedByGhost", OnReleasedByGhost);
	}

	private void Start()
	{
		_playerFogWarpDetector = Locator.GetPlayerDetector().GetComponent<PlayerFogWarpDetector>();
		AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.QuantumMoon);
		if (astroObject != null)
		{
			_quantumMoon = astroObject.GetComponent<QuantumMoon>();
		}
	}

	private void OnDestroy()
	{
		Reset();
		GlobalMessenger.RemoveListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.RemoveListener("ExitConversation", OnExitConversation);
		GlobalMessenger.RemoveListener("EnterDarkZone", OnEnterDarkZone);
		GlobalMessenger.RemoveListener("ExitDarkZone", OnExitDarkZone);
		GlobalMessenger.RemoveListener("TurnOnFlashlight", OnFlashlightOn);
		GlobalMessenger.RemoveListener("TurnOffFlashlight", OnFlashlightOff);
		GlobalMessenger.RemoveListener("InitPlayerForceAlignment", OnInitPlayerForceAlignment);
		GlobalMessenger.RemoveListener("BreakPlayerForceAlignment", OnBreakPlayerForceAlignment);
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.RemoveListener("EnterShipComputer", OnEnterShipComputer);
		GlobalMessenger.RemoveListener("ExitShipComputer", OnExitShipComputer);
		GlobalMessenger.RemoveListener("EnterLandingView", OnEnterLandingView);
		GlobalMessenger.RemoveListener("ExitLandingView", OnExitLandingView);
		GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
		GlobalMessenger.RemoveListener("ExitShip", OnExitShip);
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger.RemoveListener("EnterShuttle", OnEnterShuttle);
		GlobalMessenger.RemoveListener("ExitShuttle", OnExitShuttle);
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger<float>.RemoveListener("PlayerCameraEnterWater", OnCameraEnterWater);
		GlobalMessenger.RemoveListener("PlayerCameraExitWater", OnCameraExitWater);
		GlobalMessenger<OWRigidbody>.RemoveListener("AttachPlayerToPoint", OnAttachPlayerToPoint);
		GlobalMessenger.RemoveListener("DetachPlayerFromPoint", OnDetachPlayerFromPoint);
		GlobalMessenger.RemoveListener("PlayerEnterBrambleDimension", OnPlayerEnterBrambleDimension);
		GlobalMessenger.RemoveListener("PlayerExitBrambleDimension", OnPlayerExitBrambleDimension);
		GlobalMessenger.RemoveListener("PlayerEnterGiantsDeep", OnPlayerEnterGiantsDeep);
		GlobalMessenger.RemoveListener("PlayerExitGiantsDeep", OnPlayerExitGiantsDeep);
		GlobalMessenger.RemoveListener("EnterZeroGTraining", OnEnterZeroGTraining);
		GlobalMessenger.RemoveListener("ExitZeroGTraining", OnExitZeroGTraining);
		GlobalMessenger.RemoveListener("PlayerCameraLockOn", OnPlayerCameraLockOn);
		GlobalMessenger.RemoveListener("PlayerCameraBreakLock", OnPlayerCameraBreakLock);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("PlayerResurrection", OnPlayerResurrection);
		GlobalMessenger.RemoveListener("ShipHullBreach", OnShipHullBreach);
		GlobalMessenger.RemoveListener("EnterNomaiRemoteCamera", OnEnterNomaiRemoteCamera);
		GlobalMessenger.RemoveListener("ExitNomaiRemoteCamera", OnExitNomaiRemoteCamera);
		GlobalMessenger<EyeState>.RemoveListener("EyeStateChanged", OnEyeStateChanged);
		GlobalMessenger<bool>.RemoveListener("StartSleepingAtCampfire", OnStartSleepingAtCampfire);
		GlobalMessenger.RemoveListener("StopSleepingAtCampfire", OnStopSleepingAtCampfire);
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
		GlobalMessenger.RemoveListener("EnterCloak", OnEnterCloak);
		GlobalMessenger.RemoveListener("ExitCloak", OnExitCloak);
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.RemoveListener("StartViewingProjector", OnStartViewingProjector);
		GlobalMessenger.RemoveListener("EndViewingProjector", OnEndViewingProjector);
		GlobalMessenger<Peephole>.RemoveListener("StartPeeping", OnStartPeeping);
		GlobalMessenger<Peephole>.RemoveListener("StopPeeping", OnStopPeeping);
		GlobalMessenger.RemoveListener("PlayerEnterUndertowVolume", OnEnterUndertowVolume);
		GlobalMessenger.RemoveListener("PlayerExitUndertowVolume", OnExitUndertowVolume);
		GlobalMessenger.RemoveListener("PlayerGrabbedByGhost", OnGrabbedByGhost);
		GlobalMessenger.RemoveListener("PlayerReleasedByGhost", OnReleasedByGhost);
	}

	private void Reset()
	{
		_hasPlayerEnteredShip = false;
		_isWearingSuit = false;
		_insideShip = false;
		_insideShuttle = false;
		_atFlightConsole = false;
		_isDead = false;
		_isResurrected = false;
		_isAttached = false;
		_usingTelescope = false;
		_usingShipComputer = false;
		_inLandingView = false;
		_inBrambleDimension = false;
		_inGiantsDeep = false;
		_isFlashlightOn = false;
		_inDarkZone = false;
		_inConversation = false;
		_inZeroGTraining = false;
		_isCameraLockedOn = false;
		_isHullBreached = false;
		_usingNomaiRemoteCamera = false;
		_insideTheEye = false;
		_isSleepingAtCampfire = false;
		_isSleepingAtDreamCampfire = false;
		_isFastForwarding = false;
		_inCloakingField = false;
		_inDreamWorld = false;
		_hasPlayerhadLanternBlownOut = false;
		_viewingProjector = false;
		_isPeeping = false;
		_isGrabbedByGhost = false;
		_lastDetachTime = 0f;
		_undertowVolumeCount = 0;
		_playerFogWarpDetector = null;
		_quantumMoon = null;
	}

	public static bool IsPlayerCameraLockingOn()
	{
		return _isCameraLockedOn;
	}

	public static bool InZeroGTraining()
	{
		return _inZeroGTraining;
	}

	public static bool HasPlayerEnteredShip()
	{
		return _hasPlayerEnteredShip;
	}

	public static bool InConversation()
	{
		return _inConversation;
	}

	public static bool InDarkZone()
	{
		return _inDarkZone;
	}

	public static bool IsFlashlightOn()
	{
		return _isFlashlightOn;
	}

	public static FogWarpVolume GetOuterFogWarpVolume()
	{
		return _playerFogWarpDetector.GetOuterFogWarpVolume();
	}

	public static bool InZeroG()
	{
		return !_alignedWithField;
	}

	public static bool InMapView()
	{
		return _mapView;
	}

	public static bool UsingShipComputer()
	{
		return _usingShipComputer;
	}

	public static bool InLandingView()
	{
		return _inLandingView;
	}

	public static bool UsingTelescope()
	{
		return _usingTelescope;
	}

	public static bool IsAttached()
	{
		return _isAttached;
	}

	public static bool IsDead()
	{
		return _isDead;
	}

	public static bool IsResurrected()
	{
		return _isResurrected;
	}

	public static bool IsWearingSuit()
	{
		return _isWearingSuit;
	}

	public static bool IsInsideShip()
	{
		return _insideShip;
	}

	public static bool IsInsideShuttle()
	{
		return _insideShuttle;
	}

	public static bool AtFlightConsole()
	{
		return _atFlightConsole;
	}

	public static bool IsCameraUnderwater()
	{
		return _isCameraUnderwater;
	}

	public static bool InBrambleDimension()
	{
		return _inBrambleDimension;
	}

	public static bool OnQuantumMoon()
	{
		if (!(_quantumMoon == null))
		{
			return _quantumMoon.IsPlayerInside();
		}
		return false;
	}

	public static bool InGiantsDeep()
	{
		return _inGiantsDeep;
	}

	public static bool IsHullBreached()
	{
		return _isHullBreached;
	}

	public static bool UsingNomaiRemoteCamera()
	{
		return _usingNomaiRemoteCamera;
	}

	public static bool IsInsideTheEye()
	{
		return _insideTheEye;
	}

	public static bool IsSleepingAtCampfire()
	{
		return _isSleepingAtCampfire;
	}

	public static bool IsSleepingAtDreamCampfire()
	{
		return _isSleepingAtDreamCampfire;
	}

	public static bool IsFastForwarding()
	{
		return _isFastForwarding;
	}

	public static bool InCloakingField()
	{
		return _inCloakingField;
	}

	public static bool InDreamWorld()
	{
		return _inDreamWorld;
	}

	public static bool HasPlayerHadLanternBlownOut()
	{
		return _hasPlayerhadLanternBlownOut;
	}

	public static bool IsViewingProjector()
	{
		return _viewingProjector;
	}

	public static bool IsPeeping()
	{
		return _isPeeping;
	}

	public static bool IsRecentlyDetached()
	{
		return Time.time < _lastDetachTime + 0.5f;
	}

	public static bool InUndertowVolume()
	{
		if (_undertowVolumeCount > 0)
		{
			return !IsRidingRaft();
		}
		return false;
	}

	public static bool IsRidingRaft(bool raftMustBeInWater = true)
	{
		if (!(Locator.GetOccupiedRaft() == null))
		{
			return Locator.GetOccupiedRaft().IsPlayerRiding(raftMustBeInWater);
		}
		return false;
	}

	public static bool IsGrabbedByGhost()
	{
		return _isGrabbedByGhost;
	}

	public static bool CheckShipOutsideSolarSystem()
	{
		Transform sunTransform = Locator.GetSunTransform();
		OWRigidbody shipBody = Locator.GetShipBody();
		if (sunTransform != null && shipBody != null)
		{
			return (sunTransform.position - shipBody.transform.position).sqrMagnitude > 900000000f;
		}
		return false;
	}

	private void OnPlayerCameraLockOn()
	{
		_isCameraLockedOn = true;
	}

	private void OnPlayerCameraBreakLock()
	{
		_isCameraLockedOn = false;
	}

	private void OnEnterZeroGTraining()
	{
		_inZeroGTraining = true;
	}

	private void OnExitZeroGTraining()
	{
		_inZeroGTraining = false;
	}

	private void OnEnterConversation()
	{
		_inConversation = true;
	}

	private void OnExitConversation()
	{
		_inConversation = false;
	}

	private void OnEnterDarkZone()
	{
		_inDarkZone = true;
	}

	private void OnExitDarkZone()
	{
		_inDarkZone = false;
	}

	private static void OnFlashlightOn()
	{
		_isFlashlightOn = true;
	}

	private static void OnFlashlightOff()
	{
		_isFlashlightOn = false;
	}

	private static void OnPlayerEnterBrambleDimension()
	{
		_inBrambleDimension = true;
	}

	private static void OnPlayerExitBrambleDimension()
	{
		_inBrambleDimension = false;
	}

	private static void OnPlayerEnterGiantsDeep()
	{
		_inGiantsDeep = true;
	}

	private static void OnPlayerExitGiantsDeep()
	{
		_inGiantsDeep = false;
	}

	private static void OnInitPlayerForceAlignment()
	{
		_alignedWithField = true;
	}

	private static void OnBreakPlayerForceAlignment()
	{
		_alignedWithField = false;
	}

	private static void OnEnterSignalscopeZoom(Signalscope telescope)
	{
		_usingTelescope = true;
	}

	private static void OnExitSignalscopeZoom()
	{
		_usingTelescope = false;
	}

	private static void OnEnterShipComputer()
	{
		_usingShipComputer = true;
	}

	private static void OnExitShipComputer()
	{
		_usingShipComputer = false;
	}

	private static void OnEnterLandingView()
	{
		_inLandingView = true;
	}

	private static void OnExitLandingView()
	{
		_inLandingView = false;
	}

	private static void OnEnterShip()
	{
		_insideShip = true;
		_hasPlayerEnteredShip = true;
	}

	private static void OnExitShip()
	{
		_insideShip = false;
	}

	private static void OnSuitUp()
	{
		_isWearingSuit = true;
	}

	private static void OnRemoveSuit()
	{
		_isWearingSuit = false;
	}

	private static void OnEnterShuttle()
	{
		_insideShuttle = true;
	}

	private static void OnExitShuttle()
	{
		_insideShuttle = false;
	}

	private static void OnEnterMapView()
	{
		_mapView = true;
	}

	private static void OnExitMapView()
	{
		_mapView = false;
	}

	private static void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		_atFlightConsole = true;
	}

	private static void OnExitFlightConsole()
	{
		_atFlightConsole = false;
	}

	private static void OnCameraEnterWater(float relativeSpeed)
	{
		_isCameraUnderwater = true;
	}

	private static void OnCameraExitWater()
	{
		_isCameraUnderwater = false;
	}

	private static void OnPlayerDeath(DeathType deathType)
	{
		_isDead = true;
	}

	private static void OnPlayerResurrection()
	{
		_isDead = false;
		_isResurrected = true;
	}

	private void OnAttachPlayerToPoint(OWRigidbody attachBody)
	{
		_isAttached = true;
	}

	private void OnDetachPlayerFromPoint()
	{
		_isAttached = false;
		_lastDetachTime = Time.time;
	}

	private static void OnShipHullBreach()
	{
		_isHullBreached = true;
	}

	private void OnEnterNomaiRemoteCamera()
	{
		_usingNomaiRemoteCamera = true;
	}

	private void OnExitNomaiRemoteCamera()
	{
		_usingNomaiRemoteCamera = false;
	}

	private void OnEyeStateChanged(EyeState state)
	{
		if (Locator.GetEyeStateManager().IsInsideTheEye())
		{
			_insideTheEye = true;
		}
	}

	private void OnStartSleepingAtCampfire(bool isDreamCampfire)
	{
		_isSleepingAtCampfire = true;
		_isSleepingAtDreamCampfire = isDreamCampfire;
	}

	private void OnStopSleepingAtCampfire()
	{
		_isSleepingAtCampfire = false;
		_isSleepingAtDreamCampfire = false;
	}

	private void OnStartFastForward()
	{
		_isFastForwarding = true;
	}

	private void OnEndFastForward()
	{
		_isFastForwarding = false;
	}

	private void OnEnterCloak()
	{
		_inCloakingField = true;
	}

	private void OnExitCloak()
	{
		_inCloakingField = false;
	}

	private void OnEnterDreamWorld()
	{
		_inDreamWorld = true;
	}

	private void OnExitDreamWorld()
	{
		_inDreamWorld = false;
		if (!_hasPlayerhadLanternBlownOut && Locator.GetDreamWorldController().GetLastDreamWakeType() == DreamWakeType.LanternBlownOut)
		{
			_hasPlayerhadLanternBlownOut = true;
		}
	}

	private void OnStartViewingProjector()
	{
		_viewingProjector = true;
	}

	private void OnEndViewingProjector()
	{
		_viewingProjector = false;
	}

	private void OnStartPeeping(Peephole peephole)
	{
		_isPeeping = true;
	}

	private void OnStopPeeping(Peephole peephole)
	{
		_isPeeping = false;
	}

	private void OnEnterUndertowVolume()
	{
		_undertowVolumeCount++;
	}

	private void OnExitUndertowVolume()
	{
		_undertowVolumeCount--;
		if (_undertowVolumeCount < 0)
		{
			_undertowVolumeCount = 0;
			Debug.LogError("Undertow count should never be less than zero!");
			Debug.Break();
		}
	}

	private void OnGrabbedByGhost()
	{
		_isGrabbedByGhost = true;
	}

	private void OnReleasedByGhost()
	{
		_isGrabbedByGhost = false;
	}
}
