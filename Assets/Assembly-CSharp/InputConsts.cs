using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;

public static class InputConsts
{
	[Flags]
	public enum InputType
	{
		None = 0,
		GamePad = 1,
		Keyboard = 4,
		Mouse = 8
	}

	public enum InputValueType
	{
		NONE = 0,
		BUTTON = 1,
		SINGLE_AXIS = 2,
		DOUBLE_AXIS = 3
	}

	public struct UITextTypeInputs
	{
		public UITextType InputID;

		public UITextType TooltipID;

		public UITextTypeInputs(UITextType inputID, UITextType tooltipID)
		{
			InputID = inputID;
			TooltipID = tooltipID;
		}
	}

	public enum InputCommandType
	{
		UNDEFINED = 0,
		DEPRECATED = 4,
		SELECT = 100,
		MENU_CONFIRM = 101,
		ENTER = 102,
		ENTER2 = 103,
		CANCEL = 104,
		CANCEL_REBINDING1 = 105,
		CANCEL_REBINDING2 = 106,
		ESCAPE = 107,
		SET_DEFAULTS = 108,
		CONFIRM = 109,
		CONFIRM2 = 110,
		UP = 111,
		DOWN = 112,
		RIGHT = 113,
		LEFT = 114,
		MENU_RIGHT = 115,
		MENU_LEFT = 116,
		SUBMENU_RIGHT = 117,
		SUBMENU_LEFT = 118,
		UP2 = 119,
		DOWN2 = 120,
		RIGHT2 = 121,
		LEFT2 = 122,
		TAB = 123,
		TABL = 124,
		TABR = 125,
		TABL2 = 126,
		TABR2 = 127,
		SHIFTL = 128,
		SHIFTR = 129,
		PAUSE = 130,
		FACE_UP = 131,
		FACE_DOWN = 132,
		FACE_LEFT = 133,
		FACE_RIGHT = 134,
		SWAP_SHIP_LOG_MODE = 200,
		MARK_ENTRY_ON_HUD = 201,
		SCROLLING_TEXT = 202,
		MAP = 210,
		MAP_ZOOM = 211,
		MAP_MOUSE0 = 212,
		MAP_MOUSE1 = 213,
		MAP_ZOOM_IN = 214,
		MAP_ZOOM_OUT = 215,
		MOVE_XZ = 220,
		MOVE_X = 221,
		MOVE_Z = 222,
		LOOK = 223,
		LOOK_X = 224,
		LOOK_Y = 225,
		JUMP = 226,
		INTERACT = 230,
		SLEEP = 231,
		SUIT_MENU = 232,
		INTERACT_SECONDARY = 233,
		FLASHLIGHT = 240,
		EXTEND_STICK = 241,
		TOOL_PRIMARY = 250,
		TOOL_SECONDARY = 251,
		TOOL_X = 252,
		TOOL_Y = 253,
		TOOL_UP = 254,
		TOOL_DOWN = 255,
		TOOL_RIGHT = 256,
		TOOL_LEFT = 257,
		SIGNALSCOPE = 260,
		SCOPEVIEW = 261,
		PROBELAUNCH = 270,
		PROBERETRIEVE = 271,
		ROLL_MODE = 280,
		BOOST = 281,
		THRUSTX = 282,
		THRUSTZ = 283,
		THRUST_UP = 284,
		THRUST_DOWN = 285,
		YAW = 286,
		PITCH = 287,
		LOCKON = 300,
		MATCH_VELOCITY = 301,
		LANDING_CAMERA = 310,
		AUTOPILOT = 311,
		FREELOOK = 312,
		TAKE_SCREENSHOT = 900,
		TAKE_2XSCREENSHOT = 901,
		SLOWDOWNTIME = 902,
		SPEEDUPTIME = 903
	}

	public static class InputControlSchemes
	{
		public const string GAMEPAD = "Gamepad";

		public const string KEYBOARDMOUSE = "KeyboardMouse";
	}

	public static class InputUsages
	{
		public const string PRIMARYACTION = "{PrimaryAction}";

		public const string BACK = "{Back}";
	}

	public static class InputCompositeParts
	{
		public const string POSITIVE = "positive";

		public const string NEGATIVE = "negative";

		public const string HORIZONTAL = "Horizontal";

		public const string VERTICAL = "Vertical";

		public const string UP = "up";

		public const string DOWN = "down";

