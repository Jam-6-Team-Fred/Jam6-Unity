using GhostEnums;
using UnityEngine;

public class PrisonerBrain : MonoBehaviour
{
	[SerializeField]
	private string _name = "Kaepora";

	[SerializeField]
	private GhostNode.NodeLayer _nodeLayer = GhostNode.NodeLayer.Red;

	[Space]
	[SerializeField]
	private PrisonerEffects _effects;

	[SerializeField]
	private OWTriggerVolume _blockMovementVolume;

	[SerializeField]
	private OWTriggerVolume _allowMovementVolume;

	private GhostController _controller;

	private GhostData _data;

	private GhostSensors _sensors;

	private PrisonerBehavior _currentBehavior = PrisonerBehavior.Lurking;

	private PrisonerBehavior _pendingBehavior;

	private float _pendingBehaviorEntryTime;

	private Transform _behaviorCueMarker;

	private Transform _pendingBehaviorCueMarker;

	public OWEvent OnFinishEmergeBehavior = new OWEvent(1);

	public OWEvent OnFinishLightingBehavior = new OWEvent(1);

	public OWEvent OnFinishFetchTorchBehavior = new OWEvent(1);

	public OWEvent OnArriveAtElevatorDoor = new OWEvent(1);

	public OWEvent OnArriveAtElevatorPedestal = new OWEvent(1);

	public OWEvent OnFinishExitBehavior = new OWEvent(1);

	public string ghostName => _name;

	private Vector3 GetCueMarkerLocalPosition()
	{
		return _controller.GetNodeMap().WorldToLocalPosition(_behaviorCueMarker.position);
	}

	private Vector3 GetCueMarkerLocalDirection()
	{
		return _controller.GetNodeMap().WorldToLocalDirection(_behaviorCueMarker.forward);
	}

