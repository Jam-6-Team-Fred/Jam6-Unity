using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class InputUtil
{
	public enum GamePadPresetConfig
	{
		XBOX = 0,
		PS4 = 1,
		SWITCH_PRO = 2,
		NONE = 0x20
	}

	public enum ButtonPromptPresetConfig
	{
		XBOX = 0,
		PS4 = 1,
		SWITCH_PRO = 2,
		NONE = 0x20
	}

	public enum HardwareInputType
	{
		UNITY_RAWINPUT = 0,
		XINPUT = 1,
		SCE_PAD = 2
	}

	public enum XInputButton : ushort
	{
		None = 0,
		DPadUp = 1,
		DPadDown = 2,
		DPadLeft = 4,
		DPadRight = 8,
		Start = 0x10,
		Back = 0x20,
		LeftThumb = 0x40,
		RightThumb = 0x80,
		LeftShoulder = 0x100,
		RightShoulder = 0x200,
		A = 0x1000,
		B = 0x2000,
		X = 0x4000,
		Y = 0x8000
	}

	public struct XINPUT_State
	{
		public uint PacketNumber;

		public ushort Buttons;

		public byte LeftTrigger;

		public byte RightTrigger;

		public short ThumbLX;

		public short ThumbLY;

		public short ThumbRX;

		public short ThumbRY;
	}

	public struct XINPUT_Vibration
	{
		public ushort LeftMotorSpeed;

		public ushort RightMotorSpeed;
	}

	public struct ScePadAnalogStick
	{
		public byte x;

		public byte y;
	}

	public struct ScePadTouch
	{
		public ushort x;

		public ushort y;

		public byte id;

		public byte reserve0;

		public byte reserve1;

		public byte reserve2;
	}

	public struct ScePadTouchData
	{
		public byte touchNum;

		public byte reserveByte0;

		public byte reserveByte1;

		public byte reserveByte3;

		public uint reserve1;

		public ScePadTouch touch0;

		public ScePadTouch touch1;
	}

	public struct PS4_Pad_Data
	{
		public uint buttons;

		public ScePadAnalogStick leftStick;

		public ScePadAnalogStick rightStick;

		public float l2;

		public float r2;

		public Quaternion orientation;

		public Vector3 acceleration;

		public Vector3 angularVelocity;

		public ScePadTouchData touchData;

		public bool connected;
	}

	public enum ScePadButton
	{
		SCE_PAD_BUTTON_NONE = 0,
		SCE_PAD_BUTTON_L3 = 2,
		SCE_PAD_BUTTON_R3 = 4,
		SCE_PAD_BUTTON_OPTIONS = 8,
		SCE_PAD_BUTTON_UP = 0x10,
		SCE_PAD_BUTTON_RIGHT = 0x20,
		SCE_PAD_BUTTON_DOWN = 0x40,
		SCE_PAD_BUTTON_LEFT = 0x80,
		SCE_PAD_BUTTON_L2 = 0x100,
		SCE_PAD_BUTTON_R2 = 0x200,
		SCE_PAD_BUTTON_L1 = 0x400,
		SCE_PAD_BUTTON_R1 = 0x800,
		SCE_PAD_BUTTON_TRIANGLE = 0x1000,
		SCE_PAD_BUTTON_CIRCLE = 0x2000,
		SCE_PAD_BUTTON_CROSS = 0x4000,
		SCE_PAD_BUTTON_SQUARE = 0x8000,
		SCE_PAD_BUTTON_TOUCH_PAD = 0x100000
	}

	private static readonly Guid[] RawInputTypeGUIDs = new Guid[0];

	private static readonly Guid[] XInputTypeGUIDs = new Guid[6]
	{
		new Guid("d74a350e-fe8b-4e9e-bbcd-efff16d34115"),
		new Guid("19002688-7406-4f4a-8340-8d25335406c8"),
		new Guid("d9623ff0-6911-4028-b7a5-b98faa6d2c55"),
		new Guid("80f1d64b-b462-41cc-8c6d-452e72a2dee6"),
		new Guid("217c5027-dbf3-4bac-9818-08c9f75467ef"),
		new Guid("49d3b7cb-2572-454e-bdd1-ea83e1d744e8")
	};

	private static readonly Guid[] SCEPadTypeGUIDs = new Guid[1]
	{
		new Guid("cd9718bf-a87a-44bc-8716-60a0def28a9f")
	};

	private static readonly Guid[][] RewiredHardwareTypeGUIDList = new Guid[3][] { RawInputTypeGUIDs, XInputTypeGUIDs, SCEPadTypeGUIDs };

	private static long[] s_enableDeviceWhitelist = null;

	public static Dictionary<string, string> GamepadDisplayNameLookup = new Dictionary<string, string>
	{
		{ "XInputControllerWindows", "XInput Controller" },
		{ "DualShock4GamepadHID", "DualShock 4" },
		{ "DualShock3GamepadHID", "DualShock 3" },
		{ "DualSenseGamepadHID", "DualSense" },
		{ "SwitchProControllerHID", "Switch Pro" }
	};

	public static readonly string[] GamePadPresetStrings = GetGamepadPresetStrings();

	public const int MAX_GAMEPAD_BUTTON = 20;

	public const int MAX_GAMEPAD_AXES = 10;

	public static GamePadConfig GamePadConfig_Xbox = new GamePadConfig(GamePadPresetConfig.XBOX);

	public static GamePadConfig GamePadConfig_PS4 = new GamePadConfig(GamePadPresetConfig.PS4);

	public static GamePadConfig GamePadConfig_SwitchPro = new GamePadConfig(GamePadPresetConfig.SWITCH_PRO);

	public static readonly GamePadConfig[] DefaultGamePadConfigList = new GamePadConfig[3] { GamePadConfig_Xbox, GamePadConfig_PS4, GamePadConfig_SwitchPro };

	public const float AXIS_MINIMUM_DETECTABLE_INPUT = 0.1f;

	public const float JOYSTICK_DEFAULT_INNER_DEADZONE = 0.2f;

	public const float JOYSTICK_DEFAULT_OUTER_DEADZONE = 0.05f;

	public const float TRIGGER_DEFAULT_INNER_DEADZONE = 0.1f;

	public const float TRIGGER_DEFAULT_OUTER_DEADZONE = 0.05f;

	public static readonly XInputButton[] s_xInputButtons = new XInputButton[14]
	{
		XInputButton.DPadUp,
		XInputButton.DPadDown,
		XInputButton.DPadLeft,
		XInputButton.DPadRight,
		XInputButton.Y,
		XInputButton.A,
		XInputButton.X,
		XInputButton.B,
		XInputButton.RightShoulder,
		XInputButton.RightThumb,
		XInputButton.LeftShoulder,
		XInputButton.LeftThumb,
		XInputButton.Start,
		XInputButton.Back
	};

	public const uint XINPUT_ERROR_SUCCESS = 0u;

	public const uint XINPUT_ERROR_DEVICE_NOT_CONNECTED = 1167u;

	public const int XINPUT_USER_MAX_COUNT = 4;

	public const int XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE = 7849;

	public const int XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE = 8689;

	public const int XINPUT_GAMEPAD_TRIGGER_THRESHOLD = 30;

	public const int XINPUT_STICK_MAXIMUM = 32767;

	public const float XINPUT_TRIGGER_MAXIMUM = 255f;

	public const float PS4_STICK_MAXIMUM = 127f;

	public const float PS4_TRIGGER_MAXIMUM = 255f;

	public const float RUMBLE_LOW_POWER = 0.7f;

	public const float RUMBLE_HIGH_POWER = 0.7f;

	public static readonly ScePadButton[] s_scePadButtons = new ScePadButton[10]
	{
		ScePadButton.SCE_PAD_BUTTON_TRIANGLE,
		ScePadButton.SCE_PAD_BUTTON_CROSS,
		ScePadButton.SCE_PAD_BUTTON_SQUARE,
		ScePadButton.SCE_PAD_BUTTON_CIRCLE,
		ScePadButton.SCE_PAD_BUTTON_R1,
		ScePadButton.SCE_PAD_BUTTON_R3,
		ScePadButton.SCE_PAD_BUTTON_L1,
		ScePadButton.SCE_PAD_BUTTON_L3,
		ScePadButton.SCE_PAD_BUTTON_OPTIONS,
		ScePadButton.SCE_PAD_BUTTON_TOUCH_PAD
	};

	public static string[] GetGamepadPresetStrings()
	{
		return new string[2] { "Xbox One", "PS4" };
	}

	public static long[] GetEnableDeviceWhitelistIds()
	{
		if (s_enableDeviceWhitelist != null)
		{
			return s_enableDeviceWhitelist;
		}
		s_enableDeviceWhitelist = new long[5]
		{
			ConcatBitwiseId(1118, -1),
			ConcatBitwiseId(1356, 3302),
			ConcatBitwiseId(1356, 2508),
			ConcatBitwiseId(1356, 1476),
			ConcatBitwiseId(1356, 616)
		};
		return s_enableDeviceWhitelist;
	}

	public static long ConcatBitwiseId(int vendorId, int prodId)
	{
		return ((long)vendorId << 32) + prodId;
	}

	public static bool IsXInputController(string gamepadName)
	{
		return gamepadName.StartsWith("XInputControllerWindows");
	}

	public static HardwareInputType GetHardwareInputType(Guid rewiredHardwareTypeGuid)
	{
		HardwareInputType[] array = (HardwareInputType[])Enum.GetValues(typeof(HardwareInputType));
		foreach (HardwareInputType hardwareInputType in array)
		{
			Guid[] array2 = RewiredHardwareTypeGUIDList[(int)hardwareInputType];
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j] == rewiredHardwareTypeGuid)
				{
					return hardwareInputType;
				}
			}
		}
		return HardwareInputType.UNITY_RAWINPUT;
	}

	public static JoystickButton GetJoystickButton(XInputButton xInputButton)
	{
		switch (xInputButton)
		{
		case XInputButton.DPadUp:
			return JoystickButton.DPadUp;
		case XInputButton.DPadDown:
			return JoystickButton.DPadDown;
		case XInputButton.DPadLeft:
			return JoystickButton.DPadLeft;
		case XInputButton.DPadRight:
			return JoystickButton.DPadRight;
		case XInputButton.Y:
			return JoystickButton.FaceUp;
		case XInputButton.A:
			return JoystickButton.FaceDown;
		case XInputButton.X:
			return JoystickButton.FaceLeft;
		case XInputButton.B:
			return JoystickButton.FaceRight;
		case XInputButton.RightShoulder:
			return JoystickButton.RightBumper;
		case XInputButton.RightThumb:
			return JoystickButton.RightStickClick;
		case XInputButton.LeftShoulder:
			return JoystickButton.LeftBumper;
		case XInputButton.LeftThumb:
			return JoystickButton.LeftStickClick;
		case XInputButton.Start:
			return JoystickButton.Start;
		case XInputButton.Back:
			return JoystickButton.Select;
		default:
			return JoystickButton.None;
		}
	}

	[DllImport("XINPUT9_1_0.DLL")]
	public static extern uint XInputGetState(uint userIndex, out XINPUT_State state);

	[DllImport("XINPUT9_1_0.DLL")]
	public static extern uint XInputSetState(uint userIndex, ref XINPUT_Vibration vibration);

	[DllImport("PS4_Pad")]
	public static extern bool PS4_Pad_Init();

	[DllImport("PS4_Pad")]
	public static extern int PS4_Pad_Open(int userId);

	[DllImport("PS4_Pad")]
	public static extern void PS4_Pad_Close(int handle);

	[DllImport("PS4_Pad")]
	public static extern PS4_Pad_Data PS4_Pad_ReadData(int handle);

	[DllImport("PS4_Pad")]
	public static extern bool PS4_Pad_IsConnected(int handle);

	[DllImport("PS4_Pad")]
	public static extern bool PS4_Pad_Vibrate(int handle, float largePower, float smallPower);

	[DllImport("PS4_Pad")]
	public static extern bool PS4_Pad_SetLightBar(int handle, float r, float g, float b);

	public static JoystickButton GetJoystickButton(ScePadButton scePadButton)
	{
		switch (scePadButton)
		{
		case ScePadButton.SCE_PAD_BUTTON_UP:
			return JoystickButton.DPadUp;
		case ScePadButton.SCE_PAD_BUTTON_DOWN:
			return JoystickButton.DPadDown;
		case ScePadButton.SCE_PAD_BUTTON_LEFT:
			return JoystickButton.DPadLeft;
		case ScePadButton.SCE_PAD_BUTTON_RIGHT:
			return JoystickButton.DPadRight;
		case ScePadButton.SCE_PAD_BUTTON_TRIANGLE:
			return JoystickButton.FaceUp;
		case ScePadButton.SCE_PAD_BUTTON_CROSS:
			return JoystickButton.FaceDown;
		case ScePadButton.SCE_PAD_BUTTON_SQUARE:
			return JoystickButton.FaceLeft;
		case ScePadButton.SCE_PAD_BUTTON_CIRCLE:
			return JoystickButton.FaceRight;
		case ScePadButton.SCE_PAD_BUTTON_R1:
			return JoystickButton.RightBumper;
		case ScePadButton.SCE_PAD_BUTTON_R3:
			return JoystickButton.RightStickClick;
		case ScePadButton.SCE_PAD_BUTTON_L1:
			return JoystickButton.LeftBumper;
		case ScePadButton.SCE_PAD_BUTTON_L3:
			return JoystickButton.LeftStickClick;
		case ScePadButton.SCE_PAD_BUTTON_OPTIONS:
			return JoystickButton.Start;
		case ScePadButton.SCE_PAD_BUTTON_TOUCH_PAD:
			return JoystickButton.Select;
		default:
			return JoystickButton.None;
		}
	}
}