		public const string LEFT = "left";

		public const string RIGHT = "right";
	}

	public static class InputActionTypeNames
	{
		public const string SELECT = "select";

		public const string MENU_CONFIRM = "menuConfirm";

		public const string ENTER = "enter";

		public const string ENTER2 = "enter2";

		public const string CANCEL = "cancel";

		public const string CANCEL_REBINDING1 = "cancelRebinding1";

		public const string CANCEL_REBINDING2 = "cancelRebinding2";

		public const string ESCAPE = "escape";

		public const string SET_DEFAULTS = "setDefaults";

		public const string CONFIRM = "confirm";

		public const string CONFIRM2 = "confirm2";

		public const string UP = "up";

		public const string DOWN = "down";

		public const string RIGHT = "right";

		public const string LEFT = "left";

		public const string MENU_RIGHT = "menuRight";

		public const string MENU_LEFT = "menuLeft";

		public const string SUBMENU_RIGHT = "submenuRight";

		public const string SUBMENU_LEFT = "submenuLeft";

		public const string UP2 = "up2";

		public const string DOWN2 = "down2";

		public const string RIGHT2 = "right2";

		public const string LEFT2 = "left2";

		public const string TAB = "tab";

		public const string TABL = "tabL";

		public const string TABR = "tabR";

		public const string TABL2 = "tabL2";

		public const string TABR2 = "tabR2";

		public const string SHIFTL = "shiftL";

		public const string SHIFTR = "shiftR";

		public const string PAUSE = "pause";

		public const string FACE_UP = "faceUp";

		public const string FACE_DOWN = "faceDown";

		public const string FACE_LEFT = "faceLeft";

		public const string FACE_RIGHT = "faceRight";

		public const string SWAP_SHIP_LOG_MODE = "swapShipLogMode";

		public const string MARK_ENTRY_ON_HUD = "markEntryOnHud";

		public const string SCROLLING_TEXT = "scrollLogText";

		public const string MAP = "map";

		public const string MAP_ZOOM = "mapZoom";

		public const string MAP_MOUSE0 = "mapMouse0";

		public const string MAP_MOUSE1 = "mapMouse1";

		public const string MAP_ZOOM_IN = "mapZoomIn";

		public const string MAP_ZOOM_OUT = "mapZoomOut";

		public const string MOVE_XZ = "moveXZ";

		public const string MOVE_X = "moveX";

		public const string MOVE_Z = "moveZ";

		public const string MOVE_FORWARD = "moveForward";

		public const string MOVE_BACKWARD = "moveBackward";

		public const string MOVE_LEFT = "moveLeft";

		public const string MOVE_RIGHT = "moveRight";

		public const string LOOK = "look";

		public const string LOOK_X = "lookX";

		public const string LOOK_Y = "lookY";

		public const string LOOK_UP = "lookUp";

		public const string LOOK_DOWN = "lookDown";

		public const string LOOK_LEFT = "lookLeft";

		public const string LOOK_RIGHT = "lookRight";

		public const string JUMP = "jump";

		public const string INTERACT = "interact";

		public const string INTERACT_SECONDARY = "interactSecondary";

		public const string SLEEP = "sleep";

		public const string SUIT_MENU = "suitMenu";

		public const string FLASHLIGHT = "flashlight";

		public const string EXTEND_STICK = "extendStick";

		public const string TOOL_PRIMARY = "toolActionPrimary";

		public const string TOOL_SECONDARY = "toolActionSecondary";

		public const string TOOL_RIGHT = "toolOptionRight";

		public const string TOOL_LEFT = "toolOptionLeft";

		public const string TOOL_UP = "toolOptionUp";

		public const string TOOL_DOWN = "toolOptionDown";

		public const string SIGNALSCOPE = "signalscope";

		public const string SCOPEVIEW = "scopeView";

		public const string PROBELAUNCH = "probeLaunch";

		public const string PROBERETRIEVE = "probeRetrieve";

		public const string ROLL_MODE = "rollMode";

		public const string BOOST = "boost";

		public const string THRUSTX = "thrustX";

		public const string THRUSTZ = "thrustZ";

		public const string THRUST_UP = "thrustUp";

		public const string THRUST_DOWN = "thrustDown";

		public const string YAW = "yaw";

		public const string PITCH = "pitch";

		public const string LOCKON = "lockOn";

		public const string MATCH_VELOCITY = "matchVelocity";

