using System;
using UnityEngine;

public class VesselWarpController : MonoBehaviour
{
	[SerializeField]
	private SingularityController _blackHole;

	[SerializeField]
	private SingularityController _whiteHole;

	[Space]
	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private WarpCoreSocket _coreSocket;

	[SerializeField]
	private DirectionalForceVolume _gravityVolume;

	[SerializeField]
	private OWTriggerVolume _bridgeVolume;

	[SerializeField]
	private TransformAnimator _cageAnimator;

	[SerializeField]
	private OWTriggerVolume _cageTrigger;

	[SerializeField]
	private OWAudioSource _cageLoopingAudio;

	[Space]
	[SerializeField]
	private NomaiCoordinateInterface _coordinateInterface;

	[SerializeField]
	private NomaiWarpPlatform _sourceWarpPlatform;

	[SerializeField]
	private NomaiWarpPlatform _targetWarpPlatform;

	[Space]
	[SerializeField]
	private NomaiInterfaceSlot _coordinatePowerSlot;

	[SerializeField]
	private NomaiInterfaceSlot _warpPlatformPowerSlot;

	[SerializeField]
	private NomaiInterfaceSlot _warpVesselSlot;

	[Space]
	[SerializeField]
	private NomaiEnergyCable _coordinateCable;

	[SerializeField]
	private NomaiEnergyCable _warpPlatformCable;

	[SerializeField]
	private NomaiEnergyCable _coreCable;

	[Space]
	[SerializeField]
	private OWAudioSource _blackHoleOneShot;

	[SerializeField]
	private OWAudioSource _whiteHoleOneShot;

	[Space]
	[SerializeField]
	private Transform _defaultPlayerWarpPoint;

	private static bool s_relativeLocationSaved;

	private static RelativeLocationData s_playerWarpLocation;

	private bool _hasPower;

	private float _origGravityMagnitude;

	private bool _openingBlackHole;

	private bool _waitingForLoad;

	private bool _hasExtendedTimeLoop;

	private bool _instantlyPutOnSuit;

	private bool _cageClosed;

