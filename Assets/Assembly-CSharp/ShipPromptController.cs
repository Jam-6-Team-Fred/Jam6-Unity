using UnityEngine;

public class ShipPromptController : MonoBehaviour, ILateInitializer
{
	private bool _screenPromptsInitialized;

	private ScreenPrompt _matchVelocityPrompt;

	private ScreenPrompt _autopilotPrompt;

	private ScreenPrompt _abortAutopilotPrompt;

	private ScreenPrompt _liftoffCamera;

	private ScreenPrompt _landingModePrompt;

	private ScreenPrompt _exitLandingCamPrompt;

	private ScreenPrompt _liftoffPrompt;

	private ScreenPrompt _horizontalThrustPrompt;

	private ScreenPrompt _verticalThrustPrompt;

	private ScreenPrompt _downThrustPrompt;

	private ScreenPrompt _rollPrompt;

	private ScreenPrompt _freeLookPrompt;

	private LandingPadManager _landingPadManager;

	private ShipCockpitController _flightConsole;

	private ShipThrusterController _shipThrustController;

	private Autopilot _shipAutopilot;

	private bool _shipDestroyed;

	private void Awake()
	{
		_screenPromptsInitialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		_liftoffPrompt = new ScreenPrompt(InputLibrary.thrustUp, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.ShipLiftoffPrompt));
		_matchVelocityPrompt = new ScreenPrompt(InputLibrary.matchVelocity, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.MatchVelocityPrompt));
		_autopilotPrompt = new ScreenPrompt(InputLibrary.autopilot, "<CMD>   " + UITextLibrary.GetString(UITextType.ShipAutopilotPrompt));
		_abortAutopilotPrompt = new ScreenPrompt(InputLibrary.autopilot, "<CMD>   " + UITextLibrary.GetString(UITextType.ShipAbortAutopilotPrompt));
		_liftoffCamera = new ScreenPrompt(InputLibrary.landingCamera, "<CMD>   " + UITextLibrary.GetString(UITextType.ShipLiftoffLandingPrompt));
		_landingModePrompt = new ScreenPrompt(InputLibrary.landingCamera, "<CMD>   " + UITextLibrary.GetString(UITextType.ShipLandingPrompt));
		if (JetpackPromptController.SPLIT_VERTICAL)
		{
			_verticalThrustPrompt = new ScreenPrompt(InputLibrary.thrustUp, "<CMD>   " + UITextLibrary.GetString(UITextType.UpPrompt));
			_downThrustPrompt = new ScreenPrompt(InputLibrary.thrustDown, "<CMD>   " + UITextLibrary.GetString(UITextType.DownPrompt));
		}
		else
		{
			_verticalThrustPrompt = new ScreenPrompt(InputLibrary.thrustDown, InputLibrary.thrustUp, UITextLibrary.GetString(UITextType.DownUpPrompt), ScreenPrompt.MultiCommandType.POS_NEG);
		}
		_horizontalThrustPrompt = new ScreenPrompt(InputLibrary.moveXZ, "<CMD>   " + UITextLibrary.GetString(UITextType.HorizontalPrompt));
		_exitLandingCamPrompt = new ScreenPrompt(InputLibrary.landingCamera, "<CMD>   " + UITextLibrary.GetString(UITextType.ShipCockpitPrompt));
		_rollPrompt = new ScreenPrompt(InputLibrary.rollMode, InputLibrary.look, "<CMD1>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "  +<CMD2>   " + UITextLibrary.GetString(UITextType.RollPrompt), ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
		_freeLookPrompt = new ScreenPrompt(InputLibrary.freeLook, InputLibrary.look, "<CMD1>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "  +<CMD2>   " + UITextLibrary.GetString(UITextType.FreeLookPrompt), ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
		_landingPadManager = base.gameObject.GetRequiredComponentInChildren<LandingPadManager>();
		_flightConsole = base.gameObject.GetRequiredComponentInChildren<ShipCockpitController>();
		_shipAutopilot = base.gameObject.GetRequiredComponentInChildren<Autopilot>();
		_shipThrustController = base.gameObject.GetRequiredComponentInChildren<ShipThrusterController>();
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
		_shipDestroyed = false;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (!_screenPromptsInitialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
	}

	public void LateInitialize()
	{
		_screenPromptsInitialized = true;
		Locator.GetPromptManager().AddScreenPrompt(_exitLandingCamPrompt, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_horizontalThrustPrompt, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_verticalThrustPrompt, PromptPosition.UpperLeft);
		if (JetpackPromptController.SPLIT_VERTICAL)
		{
			Locator.GetPromptManager().AddScreenPrompt(_downThrustPrompt, PromptPosition.UpperLeft);
		}
		Locator.GetPromptManager().AddScreenPrompt(_liftoffCamera, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_liftoffPrompt, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_autopilotPrompt, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_abortAutopilotPrompt, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_matchVelocityPrompt, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_landingModePrompt, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_rollPrompt, PromptPosition.UpperLeft);
		Locator.GetPromptManager().AddScreenPrompt(_freeLookPrompt, PromptPosition.UpperLeft);
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		base.enabled = true;
	}

	private void OnExitFlightConsole()
	{
		base.enabled = false;
		HideAllPrompts();
	}

	private void OnShipSystemFailure()
	{
		_shipDestroyed = true;
	}

	private void HideAllPrompts()
	{
		_liftoffPrompt.SetVisibility(isVisible: false);
		_verticalThrustPrompt.SetVisibility(isVisible: false);
		if (JetpackPromptController.SPLIT_VERTICAL)
		{
			_downThrustPrompt.SetVisibility(isVisible: false);
		}
		_horizontalThrustPrompt.SetVisibility(isVisible: false);
		_liftoffCamera.SetVisibility(isVisible: false);
		_matchVelocityPrompt.SetVisibility(isVisible: false);
		_autopilotPrompt.SetVisibility(isVisible: false);
		_abortAutopilotPrompt.SetVisibility(isVisible: false);
		_landingModePrompt.SetVisibility(isVisible: false);
		_exitLandingCamPrompt.SetVisibility(isVisible: false);
		_rollPrompt.SetVisibility(isVisible: false);
		_freeLookPrompt.SetVisibility(isVisible: false);
	}

	private void Update()
	{
		HideAllPrompts();
		if (!OWInput.IsInputMode(InputMode.ShipCockpit | InputMode.LandingCam))
		{
			return;
		}
		if (_flightConsole.UsingLandingCam())
		{
			_exitLandingCamPrompt.SetVisibility(isVisible: true);
			if (_shipDestroyed)
			{
				return;
			}
			if (_flightConsole.InLandingMode())
			{
				_horizontalThrustPrompt.SetVisibility(isVisible: true);
				_verticalThrustPrompt.SetVisibility(isVisible: true);
				if (JetpackPromptController.SPLIT_VERTICAL)
				{
					_downThrustPrompt.SetVisibility(isVisible: true);
				}
			}
			else
			{
				_liftoffPrompt.SetVisibility(_shipThrustController.RequiresIgnition());
			}
			return;
		}
		if (_landingPadManager.IsLanded())
		{
			if (!OWInput.IsPressed(InputLibrary.freeLook) && !_shipDestroyed)
			{
				_liftoffCamera.SetVisibility(isVisible: true);
				_liftoffPrompt.SetVisibility(_shipThrustController.RequiresIgnition());
			}
			return;
		}
		if (_shipAutopilot.IsFlyingToDestination())
		{
			_abortAutopilotPrompt.SetVisibility(isVisible: true);
			return;
		}
		bool flag = !_shipDestroyed;
		if (_flightConsole.CheckLandingModePromptConditions())
		{
			_landingModePrompt.SetVisibility(isVisible: true);
			flag = false;
		}
		if (_flightConsole.IsAutopilotAvailable())
		{
			_autopilotPrompt.SetVisibility(isVisible: true);
			flag = false;
		}
		if (!_shipAutopilot.IsMatchingVelocity() && _flightConsole.IsMatchVelocityAvailable(!PlayerState.InBrambleDimension()) && _flightConsole.GetLocalVelocity().magnitude > 10f)
		{
			_matchVelocityPrompt.SetVisibility(isVisible: true);
			flag = false;
		}
		_rollPrompt.SetVisibility(flag && TimeLoop.GetLoopCount() > 1);
		_freeLookPrompt.SetVisibility(flag && TimeLoop.GetLoopCount() > 1);
		_horizontalThrustPrompt.SetVisibility(flag && TimeLoop.GetLoopCount() == 1);
		_verticalThrustPrompt.SetVisibility(flag && TimeLoop.GetLoopCount() == 1);
		if (JetpackPromptController.SPLIT_VERTICAL)
		{
			_downThrustPrompt.SetVisibility(flag && TimeLoop.GetLoopCount() == 1);
		}
	}
}
