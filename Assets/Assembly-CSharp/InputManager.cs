using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

public class InputManager : BaseInputManager, IInputManager
{
	private struct GamepadEnableData
	{
		public int userDisableNum;

		public int userEnableNum;

		public int systemAvailNum;

		public List<int> userIndexes;

		public List<int> systemIndexes;
	}

	private MonoBehaviour _owningBehavior;

	public InputCommandManager commandManager;

	public InputConsts.InputType CurrentInputType;

	public InputConsts.InputType LastCommandInputType;

	private Gamepad _currentGamepad;

	private float _inputFadeFraction = 1f;

	public InputUser ActiveUser;

	public Keyboard Keyboard => Keyboard.current;

	public Mouse Mouse => Mouse.current;

	public Gamepad OWCurrentGamepad => _currentGamepad;

	public event UpdateInputDeviceEvent OnUpdateInputDevice;

	public event UpdateInputModeEvent OnUpdateInputMode;

	public event UnpairedDeviceInput OnUnpairedDeviceInput;

	public event DeviceDisconnectedEvent OnUserDeviceDisconnected;

	public event DeviceReconnectedEvent OnUserDeviceReconnected;

	public event DevicesChangedEvent OnDevicesChanged;

	public event UpdateInputCommandsEvent OnUpdateInputCommands;

	public InputManager()
	{
		commandManager = new InputCommandManager();
	}

	public virtual void Awake(MonoBehaviour owningBehavior)
	{
		_owningBehavior = owningBehavior;
		Initialize();
		RefreshLastInputType();
		InputSystem.onDeviceChange += DevicesChanged;
		InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
	}

	public void LateInitialize()
	{
		RefreshLastInputType();
	}

	public bool UseButtonSouthAsConfirm()
	{
		return commandManager.UseButtonSouthAsConfirm();
	}

	public bool LoadDefaultInputActions()
	{
		return commandManager.LoadDefaultInputActions();
	}

	public void ModifyBindingsToDefaults(RebindableID[] ids)
	{
		commandManager.ModifyBindingsToDefaults(ids);
	}

	public bool IsBindingSameAsDefault(RebindableID idToCheck, bool gamepad)
	{
		return commandManager.IsBindingSameAsDefault(idToCheck, gamepad);
	}

	public void ModifyInputAssetDeviceList(InputDevice[] devices)
	{
		commandManager.ModifyInputAssetDeviceList(devices);
	}

	public string SerializeOverrides()
	{
		return commandManager.SerializeOverrides();
	}

	public void NotifyBindingChanged()
	{
		if (this.OnUpdateInputCommands != null)
		{
			this.OnUpdateInputCommands();
		}
	}

	public InputConsts.InputType GetLastInputType()
	{
		return LastCommandInputType;
	}

	public virtual void UpdateLastInputCommandType(InputAction.CallbackContext context)
	{
		if (!context.control.IsActuated(0.3f))
		{
			return;
		}
		InputConsts.InputType lastCommandInputType = LastCommandInputType;
		if (context.action.activeControl.device is Gamepad gamepad)
		{
			LastCommandInputType = InputConsts.InputType.GamePad;
			if (Gamepad.current != gamepad)
			{
				gamepad.MakeCurrent();
			}
			_currentGamepad = gamepad;
		}
		else if (context.action.activeControl.device is Keyboard)
		{
			LastCommandInputType = InputConsts.InputType.Keyboard;
		}
		else if (context.action.activeControl.device is Mouse)
		{
			LastCommandInputType = InputConsts.InputType.Mouse;
		}
		if (lastCommandInputType != LastCommandInputType && this.OnUpdateInputDevice != null)
		{
			this.OnUpdateInputDevice();
		}
	}

	protected virtual void DevicesChanged(InputDevice device, InputDeviceChange changeType)
	{
		switch (changeType)
		{
		case InputDeviceChange.Added:
			OnDeviceAdded(device);
			RefreshConnectedDevices();
			break;
		case InputDeviceChange.Removed:
		case InputDeviceChange.Disconnected:
		case InputDeviceChange.Reconnected:
			RefreshConnectedDevices();
			break;
		}
		RefreshLastInputType();
		RaiseDevicesChangedEvent(device, changeType);
	}

	protected virtual void RefreshConnectedDevices()
	{
	}

	protected virtual void OnDeviceAdded(InputDevice device)
	{
		if (device is Gamepad gamepad)
		{
			ProcessGamepadThroughWhitelist(gamepad);
		}
	}

