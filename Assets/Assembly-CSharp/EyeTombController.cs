using UnityEngine;

public class EyeTombController : MonoBehaviour
{
	[Header("Stage")]
	[SerializeField]
	private ObserveTrigger _graveObserveTrigger;

	[SerializeField]
	private GameObject _stageRoot;

	[SerializeField]
	private OWTriggerVolume _stageVolume;

	[SerializeField]
	private OWLightController _candleController;

	[Header("Interface")]
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private GearInterfaceEffects _gearEffects;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private Transform _lockOnTransform;

	[Header("Projection")]
	[SerializeField]
	private OWLightController _planetLightController;

	[SerializeField]
	private OWRendererFadeController _lightBeamController;

	[SerializeField]
	private GameObject _planetObject;

	[SerializeField]
	private GameObject _stateRoot;

	[SerializeField]
	private GameObject[] _states;

	[Header("Signal")]
	[SerializeField]
	private AudioSignal _buriedSignal;

	[SerializeField]
	private Transform _signalDeepSocket;

	[Header("Instrument")]
	[SerializeField]
	private QuantumInstrument _instrument;

	[SerializeField]
	private GameObject _finalTombState;

	[SerializeField]
	private Transform _returnSocket;

	private ScreenPrompt _forwardPrompt;

	private ScreenPrompt _reversePrompt;

	private ScreenPrompt _leavePrompt;

	private bool _lit;

	private int _stateIndex;

	private bool _hasMovedSignalDeeper;

	[ContextMenu("Populate States Array From Root")]
	private void PopulateStatesFromRoot()
	{
		_states = new GameObject[_stateRoot.transform.childCount];
		for (int i = 0; i < _states.Length; i++)
		{
			_states[i] = _stateRoot.transform.GetChild(i).gameObject;
		}
	}

	private void Awake()
	{
		_graveObserveTrigger.OnGainFocus += new OWEvent.OWCallback(OnObserveGrave);
		_stageVolume.OnEntry += OnEnterStage;
		_stageVolume.OnExit += OnExitStage;
		_interactReceiver.OnPressInteract += OnPressInteract;
		_instrument.OnFinishGather += OnFinishGather;
	}

	private void Start()
	{
		base.enabled = false;
		_stageRoot.SetActive(value: false);
		_finalTombState.SetActive(value: false);
		_interactReceiver.SetPromptText(UITextType.RotateGearPrompt);
		_planetObject.SetActive(value: false);
		_planetLightController.SetIntensity(0f);
		_lightBeamController.SetFade(0f);
		_candleController.SetIntensity(0f);
		_forwardPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.SlideProjectorForwardPrompt) + "   <CMD>");
		_reversePrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, UITextLibrary.GetString(UITextType.SlideProjectorReversePrompt) + "   <CMD>");
		_leavePrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.LeavePrompt) + "   <CMD>");
	}

	private void OnDestroy()
	{
		_graveObserveTrigger.OnGainFocus -= new OWEvent.OWCallback(OnObserveGrave);
		_stageVolume.OnEntry -= OnEnterStage;
		_stageVolume.OnExit -= OnExitStage;
		_interactReceiver.OnPressInteract -= OnPressInteract;
		_instrument.OnFinishGather -= OnFinishGather;
	}

	private void OnObserveGrave()
	{
		_stageRoot.SetActive(value: true);
		_graveObserveTrigger.OnGainFocus -= new OWEvent.OWCallback(OnObserveGrave);
	}

	private void OnPressInteract()
	{
		base.enabled = true;
		if (!_hasMovedSignalDeeper)
		{
			_hasMovedSignalDeeper = true;
			_buriedSignal.transform.position = _signalDeepSocket.position;
		}
		Locator.GetToolModeSwapper().UnequipTool();
		Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(_lockOnTransform, Vector3.zero);
		Locator.GetPromptManager().AddScreenPrompt(_forwardPrompt, PromptPosition.UpperRight, makeVisible: true);
		Locator.GetPromptManager().AddScreenPrompt(_reversePrompt, PromptPosition.UpperRight, makeVisible: true);
		Locator.GetPromptManager().AddScreenPrompt(_leavePrompt, PromptPosition.UpperRight, makeVisible: true);
		GlobalMessenger.FireEvent("EnterSatelliteCameraMode");
		GlobalMessenger.FireEvent("StartViewingProjector");
	}

	private void CancelInteraction()
	{
		base.enabled = false;
		Locator.GetPromptManager().RemoveScreenPrompt(_forwardPrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_reversePrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_leavePrompt);
		Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
		_interactReceiver.ResetInteraction();
		GlobalMessenger.FireEvent("ExitSatelliteCameraMode");
		GlobalMessenger.FireEvent("EndViewingProjector");
	}

	private void Update()
	{
		if (OWInput.IsInputMode(InputMode.SatelliteCam))
		{
			if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
			{
				ToggleLitState(1);
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary))
			{
				ToggleLitState(-1);
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.cancel))
			{
				CancelInteraction();
			}
		}
	}

	private void ToggleLitState(int direction)
	{
		int num = _stateIndex + direction;
		if (_lit && (num < 0 || num > _states.Length - 1))
		{
			_gearEffects.PlayFailure(direction > 0);
			return;
		}
		_lit = !_lit;
		_planetLightController.SetIntensity(_lit ? 1f : 0f);
		_planetObject.SetActive(_lit);
		_lightBeamController.SetFade(_lit ? 1f : 0f);
		if (!_lit)
		{
			_states[_stateIndex].SetActive(value: false);
			_stateIndex += direction;
			_states[_stateIndex].SetActive(value: true);
		}
		_gearEffects.AddRotation((float)direction * 45f, 0f);
		_oneShotSource.PlayOneShot(((float)direction > 0f) ? AudioType.Projector_Next : AudioType.Projector_Prev);
	}

	private void OnEnterStage(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_candleController.FadeTo(1f, 1f);
		}
	}

	private void OnExitStage(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_candleController.FadeTo(0f, 1f);
		}
	}

	private void OnFinishGather()
	{
		_stageRoot.SetActive(value: false);
		_stateRoot.SetActive(value: false);
		_finalTombState.SetActive(value: true);
		_planetObject.SetActive(value: false);
		_planetLightController.SetIntensity(0f);
		_lightBeamController.SetFade(0f);
		Locator.GetPlayerBody().WarpToPositionRotation(_returnSocket.position, _returnSocket.rotation);
		Locator.GetPlayerBody().SetVelocity(Vector3.zero);
	}
}
