using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
[Obsolete("Use InputCommands/InputAction instead", false)]
public class InputBinding
{
	public KeyCode positiveKey;

	public KeyCode negativeKey;

	public string unityJoystickAxisName;

	public AxisIdentifier axisID;

	public int displayDirection;

	[OptionalField(VersionAdded = 2)]
	public int axisDirection;

	[OptionalField(VersionAdded = 3)]
	public JoystickButton gamepadButtonPos;

	[OptionalField(VersionAdded = 3)]
	public JoystickButton gamepadButtonNeg;

	public InputBinding()
	{
		positiveKey = KeyCode.None;
		negativeKey = KeyCode.None;
		unityJoystickAxisName = "";
		axisID = AxisIdentifier.NONE;
		displayDirection = 0;
		axisDirection = 1;
	}

	[OnDeserializing]
	private void SetDefaultValuesOnDeserializing(StreamingContext context)
	{
		axisDirection = 1;
		gamepadButtonPos = JoystickButton.None;
		gamepadButtonNeg = JoystickButton.None;
	}

	public InputBinding(KeyCode keyPos)
	{
		SetBinding(keyPos);
	}

	public InputBinding(KeyCode keyPos, KeyCode keyNeg)
	{
		SetBinding(keyPos, keyNeg);
	}

	public InputBinding(AxisIdentifier axisId, int specificDirection = 0, string customAxisName = "")
	{
		SetBinding(axisId, specificDirection, customAxisName);
	}

	public InputBinding(JoystickButton button)
	{
		gamepadButtonPos = button;
	}

	public InputBinding(JoystickButton buttonPos, JoystickButton buttonNeg)
	{
		gamepadButtonPos = buttonPos;
		gamepadButtonNeg = buttonNeg;
	}

	public InputAxisType GetInputAxisType()
	{
		if (positiveKey != 0)
		{
			return InputAxisType.KEY_OR_BUTTON;
		}
		if (unityJoystickAxisName != "")
		{
			if (unityJoystickAxisName == "Mouse_X" || unityJoystickAxisName == "Mouse_Y")
			{
				return InputAxisType.MOUSE;
			}
			if (unityJoystickAxisName == "Mouse_ScrollWheel")
			{
				return InputAxisType.MOUSE_WHEEL;
			}
			return InputAxisType.JOYSTICK;
		}
		if (axisID != 0)
		{
			if (axisID == AxisIdentifier.KEYBD_MOUSE || axisID == AxisIdentifier.KEYBD_MOUSEX || axisID == AxisIdentifier.KEYBD_MOUSEY)
			{
				return InputAxisType.MOUSE;
			}
			if (axisID == AxisIdentifier.KEYBD_MOUSEWHEEL)
			{
				return InputAxisType.MOUSE_WHEEL;
			}
			if (axisID == AxisIdentifier.CTRLR_DPADX || axisID == AxisIdentifier.CTRLR_DPADY)
			{
				return InputAxisType.KEY_OR_BUTTON;
			}
			return InputAxisType.JOYSTICK;
		}
		return InputAxisType.NONE;
	}

	public void SetBinding(KeyCode keyPos)
	{
		positiveKey = keyPos;
		unityJoystickAxisName = "";
		axisID = AxisIdentifier.NONE;
	}

	public void SetBinding(KeyCode keyPos, KeyCode keyNeg)
	{
		negativeKey = keyNeg;
		SetBinding(keyPos);
	}

	public void SetBinding(AxisIdentifier axisId, int specificDirection = 0, string customName = "")
	{
		axisID = axisId;
		if (axisID == AxisIdentifier.CTRLR_CUSTOM)
		{
			if (customName == "")
			{
				Debug.LogError("Invalid binding, custom JoystickAxis name not found!");
			}
			unityJoystickAxisName = customName;
		}
		displayDirection = specificDirection;
		if (displayDirection == 0)
		{
			axisDirection = 1;
		}
		else
		{
			axisDirection = displayDirection;
		}
		positiveKey = KeyCode.None;
		negativeKey = KeyCode.None;
	}

	public void SetBinding(JoystickButton button)
	{
		gamepadButtonPos = button;
	}