	protected virtual void ProcessGamepadThroughWhitelist(Gamepad gamepad)
	{
		long[] enableDeviceWhitelistIds = InputUtil.GetEnableDeviceWhitelistIds();
		HID.HIDDeviceDescriptor hIDDeviceDescriptor = HID.HIDDeviceDescriptor.FromJson(gamepad.description.capabilities);
		long num = InputUtil.ConcatBitwiseId(hIDDeviceDescriptor.vendorId, hIDDeviceDescriptor.productId);
		if (InputUtil.IsXInputController(gamepad.name))
		{
			num = InputUtil.ConcatBitwiseId(1118, -1);
		}
		bool flag = false;
		for (int i = 0; i < enableDeviceWhitelistIds.Length; i++)
		{
			if (num == enableDeviceWhitelistIds[i])
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.Log("Unsupported controller found, disabling " + gamepad.displayName);
			InputSystem.DisableDevice(gamepad);
		}
	}

	public virtual void InitializeConnectedDevices(SettingsSave.UserDeviceInfo[] devicesToEnable)
	{
		Dictionary<long, GamepadEnableData> dictionary = new Dictionary<long, GamepadEnableData>();
		for (int i = 0; i < devicesToEnable.Length; i++)
		{
			long key = ((long)devicesToEnable[i].productId << 32) + devicesToEnable[i].vendorId;
			if (!dictionary.ContainsKey(key))
			{
				GamepadEnableData value = default(GamepadEnableData);
				value.userIndexes = new List<int>();
				value.systemIndexes = new List<int>();
				dictionary.Add(key, value);
			}
			GamepadEnableData value2 = dictionary[key];
			if (devicesToEnable[i].userEnabled)
			{
				value2.userEnableNum++;
			}
			else
			{
				value2.userDisableNum++;
			}
			value2.userIndexes.Add(i);
			dictionary[key] = value2;
		}
		ReadOnlyArray<Gamepad> gamepadList = OWInput.GetGamepadList();
		for (int j = 0; j < gamepadList.Count; j++)
		{
			Gamepad gamepad = gamepadList[j];
			HID.HIDDeviceDescriptor hIDDeviceDescriptor = HID.HIDDeviceDescriptor.FromJson(gamepad.description.capabilities);
			long key2 = InputUtil.ConcatBitwiseId(hIDDeviceDescriptor.vendorId, hIDDeviceDescriptor.productId);
			if (InputUtil.IsXInputController(gamepad.name))
			{
				key2 = InputUtil.ConcatBitwiseId(1118, -1);
			}
			if (!dictionary.ContainsKey(key2))
			{
				GamepadEnableData value3 = default(GamepadEnableData);
				value3.userIndexes = new List<int>();
				value3.systemIndexes = new List<int>();
				dictionary.Add(key2, value3);
			}
			GamepadEnableData value4 = dictionary[key2];
			value4.systemAvailNum++;
			value4.systemIndexes.Add(j);
			dictionary[key2] = value4;
			ProcessGamepadThroughWhitelist(gamepadList[j]);
		}
		foreach (KeyValuePair<long, GamepadEnableData> item in dictionary)
		{
			if (item.Value.userIndexes.Count != item.Value.systemIndexes.Count)
			{
				continue;
			}
			for (int k = 0; k < item.Value.userIndexes.Count; k++)
			{
				int num = item.Value.userIndexes[k];
				int index = item.Value.systemIndexes[k];
				Gamepad gamepad2 = gamepadList[index];
				if (devicesToEnable[num].userEnabled && !gamepad2.enabled)
				{
					InputSystem.EnableDevice(gamepad2);
				}
				else if (!devicesToEnable[num].userEnabled && gamepad2.enabled)
				{
					InputSystem.DisableDevice(gamepad2);
				}
			}
		}
	}

	private void RefreshLastInputType()
	{
		CurrentInputType = InputConsts.InputType.None;
		if (Keyboard != null)
		{
			CurrentInputType |= InputConsts.InputType.Keyboard;
		}
		if (Mouse != null)
		{
			CurrentInputType |= InputConsts.InputType.Mouse;
		}
		if (_currentGamepad != null)
		{
			CurrentInputType |= InputConsts.InputType.GamePad;
		}
		_ = CurrentInputType;
	}

	protected virtual void RaiseDevicesChangedEvent(InputDevice device, InputDeviceChange changeType)
	{
		if (this.OnDevicesChanged != null)
		{
			this.OnDevicesChanged(device, changeType);
		}
	}

	protected virtual void RaiseDeviceDisconnectedEvent()
	{
		if (this.OnUserDeviceDisconnected != null)
		{
			this.OnUserDeviceDisconnected();
		}
	}