		public const string LANDING_CAMERA = "landingCamera";

		public const string AUTOPILOT = "autopilot";

		public const string FREELOOK = "freeLook";

		public const string TAKE_SCREENSHOT = "takeScreenshot";

		public const string TAKE_2XSCREENSHOT = "take2xScreenshot";

		public const string SLOWDOWNTIME = "slowDownTime";

		public const string SPEEDUPTIME = "speedUpTime";
	}

	public static class HwVendorId
	{
		public const int MICROSOFT = 1118;

		public const int SONY = 1356;

		public const int NINTENDO = 1406;
	}

	public static class HwProdId
	{
		public const int XINPUT_DEVICE_INTERNAL = -1;

		public const int DUALSENSE = 3302;

		public const int DUALSHOCK4 = 2508;

		public const int DUALSHOCK4_ALT = 1476;

		public const int DUALSHOCK3 = 616;

		public const int SWITCHPRO = 8201;
	}

	public static readonly Dictionary<InputCommandType, UITextTypeInputs> UITextInputActionTypes;

	public static List<InputCommandType> NonBindableCommandTypes;

	public const string UnityXInputDeviceName = "XInputControllerWindows";

	public static InputType GetDeviceType(InputDevice device)
	{
		if (device is Gamepad)
		{
			return InputType.GamePad;
		}
		if (device is Mouse)
		{
			return InputType.Mouse;
		}
		if (device is Keyboard)
		{
			return InputType.Keyboard;
		}
		return InputType.None;
	}

	public static bool ContainsInputType(InputType flags, InputType compare)
	{
		return (flags & compare) == compare;
	}

	public static InputValueType FindValueType(InputAction action)
	{
		InputValueType result = InputValueType.NONE;
		if (action == null)
		{
			return result;
		}
		if (action.type == InputActionType.Button)
		{
			result = InputValueType.BUTTON;
		}
		else if (action.type == InputActionType.Value)
		{
			result = ((action.expectedControlType == "Vector2") ? InputValueType.DOUBLE_AXIS : InputValueType.SINGLE_AXIS);
		}
		return result;
	}

	public static string GetControlScheme(bool usingGamepad)
	{
		if (!usingGamepad)
		{
			return "KeyboardMouse";
		}
		return "Gamepad";
	}

	public static bool IsUsagePath(string path)
	{
		if (!(path == "{PrimaryAction}"))
		{
			return path == "{Back}";
		}
		return true;
	}

	public static string ConvertUsage(string usage)
	{
		bool flag = false;
		if (Gamepad.current is SwitchProControllerHID)
		{
			flag = true;
		}
		if (!(usage == "{PrimaryAction}"))
		{
			if (usage == "{Back}")
			{
				if (!flag)
				{
					return "buttonEast";
				}
				return "buttonSouth";
			}
			Debug.LogWarning("No usage found for " + usage);
			return usage;
		}
		if (!flag)
		{
			return "buttonSouth";
		}
		return "buttonEast";
	}

