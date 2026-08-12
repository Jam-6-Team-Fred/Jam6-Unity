using System;
using UnityEngine;

public class PrisonerDirector : MonoBehaviour
{
	[SerializeField]
	private GhostController _prisonerController;

	[SerializeField]
	private PrisonerBrain _prisonerBrain;

	[SerializeField]
	private PrisonerEffects _prisonerEffects;

	[SerializeField]
	private OWAudioSource _musicSource;

	[Space]
	[SerializeField]
	private PrisonCellElevator _cellevator;

	[SerializeField]
	private OWTriggerVolume _prisonEntryTrigger;

	[SerializeField]
	private OWTriggerVolume _callCellevatorTrigger;

	[Space]
	[SerializeField]
	private OWTriggerVolume _emergeTrigger;

	[Space]
	[SerializeField]
	private Transform _lanternTableSocket;

	[Space]
	[SerializeField]
	private CharacterDialogueTree _characterDialogueTree;

	[Header("Cell Lighting")]
	[SerializeField]
	private OWFlameController _prisonLighting;

	[SerializeField]
	private OWRendererFadeController _alcoveDarkness;

	[SerializeField]
	private OWCollider _alcoveCollider;

	[SerializeField]
	private OWAudioSource _hangingLampSource;

	[SerializeField]
	private AudioVolume _lightsOnAudioVolume;

	[SerializeField]
	private OWTriggerVolume _lightsOutTrigger;

	[Header("Vision Torch")]
	[SerializeField]
	private VisionTorchItem _visionTorchItem;

	[SerializeField]
	private Shape _prisonerDetector;

	[SerializeField]
	private MindSlideCollection _storyReel5_Complete;

	[SerializeField]
	private MindSlideCollection _storyReel5_NoInterloper;

	[SerializeField]
	private MindSlideCollection _storyReel5_NoInterloperNoVessel;

	[SerializeField]
	private MindSlideCollection _storyReel5_NoVessel;

	[SerializeField]
	private VisionTorchSocket _prisonerTorchSocket;

	[SerializeField]
	private Transform _torchCueMarker;

	[SerializeField]
	private Transform _scanCueMarker;

	[Space]
	[SerializeField]
	private Transform _torchReturnCueMarker;

	[SerializeField]
	private OWCollider _cellevatorLowerDoorProxyCollider;

	[Space]
	[SerializeField]
	private RotatingDoor _cellevatorLowerDoor;

	[SerializeField]
	private Transform _cellevatorPedestalMarker;

	[Space]
	[SerializeField]
	private GhostNodeMap _cellevatorNodeMap;

	[SerializeField]
	private DreamLibraryPedestal _cellevatorPedestal;

	[SerializeField]
	private Transform _cellevatorWindowMarker;

	[Space]
	[SerializeField]
	private RotatingDoor _cellevatorUpperDoor;

	[SerializeField]
	private GhostNodeMap _upperFloorNodeMap;

	[SerializeField]
	private Transform _exitCueMarker;

	[SerializeField]
	private OWTriggerVolume _sendCellevatorDownTrigger;

	[Space]
	[SerializeField]
	private GameObject[] _activateOnFarewell;

	[SerializeField]
	private GameObject[] _deactivateOnFarewell;

	[SerializeField]
	private AudioVolume _quietAudioVolume;

	[SerializeField]
	private DreamCandle[] _dreamCandles;

	[SerializeField]
	private MindSlideProjector _farewellProjector;

	private bool _sequenceInitialized;

	private bool _darknessAwoken;

	private bool _projectingVision;

	private bool _waitingForPlayerToTakeTorch = true;

	private bool _waitingForPlayerToReturnTorch;

	private bool _arrivedAtCellevatorPedestal;

	public OWEvent OnPrisonerDeparted = new OWEvent(1);