	protected virtual void RaiseDeviceReconnectedEvent()
	{
		if (this.OnUserDeviceReconnected != null)
		{
			this.OnUserDeviceReconnected();
		}
	}

	public void EnableListeningToUnpairedDevices(bool enable)
	{
		if (enable)
		{
			InputUser.listenForUnpairedDeviceActivity++;
		}
		else if (InputUser.listenForUnpairedDeviceActivity > 0)
		{
			InputUser.listenForUnpairedDeviceActivity--;
		}
		else
		{
			Debug.LogWarning("No longer listening but requested to turn off listening to unpaired devices. Check you're not calling this more than necessary");
		}
	}

	public void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
	{
		if (this.OnUnpairedDeviceInput != null)
		{
			this.OnUnpairedDeviceInput(control, eventPtr);
		}
	}

	protected virtual void SetPairedDeviceExclusive(InputDevice exclusiveDevice)
	{
		bool flag = false;
		foreach (Gamepad item in Gamepad.all)
		{
			if (item == exclusiveDevice)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			Debug.LogError("Trying to set device exclusive but is not recognized by Unity InputSystem");
			return;
		}
		string text = "InputManager.SetPairedDeviceExclusive";
		foreach (Gamepad item2 in Gamepad.all)
		{
			text = ((item2 != exclusiveDevice) ? (text + "\nDisable Device deviceId:" + item2.deviceId) : (text + "\nEnable Paired Device deviceId:" + item2.deviceId));
		}
		Debug.Log(text);
		OWInput.SharedInputManager.ModifyInputAssetDeviceList(new InputDevice[1] { exclusiveDevice });
	}

	public ReadOnlyArray<Gamepad> GetGamepadList()
	{
		return Gamepad.all;
	}

	public bool IsGamepadEnabled()
	{
		return (CurrentInputType & InputConsts.InputType.GamePad) == InputConsts.InputType.GamePad;
	}

	public void SetGamePadConfig(InputUtil.GamePadPresetConfig configPreset, int controllerIndex)
	{
	}

	public virtual void OnDestroy()
	{
		InputSystem.onDeviceChange -= DevicesChanged;
		InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
		Shutdown();
	}

	public override void Update()
	{
		UpdateMouse();
		InputSystem.Update();
		commandManager.Update();
		if (BaseInputManager._currentInputMode != BaseInputManager._pendingInputMode)
		{
			BaseInputManager._currentInputMode = BaseInputManager._pendingInputMode;
			if (this.OnUpdateInputMode != null)
			{
				this.OnUpdateInputMode();
			}
		}
	}

	public string GetAnyRawAxisInput(float minimumInput, out float value)
	{
		value = 0f;
		return string.Empty;
	}

	public KeyCode GetNewlyPressedKeyCode()
	{
		KeyCode code = KeyCode.None;
		foreach (ButtonControl item in InputSystem.FindControls<ButtonControl>("*"))
		{
			if (item.wasPressedThisFrame)
			{
				bool flag = false;
				if (item.device is Gamepad)
				{
					flag = InputTransitionUtil.TryGetMouseButtonKeyCode(item.path, out code);
				}
				else if (item.device is Mouse)
				{
					flag = InputTransitionUtil.TryGetMouseButtonKeyCode(item.path, out code);
				}
				else if (item.device is Keyboard && item is KeyControl keyControl)
				{
					flag = InputTransitionUtil.TryGetKeyCode(keyControl.keyCode, out code);
				}
				if (flag)
				{
					break;
				}
			}
		}
		return code;
	}

	public KeyCode GetNewlyReleasedKeyCode()
	{
		KeyCode code = KeyCode.None;
		foreach (ButtonControl item in InputSystem.FindControls<ButtonControl>("*"))
		{
			if (item.wasReleasedThisFrame)
			{
				bool flag = false;
				if (item.device is Gamepad)
				{
					flag = InputTransitionUtil.TryGetMouseButtonKeyCode(item.path, out code);
				}
				else if (item.device is Mouse)
				{
					flag = InputTransitionUtil.TryGetMouseButtonKeyCode(item.path, out code);
				}
				else if (item.device is Keyboard && item is KeyControl keyControl)
				{
					flag = InputTransitionUtil.TryGetKeyCode(keyControl.keyCode, out code);
				}
				if (flag)
				{
					break;
				}
			}
		}
		return code;
	}

	public virtual bool UsingGamepad()
	{
		return (LastCommandInputType & InputConsts.InputType.GamePad) == InputConsts.InputType.GamePad;
	}

	public Vector2 GetAxisValue(IInputCommands command, InputMode mask = InputMode.All)
	{
		if (IsInputMode(mask))
		{
			return command.GetAxisValue() * _inputFadeFraction;
		}
		return Vector2.zero;
	}

