using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputCommandManager
{
	private InputActionAsset _defaultInputActionAsset;

	private static Dictionary<InputConsts.InputCommandType, IInputCommands> DefaultMappedInputActions = new Dictionary<InputConsts.InputCommandType, IInputCommands>();

	private InputActionAsset _inputActionAsset;

	public static Dictionary<InputConsts.InputCommandType, IInputCommands> MappedInputActions = new Dictionary<InputConsts.InputCommandType, IInputCommands>();

	public static Dictionary<RebindableID, IRebindableInputAction> RebindableInputActionsMap = new Dictionary<RebindableID, IRebindableInputAction>();

	private const string c_primaryActionBindingPath = "<Gamepad>/{PrimaryAction}";

	private const string c_backActionBindingPath = "<Gamepad>/{Back}";

	private const string c_gamepadButtonSouthBindingPath = "<Gamepad>/buttonSouth";

	private const string c_gamepadButtonEastBindingPath = "<Gamepad>/buttonEast";

	private const string c_bugDualShockTouchpadBinding = "<DualShockGamepad>/touchpadButton";

	private const string c_bugGamepadCancelBindingSouth = "<Keyboard>/buttonSouth";

	private const string c_bugGamepadCancelBindingEast = "<Keyboard>/buttonEast";

	private static string s_primaryActionPlatformOverride = null;

	private static string s_backActionPlatformOverride = null;

	public InputActionAsset DefaultInputActions => _defaultInputActionAsset;

	public InputActionAsset InputActions => _inputActionAsset;

	private static string PrimaryActionOverride
	{
		get
		{
			if (s_primaryActionPlatformOverride == null)
			{
				InitializePlatformActionOverrides();
			}
			return s_primaryActionPlatformOverride;
		}
	}

	private static string BackActionOverride
	{
		get
		{
			if (s_backActionPlatformOverride == null)
			{
				InitializePlatformActionOverrides();
			}
			return s_backActionPlatformOverride;
		}
	}

	public event InputCommandsInitializedEvent OnInputCommandsInitialized;

	private static void InitializePlatformActionOverrides()
	{
		s_primaryActionPlatformOverride = "<Gamepad>/buttonSouth";
		s_backActionPlatformOverride = "<Gamepad>/buttonEast";
	}

	public bool UseButtonSouthAsConfirm()
	{
		if (PrimaryActionOverride == "<Gamepad>/buttonSouth")
		{
			return true;
		}
		return false;
	}

	private bool TryLoadActions(in InputActionAsset inputAsset, out Dictionary<InputConsts.InputCommandType, IInputCommands> inputCommandsMap)
	{
		inputCommandsMap = null;
		if (inputAsset == null)
		{
			return false;
		}
		if (inputCommandsMap == null)
		{
			inputCommandsMap = new Dictionary<InputConsts.InputCommandType, IInputCommands>();
		}
		Debug.Log("Loading Input Actions!");
		IReadOnlyCollection<InputCommandDefinitions.InputCommandData> commandData = InputCommandDefinitions.CommandData;
		bool flag = true;
		RebindableInputActionsMap.Clear();
		foreach (InputCommandDefinitions.InputCommandData item in commandData)
		{
			if (!inputCommandsMap.TryGetValue(item.CommandType, out var value))
			{
				if (InputCommandUtils.TryCreateInputCommands(item, inputAsset, out value))
				{
					inputCommandsMap.Add(item.CommandType, value);
					continue;
				}
				if (InputCommandUtils.TryCreateMissingInputCommand(item, inputAsset, DefaultInputActions, out value))
				{
					inputCommandsMap.Add(item.CommandType, value);
					continue;
				}
				Debug.LogError("Failed to create InputCommand for " + item.CommandType);
				flag = false;
			}
		}
		return flag && TestForSetDefaultsConflictBug(inputCommandsMap);
	}

	public bool LoadActions(InputActionAsset inputActionAsset)
	{
		if (inputActionAsset == null)
		{
			Debug.LogError("InputActionAsset is null - cannot load input data");
			return false;
		}
		if (_inputActionAsset == inputActionAsset)
		{
			return true;
		}
		if (_inputActionAsset != null)
		{
			_inputActionAsset.Disable();
		}
		inputActionAsset.Disable();
		if (TryLoadActions(in inputActionAsset, out var inputCommandsMap))
		{
			UnityEngine.Object.Destroy(_inputActionAsset);
			_inputActionAsset = inputActionAsset;
			MappedInputActions = inputCommandsMap;
			_inputActionAsset.Enable();
			InputLibrary.menuRight.PressedThreshold = 0.8f;
			InputLibrary.menuLeft.PressedThreshold = 0.8f;
			if (this.OnInputCommandsInitialized != null)
			{
				this.OnInputCommandsInitialized();
			}
			return true;
		}
		inputActionAsset.Disable();
		UnityEngine.Object.Destroy(inputActionAsset);
		return false;
	}

	public bool LoadActions(string json)
	{
		bool result = false;
		try
		{
			InputActionAsset inputActionAsset = InputActionAsset.FromJson(json);
			result = LoadActions(inputActionAsset);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return result;
	}

	public bool LoadDefaultInputActions()
	{
		if (_defaultInputActionAsset == null)
		{
			InputActionAsset inputActionAsset = Resources.Load<InputActionAsset>("OWInputCommands");
			if (inputActionAsset == null)
			{
				Debug.LogError("Unable to load Default InputActionAsset (OWInputCommands) from Disk.");
				return false;
			}
			inputActionAsset.Disable();
			_defaultInputActionAsset = UnityEngine.Object.Instantiate(inputActionAsset);
			_defaultInputActionAsset.name = "Default OWInputActions";
			Resources.UnloadAsset(inputActionAsset);
			if (!TryLoadActions(in _defaultInputActionAsset, out var inputCommandsMap))
			{
				Resources.UnloadAsset(inputActionAsset);
				return false;
			}
			DefaultMappedInputActions = inputCommandsMap;
			_defaultInputActionAsset.Disable();
			Resources.UnloadAsset(inputActionAsset);
		}
		InputActionAsset inputActionAsset2 = UnityEngine.Object.Instantiate(_defaultInputActionAsset);
		return LoadActions(inputActionAsset2);
	}

	public string SerializeOverrides()
	{
		return _inputActionAsset.ToJson();
	}

	public void Update()
	{
		foreach (IInputCommands value in MappedInputActions.Values)
		{
			value.Update();
		}
	}

	public void ModifyBindingsToDefaults(RebindableID[] ids)
	{
		for (int i = 0; i < ids.Length; i++)
		{
			if (!InputCommandDefinitions.TryGetInputCommandData(ids[i], out var data))
			{
				continue;
			}
			InputConsts.InputCommandType commandType = data.CommandType;
			if (!MappedInputActions.TryGetValue(commandType, out var value))
			{
				continue;
			}
			if (!value.IsRebindable)
			{
				Debug.LogError(value.CommandType.ToString() + " is non-bindable!");
			}
			else if (value is InputCommands inputCommands && inputCommands.Action is RebindableInputAction rebindableInputAction)
			{
				InputAction action = _defaultInputActionAsset.FindAction(data.Primary.ActionName1);
				rebindableInputAction.UpdateFromAction(action);
			}
			else if (value is InputAxisCommands inputAxisCommands && inputAxisCommands.Action is IRebindableInputAction)
			{
				if (inputAxisCommands.Action is RebindableInputActionPair rebindableInputActionPair)
				{
					InputAction primary = _defaultInputActionAsset.FindAction(data.Primary.ActionName1);
					InputAction secondary = _defaultInputActionAsset.FindAction(data.Primary.ActionName2);
					rebindableInputActionPair.UpdateFromAction(primary, secondary);
				}
				else if (inputAxisCommands.Action is RebindableAxisInputAction rebindableAxisInputAction)
				{
					InputAction action2 = _defaultInputActionAsset.FindAction(data.Primary.ActionName1);
					rebindableAxisInputAction.UpdateFromAction(action2);
				}
			}
			else if (value is CompositeInputCommands compositeInputCommands && compositeInputCommands.PrimaryAction is RebindableInputActionPair rebindableInputActionPair2 && compositeInputCommands.SecondaryAction is RebindableInputActionPair rebindableInputActionPair3)
			{
				InputAction primary2 = _defaultInputActionAsset.FindAction(data.Primary.ActionName1);
				InputAction secondary2 = _defaultInputActionAsset.FindAction(data.Primary.ActionName2);
				rebindableInputActionPair2.UpdateFromAction(primary2, secondary2);
				InputAction primary3 = _defaultInputActionAsset.FindAction(data.Secondary.ActionName1);
				InputAction secondary3 = _defaultInputActionAsset.FindAction(data.Secondary.ActionName2);
				rebindableInputActionPair3.UpdateFromAction(primary3, secondary3);
			}
		}
	}

	public bool IsBindingSameAsDefault(RebindableID idToCheck, bool gamepad)
	{
		InputConsts.InputCommandType key = RebindableLookup.SharedInstance.LookupInputCommandType(idToCheck);
		string text = "";
		string text2 = "";
		if (MappedInputActions.TryGetValue(key, out var value) && DefaultMappedInputActions.TryGetValue(key, out var value2))
		{
			if (!(value is ISingleInputCommand singleInputCommand) || !(value2 is ISingleInputCommand singleInputCommand2))
			{
				return false;
			}
			if (!singleInputCommand.TryCastAction<ISingleAction>(out var castAction) || !singleInputCommand2.TryCastAction<ISingleAction>(out var castAction2))
			{
				return false;
			}
			InputAction action = castAction.Action;
			InputAction action2 = castAction2.Action;
			for (int i = 0; i < action2.bindings.Count; i++)
			{
				if (gamepad && action2.bindings[i].effectivePath.StartsWith("<Gamepad>"))
				{
					if (action2.bindings[i].isPartOfComposite)
					{
						Debug.LogError("InputCommandManager.IsBindingSameAsDefault cannot handle composite bindings at this time");
						return false;
					}
					text = action2.bindings[i].effectivePath;
					break;
				}
				if (!gamepad && (action2.bindings[i].effectivePath.StartsWith("<Keyboard>") || action2.bindings[i].effectivePath.StartsWith("<Mouse>")))
				{
					if (action2.bindings[i].isPartOfComposite)
					{
						Debug.LogError("InputCommandManager.IsBindingSameAsDefault cannot handle composite bindings at this time");
						return false;
					}
					text = action2.bindings[i].effectivePath;
					break;
				}
			}
			for (int j = 0; j < action.bindings.Count; j++)
			{
				if (gamepad && action.bindings[j].effectivePath.StartsWith("<Gamepad>"))
				{
					text2 = action.bindings[j].effectivePath;
					break;
				}
				if (!gamepad && (action.bindings[j].effectivePath.StartsWith("<Keyboard>") || action.bindings[j].effectivePath.StartsWith("<Mouse>")))
				{
					text2 = action.bindings[j].effectivePath;
					break;
				}
			}
		}
		if (text2 == "")
		{
			return false;
		}
		return text2 == text;
	}

	public void ModifyInputAssetDeviceList(InputDevice[] devices)
	{
		if (_inputActionAsset == null)
		{
			Debug.Log("InputCommandManager.ModifyInputAssetDeviceList null _inputActionAsset");
			return;
		}
		_inputActionAsset.Disable();
		_inputActionAsset.devices = new ReadOnlyArray<InputDevice>(devices);
		_inputActionAsset.Enable();
	}

	public void ConsumeAllInputCommands()
	{
		foreach (IInputCommands value in MappedInputActions.Values)
		{
			value.ConsumeInput();
		}
	}

	public void EnableAllInputCommandActions(bool enable)
	{
		foreach (IInputCommands value in MappedInputActions.Values)
		{
			value.EnableAllActions(enable);
		}
	}

	public static bool SanitizeActionForPlatform(InputAction action)
	{
		bool result = false;
		int count = action.bindings.Count;
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < count; i++)
		{
			if (action.bindings[i].path == "<Gamepad>/{PrimaryAction}")
			{
				action.ChangeBinding(i).WithPath(PrimaryActionOverride);
				result = true;
			}
			if (action.bindings[i].path == "<Gamepad>/{Back}")
			{
				action.ChangeBinding(i).WithPath(BackActionOverride);
				result = true;
			}
			if (action.bindings[i].path == "<DualShockGamepad>/touchpadButton")
			{
				num = i;
			}
			if (action.bindings[i].path == "<Keyboard>/buttonEast")
			{
				num2 = i;
			}
			if (action.bindings[i].path == "<Keyboard>/buttonSouth")
			{
				num3 = i;
			}
		}
		if (num != -1)
		{
			Debug.Log("SanitizeActionForPlatform, remove binding <DualShockGamepad>/touchpadButton");
			action.ChangeBinding(num).Erase();
			result = true;
		}
		if (num2 != -1)
		{
			Debug.Log("SanitizeActionForPlatform, fix binding <Keyboard>/buttonEast");
			action.ChangeBinding(num2).WithPath("<Gamepad>/buttonSouth");
			result = true;
		}
		if (num3 != -1)
		{
			Debug.Log("SanitizeActionForPlatform, fix binding <Keyboard>/buttonSouth");
			action.ChangeBinding(num3).WithPath("<Gamepad>/buttonEast");
			result = true;
		}
		return result;
	}

	private static bool TestForSetDefaultsConflictBug(Dictionary<InputConsts.InputCommandType, IInputCommands> commandsMap)
	{
		IInputCommands value = null;
		IInputCommands value2 = null;
		IInputCommands value3 = null;
		if (commandsMap.TryGetValue(InputConsts.InputCommandType.CANCEL, out value) && commandsMap.TryGetValue(InputConsts.InputCommandType.MENU_CONFIRM, out value2))
		{
			commandsMap.TryGetValue(InputConsts.InputCommandType.SET_DEFAULTS, out value3);
		}
		else
			_ = 0;
		IInputCommands[] array = new IInputCommands[3] { value, value2, value3 };
		RebindableAxisInputAction[] array2 = new RebindableAxisInputAction[3];
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] is ISingleInputCommand singleInputCommand))
			{
				Debug.LogError("Invalid Binding found for Confirm/Cancel/Defaults");
				return false;
			}
			if (!singleInputCommand.TryCastAction<RebindableAxisInputAction>(out var castAction))
			{
				Debug.LogError("Invalid Action found for Confirm/Cancel/Defaults");
				return false;
			}
			array2[i] = castAction;
		}
		string[] array3 = new string[3];
		string controlScheme = InputConsts.GetControlScheme(usingGamepad: true);
		string text = "";
		for (int j = 0; j < array3.Length; j++)
		{
			int bindingIndex = array2[j].Action.GetBindingIndex(UnityEngine.InputSystem.InputBinding.MaskByGroup(controlScheme));
			array3[j] = array2[j].Action.bindings[bindingIndex].effectivePath;
			if (j == 2)
			{
				_ = array2[j];
				text = array3[j];
			}
		}
		if (text != array3[0] && text == array3[1])
		{
			return false;
		}
		controlScheme = InputConsts.GetControlScheme(usingGamepad: false);
		for (int k = 0; k < array3.Length; k++)
		{
			int bindingIndex2 = array2[k].Action.GetBindingIndex(UnityEngine.InputSystem.InputBinding.MaskByGroup(controlScheme));
			array3[k] = array2[k].Action.bindings[bindingIndex2].effectivePath;
			if (k == 2)
			{
				_ = array2[k];
				text = array3[k];
			}
		}
		if (text != array3[0] && text == array3[1])
		{
			return false;
		}
		return true;
	}
}