	private void Awake()
	{
		_controller = GetComponent<GhostController>();
		_sensors = GetComponent<GhostSensors>();
		_data = new GhostData();
		_controller.OnArriveAtPosition += new OWEvent.OWCallback(OnArriveAtPosition);
		_effects.OnTurn180Complete += new OWEvent.OWCallback(OnTurn180Complete);
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void Start()
	{
		base.enabled = false;
		_controller.GetDreamLanternController().enabled = false;
		_controller.Initialize(_nodeLayer, _effects);
		_sensors.Initialize(_data);
		_effects.Initialize(_controller.GetNodeRoot(), _controller, _data);
	}

	private void OnDestroy()
	{
		_sensors.RemoveEventListeners();
		_controller.OnArriveAtPosition -= new OWEvent.OWCallback(OnArriveAtPosition);
		_effects.OnTurn180Complete -= new OWEvent.OWCallback(OnTurn180Complete);
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void OnEnterDreamWorld()
	{
		base.enabled = true;
		_controller.GetDreamLanternController().enabled = true;
	}

	private void OnExitDreamWorld()
	{
		base.enabled = false;
		_controller.GetDreamLanternController().enabled = false;
		_data.OnPlayerExitDreamWorld();
	}

	private void FixedUpdate()
	{
		_controller.FixedUpdate_Controller();
		_sensors.FixedUpdate_Sensors();
		_data.FixedUpdate_Data(_controller, _sensors);
		if (_controller.IsMoving())
		{
			bool movementPaused = _blockMovementVolume.IsTrackingObject(Locator.GetPlayerDetector()) || !_allowMovementVolume.IsTrackingObject(Locator.GetPlayerDetector());
			_controller.SetMovementPaused(movementPaused);
		}
	}

	private void Update()
	{
		_controller.Update_Controller();
		_sensors.Update_Sensors();
		_effects.Update_Effects();
		if (_pendingBehavior != 0 && Time.time > _pendingBehaviorEntryTime)
		{
			ExitBehavior(_currentBehavior);
			PrisonerBehavior currentBehavior = _currentBehavior;
			_currentBehavior = _pendingBehavior;
			_behaviorCueMarker = _pendingBehaviorCueMarker;
			_pendingBehavior = PrisonerBehavior.None;
			_pendingBehaviorCueMarker = null;
			EnterBehavior(_currentBehavior, currentBehavior);
		}
	}

	private void ExitBehavior(PrisonerBehavior behavior)
	{
	}

	private void EnterBehavior(PrisonerBehavior behavior, PrisonerBehavior previousBehavior)
	{
		switch (behavior)
		{
		case PrisonerBehavior.Emerge:
			_effects.OnRevealAnimationComplete += new OWEvent.OWCallback(OnFinishEmergeAnimation);
			_effects.PlayRevealAnimation();
			break;
		case PrisonerBehavior.LightLamp:
			_effects.OnTurnOnLightsAnimationComplete += new OWEvent.OWCallback(OnFinishLightingAnimation);
			_effects.PlayTurnOnLightsAnimation();
			break;
		case PrisonerBehavior.FetchTorch:
			_controller.PathfindToLocalPosition(GetCueMarkerLocalPosition(), 2f);
			_controller.FaceVelocity();
			break;
		case PrisonerBehavior.ProjectVision:
			_controller.StopFacing();
			_effects.Play180TurnAnimation();
			break;
		case PrisonerBehavior.OfferTorch:
			_effects.PlayOfferTorchAnimation();
			break;
		case PrisonerBehavior.WaitForProjection:
			if (previousBehavior == PrisonerBehavior.OfferTorch)
			{
				_effects.PlayOfferTorchEndAnimation();
			}
			else
			{
				_effects.PlayDefaultAnimation();
			}
			break;
		case PrisonerBehavior.ExperienceVision:
			_effects.PlayExperienceVisionAnimation();
			break;
		case PrisonerBehavior.ExperienceEmotionalCatharsis:
			_effects.PlayReactToVisionAnimation();
			break;
		case PrisonerBehavior.MoveToElevatorDoor:
			_controller.MoveToLocalPosition(GetCueMarkerLocalPosition(), 2f);
			_controller.FaceVelocity();
			break;
		case PrisonerBehavior.WaitForTorchReturn:
			_effects.PlayWaitForTorchReturnAnimation();
			_controller.FaceLocalDirection(GetCueMarkerLocalDirection(), TurnSpeed.SLOWEST);
			break;
		case PrisonerBehavior.Farewell:
			_effects.OnFarewellTurnComplete += new OWEvent.OWCallback(OnFinishFarewellAnimation);
			_effects.PlayFarewellBowAnimation();
			_controller.StopMoving();
			_controller.StopFacing();
			break;
		case PrisonerBehavior.RideElevator:
			_controller.MoveToLocalPosition(GetCueMarkerLocalPosition(), 1f);
			_controller.FaceLocalDirection(GetCueMarkerLocalDirection(), TurnSpeed.MEDIUM);
			break;
		case PrisonerBehavior.Exit:
			_controller.MoveToLocalPosition(GetCueMarkerLocalPosition(), 2f);
			_controller.FaceVelocity();
			break;
		case PrisonerBehavior.WaitForConversation:
			break;
		}
	}

	private void OnArriveAtPosition()
	{
		switch (_currentBehavior)
		{
		case PrisonerBehavior.FetchTorch:
			_effects.OnPickUpTorchAnimationComplete += new OWEvent.OWCallback(OnFinishPickUpTorchAnimation);
			_controller.FaceLocalDirection(GetCueMarkerLocalDirection(), TurnSpeed.MEDIUM);
			_effects.PlayPickUpTorchAnimation();
			break;
		case PrisonerBehavior.ProjectVision:
			_effects.PlayProjectVisionAnimation();
			break;
		case PrisonerBehavior.MoveToElevatorDoor:
			_controller.StopFacing();
			_effects.Play180TurnAnimation();
			break;
		case PrisonerBehavior.Farewell:
			OnArriveAtElevatorPedestal.Invoke();
			break;
		case PrisonerBehavior.Exit:
			OnFinishExitBehavior.Invoke();
			break;
		}
	}

	private void OnTurn180Complete()
	{
		switch (_currentBehavior)
		{
		case PrisonerBehavior.ProjectVision:
			_controller.MoveToLocalPosition(GetCueMarkerLocalPosition(), 1f);
			_controller.FaceLocalDirection(GetCueMarkerLocalDirection(), TurnSpeed.SLOW);
			break;
		case PrisonerBehavior.MoveToElevatorDoor:
			OnArriveAtElevatorDoor.Invoke();
			break;
		}
	}

	private void OnFinishEmergeAnimation()
	{
		if (_currentBehavior == PrisonerBehavior.Emerge)
		{
			_effects.OnRevealAnimationComplete -= new OWEvent.OWCallback(OnFinishEmergeAnimation);
			OnFinishEmergeBehavior.Invoke();
		}
	}

	private void OnFinishLightingAnimation()
	{
		if (_currentBehavior == PrisonerBehavior.LightLamp)
		{
			_effects.OnTurnOnLightsAnimationComplete -= new OWEvent.OWCallback(OnFinishLightingAnimation);
			_controller.SetLanternConcealed(concealed: true);
			OnFinishLightingBehavior.Invoke();
		}
	}

	private void OnFinishPickUpTorchAnimation()
	{
		if (_currentBehavior == PrisonerBehavior.FetchTorch)
		{
			_effects.OnPickUpTorchAnimationComplete -= new OWEvent.OWCallback(OnFinishPickUpTorchAnimation);
			OnFinishFetchTorchBehavior.Invoke();
		}
	}

	private void OnFinishFarewellAnimation()
	{
		if (_currentBehavior == PrisonerBehavior.Farewell)
		{
			_effects.OnFarewellTurnComplete -= new OWEvent.OWCallback(OnFinishFarewellAnimation);
			_controller.MoveToLocalPosition(GetCueMarkerLocalPosition(), 2f);
			_controller.FaceVelocity();
		}
	}

	public void BeginBehavior(PrisonerBehavior behavior, float delay = 0f)
	{
		BeginBehavior(behavior, null, delay);
	}

	public void BeginBehavior(PrisonerBehavior behavior, Transform marker, float delay = 0f)
	{
		if (delay > 0f)
		{
			_pendingBehavior = behavior;
			_pendingBehaviorCueMarker = marker;
			_pendingBehaviorEntryTime = Time.time + delay;
			return;
		}
		ExitBehavior(_currentBehavior);
		PrisonerBehavior currentBehavior = _currentBehavior;
		_currentBehavior = behavior;
		_behaviorCueMarker = marker;
		_pendingBehavior = PrisonerBehavior.None;
		_pendingBehaviorCueMarker = null;
		EnterBehavior(_currentBehavior, currentBehavior);
	}
}