	private void Awake()
	{
		_prisonerBrain.OnFinishEmergeBehavior += new OWEvent.OWCallback(OnPrisonerEmerged);
		_prisonerBrain.OnFinishLightingBehavior += new OWEvent.OWCallback(OnPrisonerLightingSequenceComplete);
		_prisonerBrain.OnFinishFetchTorchBehavior += new OWEvent.OWCallback(OnPrisonerFetchTorchComplete);
		_prisonerBrain.OnArriveAtElevatorDoor += new OWEvent.OWCallback(OnPrisonerArriveAtElevatorDoor);
		_prisonerBrain.OnArriveAtElevatorPedestal += new OWEvent.OWCallback(OnPrisonerArriveAtElevatorPedestal);
		_prisonerBrain.OnFinishExitBehavior += new OWEvent.OWCallback(OnPrisonerExit);
		_prisonerEffects.OnPickUpLantern += new OWEvent.OWCallback(OnPrisonerPickUpLantern);
		_prisonerEffects.OnTurnOnLights += new OWEvent.OWCallback(OnPrisonerLitLights);
		_prisonerEffects.OnPickUpTorch += new OWEvent.OWCallback(OnPrisonerPickUpTorch);
		_prisonerEffects.OnProjectVision += new OWEvent.OWCallback(OnPrisonerProjectVision);
		_prisonerEffects.OnOfferTorch += new OWEvent.OWCallback(OnPrisonerOfferTorch);
		_prisonerEffects.OnReactToVisionAnimationComplete += new OWEvent.OWCallback(OnPrisonerReactToVisionComplete);
		_prisonerEffects.OnReadyToReceiveTorch += new OWEvent.OWCallback(OnPrisonerReadyToReceiveTorch);
		_prisonerEffects.OnFarewellTurnComplete += new OWEvent.OWCallback(OnPrisonerEnterElevator);
		_prisonEntryTrigger.OnEntry += OnEnterPrison;
		_callCellevatorTrigger.OnEntry += OnEnterCallCellevatorTrigger;
		_emergeTrigger.OnEntry += OnEnterEmergeTrigger;
		_characterDialogueTree.OnEndConversation += OnFinishDialogue;
		_visionTorchItem.onPickedUp += new OWEvent<OWItem>.OWCallback(OnPlayerTakeTorch);
		_visionTorchItem.mindProjectorTrigger.OnBeamStartHitPrisoner += new OWEvent.OWCallback(OnStartProjectingOnPrisoner);
		_visionTorchItem.mindProjectorTrigger.OnBeamStopHitPrisoner += new OWEvent.OWCallback(OnStopProjectingOnPrisoner);
		_visionTorchItem.mindSlideProjector.OnProjectionComplete += new OWEvent.OWCallback(OnMindProjectionComplete);
		VisionTorchSocket prisonerTorchSocket = _prisonerTorchSocket;
		prisonerTorchSocket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(prisonerTorchSocket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnPrisonerReceiveTorch));
		_sendCellevatorDownTrigger.OnEntry += OnEnterSendCellevatorTrigger;
		_farewellProjector.OnProjectionComplete += new OWEvent.OWCallback(OnFarewellProjectionComplete);
	}

	private void OnDestroy()
	{
		_prisonerBrain.OnFinishEmergeBehavior -= new OWEvent.OWCallback(OnPrisonerEmerged);
		_prisonerBrain.OnFinishLightingBehavior -= new OWEvent.OWCallback(OnPrisonerLitLights);
		_prisonerBrain.OnFinishFetchTorchBehavior -= new OWEvent.OWCallback(OnPrisonerFetchTorchComplete);
		_prisonerBrain.OnArriveAtElevatorDoor -= new OWEvent.OWCallback(OnPrisonerArriveAtElevatorDoor);
		_prisonerBrain.OnArriveAtElevatorPedestal -= new OWEvent.OWCallback(OnPrisonerArriveAtElevatorPedestal);
		_prisonerBrain.OnFinishExitBehavior -= new OWEvent.OWCallback(OnPrisonerExit);
		_prisonerEffects.OnPickUpLantern -= new OWEvent.OWCallback(OnPrisonerPickUpLantern);
		_prisonerEffects.OnTurnOnLights -= new OWEvent.OWCallback(OnPrisonerLitLights);
		_prisonerEffects.OnPickUpTorch -= new OWEvent.OWCallback(OnPrisonerPickUpTorch);
		_prisonerEffects.OnProjectVision -= new OWEvent.OWCallback(OnPrisonerProjectVision);
		_prisonerEffects.OnOfferTorch -= new OWEvent.OWCallback(OnPrisonerOfferTorch);
		_prisonerEffects.OnReactToVisionAnimationComplete -= new OWEvent.OWCallback(OnPrisonerReactToVisionComplete);
		_prisonerEffects.OnReadyToReceiveTorch -= new OWEvent.OWCallback(OnPrisonerReadyToReceiveTorch);
		_prisonerEffects.OnFarewellTurnComplete -= new OWEvent.OWCallback(OnPrisonerEnterElevator);
		_prisonEntryTrigger.OnEntry -= OnEnterPrison;
		_callCellevatorTrigger.OnEntry -= OnEnterCallCellevatorTrigger;
		_emergeTrigger.OnEntry -= OnEnterEmergeTrigger;
		_characterDialogueTree.OnEndConversation -= OnFinishDialogue;
		_visionTorchItem.onPickedUp -= new OWEvent<OWItem>.OWCallback(OnPlayerTakeTorch);
		_visionTorchItem.mindProjectorTrigger.OnBeamStartHitPrisoner -= new OWEvent.OWCallback(OnStartProjectingOnPrisoner);
		_visionTorchItem.mindProjectorTrigger.OnBeamStopHitPrisoner -= new OWEvent.OWCallback(OnStopProjectingOnPrisoner);
		_visionTorchItem.mindSlideProjector.OnProjectionComplete -= new OWEvent.OWCallback(OnMindProjectionComplete);
		VisionTorchSocket prisonerTorchSocket = _prisonerTorchSocket;
		prisonerTorchSocket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(prisonerTorchSocket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnPrisonerReceiveTorch));
		_sendCellevatorDownTrigger.OnEntry -= OnEnterSendCellevatorTrigger;
		_farewellProjector.OnProjectionComplete -= new OWEvent.OWCallback(OnFarewellProjectionComplete);
	}

	private void InitializeSequence()
	{
		_prisonerController.MoveLanternToSocket(_lanternTableSocket);
		_characterDialogueTree.GetInteractVolume().DisableInteraction();
		_prisonLighting.SetIntensity(0f);
		_alcoveDarkness.SetFade(1f);
		_alcoveCollider.SetActivation(active: true);
		_lightsOnAudioVolume.SetVolumeActivation(active: false);
		_visionTorchItem.EnableInteraction(value: false);
		_prisonerTorchSocket.EnableInteraction(value: false);
		_sendCellevatorDownTrigger.SetTriggerActivation(active: false);
		_sequenceInitialized = true;
	}

	private void OnEnterPrison(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") && !_darknessAwoken && !_sequenceInitialized)
		{
			InitializeSequence();
		}
	}

	private void OnEnterCallCellevatorTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_cellevator.CallToTopFloor();
		}
	}

	private void OnEnterEmergeTrigger(GameObject hitObj)
	{
		if (!_darknessAwoken && hitObj.CompareTag("PlayerDetector"))
		{
			_darknessAwoken = true;
			_prisonerBrain.BeginBehavior(PrisonerBehavior.Emerge);
			_cellevator.OnPrisonerReveal();
			_musicSource.SetLocalVolume(Locator.GetAudioManager().GetAudioEntry(_musicSource.audioLibraryClip).volume);
			_musicSource.Play();
		}
	}

	private void OnPrisonerPickUpLantern()
	{
		_prisonerController.MoveLanternToCarrySocket(lerpTransform: true, 0.5f);
	}

	private void OnPrisonerEmerged()
	{
		_characterDialogueTree.GetInteractVolume().EnableInteraction();
		_prisonerBrain.BeginBehavior(PrisonerBehavior.WaitForConversation);
	}

	private void OnFinishDialogue()
	{
		_characterDialogueTree.GetInteractVolume().DisableInteraction();
		_prisonerBrain.BeginBehavior(PrisonerBehavior.LightLamp, 0.66f);
	}

	private void OnPrisonerLitLights()
	{
		_prisonLighting.FadeTo(1f, 1f);
		_alcoveDarkness.FadeTo(0f, 1f);
		_alcoveCollider.SetActivation(active: false);
		_hangingLampSource.PlayOneShot(AudioType.Candle_Light_Big);
		_lightsOnAudioVolume.SetVolumeActivation(active: true);
	}

	private void OnPrisonerLightingSequenceComplete()
	{
		_prisonerBrain.BeginBehavior(PrisonerBehavior.FetchTorch, _torchCueMarker, 2f);
	}

	private void OnPrisonerPickUpTorch()
	{
		_prisonerTorchSocket.PlaceIntoSocket(_visionTorchItem);
	}

	private void OnPrisonerFetchTorchComplete()
	{
		_prisonerBrain.BeginBehavior(PrisonerBehavior.ProjectVision, _scanCueMarker);
	}

	private void OnPrisonerProjectVision()
	{
		_visionTorchItem.mindProjectorTrigger.SetProjectorActive(active: true);
		_projectingVision = true;
	}

	private void OnMindProjectionComplete()
	{
		if (_projectingVision)
		{
			_visionTorchItem.mindProjectorTrigger.SetProjectorActive(active: false);
			_projectingVision = false;
			_prisonerBrain.BeginBehavior(PrisonerBehavior.OfferTorch, 3f);
		}
	}

	private void OnPrisonerOfferTorch()
	{
		_waitingForPlayerToTakeTorch = true;
		_visionTorchItem.EnableInteraction(value: true);
		_prisonerTorchSocket.EnableInteraction(value: true);
	}

	private void OnPlayerTakeTorch(OWItem owItem)
	{
		if (_waitingForPlayerToTakeTorch)
		{
			_waitingForPlayerToTakeTorch = false;
			_prisonerTorchSocket.EnableInteraction(value: false);
			_prisonerBrain.BeginBehavior(PrisonerBehavior.WaitForProjection, 0.2f);
			ShipLogManager shipLogManager = Locator.GetShipLogManager();
			bool num = shipLogManager.IsFactRevealed("BH_MURAL_1_X1") || shipLogManager.IsFactRevealed("CT_SUNLESS_CITY_X3") || shipLogManager.IsFactRevealed("BH_OLD_SETTLEMENT_X2") || shipLogManager.IsFactRevealed("BH_HANGING_CITY_X4") || shipLogManager.IsFactRevealed("DB_VESSEL_R1") || shipLogManager.IsFactRevealed("BH_OBSERVATORY_X1");
			bool flag = shipLogManager.IsFactRevealed("BH_MURAL_2_X1") || shipLogManager.IsFactRevealed("BH_MURAL_3_X1") || shipLogManager.IsFactRevealed("DB_ESCAPE_POD_X2") || shipLogManager.IsFactRevealed("DB_VESSEL_R2") || shipLogManager.IsFactRevealed("DB_VESSEL_R3") || shipLogManager.IsFactRevealed("DB_VESSEL_R4");
			bool flag2 = (num && flag) || shipLogManager.IsFactRevealed("DB_VESSEL_X6");
			bool flag3 = shipLogManager.IsFactRevealed("COMET_INTERIOR_X1");
			MindSlideCollection mindSlideCollection = _storyReel5_NoInterloperNoVessel;
			if (flag2 && flag3)
			{
				mindSlideCollection = _storyReel5_Complete;
			}
			else if (flag2 && !flag3)
			{
				mindSlideCollection = _storyReel5_NoInterloper;
			}
			else if (!flag2 && flag3)
			{
				mindSlideCollection = _storyReel5_NoVessel;
			}
			_visionTorchItem.mindSlideProjector.SetMindSlideCollection(mindSlideCollection);
		}
	}

	private void OnStartProjectingOnPrisoner()
	{
		_prisonerBrain.BeginBehavior(PrisonerBehavior.ExperienceVision);
	}

	private void OnStopProjectingOnPrisoner()
	{
		if (!_visionTorchItem.mindSlideProjector.mindSlideCollection.slideCollectionContainer.isEndOfSlide)
		{
			_prisonerBrain.BeginBehavior(PrisonerBehavior.WaitForProjection, 0.5f);
			return;
		}
		_prisonerDetector.SetActivation(newActive: false);
		_prisonerBrain.BeginBehavior(PrisonerBehavior.ExperienceEmotionalCatharsis, 0.5f);
	}

	private void OnPrisonerReactToVisionComplete()
	{
		_prisonerBrain.BeginBehavior(PrisonerBehavior.MoveToElevatorDoor, _torchReturnCueMarker, 1f);
	}

	private void OnPrisonerArriveAtElevatorDoor()
	{
		_prisonerBrain.BeginBehavior(PrisonerBehavior.WaitForTorchReturn, _torchReturnCueMarker);
	}

	private void OnPrisonerReadyToReceiveTorch()
	{
		_waitingForPlayerToReturnTorch = true;
		_prisonerTorchSocket.EnableInteraction(value: true);
	}

	private void OnPrisonerReceiveTorch(OWItem owItem)
	{
		if (_waitingForPlayerToReturnTorch)
		{
			_waitingForPlayerToReturnTorch = false;
			_visionTorchItem.EnableInteraction(value: false);
			_prisonerTorchSocket.EnableInteraction(value: false);
			_cellevatorLowerDoorProxyCollider.SetActivation(active: true);
			_prisonerBrain.BeginBehavior(PrisonerBehavior.Farewell, _cellevatorPedestalMarker, 0.1f);
			_musicSource.SetLocalVolume(0f);
			_musicSource.AssignAudioLibraryClip(AudioType.Prisoner_Catharsis);
			_musicSource.FadeInToLibraryVolume(4f);
		}
	}

	private void OnPrisonerEnterElevator()
	{
		_cellevatorLowerDoor.OnCloseFinish += new OWEvent.OWCallback(OnCellevatorLowerDoorCloseWithPrisonerInside);
		_cellevatorLowerDoor.Close();
	}

	private void OnPrisonerArriveAtElevatorPedestal()
	{
		_arrivedAtCellevatorPedestal = true;
		if (!_cellevatorLowerDoor.IsOpen() && !_cellevatorLowerDoor.IsClosing())
		{
			ElevatePrisoner();
		}
	}

	private void OnCellevatorLowerDoorCloseWithPrisonerInside()
	{
		_cellevatorLowerDoor.OnCloseFinish -= new OWEvent.OWCallback(OnCellevatorLowerDoorCloseWithPrisonerInside);
		if (_arrivedAtCellevatorPedestal)
		{
			ElevatePrisoner();
		}
	}

	private void ElevatePrisoner()
	{
		_cellevatorLowerDoor.OnCloseFinish -= new OWEvent.OWCallback(OnCellevatorLowerDoorCloseWithPrisonerInside);
		_prisonerController.SetNodeMap(_cellevatorNodeMap);
		_prisonerController.MoveLanternToSocket(_cellevatorPedestal.GetSocket().transform);
		_cellevatorPedestal.Activate();
		_prisonerBrain.BeginBehavior(PrisonerBehavior.RideElevator, _cellevatorWindowMarker);
		_cellevator.OnArriveAtTopFloor += new OWEvent.OWCallback(OnCellevatorArriveAtTopFloorWithPrisonerInside);
	}

	private void OnCellevatorArriveAtTopFloorWithPrisonerInside()
	{
		_cellevator.OnArriveAtTopFloor -= new OWEvent.OWCallback(OnCellevatorArriveAtTopFloorWithPrisonerInside);
		_cellevatorUpperDoor.OnOpenFinish += new OWEvent.OWCallback(OnCellevatorUpperDoorOpenWithPrisonerInside);
		_prisonerController.SetNodeMap(_upperFloorNodeMap);
		_prisonerController.MoveLanternToCarrySocket();
		_cellevatorPedestal.Deactivate();
		_sendCellevatorDownTrigger.SetTriggerActivation(active: true);
		_lightsOutTrigger.OnEntry += OnEnterLightsOutTrigger;
	}

	private void OnCellevatorUpperDoorOpenWithPrisonerInside()
	{
		_cellevatorUpperDoor.OnOpenFinish -= new OWEvent.OWCallback(OnCellevatorUpperDoorOpenWithPrisonerInside);
		_prisonerBrain.BeginBehavior(PrisonerBehavior.Exit, _exitCueMarker);
	}

	private void OnEnterSendCellevatorTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("GhostDetector"))
		{
			_cellevator.CallToBottomFloor();
		}
	}

	private void OnPrisonerExit()
	{
		for (int i = 0; i < _activateOnFarewell.Length; i++)
		{
			_activateOnFarewell[i].SetActive(value: true);
		}
		for (int j = 0; j < _deactivateOnFarewell.Length; j++)
		{
			_deactivateOnFarewell[j].SetActive(value: false);
		}
		_prisonerController.gameObject.SetActive(value: false);
		_quietAudioVolume.SetVolumeActivation(active: false);
		_callCellevatorTrigger.SetTriggerActivation(active: false);
		_sendCellevatorDownTrigger.SetTriggerActivation(active: false);
		for (int k = 0; k < _dreamCandles.Length; k++)
		{
			_dreamCandles[k].SetLit(lit: false, playAudio: false);
		}
		OnPrisonerDeparted.Invoke();
	}

	private void OnEnterLightsOutTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_lightsOutTrigger.OnEntry -= OnEnterLightsOutTrigger;
			_prisonLighting.FadeTo(0f, 1f);
			_hangingLampSource.PlayOneShot(AudioType.Candle_Extinguish);
			_lightsOnAudioVolume.SetVolumeActivation(active: false);
		}
	}

	private void OnFarewellProjectionComplete()
	{
		_farewellProjector.OnProjectionComplete -= new OWEvent.OWCallback(OnFarewellProjectionComplete);
		if (PlayerState.IsResurrected())
		{
			Locator.GetDeathManager().FinishedDLC();
		}
	}
}
