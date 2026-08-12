using System.Linq;
using UnityEngine;

public static class InputTranslator
{
	private static AxisIdentifier[] s_leftStickSet = new AxisIdentifier[2]
	{
		AxisIdentifier.CTRLR_LSTICKX,
		AxisIdentifier.CTRLR_LSTICKY
	};

	private static AxisIdentifier[] s_rightStickSet = new AxisIdentifier[2]
	{
		AxisIdentifier.CTRLR_RSTICKX,
		AxisIdentifier.CTRLR_RSTICKY
	};

	private static AxisIdentifier[] s_dPadSet = new AxisIdentifier[2]
	{
		AxisIdentifier.CTRLR_DPADX,
		AxisIdentifier.CTRLR_DPADY
	};

	private static AxisIdentifier[] s_mouseSet = new AxisIdentifier[2]
	{
		AxisIdentifier.KEYBD_MOUSEX,
		AxisIdentifier.KEYBD_MOUSEY
	};

	public static bool IsTrigger(string unityAxisName)
	{
		return false;
	}

	public static InputAxisType GetInputAxisType(string unityAxisName)
	{
		switch (unityAxisName)
		{
		case "Mouse_X":
		case "Mouse_Y":
			return InputAxisType.MOUSE;
		case "Mouse_ScrollWheel":
			return InputAxisType.MOUSE_WHEEL;
		default:
			if (IsTrigger(unityAxisName))
			{
				return InputAxisType.TRIGGER;
			}
			return InputAxisType.JOYSTICK;
		}
	}

	public static KeyCode GetGenericButtonKeyCode(JoystickButton button)
	{
		if (button != 0)
		{
			_ = 20;
		}
		return KeyCode.None;
	}

	public static KeyCode GetButtonKeyCode(JoystickButton button)
	{
		if (button != 0)
		{
			_ = 20;
		}
		return KeyCode.None;
	}

	public static JoystickButton ConvertKeyCodeToButton(KeyCode key, GamePadConfig config)
	{
		if (key < KeyCode.JoystickButton0)
		{
			return JoystickButton.None;
		}
		while (key >= KeyCode.Joystick1Button0)
		{
			key -= 20;
		}
		int num = (int)(key - 330);
		for (int i = 0; i < config.buttonData.Length; i++)
		{
			if (config.buttonData[i].buttonNum == num)
			{
				return config.buttonData[i].buttonId;
			}
		}
		return JoystickButton.Custom;
	}

	public static bool IsJoystickButton(KeyCode key)
	{
		return key >= KeyCode.JoystickButton0;
	}

	public static string GetAxisName(AxisIdentifier axisId)
	{
		string result = "";
		switch (axisId)
		{
		case AxisIdentifier.KEYBD_MOUSEWHEEL:
			result = "Mouse_ScrollWheel";
			break;
		case AxisIdentifier.KEYBD_MOUSEX:
			result = "Mouse_X";
			break;
		case AxisIdentifier.KEYBD_MOUSEY:
			result = "Mouse_Y";
			break;
		default:
			_ = 16;
			break;
		case AxisIdentifier.NONE:
			break;
		}
		return result;
	}

	public static bool CanCombineAxis(AxisIdentifier axisId1, AxisIdentifier axisId2, out AxisIdentifier combinedAxisID)
	{
		combinedAxisID = AxisIdentifier.NONE;
		AxisIdentifier[] second = new AxisIdentifier[2] { axisId1, axisId2 };
		if (Enumerable.Count(Enumerable.Except(s_leftStickSet, second)) == 0)
		{
			combinedAxisID = AxisIdentifier.CTRLR_LSTICK;
			return true;
		}
		if (Enumerable.Count(Enumerable.Except(s_rightStickSet, second)) == 0)
		{
			combinedAxisID = AxisIdentifier.CTRLR_RSTICK;
			return true;
		}
		if (Enumerable.Count(Enumerable.Except(s_dPadSet, second)) == 0)
		{
			combinedAxisID = AxisIdentifier.CTRLR_FULLDPAD;
			return true;
		}
		if (Enumerable.Count(Enumerable.Except(s_mouseSet, second)) == 0)
		{
			combinedAxisID = AxisIdentifier.KEYBD_MOUSE;
			return true;
		}
		return false;
	}
}