	private void Awake()
	{
		if (_cageTrigger != null)
		{
			_cageTrigger.OnExit += OnExitCageTrigger;
		}
		if (_coreSocket != null)
		{
			WarpCoreSocket coreSocket = _coreSocket;
			coreSocket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(coreSocket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnWarpCorePlaced));
			WarpCoreSocket coreSocket2 = _coreSocket;
			coreSocket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(coreSocket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnWarpCoreRemoved));
		}
		if (_coordinatePowerSlot != null)
		{
			_coordinatePowerSlot.OnSlotActivated += OnSlotActivated;
			_coordinatePowerSlot.OnSlotDeactivated += OnSlotDeactivated;
		}
		if (_warpPlatformPowerSlot != null)
		{
			_warpPlatformPowerSlot.OnSlotActivated += OnSlotActivated;
			_warpPlatformPowerSlot.OnSlotDeactivated += OnSlotDeactivated;
		}
		if (_warpVesselSlot != null)
		{
			_warpVesselSlot.OnSlotActivated += OnSlotActivated;
		}
		GlobalMessenger.AddListener("DebugWarpVessel", OnDebugWarpVessel);
	}

	private void Start()
	{
		_origGravityMagnitude = _gravityVolume.GetFieldMagnitude();
		if (_cageLoopingAudio != null)
		{
			_cageLoopingAudio.SetLocalVolume(0f);
		}
		WarpCoreItem warpCoreItem = (WarpCoreItem)_coreSocket.GetSocketedItem();
		SetPowered(warpCoreItem != null && warpCoreItem.GetWarpCoreType() == WarpCoreType.Vessel);
		CheckSystemActivation();
		if (Locator.GetEyeStateManager() != null && PlayerData.GetWarpedToTheEye())
		{
			if (EyeStateManager.ParadoxExists() && PlayerData.GetSecondsRemainingOnWarp() < 180f)
			{
				TimeLoop.SetSecondsRemaining(180f);
			}
			else
			{
				TimeLoop.SetSecondsRemaining(PlayerData.GetSecondsRemainingOnWarp());
			}
			if (s_relativeLocationSaved)
			{
				Locator.GetPlayerBody().MoveToRelativeLocation(s_playerWarpLocation, base.transform);
				s_relativeLocationSaved = false;
			}
			else
			{
				Locator.GetPlayerBody().SetPosition(_defaultPlayerWarpPoint.position);
				Locator.GetPlayerBody().SetRotation(_defaultPlayerWarpPoint.rotation);
				Locator.GetPlayerBody().SetVelocity(Vector3.zero);
			}
			_bridgeVolume.AddObjectToVolume(Locator.GetPlayerDetector());
			_bridgeVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());
			_instantlyPutOnSuit = true;
		}
	}

	private void OnDestroy()
	{
		if (_cageTrigger != null)
		{
			_cageTrigger.OnExit -= OnExitCageTrigger;
		}
		if (_coreSocket != null)
		{
			WarpCoreSocket coreSocket = _coreSocket;
			coreSocket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(coreSocket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnWarpCorePlaced));
			WarpCoreSocket coreSocket2 = _coreSocket;
			coreSocket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(coreSocket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnWarpCoreRemoved));
		}
		if (_coordinatePowerSlot != null)
		{
			_coordinatePowerSlot.OnSlotActivated -= OnSlotActivated;
			_coordinatePowerSlot.OnSlotDeactivated -= OnSlotDeactivated;
		}
		if (_warpPlatformPowerSlot != null)
		{
			_warpPlatformPowerSlot.OnSlotActivated -= OnSlotActivated;
			_warpPlatformPowerSlot.OnSlotDeactivated -= OnSlotDeactivated;
		}
		if (_warpVesselSlot != null)
		{
			_warpVesselSlot.OnSlotActivated -= OnSlotActivated;
		}
		if (_cageAnimator != null)
		{
			_cageAnimator.OnTranslationComplete -= OnCageAnimationComplete;
		}
		GlobalMessenger.RemoveListener("DebugWarpVessel", OnDebugWarpVessel);
	}

	public bool HasPower()
	{
		return _hasPower;
	}

	private void SetPowered(bool powered)
	{
		if (_hasPower != powered)
		{
			_hasPower = powered;
			CheckSystemActivation();
		}
	}

	private void CheckSystemActivation()
	{
		_gravityVolume.SetFieldMagnitude(_hasPower ? _origGravityMagnitude : 0f);
		if (Locator.GetEyeStateManager() != null)
		{
			if (!_sourceWarpPlatform.IsBlackHoleOpen() && _hasPower && _warpPlatformPowerSlot.IsActivated() && _targetWarpPlatform != null)
			{
				_sourceWarpPlatform.OpenBlackHole(_targetWarpPlatform, stayOpen: true);
			}
			else if (_sourceWarpPlatform.IsBlackHoleOpen() && (!_hasPower || !_warpPlatformPowerSlot.IsActivated()))
			{
				_sourceWarpPlatform.CloseBlackHole();
			}
		}
		_coreCable.SetPowered(_hasPower);
		_coordinateCable.SetPowered(_hasPower && _coordinatePowerSlot.IsActivated());
		_warpPlatformCable.SetPowered(_hasPower && _warpPlatformPowerSlot.IsActivated());
	}

	private void Update()
	{
		if (_instantlyPutOnSuit && !Locator.GetPlayerSuit().IsWearingSuit())
		{
			Locator.GetPlayerSuit().SuitUp(isTrainingSuit: false, instantSuitUp: true);
			_instantlyPutOnSuit = false;
		}
		if (_openingBlackHole && _blackHole.GetState() == SingularityController.State.Stable)
		{
			RumbleManager.StopVesselWarp();
			_openingBlackHole = false;
			if (LoadManager.IsAsyncLoadComplete())
			{
				WarpVessel(debugWarp: false);
			}
			else
			{
				_waitingForLoad = true;
				OWTime.Pause(OWTime.PauseType.Loading);
			}
		}
		else if (_whiteHole != null && _whiteHole.GetState() == SingularityController.State.Stable && LateInitializerManager.isDoneInitializing)
		{
			_whiteHole.Collapse();
			_whiteHoleOneShot.PlayOneShot(AudioType.VesselSingularityCollapse);
		}
		else if (_waitingForLoad && LoadManager.IsAsyncLoadComplete())
		{
			OWTime.Unpause(OWTime.PauseType.Loading);
			WarpVessel(debugWarp: false);
			_waitingForLoad = false;
		}
		if (!_openingBlackHole && !_waitingForLoad && (_whiteHole == null || _whiteHole.GetState() == SingularityController.State.Collapsed))
		{
			base.enabled = false;
		}
	}

	private void OnDebugWarpVessel()
	{
		WarpVessel(debugWarp: true);
	}

	private void WarpVessel(bool debugWarp)
	{
		if (TimeLoop.GetLoopCount() < 2)
		{
			Achievements.Earn(Achievements.Type.BEGINNERS_LUCK);
		}
		s_playerWarpLocation = new RelativeLocationData(Locator.GetPlayerBody(), base.transform);
		s_relativeLocationSaved = ((!debugWarp) ? true : false);
		PlayerData.SaveWarpedToTheEye(TimeLoop.GetSecondsRemaining());
		LoadManager.EnableAsyncLoadTransition();
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		if (slot == _warpVesselSlot && _hasPower && _coordinateInterface.CheckEyeCoordinates() && _blackHole.GetState() == SingularityController.State.Collapsed && LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse)
		{
			_blackHole.Create();
			RumbleManager.StartVesselWarp();
			_openingBlackHole = true;
			base.enabled = true;
			Locator.GetPauseCommandListener().AddPauseCommandLock();
			LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, autoTransition: false, LoadManager.FadeType.ToWhite);
			_blackHoleOneShot.PlayOneShot(AudioType.VesselSingularityCreate);
			GlobalMessenger.FireEvent("StartVesselWarp");
		}
		else
		{
			if (slot == _coordinatePowerSlot)
			{
				_coordinateInterface.SetPillarRaised(raised: true, powered: true);
			}
			CheckSystemActivation();
		}
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		if (slot == _coordinatePowerSlot)
		{
			_coordinateInterface.SetPillarRaised(raised: false, powered: false);
		}
		CheckSystemActivation();
	}

	private void OnWarpCorePlaced(OWItem item)
	{
		WarpCoreItem warpCoreItem = (WarpCoreItem)item;
		if (warpCoreItem.GetWarpCoreType() == WarpCoreType.Vessel)
		{
			SetPowered(warpCoreItem.GetWarpCoreType() == WarpCoreType.Vessel);
			_audioSource.PlayOneShot(AudioType.NomaiVesselPowerUp);
			if (!_hasExtendedTimeLoop && Locator.GetShipLogManager().IsFactRevealed("OPC_EYE_COORDINATES_X1"))
			{
				_hasExtendedTimeLoop = true;
				if (TimeLoop.GetSecondsRemaining() < 180f && TimeLoop.GetSecondsRemaining() > 1f)
				{
					Debug.Log("Rewinding Time Loop to 3 minutes remaining.");
					TimeLoop.SetSecondsRemaining(180f);
				}
			}
		}
		else
		{
			Locator.GetShipLogManager().RevealFact("DB_VESSEL_X2");
		}
	}

	private void OnWarpCoreRemoved(OWItem item)
	{
		if (_hasPower)
		{
			SetPowered(powered: false);
			if (_cageClosed)
			{
				_cageAnimator.TranslateToOriginalLocalPosition(5f);
				_cageAnimator.RotateToOriginalLocalRotation(5f);
				_cageAnimator.OnTranslationComplete -= OnCageAnimationComplete;
				_cageAnimator.OnTranslationComplete += OnCageAnimationComplete;
				_cageLoopingAudio.FadeIn(1f);
				_cageClosed = false;
			}
			_audioSource.PlayOneShot(AudioType.NomaiPowerOff);
		}
	}

	private void OnExitCageTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") && _hasPower)
		{
			_cageClosed = true;
			_cageAnimator.TranslateToLocalPosition(new Vector3(0f, -8.1f, 0f), 5f);
			_cageAnimator.RotateToLocalEulerAngles(new Vector3(0f, 180f, 0f), 5f);
			_cageAnimator.OnTranslationComplete -= OnCageAnimationComplete;
			_cageAnimator.OnTranslationComplete += OnCageAnimationComplete;
			_cageLoopingAudio.FadeIn(1f);
		}
	}

	private void OnCageAnimationComplete()
	{
		_cageAnimator.OnTranslationComplete -= OnCageAnimationComplete;
		_cageLoopingAudio.FadeOut(1f);
		_cageLoopingAudio.PlayOneShot(AudioType.NomaiPillarRotate);
	}
}