	public float GetValue(IInputCommands command, InputMode mask = InputMode.All)
	{
		if (IsInputMode(mask))
		{
			return command.GetValue() * _inputFadeFraction;
		}
		return 0f;
	}

	public bool IsPressed(IInputCommands command, float minPressDuration, InputMode mask = InputMode.All)
	{
		if (IsInputMode(mask))
		{
			return command.IsPressed(minPressDuration);
		}
		return false;
	}

	public bool IsNewlyPressed(IInputCommands command, InputMode mask = InputMode.All)
	{
		if (IsInputMode(mask))
		{
			return command.IsNewlyPressed();
		}
		return false;
	}

	public bool IsNewlyReleased(IInputCommands command, InputMode mask = InputMode.All)
	{
		if (IsInputMode(mask))
		{
			return command.IsNewlyReleased();
		}
		return false;
	}

	public virtual void Rumble(float hiPower, float lowPower)
	{
		if (_currentGamepad != null)
		{
			_currentGamepad.SetMotorSpeeds(lowPower, hiPower);
		}
	}

	public bool GetAnyJoystickButtonPressed()
	{
		if (_currentGamepad != null)
		{
			for (int i = 0; i < _currentGamepad.allControls.Count; i++)
			{
				InputControl inputControl = _currentGamepad.allControls[i];
				if (inputControl is ButtonControl && inputControl.IsPressed() && !inputControl.synthetic)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void ChangeInputMode(InputMode mode)
	{
		base.ChangeInputMode(mode);
		commandManager.ConsumeAllInputCommands();
	}

	public override void RestorePreviousInputs()
	{
		base.RestorePreviousInputs();
		commandManager.ConsumeAllInputCommands();
	}

	protected IEnumerator FadeInput(float start, float target, float fadeDuration, float delayBeforeFade = 0f, bool isStartOfTimeLoop = false)
	{
		_inputFadeFraction = start;
		if (delayBeforeFade <= 0f)
		{
			yield return null;
		}
		else
		{
			yield return new WaitForSeconds(delayBeforeFade);
		}
		if (isStartOfTimeLoop)
		{
			ChangeInputMode(InputMode.Character);
			GlobalMessenger.FireEvent("TakeFirstFlashbackSnapshot");
		}
		target = ((target > 1f) ? 1f : target);
		float fadeStartTime = Time.time;
		float percentToTarget;
		do
		{
			percentToTarget = (Time.time - fadeStartTime) / fadeDuration;
			_inputFadeFraction = Mathf.Lerp(start, target, percentToTarget);
			yield return null;
		}
		while (percentToTarget <= 1f);
		_inputFadeFraction = 1f;
	}

	public void ChangeControllersEnabled(List<int> gamepadIdsToUpdate)
	{
		commandManager.EnableAllInputCommandActions(enable: false);
		ReadOnlyArray<Gamepad> gamepadList = OWInput.GetGamepadList();
		for (int i = 0; i < gamepadList.Count; i++)
		{
			int deviceId = gamepadList[i].deviceId;
			if (!gamepadIdsToUpdate.Contains(deviceId))
			{
				continue;
			}
			foreach (Gamepad item in Gamepad.all)
			{
				if (item.deviceId == deviceId)
				{
					if (item.enabled)
					{
						InputSystem.DisableDevice(item);
					}
					else
					{
						InputSystem.EnableDevice(item);
					}
				}
			}
		}
		commandManager.EnableAllInputCommandActions(enable: true);
	}

	protected override void OnStartOfTimeLoop(int loopCount)
	{
		if (_owningBehavior != null && LoadManager.GetCurrentScene() == OWScene.SolarSystem)
		{
			_owningBehavior.StartCoroutine(FadeInput(0f, 1f, 0.5f, 2f, isStartOfTimeLoop: true));
		}
		else
		{
			ChangeInputMode(InputMode.Character);
		}
	}

	protected override void OnResumeSimulation()
	{
		ChangeInputMode(InputMode.Character);
		if (!(_owningBehavior == null))
		{
			_owningBehavior.StartCoroutine(FadeInput(0f, 1f, 0.5f));
		}
	}

	protected override void OnStopSleepingAtCampfire()
	{
		if (_owningBehavior != null)
		{
			_owningBehavior.StartCoroutine(FadeInput(0f, 1f, 0.5f));
		}
	}

	protected override void OnExitDreamWorld()
	{
		if (_owningBehavior != null)
		{
			_owningBehavior.StartCoroutine(FadeInput(0f, 1f, 0.5f, 0.5f));
		}
	}
}
