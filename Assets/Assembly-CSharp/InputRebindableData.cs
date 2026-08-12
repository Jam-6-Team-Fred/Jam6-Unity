using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class InputRebindableData
{
	public struct ValidationResult
	{
		public bool notifyPlayerOfChanges;

		public int previousVersion;
	}

	public const int c_deviceNotInitialized = -1;

	public const int c_disconnectAllDevices = -2;

	public const int s_totalRebindables = 36;

	public const int s_version = 4;

	public int version;

	public InputRebindable[] rebindableList;

	public InputUtil.GamePadPresetConfig gamepadConfig;

	public InputUtil.ButtonPromptPresetConfig promptConfig;

	public string lastUsedDeviceName;

	public int lastUsedDeviceIndex;

	[NonSerialized]
	public ValidationResult validationResult;

	[OnDeserializing]
	private void SetDefaultValuesOnDeserializing(StreamingContext context)
	{
		gamepadConfig = InputUtil.GamePadPresetConfig.NONE;
		promptConfig = InputUtil.ButtonPromptPresetConfig.NONE;
		lastUsedDeviceName = "";
		lastUsedDeviceIndex = -1;
	}

	[OnDeserialized]
	private void SetDefaultValuesOnDeserialized(StreamingContext context)
	{
		if (version < 3)
		{
			InputRebindable[] array = new InputRebindable[36];
			for (int i = 0; i < rebindableList.Length; i++)
			{
				array[i] = rebindableList[i];
			}
			rebindableList = array;
		}
		if (gamepadConfig < InputUtil.GamePadPresetConfig.XBOX || gamepadConfig > InputUtil.GamePadPresetConfig.NONE)
		{
			Debug.Log("Invalid Gamepad Config, resetting to NONE");
			gamepadConfig = InputUtil.GamePadPresetConfig.NONE;
		}
		if (promptConfig < InputUtil.ButtonPromptPresetConfig.XBOX || gamepadConfig > InputUtil.GamePadPresetConfig.NONE)
		{
			Debug.Log("Invalid Prompt Config, resetting to NONE");
			promptConfig = InputUtil.ButtonPromptPresetConfig.NONE;
		}
	}

	public InputRebindableData(bool init = false)
	{
		version = 4;
		gamepadConfig = InputUtil.GamePadPresetConfig.NONE;
		promptConfig = InputUtil.ButtonPromptPresetConfig.NONE;
		lastUsedDeviceName = "";
		lastUsedDeviceIndex = -1;
	}

	public InputRebindableData(InputRebindable[] list)
		: this()
	{
		rebindableList = list;
	}

	public void UpdateRebindableList(InputRebindable[] list)
	{
		rebindableList = list;
	}

	public void InitializeControllerAndPromptSettings()
	{
		if (gamepadConfig == InputUtil.GamePadPresetConfig.NONE)
		{
			gamepadConfig = PlayerData.GetGamepadConfig();
		}
	}

	public ValidationResult InitializeInputSettings()
	{
		gamepadConfig = InputUtil.GamePadPresetConfig.XBOX;
		promptConfig = InputUtil.ButtonPromptPresetConfig.XBOX;
		validationResult = new ValidationResult
		{
			notifyPlayerOfChanges = false,
			previousVersion = version
		};
		version = 4;
		if (false)
		{
			OWInput.SetGamePadConfig(gamepadConfig, lastUsedDeviceIndex);
		}
		return validationResult;
	}

	public void ApplyUpdatedInputSettings(int activeGamepad)
	{
		OWInput.SetGamePadConfig(gamepadConfig, activeGamepad);
	}
}
