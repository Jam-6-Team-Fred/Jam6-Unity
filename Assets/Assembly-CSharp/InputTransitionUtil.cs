using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class InputTransitionUtil
{
	private static Dictionary<Key, KeyCode> KeyCodeCache;

	private static Dictionary<KeyCode, Key> KeyCache;

	private static Dictionary<MouseButton, KeyCode> MouseKeyCache;

	private static Dictionary<string, MouseButton> MouseCache;

	private static Dictionary<string, JoystickButton> JoystickCache;

	private static Dictionary<string, AxisIdentifier> AxisIDCache;

	private static Dictionary<AxisIdentifier, AxisIdentifier> AxisParentLookup;

	public static bool TryGetParentAxisID(AxisIdentifier childAxis, out AxisIdentifier parentAxis)
	{
		if (AxisParentLookup.TryGetValue(childAxis, out parentAxis))
		{
			return true;
		}
		parentAxis = AxisIdentifier.NONE;
		return false;
	}

	public static bool TryGetKeyCode(Key key, out KeyCode code)
	{
		if (KeyCodeCache.TryGetValue(key, out code))
		{
			return true;
		}
		if (Enum.TryParse<KeyCode>(key.ToString(), ignoreCase: true, out code))
		{
			KeyCodeCache[key] = code;
			KeyCache[code] = key;
			return true;
		}
		code = KeyCode.None;
		return false;
	}

	public static bool TryGetKeyCode(MouseButton key, out KeyCode code)
	{
		return MouseKeyCache.TryGetValue(key, out code);
	}

	public static bool TryGetMouseButtonKeyCode(string name, out KeyCode code)
	{
		if (TryGetMouseButton(name, out var button))
		{
			return TryGetKeyCode(button, out code);
		}
		code = KeyCode.None;
		return false;
	}

	public static bool TryGetKey(KeyCode code, out Key key)
	{
		if (KeyCache.TryGetValue(code, out key))
		{
			return true;
		}
		if (Enum.TryParse<Key>(code.ToString(), ignoreCase: true, out key))
		{
			KeyCache[code] = key;
			KeyCodeCache[key] = code;
			return true;
		}
		key = Key.None;
		return false;
	}

	public static bool TryGetJoystickButton(string name, out JoystickButton stickButton)
	{
		return JoystickCache.TryGetValue(name, out stickButton);
	}

	public static bool TryGetMouseButton(string name, out MouseButton button)
	{
		return MouseCache.TryGetValue(name, out button);
	}

	public static bool TryGetAxisIdentifier(string firstControlPath, string secondControlPath, out AxisIdentifier axisID)
	{
		axisID = AxisIdentifier.NONE;
		if (string.IsNullOrEmpty(firstControlPath) || string.IsNullOrEmpty(secondControlPath))
		{
			return false;
		}
		string[] array = firstControlPath.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = secondControlPath.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0 || array2.Length == 0)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		switch (array[array.Length - 1])
		{
		case "up":
			if (array2[array2.Length - 1] == "down")
			{
				flag = true;
			}
			break;
		case "down":
			if (array2[array2.Length - 1] == "up")
			{
				flag = true;
			}
			break;
		case "left":
			if (array2[array2.Length - 1] == "right")
			{
				flag2 = true;
			}
			break;
		case "right":
			if (array2[array2.Length - 1] == "left")
			{
				flag2 = true;
			}
			break;
		}
		string text = "";
		if (array[0] == "<Gamepad>")
		{
			string[] array3 = new string[array.Length - 1];
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i] = array[i + 1];
			}
			array = array3;
		}
		if (flag)
		{
			array[array.Length - 1] = "y";
			text = string.Join("/", array);
		}
		else
		{
			if (!flag2)
			{
				return false;
			}
			array[array.Length - 1] = "x";
			text = string.Join("/", array);
		}
		return AxisIDCache.TryGetValue(text, out axisID);
	}

	public static bool TryGetAxisIdentifier(string path, out AxisIdentifier axisID)
	{
		axisID = AxisIdentifier.NONE;
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		return AxisIDCache.TryGetValue(path, out axisID);
	}

	static InputTransitionUtil()
	{
		KeyCodeCache = new Dictionary<Key, KeyCode>
		{
			{
				Key.Enter,
				KeyCode.Return
			},
			{
				Key.Backquote,
				KeyCode.BackQuote
			},
			{
				Key.LeftCtrl,
				KeyCode.LeftControl
			},
			{
				Key.RightCtrl,
				KeyCode.RightControl
			},
			{
				Key.NumpadEnter,
				KeyCode.KeypadEnter
			},
			{
				Key.NumpadDivide,
				KeyCode.KeypadDivide
			},
			{
				Key.NumpadMultiply,
				KeyCode.KeypadMultiply
			},
			{
				Key.NumpadPlus,
				KeyCode.KeypadPlus
			},
			{
				Key.NumpadMinus,
				KeyCode.KeypadMinus
			},
			{
				Key.NumpadPeriod,
				KeyCode.KeypadPeriod
			},
			{
				Key.NumpadEquals,
				KeyCode.KeypadEquals
			},
			{
				Key.PrintScreen,
				KeyCode.Print
			},
			{
				Key.ContextMenu,
				KeyCode.Menu
			},
			{
				Key.NumLock,
				KeyCode.Numlock
			},
			{
				Key.Numpad0,
				KeyCode.Keypad0
			},
			{
				Key.Numpad1,
				KeyCode.Keypad1
			},
			{
				Key.Numpad2,
				KeyCode.Keypad2
			},
			{
				Key.Numpad3,
				KeyCode.Keypad3
			},
			{
				Key.Numpad4,
				KeyCode.Keypad4
			},
			{
				Key.Numpad5,
				KeyCode.Keypad5
			},
			{
				Key.Numpad6,
				KeyCode.Keypad6
			},
			{
				Key.Numpad7,
				KeyCode.Keypad7
			},
			{
				Key.Numpad8,
				KeyCode.Keypad8
			},
			{
				Key.Numpad9,
				KeyCode.Keypad9
			},
			{
				Key.Digit1,
				KeyCode.Alpha1
			},
			{
				Key.Digit2,
				KeyCode.Alpha2
			},
			{
				Key.Digit3,
				KeyCode.Alpha3
			},
			{
				Key.Digit4,
				KeyCode.Alpha4
			},
			{
				Key.Digit5,
				KeyCode.Alpha5
			},
			{
				Key.Digit6,
				KeyCode.Alpha6
			},
			{
				Key.Digit7,
				KeyCode.Alpha7
			},
			{
				Key.Digit8,
				KeyCode.Alpha8
			},
			{
				Key.Digit9,
				KeyCode.Alpha9
			},
			{
				Key.Digit0,
				KeyCode.Alpha0
			}
		};
		KeyCache = new Dictionary<KeyCode, Key>();
		MouseKeyCache = new Dictionary<MouseButton, KeyCode>
		{
			{
				MouseButton.Left,
				KeyCode.Mouse0
			},
			{
				MouseButton.Right,
				KeyCode.Mouse1
			},
			{
				MouseButton.Middle,
				KeyCode.Mouse2
			},
			{
				MouseButton.Forward,
				KeyCode.Mouse3
			},
			{
				MouseButton.Back,
				KeyCode.Mouse4
			}
		};
		MouseCache = new Dictionary<string, MouseButton>
		{
			{
				"leftButton",
				MouseButton.Left
			},
			{
				"rightButton",
				MouseButton.Right
			},
			{
				"middleButton",
				MouseButton.Middle
			},
			{
				"forwardButton",
				MouseButton.Forward
			},
			{
				"backButton",
				MouseButton.Back
			}
		};
		JoystickCache = new Dictionary<string, JoystickButton>
		{
			{
				"buttonNorth",
				JoystickButton.FaceUp
			},
			{
				"buttonSouth",
				JoystickButton.FaceDown
			},
			{
				"buttonWest",
				JoystickButton.FaceLeft
			},
			{
				"buttonEast",
				JoystickButton.FaceRight
			},
			{
				"leftTriggerButton",
				JoystickButton.LeftTrigger
			},
			{
				"rightTriggerButton",
				JoystickButton.RightTrigger
			},
			{
				"triggers",
				JoystickButton.Triggers
			},
			{
				"leftStick",
				JoystickButton.LeftStick
			},
			{
				"rightStick",
				JoystickButton.RightStick
			},
			{
				"start",
				JoystickButton.Start
			},
			{
				"select",
				JoystickButton.Select
			},
			{
				"leftShoulder",
				JoystickButton.LeftBumper
			},
			{
				"rightShoulder",
				JoystickButton.RightBumper
			},
			{
				"leftStickPress",
				JoystickButton.LeftStickClick
			},
			{
				"rightStickPress",
				JoystickButton.RightStickClick
			},
			{
				"up",
				JoystickButton.DPadUp
			},
			{
				"down",
				JoystickButton.DPadDown
			},
			{
				"left",
				JoystickButton.DPadLeft
			},
			{
				"right",
				JoystickButton.DPadRight
			},
			{
				"share",
				JoystickButton.DS4_Share
			}
		};
		AxisIDCache = new Dictionary<string, AxisIdentifier>
		{
			{
				"delta",
				AxisIdentifier.KEYBD_MOUSE
			},
			{
				"position",
				AxisIdentifier.KEYBD_MOUSE
			},
			{
				"delta/x",
				AxisIdentifier.KEYBD_MOUSEX
			},
			{
				"position/x",
				AxisIdentifier.KEYBD_MOUSEX
			},
			{
				"delta/y",
				AxisIdentifier.KEYBD_MOUSEY
			},
			{
				"position/y",
				AxisIdentifier.KEYBD_MOUSEY
			},
			{
				"scroll/y",
				AxisIdentifier.KEYBD_MOUSEWHEEL
			},
			{
				"dpad",
				AxisIdentifier.CTRLR_FULLDPAD
			},
			{
				"dpad/x",
				AxisIdentifier.CTRLR_DPADX
			},
			{
				"dpad/y",
				AxisIdentifier.CTRLR_DPADY
			},
			{
				"leftStick",
				AxisIdentifier.CTRLR_LSTICK
			},
			{
				"leftStick/x",
				AxisIdentifier.CTRLR_LSTICKX
			},
			{
				"leftStick/left",
				AxisIdentifier.CTRLR_LSTICKX
			},
			{
				"leftStick/right",
				AxisIdentifier.CTRLR_LSTICKX
			},
			{
				"leftStick/y",
				AxisIdentifier.CTRLR_LSTICKY
			},
			{
				"leftStick/up",
				AxisIdentifier.CTRLR_LSTICKY
			},
			{
				"leftStick/down",
				AxisIdentifier.CTRLR_LSTICKY
			},
			{
				"rightStick",
				AxisIdentifier.CTRLR_RSTICK
			},
			{
				"rightStick/x",
				AxisIdentifier.CTRLR_RSTICKX
			},
			{
				"rightStick/left",
				AxisIdentifier.CTRLR_RSTICKX
			},
			{
				"rightStick/right",
				AxisIdentifier.CTRLR_RSTICKX
			},
			{
				"rightStick/y",
				AxisIdentifier.CTRLR_RSTICKY
			},
			{
				"rightStick/up",
				AxisIdentifier.CTRLR_RSTICKY
			},
			{
				"rightStick/down",
				AxisIdentifier.CTRLR_RSTICKY
			},
			{
				"leftTrigger",
				AxisIdentifier.CTRLR_LTRIGGER
			},
			{
				"rightTrigger",
				AxisIdentifier.CTRLR_RTRIGGER
			}
		};
		AxisParentLookup = new Dictionary<AxisIdentifier, AxisIdentifier>
		{
			{
				AxisIdentifier.KEYBD_MOUSE,
				AxisIdentifier.KEYBD_MOUSE
			},
			{
				AxisIdentifier.KEYBD_MOUSEX,
				AxisIdentifier.KEYBD_MOUSE
			},
			{
				AxisIdentifier.KEYBD_MOUSEY,
				AxisIdentifier.KEYBD_MOUSE
			},
			{
				AxisIdentifier.CTRLR_FULLDPAD,
				AxisIdentifier.CTRLR_FULLDPAD
			},
			{
				AxisIdentifier.CTRLR_DPADX,
				AxisIdentifier.CTRLR_FULLDPAD
			},
			{
				AxisIdentifier.CTRLR_DPADY,
				AxisIdentifier.CTRLR_FULLDPAD
			},
			{
				AxisIdentifier.CTRLR_LSTICK,
				AxisIdentifier.CTRLR_LSTICK
			},
			{
				AxisIdentifier.CTRLR_LSTICKX,
				AxisIdentifier.CTRLR_LSTICK
			},
			{
				AxisIdentifier.CTRLR_LSTICKY,
				AxisIdentifier.CTRLR_LSTICK
			},
			{
				AxisIdentifier.CTRLR_RSTICK,
				AxisIdentifier.CTRLR_RSTICK
			},
			{
				AxisIdentifier.CTRLR_RSTICKX,
				AxisIdentifier.CTRLR_RSTICK
			},
			{
				AxisIdentifier.CTRLR_RSTICKY,
				AxisIdentifier.CTRLR_RSTICK
			}
		};
		foreach (KeyValuePair<Key, KeyCode> item in KeyCodeCache)
		{
			KeyCache.Add(item.Value, item.Key);
		}
	}

	public static string GetLabel(InputConsts.InputCommandType inputCommandType)
	{
		if (InputConsts.UITextInputActionTypes.TryGetValue(inputCommandType, out var value) && value.InputID != 0 && TextTranslation.Get() != null)
		{
			return UITextLibrary.GetString(value.InputID);
		}
		return inputCommandType.ToString();
	}
}
