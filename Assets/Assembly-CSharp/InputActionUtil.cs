using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class InputActionUtil
{
	public static UnityEngine.InputSystem.InputBinding GamepadBindingMask = UnityEngine.InputSystem.InputBinding.MaskByGroup(InputConsts.GetControlScheme(usingGamepad: true));

	public static UnityEngine.InputSystem.InputBinding DesktopBindingMask = UnityEngine.InputSystem.InputBinding.MaskByGroup(InputConsts.GetControlScheme(usingGamepad: false));

	private static InputControlList<InputControl> s_ctrlList1;

	private static InputControlList<InputControl> s_ctrlList2;

	private static InputControlList<InputControl> s_ctrlList3;

	private static InputControlList<InputControl> s_ctrlList4;

	private const string c_trigger = "Trigger";

	public static bool ExtractInputActions(IInputAction action, out InputAction first, out InputAction second)
	{
		first = null;
		second = null;
		if (action is BasicInputAction basicInputAction)
		{
			first = basicInputAction.Action;
		}
		else if (action is RebindableAxisInputAction rebindableAxisInputAction)
		{
			first = rebindableAxisInputAction.Action;
		}
		else if (action is RebindableInputActionPair rebindableInputActionPair)
		{
			first = rebindableInputActionPair.PrimaryAction;
			second = rebindableInputActionPair.SecondaryAction;
		}
		if (first == null)
		{
			return second != null;
		}
		return true;
	}

	public static bool ExtractInputActions(IInputCommands commands, out IInputAction primary, out IInputAction secondary)
	{
		primary = null;
		secondary = null;
		if (commands is InputCommands inputCommands)
		{
			primary = inputCommands.Action;
		}
		else if (commands is InputAxisCommands inputAxisCommands)
		{
			primary = inputAxisCommands.Action;
		}
		else
		{
			if (!(commands is CompositeInputCommands compositeInputCommands))
			{
				return false;
			}
			primary = compositeInputCommands.PrimaryAction;
			secondary = compositeInputCommands.SecondaryAction;
		}
		return true;
	}

	public static bool UsingSameBinding(IInputAction firstAction, IInputAction secondAction, bool gamepad)
	{
		if (firstAction == null || secondAction == null)
		{
			return false;
		}
		return firstAction.HasSameBinding(secondAction, gamepad);
	}

	public static bool UsingSameBinding(InputAction firstAction, InputAction secondAction, bool gamepad)
	{
		UnityEngine.InputSystem.InputBinding bindingMask = (gamepad ? GamepadBindingMask : DesktopBindingMask);
		int bindingIndex = firstAction.GetBindingIndex(bindingMask);
		int bindingIndex2 = secondAction.GetBindingIndex(bindingMask);
		if (bindingIndex == -1 || bindingIndex2 == -1)
		{
			return false;
		}
		return firstAction.bindings[bindingIndex].effectivePath == secondAction.bindings[bindingIndex2].effectivePath;
	}

	public static bool ControlPathIsGamepadTrigger(InputControl control)
	{
		bool result = false;
		if (control.path.EndsWith("Trigger", StringComparison.Ordinal))
		{
			result = true;
		}
		return result;
	}

	public static bool TryGetAxisID(InputAction action, bool gamepad, out AxisIdentifier axis)
	{
		axis = AxisIdentifier.NONE;
		UnityEngine.InputSystem.InputBinding bindingMask = (gamepad ? GamepadBindingMask : DesktopBindingMask);
		int bindingIndex = action.GetBindingIndex(bindingMask);
		if (bindingIndex == -1)
		{
			return false;
		}
		action.GetBindingDisplayString(bindingIndex, out var _, out var controlPath, UnityEngine.InputSystem.InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
		if (string.IsNullOrEmpty(controlPath))
		{
			return false;
		}
		return InputTransitionUtil.TryGetAxisIdentifier(controlPath, out axis);
	}

	public static bool TryGetSharedAxisID(InputAction first, InputAction second, bool gamepad, out AxisIdentifier axis)
	{
		axis = AxisIdentifier.NONE;
		UnityEngine.InputSystem.InputBinding bindingMask = (gamepad ? GamepadBindingMask : DesktopBindingMask);
		int bindingIndex = first.GetBindingIndex(bindingMask);
		int bindingIndex2 = second.GetBindingIndex(bindingMask);
		if (bindingIndex == -1 || bindingIndex2 == -1)
		{
			return false;
		}
		UnityEngine.InputSystem.InputBinding inputBinding = first.bindings[bindingIndex];
		UnityEngine.InputSystem.InputBinding inputBinding2 = second.bindings[bindingIndex2];
		if (inputBinding.effectivePath == inputBinding2.effectivePath)
		{
			inputBinding.ToDisplayString(out var _, out var controlPath, UnityEngine.InputSystem.InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
			return InputTransitionUtil.TryGetAxisIdentifier(controlPath, out axis);
		}
		InputControl inputControl = InputSystem.FindControl(inputBinding.effectivePath) as AxisControl;
		InputControl inputControl2 = InputSystem.FindControl(inputBinding2.effectivePath) as AxisControl;
		if (inputControl == null || inputControl2 == null)
		{
			if (gamepad)
			{
				return InputTransitionUtil.TryGetAxisIdentifier(inputBinding.effectivePath, inputBinding2.effectivePath, out axis);
			}
			return false;
		}
		if (inputControl is DiscreteButtonControl || inputControl2 is DiscreteButtonControl)
		{
			return false;
		}
		if (inputControl.parent == null || inputControl2.parent == null)
		{
			return false;
		}
		if (inputControl.parent != inputControl2.parent)
		{
			return false;
		}
		if (InputTransitionUtil.TryGetAxisIdentifier(inputBinding.effectivePath, inputBinding2.effectivePath, out axis))
		{
			return true;
		}
		return InputTransitionUtil.TryGetAxisIdentifier(inputControl.parent.name, out axis);
	}

	public static bool TryGetSharedAxisID(InputAction action1, InputAction action2, InputAction action3, InputAction action4, bool gamepad, out AxisIdentifier axis)
	{
		axis = AxisIdentifier.NONE;
		UnityEngine.InputSystem.InputBinding bindingMask = (gamepad ? GamepadBindingMask : DesktopBindingMask);
		int bindingIndex = action1.GetBindingIndex(bindingMask);
		int bindingIndex2 = action2.GetBindingIndex(bindingMask);
		int bindingIndex3 = action3.GetBindingIndex(bindingMask);
		int bindingIndex4 = action4.GetBindingIndex(bindingMask);
		if (bindingIndex == -1 || bindingIndex2 == -1 || bindingIndex3 == -1 || bindingIndex4 == -1)
		{
			return false;
		}
		AxisControl axisControl = InputSystem.FindControl(action1.bindings[bindingIndex].effectivePath) as AxisControl;
		AxisControl axisControl2 = InputSystem.FindControl(action2.bindings[bindingIndex2].effectivePath) as AxisControl;
		AxisControl axisControl3 = InputSystem.FindControl(action3.bindings[bindingIndex3].effectivePath) as AxisControl;
		AxisControl axisControl4 = InputSystem.FindControl(action4.bindings[bindingIndex4].effectivePath) as AxisControl;
		if (axisControl == null || axisControl2 == null || axisControl3 == null || axisControl4 == null)
		{
			return false;
		}
		if (axisControl.parent == null || axisControl2.parent == null || axisControl3.parent == null || axisControl4.parent == null)
		{
			return false;
		}
		if (axisControl.parent != axisControl2.parent || axisControl2.parent != axisControl3.parent || axisControl3.parent != axisControl4.parent)
		{
			return false;
		}
		return InputTransitionUtil.TryGetAxisIdentifier(axisControl.parent.name, out axis);
	}

	public static bool TryGetSharedControl(InputAction action1, InputAction action2, InputAction action3, InputAction action4, bool gamepad, out InputControl control)
	{
		control = null;
		UnityEngine.InputSystem.InputBinding bindingMask = (gamepad ? GamepadBindingMask : DesktopBindingMask);
		int bindingIndex = action1.GetBindingIndex(bindingMask);
		int bindingIndex2 = action2.GetBindingIndex(bindingMask);
		int bindingIndex3 = action3.GetBindingIndex(bindingMask);
		int bindingIndex4 = action4.GetBindingIndex(bindingMask);
		if (bindingIndex == -1 || bindingIndex2 == -1 || bindingIndex3 == -1 || bindingIndex4 == -1)
		{
			return false;
		}
		s_ctrlList1.Clear();
		s_ctrlList2.Clear();
		s_ctrlList3.Clear();
		s_ctrlList4.Clear();
		int num = InputSystem.FindControls(action1.bindings[bindingIndex].effectivePath, ref s_ctrlList1);
		int num2 = InputSystem.FindControls(action2.bindings[bindingIndex2].effectivePath, ref s_ctrlList2);
		int num3 = InputSystem.FindControls(action3.bindings[bindingIndex3].effectivePath, ref s_ctrlList3);
		int num4 = InputSystem.FindControls(action4.bindings[bindingIndex4].effectivePath, ref s_ctrlList4);
		if (num == 0 || num2 == 0 || num3 == 0 || num4 == 0)
		{
			return false;
		}
		InputControl foundControl = s_ctrlList1[0];
		InputControl foundControl2 = s_ctrlList2[0];
		InputControl foundControl3 = s_ctrlList3[0];
		InputControl foundControl4 = s_ctrlList4[0];
		if (s_ctrlList1.Count == 1 || s_ctrlList2.Count == 1 || s_ctrlList3.Count == 1 || s_ctrlList4.Count == 1)
		{
			if (foundControl == null || foundControl2 == null || foundControl3 == null || foundControl4 == null)
			{
				return false;
			}
			if (foundControl.parent == null || foundControl2.parent == null || foundControl3.parent == null || foundControl4.parent == null)
			{
				return false;
			}
			if (foundControl.parent != foundControl2.parent || foundControl2.parent != foundControl3.parent || foundControl3.parent != foundControl4.parent)
			{
				return false;
			}
			control = foundControl.parent;
			return true;
		}
		if (gamepad)
		{
			Gamepad gamepad2 = ((InputManager)OWInput.SharedInputManager).OWCurrentGamepad;
			if (gamepad2 == null)
			{
				gamepad2 = Gamepad.current;
			}
			string name = gamepad2.name;
			TryFindInputControlWithGamepadName(s_ctrlList1, name, out foundControl);
			TryFindInputControlWithGamepadName(s_ctrlList2, name, out foundControl2);
			TryFindInputControlWithGamepadName(s_ctrlList3, name, out foundControl3);
			TryFindInputControlWithGamepadName(s_ctrlList4, name, out foundControl4);
		}
		if (foundControl == null || foundControl2 == null || foundControl3 == null || foundControl4 == null)
		{
			return false;
		}
		if (foundControl.parent == null || foundControl2.parent == null || foundControl3.parent == null || foundControl4.parent == null)
		{
			return false;
		}
		if (foundControl.parent != foundControl2.parent || foundControl2.parent != foundControl3.parent || foundControl3.parent != foundControl4.parent)
		{
			return false;
		}
		control = foundControl.parent;
		return true;
	}

	public static bool TryGetSharedControl(InputAction action1, InputAction action2, bool gamepad, out InputControl control)
	{
		control = null;
		UnityEngine.InputSystem.InputBinding bindingMask = (gamepad ? GamepadBindingMask : DesktopBindingMask);
		int bindingIndex = action1.GetBindingIndex(bindingMask);
		int bindingIndex2 = action2.GetBindingIndex(bindingMask);
		if (bindingIndex == -1 || bindingIndex2 == -1)
		{
			return false;
		}
		s_ctrlList1.Clear();
		s_ctrlList2.Clear();
		int num = InputSystem.FindControls(action1.bindings[bindingIndex].effectivePath, ref s_ctrlList1);
		int num2 = InputSystem.FindControls(action2.bindings[bindingIndex2].effectivePath, ref s_ctrlList2);
		if (num == 0 || num2 == 0)
		{
			return false;
		}
		InputControl foundControl = s_ctrlList1[0];
		InputControl foundControl2 = s_ctrlList2[0];
		if (s_ctrlList1.Count == 1 || s_ctrlList2.Count == 1)
		{
			if (foundControl == null || foundControl2 == null)
			{
				return false;
			}
			if (foundControl.parent == null || foundControl2.parent == null)
			{
				return false;
			}
			if (foundControl.parent != foundControl2.parent)
			{
				return false;
			}
			control = foundControl.parent;
			return true;
		}
		if (gamepad)
		{
			Gamepad gamepad2 = ((InputManager)OWInput.SharedInputManager).OWCurrentGamepad;
			if (gamepad2 == null)
			{
				gamepad2 = Gamepad.current;
			}
			string name = gamepad2.name;
			TryFindInputControlWithGamepadName(s_ctrlList1, name, out foundControl);
			TryFindInputControlWithGamepadName(s_ctrlList2, name, out foundControl2);
		}
		if (foundControl == null || foundControl2 == null)
		{
			return false;
		}
		if (foundControl.parent == null || foundControl2.parent == null)
		{
			return false;
		}
		if (foundControl.parent != foundControl2.parent)
		{
			return false;
		}
		control = foundControl.parent;
		return true;
	}

	private static bool TryFindInputControlWithGamepadName(InputControlList<InputControl> icList, string gamepadName, out InputControl foundControl)
	{
		foundControl = null;
		for (int i = 0; i < icList.Count; i++)
		{
			InputControl inputControl = icList[i];
			if (inputControl.parent != null && inputControl.parent.parent != null && inputControl.parent.parent.name == gamepadName)
			{
				foundControl = inputControl;
				return true;
			}
		}
		return false;
	}

	public static bool EfficientTryGetSharedControl(InputAction action1, InputAction action2, InputAction action3, InputAction action4, bool gamepad, out InputControl control)
	{
		control = null;
		InputControlScheme? inputControlScheme = action1.actionMap.asset.FindControlScheme(InputConsts.GetControlScheme(gamepad));
		if (!inputControlScheme.HasValue)
		{
			return false;
		}
		InputControl inputControl = null;
		foreach (InputControl control2 in action1.controls)
		{
			InputDevice inputDevice = control2.FindInParentChain<InputDevice>();
			if (inputDevice.enabled && inputControlScheme.Value.SupportsDevice(inputDevice))
			{
				inputControl = control2;
				break;
			}
		}
		if (inputControl == null)
		{
			return false;
		}
		InputControl parent = inputControl.parent;
		UnityEngine.InputSystem.InputBinding bindingMask = (gamepad ? GamepadBindingMask : DesktopBindingMask);
		int bindingIndex = action1.GetBindingIndex(bindingMask);
		int bindingIndex2 = action2.GetBindingIndex(bindingMask);
		int bindingIndex3 = action3.GetBindingIndex(bindingMask);
		int bindingIndex4 = action4.GetBindingIndex(bindingMask);
		if (bindingIndex == -1 || bindingIndex2 == -1 || bindingIndex3 == -1 || bindingIndex4 == -1)
		{
			return false;
		}
		int num;
		if (parent != null && parent.TryGetChildControl(action2.bindings[bindingIndex2].effectivePath) != null && parent.TryGetChildControl(action3.bindings[bindingIndex3].effectivePath) != null)
		{
			num = ((parent.TryGetChildControl(action4.bindings[bindingIndex4].effectivePath) != null) ? 1 : 0);
			if (num != 0)
			{
				control = inputControl;
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	public static bool PopulateUITextureList(IInputCommands commands, in List<Texture2D> textureList, bool gamepad, bool clearList = false)
	{
		ExtractInputActions(commands, out var primary, out var secondary);
		ExtractInputActions(primary, out var first, out var second);
		ExtractInputActions(secondary, out var first2, out var second2);
		if (clearList)
		{
			textureList.Clear();
		}
		PopulateUITextureList(first, in textureList, gamepad);
		PopulateUITextureList(second, in textureList, gamepad);
		PopulateUITextureList(first2, in textureList, gamepad);
		PopulateUITextureList(second2, in textureList, gamepad);
		return true;
	}

	public static bool PopulateUITextureList(IInputAction action, in List<Texture2D> textureList, bool gamepad, bool clearList = false)
	{
		ExtractInputActions(action, out var first, out var second);
		if (clearList)
		{
			textureList.Clear();
		}
		PopulateUITextureList(first, in textureList, gamepad);
		PopulateUITextureList(second, in textureList, gamepad);
		return true;
	}

	public static bool PopulateUITextureList(InputAction action, in List<Texture2D> textureList, bool gamepad)
	{
		if (action == null)
		{
			return false;
		}
		UnityEngine.InputSystem.InputBinding bindingMask = (gamepad ? GamepadBindingMask : DesktopBindingMask);
		int bindingIndex = action.GetBindingIndex(bindingMask);
		if (bindingIndex == -1)
		{
			return false;
		}
		action.GetBindingDisplayString(bindingIndex, out var _, out var controlPath, UnityEngine.InputSystem.InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
		if (string.IsNullOrEmpty(controlPath))
		{
			Debug.LogWarning("No Control Path found for " + bindingMask.groups + " on InputAction " + action.name);
			return false;
		}
		KeyCode code;
		if (InputTransitionUtil.TryGetAxisIdentifier(controlPath, out var axisID))
		{
			textureList.Add(ButtonPromptLibrary.SharedInstance.GetAxisTexture(axisID));
		}
		else if (gamepad)
		{
			int num = controlPath.LastIndexOf('/');
			if (num > 0)
			{
				controlPath = controlPath.Substring(num + 1);
			}
			if (InputTransitionUtil.TryGetJoystickButton(controlPath, out var stickButton) && stickButton != 0 && stickButton != JoystickButton.Custom)
			{
				textureList.Add(ButtonPromptLibrary.SharedInstance.GetButtonTexture(stickButton));
			}
			else
			{
				textureList.Add(ButtonPromptLibrary.SharedInstance.GetButtonTexture(stickButton));
				Debug.LogWarning("Failed to find button texture for " + controlPath);
			}
		}
		else if (InputTransitionUtil.TryGetMouseButtonKeyCode(controlPath, out code) && code != 0)
		{
			textureList.Add(ButtonPromptLibrary.SharedInstance.GetButtonTexture(code));
		}
		if (gamepad)
		{
			return true;
		}
		for (int i = 0; i < action.controls.Count; i++)
		{
			if (action.controls[i] is KeyControl keyControl && keyControl.keyCode != 0)
			{
				if (InputTransitionUtil.TryGetKeyCode(keyControl.keyCode, out var code2))
				{
					textureList.Add(ButtonPromptLibrary.SharedInstance.GetButtonTexture(code2));
					continue;
				}
				textureList.Add(ButtonPromptLibrary.SharedInstance.GetButtonTexture(code2));
				Debug.LogWarning("Failed to find button texture for $" + keyControl.path);
			}
		}
		return true;
	}
}