	public bool IsBindingEmpty()
	{
		if (positiveKey == KeyCode.None && negativeKey == KeyCode.None && axisID == AxisIdentifier.NONE && gamepadButtonPos == JoystickButton.None)
		{
			return true;
		}
		return false;
	}

	public bool DoesThisGamepadBindingConflict(InputBinding bindingToTestAgainst)
	{
		bool result = false;
		if (axisID == bindingToTestAgainst.axisID && axisID != 0 && axisDirection == bindingToTestAgainst.axisDirection && displayDirection == bindingToTestAgainst.displayDirection)
		{
			result = true;
		}
		else if (gamepadButtonPos == bindingToTestAgainst.gamepadButtonPos && gamepadButtonNeg == bindingToTestAgainst.gamepadButtonNeg && gamepadButtonPos != 0 && gamepadButtonNeg != 0)
		{
			result = (gamepadButtonPos != JoystickButton.Custom && gamepadButtonNeg != JoystickButton.Custom) || DoesThisKeybdMouseBindingConflict(bindingToTestAgainst);
		}
		else if (gamepadButtonPos == bindingToTestAgainst.gamepadButtonPos && gamepadButtonPos != 0 && gamepadButtonNeg == JoystickButton.None && bindingToTestAgainst.gamepadButtonNeg == JoystickButton.None)
		{
			result = gamepadButtonPos != JoystickButton.Custom || DoesThisKeybdMouseBindingConflict(bindingToTestAgainst);
		}
		return result;
	}

	public bool DoesThisKeybdMouseBindingConflict(InputBinding bindingToTestAgainst)
	{
		bool result = false;
		if (axisID == bindingToTestAgainst.axisID && axisID != 0 && axisDirection == bindingToTestAgainst.axisDirection && displayDirection == bindingToTestAgainst.displayDirection)
		{
			result = true;
		}
		else if (positiveKey == bindingToTestAgainst.positiveKey && negativeKey == bindingToTestAgainst.negativeKey && positiveKey != 0 && negativeKey != 0)
		{
			result = true;
		}
		else if (positiveKey == bindingToTestAgainst.positiveKey && positiveKey != 0 && negativeKey == KeyCode.None && bindingToTestAgainst.negativeKey == KeyCode.None)
		{
			result = true;
		}
		return result;
	}

	public InputBinding DeriveSingleButtonBindingWithDirection(int direction)
	{
		InputBinding inputBinding = new InputBinding();
		if (axisID == AxisIdentifier.NONE)
		{
			if (direction == -1)
			{
				inputBinding.SetBinding(negativeKey);
			}
			else
			{
				inputBinding.SetBinding(positiveKey);
			}
		}
		else
		{
			inputBinding.SetBinding(axisID, direction, unityJoystickAxisName);
		}
		return inputBinding;
	}

	public void Reset()
	{
		positiveKey = KeyCode.None;
		negativeKey = KeyCode.None;
		unityJoystickAxisName = "";
		axisID = AxisIdentifier.NONE;
		displayDirection = 0;
		axisDirection = 1;
	}

	public InputBinding Clone()
	{
		return new InputBinding
		{
			positiveKey = positiveKey,
			negativeKey = negativeKey,
			unityJoystickAxisName = unityJoystickAxisName,
			axisID = axisID,
			displayDirection = displayDirection,
			axisDirection = axisDirection,
			gamepadButtonPos = gamepadButtonPos
		};
	}

	public override bool Equals(object obj)
	{
		if (obj is InputBinding)
		{
			InputBinding inputBinding = obj as InputBinding;
			if (inputBinding.positiveKey == positiveKey && inputBinding.negativeKey == negativeKey && inputBinding.axisID == axisID && inputBinding.gamepadButtonPos == gamepadButtonPos && inputBinding.gamepadButtonNeg == gamepadButtonNeg)
			{
				if (axisID == AxisIdentifier.CTRLR_CUSTOM && inputBinding.unityJoystickAxisName != unityJoystickAxisName)
				{
					return false;
				}
				if (axisID != 0 && inputBinding.axisDirection != axisDirection)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
