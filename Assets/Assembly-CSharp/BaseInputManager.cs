using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseInputManager
{
	protected static InputMode _currentInputMode;

	protected static InputMode _pendingInputMode;

	protected static InputMode[] _inputModeLimitedStack;

	private const int c_inputModeLimitedStackLength = 8;

	public static Vector2 MouseDelta;

	protected void Initialize()
	{
		_inputModeLimitedStack = new InputMode[8];
		for (int i = 0; i < _inputModeLimitedStack.Length; i++)
		{
			_inputModeLimitedStack[i] = InputMode.None;
		}
		_currentInputMode = InputMode.None;
		_pendingInputMode = InputMode.None;
		AddListeners();
	}

	protected void Shutdown()
	{
		RemoveListeners();
	}

	public virtual void Update()
	{
		_currentInputMode = _pendingInputMode;
	}

	protected void UpdateMouse()
	{
		MouseDelta = ((Mouse.current != null) ? Mouse.current.delta.ReadValue() : new Vector2(0f, 0f));
	}

	protected abstract void OnStartOfTimeLoop(int loopCount);

	protected abstract void OnResumeSimulation();

	protected abstract void OnStopSleepingAtCampfire();

	protected abstract void OnExitDreamWorld();

	public void UpdateInversion()
	{
		if (_pendingInputMode == InputMode.ShipCockpit || _pendingInputMode == InputMode.ModelShip)
		{
			InputLibrary.pitch.InversionFactor = PlayerData.GetShipLookInversionFactor();
			InputLibrary.look.InversionFactor = PlayerData.GetLookInversionFactor();
			InputLibrary.freeLook.InversionFactor = PlayerData.GetLookInversionFactor();
		}
		else
		{
			InputLibrary.pitch.InversionFactor = PlayerData.GetLookInversionFactor();
			InputLibrary.look.InversionFactor = PlayerData.GetLookInversionFactor();
			InputLibrary.freeLook.InversionFactor = PlayerData.GetLookInversionFactor();
		}
	}

	public virtual void RestorePreviousInputs()
	{
		for (int i = 0; i < _inputModeLimitedStack.Length - 1; i++)
		{
			_inputModeLimitedStack[i] = _inputModeLimitedStack[i + 1];
		}
		_inputModeLimitedStack[_inputModeLimitedStack.Length - 1] = InputMode.None;
		_pendingInputMode = _inputModeLimitedStack[0];
		if (PlayerData.IsLoaded())
		{
			UpdateInversion();
		}
	}

	public virtual void ChangeInputMode(InputMode mode)
	{
		for (int num = _inputModeLimitedStack.Length - 1; num > 0; num--)
		{
			_inputModeLimitedStack[num] = _inputModeLimitedStack[num - 1];
		}
		_inputModeLimitedStack[0] = mode;
		_pendingInputMode = mode;
		if (PlayerData.IsLoaded())
		{
			UpdateInversion();
		}
	}

	public bool IsInputMode(InputMode mask)
	{
		if (mask != 0)
		{
			return (mask & _currentInputMode) == _currentInputMode;
		}
		return true;
	}

	public bool IsChangePending()
	{
		return _pendingInputMode != _currentInputMode;
	}

	public InputMode GetInputMode()
	{
		return _currentInputMode;
	}

	public InputMode[] GetInputModeStack()
	{
		return _inputModeLimitedStack;
	}

	private void OnEnterSatelliteCameraMode()
	{
		ChangeInputMode(InputMode.SatelliteCam);
	}

	private void OnExitSatelliteCameraMode()
	{
		RestorePreviousInputs();
	}

	private void OnEnterLandingView()
	{
		ChangeInputMode(InputMode.LandingCam);
	}

	private void OnExitLandingView()
	{
		RestorePreviousInputs();
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		ChangeInputMode(InputMode.None);
	}

	private void OnEnterShipComputer()
	{
		ChangeInputMode(InputMode.ShipComputer);
	}

	private void OnExitShipComputer()
	{
		RestorePreviousInputs();
	}

	private void OnEnterKeyboardInputMode()
	{
		ChangeInputMode(InputMode.KeyboardInput);
	}

	private void OnExitKeyboardInputMode()
	{
		RestorePreviousInputs();
	}

	private void OnEnterDialogueMode()
	{
		ChangeInputMode(InputMode.Dialogue);
	}

	private void OnExitDialogueMode()
	{
		RestorePreviousInputs();
	}

	private void OnEnterMapView()
	{
		ChangeInputMode(InputMode.Map);
	}

	private void OnExitMapView()
	{
		RestorePreviousInputs();
	}

	private void OnEnterRemoteFlightConsole(OWRigidbody modelShipBody)
	{
		ChangeInputMode(InputMode.ModelShip);
	}

	private void OnExitRemoteFlightConsole()
	{
		ChangeInputMode(InputMode.Character);
	}

	private void OnEnterSignalscopeZoom(Signalscope telescope)
	{
		ChangeInputMode(InputMode.ScopeZoom);
	}

	private void OnExitSignalscopeZoom()
	{
		RestorePreviousInputs();
	}

	private void OnEnterRoastingMode(Campfire campfire)
	{
		ChangeInputMode(InputMode.Roasting);
	}

	private void OnExitRoastingMode()
	{
		ChangeInputMode(InputMode.Character);
	}

	private void OnEnterNomaiRemoteCamera()
	{
		ChangeInputMode(InputMode.NomaiRemoteCam);
	}

	private void OnExitNomaiRemoteCamera()
	{
		RestorePreviousInputs();
	}

	private void AddListeners()
	{
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger.AddListener("ResumeSimulation", OnResumeSimulation);
		GlobalMessenger.AddListener("StopSleepingAtCampfire", OnStopSleepingAtCampfire);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.AddListener("EnterLandingView", OnEnterLandingView);
		GlobalMessenger.AddListener("ExitLandingView", OnExitLandingView);
		GlobalMessenger.AddListener("EnterShipComputer", OnEnterShipComputer);
		GlobalMessenger.AddListener("ExitShipComputer", OnExitShipComputer);
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		GlobalMessenger<OWRigidbody>.AddListener("EnterRemoteFlightConsole", OnEnterRemoteFlightConsole);
		GlobalMessenger.AddListener("ExitRemoteFlightConsole", OnExitRemoteFlightConsole);
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.AddListener("EnterConversation", OnEnterDialogueMode);
		GlobalMessenger.AddListener("ExitConversation", OnExitDialogueMode);
		GlobalMessenger.AddListener("EnterSatelliteCameraMode", OnEnterSatelliteCameraMode);
		GlobalMessenger.AddListener("ExitSatelliteCameraMode", OnExitSatelliteCameraMode);
		GlobalMessenger<Campfire>.AddListener("EnterRoastingMode", OnEnterRoastingMode);
		GlobalMessenger.AddListener("ExitRoastingMode", OnExitRoastingMode);
		GlobalMessenger.AddListener("EnterNomaiRemoteCamera", OnEnterNomaiRemoteCamera);
		GlobalMessenger.AddListener("ExitNomaiRemoteCamera", OnExitNomaiRemoteCamera);
		GlobalMessenger.AddListener("EnterKeyboardInputMode", OnEnterKeyboardInputMode);
		GlobalMessenger.AddListener("ExitKeyboardInputMode", OnExitKeyboardInputMode);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
	}

	private void RemoveListeners()
	{
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger.RemoveListener("ResumeSimulation", OnResumeSimulation);
		GlobalMessenger.RemoveListener("StopSleepingAtCampfire", OnStopSleepingAtCampfire);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.RemoveListener("EnterLandingView", OnEnterLandingView);
		GlobalMessenger.RemoveListener("ExitLandingView", OnExitLandingView);
		GlobalMessenger.RemoveListener("EnterShipComputer", OnEnterShipComputer);
		GlobalMessenger.RemoveListener("ExitShipComputer", OnExitShipComputer);
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterRemoteFlightConsole", OnEnterRemoteFlightConsole);
		GlobalMessenger.RemoveListener("ExitRemoteFlightConsole", OnExitRemoteFlightConsole);
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.RemoveListener("EnterConversation", OnEnterDialogueMode);
		GlobalMessenger.RemoveListener("ExitConversation", OnExitDialogueMode);
		GlobalMessenger.RemoveListener("EnterSatelliteCameraMode", OnEnterSatelliteCameraMode);
		GlobalMessenger.RemoveListener("ExitSatelliteCameraMode", OnExitSatelliteCameraMode);
		GlobalMessenger<Campfire>.RemoveListener("EnterRoastingMode", OnEnterRoastingMode);
		GlobalMessenger.RemoveListener("ExitRoastingMode", OnExitRoastingMode);
		GlobalMessenger.RemoveListener("EnterNomaiRemoteCamera", OnEnterNomaiRemoteCamera);
		GlobalMessenger.RemoveListener("ExitNomaiRemoteCamera", OnExitNomaiRemoteCamera);
		GlobalMessenger.RemoveListener("EnterKeyboardInputMode", OnEnterKeyboardInputMode);
		GlobalMessenger.RemoveListener("ExitKeyboardInputMode", OnExitKeyboardInputMode);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
	}
}
