using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public interface IInputManager
{
	event UpdateInputDeviceEvent OnUpdateInputDevice;

	event UpdateInputModeEvent OnUpdateInputMode;

	event UnpairedDeviceInput OnUnpairedDeviceInput;

	event DeviceDisconnectedEvent OnUserDeviceDisconnected;

	event DeviceReconnectedEvent OnUserDeviceReconnected;

	event DevicesChangedEvent OnDevicesChanged;

	event UpdateInputCommandsEvent OnUpdateInputCommands;

	void Awake(MonoBehaviour owningBehavior);

	void LateInitialize();

	void OnDestroy();

	bool LoadDefaultInputActions();

	bool UseButtonSouthAsConfirm();

	void ModifyBindingsToDefaults(RebindableID[] ids);

	bool IsBindingSameAsDefault(RebindableID idToCheck, bool gamepad);

	void ModifyInputAssetDeviceList(InputDevice[] devices);

	string SerializeOverrides();

	void UpdateLastInputCommandType(InputAction.CallbackContext context);

	void NotifyBindingChanged();

	ReadOnlyArray<Gamepad> GetGamepadList();

	bool IsGamepadEnabled();

	void EnableListeningToUnpairedDevices(bool enable);

	void InitializeConnectedDevices(SettingsSave.UserDeviceInfo[] enabledList);

	void SetGamePadConfig(InputUtil.GamePadPresetConfig configPreset, int controllerIndex);

	void Update();

	string GetAnyRawAxisInput(float minimumInput, out float value);

	KeyCode GetNewlyPressedKeyCode();

	KeyCode GetNewlyReleasedKeyCode();

	bool UsingGamepad();

	InputMode GetInputMode();

	bool IsInputMode(InputMode mask);

	InputMode[] GetInputModeStack();

	void RestorePreviousInputs();

	void ChangeInputMode(InputMode mode);

	bool IsChangePending();

	void UpdateInversion();

	float GetValue(IInputCommands command, InputMode mask = InputMode.All);

	Vector2 GetAxisValue(IInputCommands command, InputMode mask = InputMode.All);

	bool IsNewlyPressed(IInputCommands command, InputMode mask = InputMode.All);

	bool IsPressed(IInputCommands command, float minPressDuration, InputMode mask = InputMode.All);

	bool IsNewlyReleased(IInputCommands command, InputMode mask = InputMode.All);

	void Rumble(float hiPower, float lowPower);

	void ChangeControllersEnabled(List<int> gamepadIdsToUpdate);

	bool GetAnyJoystickButtonPressed();
}