	static InputConsts()
	{
		UITextInputActionTypes = new Dictionary<InputCommandType, UITextTypeInputs>
		{
			{
				InputCommandType.PAUSE,
				new UITextTypeInputs(UITextType.RebindPause, UITextType.Tooltip_Rebind_Pause)
			},
			{
				InputCommandType.MENU_CONFIRM,
				new UITextTypeInputs(UITextType.KeyRebindingConfirm, UITextType.Tooltip_Rebind_Confirm)
			},
			{
				InputCommandType.CANCEL,
				new UITextTypeInputs(UITextType.RebindCancel, UITextType.Tooltip_Rebind_Cancel)
			},
			{
				InputCommandType.MOVE_XZ,
				new UITextTypeInputs(UITextType.KeyRebindingMoveXAxis, UITextType.Tooltip_Rebind_MoveX)
			},
			{
				InputCommandType.LOOK,
				new UITextTypeInputs(UITextType.KeyRebindingLookXAxis, UITextType.Tooltip_Rebind_LookX)
			},
			{
				InputCommandType.INTERACT,
				new UITextTypeInputs(UITextType.RebindX, UITextType.Tooltip_Rebind_Interact)
			},
			{
				InputCommandType.INTERACT_SECONDARY,
				new UITextTypeInputs(UITextType.KeyRebindingInteractSecondary, UITextType.None)
			},
			{
				InputCommandType.JUMP,
				new UITextTypeInputs(UITextType.JumpPrompt, UITextType.Tooltip_Rebind_Jump)
			},
			{
				InputCommandType.FLASHLIGHT,
				new UITextTypeInputs(UITextType.FlashlightPrompt, UITextType.Tooltip_Rebind_Flashlight)
			},
			{
				InputCommandType.MAP,
				new UITextTypeInputs(UITextType.MapPrompt, UITextType.Tooltip_Rebind_Map)
			},
			{
				InputCommandType.MAP_ZOOM,
				new UITextTypeInputs(UITextType.KeyRebindingMapZoomIn, UITextType.Tooltip_Rebind_MapZoomIn)
			},
			{
				InputCommandType.SIGNALSCOPE,
				new UITextTypeInputs(UITextType.RebindSignalscope, UITextType.Tooltip_Rebind_Signalscope)
			},
			{
				InputCommandType.PROBELAUNCH,
				new UITextTypeInputs(UITextType.KeyRebindingLaunchRetrieveScout, UITextType.Tooltip_Rebind_Scout)
			},
			{
				InputCommandType.PROBERETRIEVE,
				new UITextTypeInputs(UITextType.KeyRebindingRetrieveScout, UITextType.None)
			},
			{
				InputCommandType.TOOL_PRIMARY,
				new UITextTypeInputs(UITextType.KeyRebindingToolPrimary, UITextType.Tooltip_Rebind_ToolPrimary)
			},
			{
				InputCommandType.TOOL_SECONDARY,
				new UITextTypeInputs(UITextType.KeyRebindingToolSecondary, UITextType.Tooltip_Rebind_ToolSecondary)
			},
			{
				InputCommandType.THRUST_UP,
				new UITextTypeInputs(UITextType.UpPrompt, UITextType.Tooltip_Rebind_ThrustUp)
			},
			{
				InputCommandType.THRUST_DOWN,
				new UITextTypeInputs(UITextType.DownPrompt, UITextType.Tooltip_Rebind_ThrustDown)
			},
			{
				InputCommandType.ROLL_MODE,
				new UITextTypeInputs(UITextType.KeyRebindingRollMode, UITextType.Tooltip_Rebind_Roll)
			},
			{
				InputCommandType.BOOST,
				new UITextTypeInputs(UITextType.KeyRebindingJetpackBoost, UITextType.Tooltip_Rebind_Boost)
			},
			{
				InputCommandType.LOCKON,
				new UITextTypeInputs(UITextType.LockOnPrompt, UITextType.Tooltip_Rebind_LockOn)
			},
			{
				InputCommandType.MATCH_VELOCITY,
				new UITextTypeInputs(UITextType.MatchVelocityPrompt, UITextType.Tooltip_Rebind_MatchV)
			},
			{
				InputCommandType.LANDING_CAMERA,
				new UITextTypeInputs(UITextType.KeyRebindingLandingCamera, UITextType.Tooltip_Rebind_LandingCam)
			},
			{
				InputCommandType.AUTOPILOT,
				new UITextTypeInputs(UITextType.RebindAutopilot, UITextType.Tooltip_Rebind_Autopilot)
			},
			{
				InputCommandType.FREELOOK,
				new UITextTypeInputs(UITextType.KeyRebindingCockpitFreeLook, UITextType.Tooltip_Rebind_FreeLook)
			},
			{
				InputCommandType.SWAP_SHIP_LOG_MODE,
				new UITextTypeInputs(UITextType.KeyRebindingChangeShipLogMode, UITextType.Tooltip_Rebind_ShipLogMode)
			},
			{
				InputCommandType.MARK_ENTRY_ON_HUD,
				new UITextTypeInputs(UITextType.KeyRebindingMarkEntryOnHUD, UITextType.Tooltip_Rebind_ShipLogMarker)
			},
			{
				InputCommandType.SCROLLING_TEXT,
				new UITextTypeInputs(UITextType.KeyRebindingScrollShipLogText, UITextType.Tooltip_Rebind_ShipLogScroll)
			},
			{
				InputCommandType.SET_DEFAULTS,
				new UITextTypeInputs(UITextType.MenuResetToDefault, UITextType.None)
			}
		};
		NonBindableCommandTypes = new List<InputCommandType>
		{
			InputCommandType.SET_DEFAULTS,
			InputCommandType.DEPRECATED
		};
	}
}
