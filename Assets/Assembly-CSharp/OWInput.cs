using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class OWInput : MonoBehaviour
{
	private static IInputManager inputManagerInstance = new InputManager();

	private static bool isReady = false;

	public const float DEFAULT_TAP_WINDOW = 0.3f;

	public static IInputManager SharedInputManager
	{
		get
		{
			if (!isReady && inputManagerInstance is InputManager inputManager)
			{
				isReady = inputManager.commandManager.LoadDefaultInputActions();
			}
			return inputManagerInstance;
		}
	}

	private void Awake()
	{
		InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
		InputSystem.settings.SetInternalFeatureFlag("DISABLE_SHORTCUT_SUPPORT", enabled: true);
		SharedInputManager.Awake(this);
	}

	public static ReadOnlyArray<Gamepad> GetGamepadList()
	{
		return SharedInputManager.GetGamepadList();
	}

	public static bool UseButtonSouthAsConfirm()
	{
		return SharedInputManager.UseButtonSouthAsConfirm();
	}

	public static void ModifyInputAssetDeviceList(InputDevice[] devices)
	{
		SharedInputManager.ModifyInputAssetDeviceList(devices);
	}

	public static bool IsGamepadEnabled()
	{
		return SharedInputManager.IsGamepadEnabled();
	}

	public static void SetGamePadConfig(InputUtil.GamePadPresetConfig configPreset, int controllerIndex)
	{
		SharedInputManager.SetGamePadConfig(configPreset, controllerIndex);
	}

	public static void InitializeConnectedDevices(SettingsSave.UserDeviceInfo[] devicesToEnable)
	{
		SharedInputManager.InitializeConnectedDevices(devicesToEnable);
	}

	private void OnDestroy()
	{
		SharedInputManager.OnDestroy();
	}

	private void Update()
	{
		SharedInputManager.Update();
	}

	public static string GetAnyRawAxisInput(float minimumInput, out float value)
	{
		return SharedInputManager.GetAnyRawAxisInput(minimumInput, out value);
	}

	public static KeyCode GetNewlyPressedKeyCode()
	{
		return SharedInputManager.GetNewlyPressedKeyCode();
	}

	public static KeyCode GetNewlyReleasedKeyCode()
	{
		return SharedInputManager.GetNewlyReleasedKeyCode();
	}

	public static void RestorePreviousInputs()
	{
		SharedInputManager.RestorePreviousInputs();
	}

	public static bool UsingGamepad()
	{
		return SharedInputManager.UsingGamepad();
	}

	public static InputMode GetInputMode()
	{
		return SharedInputManager.GetInputMode();
	}

	public static InputMode[] GetInputModeStack()
	{
		return SharedInputManager.GetInputModeStack();
	}

	public static void ChangeInputMode(InputMode mode)
	{
		SharedInputManager.ChangeInputMode(mode);
	}

	public static void UpdateInversion()
	{
		SharedInputManager.UpdateInversion();
	}

	public static bool IsInputMode(InputMode mask)
	{
		return SharedInputManager.IsInputMode(mask);
	}

	public static bool IsChangePending()
	{
		return SharedInputManager.IsChangePending();
	}

	public static void ModifyBindingsToDefaults(RebindableID[] ids)
	{
		SharedInputManager.ModifyBindingsToDefaults(ids);
	}

	public static bool IsBindingSameAsDefault(RebindableID idToCheck, bool gamepad)
	{
		return SharedInputManager.IsBindingSameAsDefault(idToCheck, gamepad);
	}

	public static void NotifyBindingChanged()
	{
		SharedInputManager.NotifyBindingChanged();
	}

	public static Vector2 GetAxisValue(IInputCommands command, InputMode mask = InputMode.All)
	{
		return SharedInputManager.GetAxisValue(command, mask);
	}

	public static float GetValue(IInputCommands command, InputMode mask = InputMode.All)
	{
		return SharedInputManager.GetValue(command, mask);
	}

	public static bool IsPressed(IInputCommands command, InputMode mask, float minPressDuration = 0f)
	{
		return SharedInputManager.IsPressed(command, minPressDuration, mask);
	}

	public static bool IsPressed(IInputCommands command, float minPressDuration = 0f)
	{
		return SharedInputManager.IsPressed(command, minPressDuration);
	}

	public static bool IsNewlyPressed(IInputCommands command, InputMode mask = InputMode.All)
	{
		return SharedInputManager.IsNewlyPressed(command, mask);
	}

	public static bool IsNewlyReleased(IInputCommands command, InputMode mask = InputMode.All)
	{
		return SharedInputManager.IsNewlyReleased(command, mask);
	}

	public static void Rumble(float hiPower, float lowPower)
	{
		SharedInputManager.Rumble(hiPower, lowPower);
	}

	public static void ChangeControllersEnabled(List<int> gamepadIdsToUpdate)
	{
		SharedInputManager.ChangeControllersEnabled(gamepadIdsToUpdate);
	}

	public static bool GetAnyJoystickButtonPressed()
	{
		return SharedInputManager.GetAnyJoystickButtonPressed();
	}
}
