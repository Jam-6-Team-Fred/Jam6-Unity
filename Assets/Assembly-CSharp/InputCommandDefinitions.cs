using System;
using System.Collections.Generic;
using UnityEngine;

public static class InputCommandDefinitions
{
	[Serializable]
	public class InputActionData
	{
		public bool IsValid
		{
			get
			{
				if (string.IsNullOrEmpty(ActionName1))
				{
					return !string.IsNullOrEmpty(ActionName2);
				}
				return true;
			}
		}

		public bool IsRebindable => ID != RebindableID.UNDEFINED;

		public bool IsSingle
		{
			get
			{
				if (!string.IsNullOrEmpty(ActionName1))
				{
					return string.IsNullOrEmpty(ActionName2);
				}
				return false;
			}
		}

		public bool IsPair
		{
			get
			{
				if (!string.IsNullOrEmpty(ActionName1))
				{
					return !string.IsNullOrEmpty(ActionName2);
				}
				return false;
			}
		}

		public RebindableID ID { get; private set; }

		public string ActionName1 { get; private set; }

		public string ActionName2 { get; private set; }

		public InputActionData(RebindableID id, string action1, string action2 = null)
		{
			ID = id;
			ActionName1 = action1;
			ActionName2 = action2;
		}
	}

	[Serializable]
	public class InputCommandData
	{
		public bool IsValid
		{
			get
			{
				if (DataType != 0 && CommandType != 0)
				{
					return IsActionDataValid;
				}
				return false;
			}
		}

		public bool IsActionDataValid
		{
			get
			{
				if (!IsSingle)
				{
					return IsComposite;
				}
				return true;
			}
		}

		public CommandDataType DataType { get; private set; }

		public InputConsts.InputCommandType CommandType { get; private set; }

		public bool IsSingle
		{
			get
			{
				if (Primary != null)
				{
					return Secondary == null;
				}
				return false;
			}
		}

		public bool IsComposite
		{
			get
			{
				if (Primary != null)
				{
					return Secondary != null;
				}
				return false;
			}
		}

		public InputActionData Primary { get; private set; }

		public InputActionData Secondary { get; private set; }

		public bool HasRebindable
		{
			get
			{
				if (IsSingle)
				{
					return Primary.IsRebindable;
				}
				if (IsComposite)
				{
					if (!Primary.IsRebindable)
					{
						return Primary.IsRebindable;
					}
					return true;
				}
				return false;
			}
		}

		public static InputCommandData CreateCommandData(CommandDataType dataType, InputConsts.InputCommandType cmdType)
		{
			return new InputCommandData
			{
				DataType = dataType,
				CommandType = cmdType
			};
		}

		public bool TrySetAsBasic(string actionName, RebindableID id = RebindableID.UNDEFINED)
		{
			if (DataType != CommandDataType.Basic)
			{
				return false;
			}
			Primary = new InputActionData(id, actionName);
			return true;
		}

		public bool TrySetAsAxis(RebindableID id, string actionName)
		{
			return TrySetAsAxis(actionName, id);
		}

		public bool TrySetAsAxis(string actionName, RebindableID id = RebindableID.UNDEFINED)
		{
			if (DataType != CommandDataType.Axis)
			{
				return false;
			}
			Primary = new InputActionData(id, actionName);
			return true;
		}

		public bool TrySetAsAxis(RebindableID id, string primary, string secondary)
		{
			return TrySetAsAxis(primary, secondary, id);
		}

		public bool TrySetAsAxis(string primary, string secondary, RebindableID id = RebindableID.UNDEFINED)
		{
			if (DataType != CommandDataType.Axis)
			{
				return false;
			}
			Primary = new InputActionData(id, primary, secondary);
			return true;
		}

		public bool TrySetAsComposite(string action1, string action2, string action3, string action4)
		{
			return TrySetAsCompositeInternal(action1, action2, action3, action4);
		}

		public bool TrySetAsComposite(RebindableID id1, string action1, string action2, RebindableID id2, string action3, string action4)
		{
			return TrySetAsCompositeInternal(action1, action2, action3, action4, id1, id2);
		}

		private bool TrySetAsCompositeInternal(string action1, string action2, string action3, string action4, RebindableID id1 = RebindableID.UNDEFINED, RebindableID id2 = RebindableID.UNDEFINED)
		{
			if (DataType != CommandDataType.Composite)
			{
				return false;
			}
			Primary = new InputActionData(id1, action1, action2);
			Secondary = new InputActionData(id2, action3, action4);
			return true;
		}
	}

	public static readonly Dictionary<InputConsts.InputCommandType, InputCommandData> InputCommandDataLookup;

	public static readonly Dictionary<RebindableID, InputCommandData> RebindableCommandDataLookup;

	public static IReadOnlyCollection<InputCommandData> CommandData => InputCommandDataLookup.Values;

	public static IReadOnlyCollection<InputCommandData> RebindableData => RebindableCommandDataLookup.Values;

	static InputCommandDefinitions()
	{
		InputCommandDataLookup = new Dictionary<InputConsts.InputCommandType, InputCommandData>();
		RebindableCommandDataLookup = new Dictionary<RebindableID, InputCommandData>();
		InputCommandData inputCommandData = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.SELECT);
		inputCommandData.TrySetAsBasic("select");
		AddInputCommandData(inputCommandData);
		InputCommandData inputCommandData2 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.ENTER);
		inputCommandData2.TrySetAsBasic("enter");
		AddInputCommandData(inputCommandData2);
		InputCommandData inputCommandData3 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.ENTER2);
		inputCommandData3.TrySetAsBasic("enter2");
		AddInputCommandData(inputCommandData3);
		InputCommandData inputCommandData4 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.CANCEL_REBINDING1);
		inputCommandData4.TrySetAsBasic("cancelRebinding1");
		AddInputCommandData(inputCommandData4);
		InputCommandData inputCommandData5 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.CANCEL_REBINDING2);
		inputCommandData5.TrySetAsBasic("cancelRebinding2");
		AddInputCommandData(inputCommandData5);
		InputCommandData inputCommandData6 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.ESCAPE);
		inputCommandData6.TrySetAsBasic("escape");
		AddInputCommandData(inputCommandData6);
		InputCommandData inputCommandData7 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.CONFIRM);
		inputCommandData7.TrySetAsBasic("confirm");
		AddInputCommandData(inputCommandData7);
		InputCommandData inputCommandData8 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.CONFIRM2);
		inputCommandData8.TrySetAsBasic("confirm2");
		AddInputCommandData(inputCommandData8);
		InputCommandData inputCommandData9 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.UP);
		inputCommandData9.TrySetAsBasic("up");
		AddInputCommandData(inputCommandData9);
		InputCommandData inputCommandData10 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.DOWN);
		inputCommandData10.TrySetAsBasic("down");
		AddInputCommandData(inputCommandData10);
		InputCommandData inputCommandData11 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.RIGHT);
		inputCommandData11.TrySetAsBasic("right");
		AddInputCommandData(inputCommandData11);
		InputCommandData inputCommandData12 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.LEFT);
		inputCommandData12.TrySetAsBasic("left");
		AddInputCommandData(inputCommandData12);
		InputCommandData inputCommandData13 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.MENU_RIGHT);
		inputCommandData13.TrySetAsBasic("menuRight");
		AddInputCommandData(inputCommandData13);
		InputCommandData inputCommandData14 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.MENU_LEFT);
		inputCommandData14.TrySetAsBasic("menuLeft");
		AddInputCommandData(inputCommandData14);
		InputCommandData inputCommandData15 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.SUBMENU_RIGHT);
		inputCommandData15.TrySetAsBasic("submenuRight");
		AddInputCommandData(inputCommandData15);
		InputCommandData inputCommandData16 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.SUBMENU_LEFT);
		inputCommandData16.TrySetAsBasic("submenuLeft");
		AddInputCommandData(inputCommandData16);
		InputCommandData inputCommandData17 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.UP2);
		inputCommandData17.TrySetAsBasic("up2");
		AddInputCommandData(inputCommandData17);
		InputCommandData inputCommandData18 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.DOWN2);
		inputCommandData18.TrySetAsBasic("down2");
		AddInputCommandData(inputCommandData18);
		InputCommandData inputCommandData19 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.RIGHT2);
		inputCommandData19.TrySetAsBasic("right2");
		AddInputCommandData(inputCommandData19);
		InputCommandData inputCommandData20 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.LEFT2);
		inputCommandData20.TrySetAsBasic("left2");
		AddInputCommandData(inputCommandData20);
		InputCommandData inputCommandData21 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TAB);
		inputCommandData21.TrySetAsBasic("tab");
		AddInputCommandData(inputCommandData21);
		InputCommandData inputCommandData22 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TABL);
		inputCommandData22.TrySetAsBasic("tabL");
		AddInputCommandData(inputCommandData22);
		InputCommandData inputCommandData23 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TABR);
		inputCommandData23.TrySetAsBasic("tabR");
		AddInputCommandData(inputCommandData23);
		InputCommandData inputCommandData24 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TABL2);
		inputCommandData24.TrySetAsBasic("tabL2");
		AddInputCommandData(inputCommandData24);
		InputCommandData inputCommandData25 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TABR2);
		inputCommandData25.TrySetAsBasic("tabR2");
		AddInputCommandData(inputCommandData25);
		InputCommandData inputCommandData26 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.SHIFTL);
		inputCommandData26.TrySetAsBasic("shiftL");
		AddInputCommandData(inputCommandData26);
		InputCommandData inputCommandData27 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.SHIFTR);
		inputCommandData27.TrySetAsBasic("shiftR");
		AddInputCommandData(inputCommandData27);
		InputCommandData inputCommandData28 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.FACE_UP);
		inputCommandData28.TrySetAsBasic("faceUp");
		AddInputCommandData(inputCommandData28);
		InputCommandData inputCommandData29 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.FACE_DOWN);
		inputCommandData29.TrySetAsBasic("faceDown");
		AddInputCommandData(inputCommandData29);
		InputCommandData inputCommandData30 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.FACE_LEFT);
		inputCommandData30.TrySetAsBasic("faceLeft");
		AddInputCommandData(inputCommandData30);
		InputCommandData inputCommandData31 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.FACE_RIGHT);
		inputCommandData31.TrySetAsBasic("faceRight");
		AddInputCommandData(inputCommandData31);
		InputCommandData inputCommandData32 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.EXTEND_STICK);
		inputCommandData32.TrySetAsBasic("extendStick");
		AddInputCommandData(inputCommandData32);
		InputCommandData inputCommandData33 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.THRUSTX);
		inputCommandData33.TrySetAsBasic("thrustX");
		AddInputCommandData(inputCommandData33);
		InputCommandData inputCommandData34 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.THRUSTZ);
		inputCommandData34.TrySetAsBasic("thrustZ");
		AddInputCommandData(inputCommandData34);
		InputCommandData inputCommandData35 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.YAW);
		inputCommandData35.TrySetAsBasic("yaw");
		AddInputCommandData(inputCommandData35);
		InputCommandData inputCommandData36 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.PITCH);
		inputCommandData36.TrySetAsBasic("pitch");
		AddInputCommandData(inputCommandData36);
		InputCommandData inputCommandData37 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TAKE_SCREENSHOT);
		inputCommandData37.TrySetAsBasic("takeScreenshot");
		AddInputCommandData(inputCommandData37);
		InputCommandData inputCommandData38 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TAKE_2XSCREENSHOT);
		inputCommandData38.TrySetAsBasic("take2xScreenshot");
		AddInputCommandData(inputCommandData38);
		InputCommandData inputCommandData39 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.SLOWDOWNTIME);
		inputCommandData39.TrySetAsBasic("slowDownTime");
		AddInputCommandData(inputCommandData39);
		InputCommandData inputCommandData40 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.SPEEDUPTIME);
		inputCommandData40.TrySetAsBasic("speedUpTime");
		AddInputCommandData(inputCommandData40);
		InputCommandData inputCommandData41 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TOOL_UP);
		inputCommandData41.TrySetAsBasic("toolOptionUp");
		AddInputCommandData(inputCommandData41);
		InputCommandData inputCommandData42 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TOOL_DOWN);
		inputCommandData42.TrySetAsBasic("toolOptionDown");
		AddInputCommandData(inputCommandData42);
		InputCommandData inputCommandData43 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TOOL_RIGHT);
		inputCommandData43.TrySetAsBasic("toolOptionRight");
		AddInputCommandData(inputCommandData43);
		InputCommandData inputCommandData44 = InputCommandData.CreateCommandData(CommandDataType.Basic, InputConsts.InputCommandType.TOOL_LEFT);
		inputCommandData44.TrySetAsBasic("toolOptionLeft");
		AddInputCommandData(inputCommandData44);
		InputCommandData inputCommandData45 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.PAUSE);
		inputCommandData45.TrySetAsAxis(RebindableID.PAUSE, "pause");
		AddInputCommandData(inputCommandData45);
		InputCommandData inputCommandData46 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.MENU_CONFIRM);
		inputCommandData46.TrySetAsAxis(RebindableID.MENU_CONFIRM, "menuConfirm");
		AddInputCommandData(inputCommandData46);
		InputCommandData inputCommandData47 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.CANCEL);
		inputCommandData47.TrySetAsAxis(RebindableID.MENU_CANCEL, "cancel");
		AddInputCommandData(inputCommandData47);
		InputCommandData inputCommandData48 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.INTERACT);
		inputCommandData48.TrySetAsAxis(RebindableID.INTERACT, "interact");
		AddInputCommandData(inputCommandData48);
		InputCommandData inputCommandData49 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.INTERACT_SECONDARY);
		inputCommandData49.TrySetAsAxis(RebindableID.INTERACT_SECONDARY, "interactSecondary");
		AddInputCommandData(inputCommandData49);
		InputCommandData inputCommandData50 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.JUMP);
		inputCommandData50.TrySetAsAxis(RebindableID.JUMP, "jump");
		AddInputCommandData(inputCommandData50);
		InputCommandData inputCommandData51 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.FLASHLIGHT);
		inputCommandData51.TrySetAsAxis(RebindableID.FLASHLIGHT, "flashlight");
		AddInputCommandData(inputCommandData51);
		InputCommandData inputCommandData52 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.MAP);
		inputCommandData52.TrySetAsAxis(RebindableID.MAP, "map");
		AddInputCommandData(inputCommandData52);
		InputCommandData inputCommandData53 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.SIGNALSCOPE);
		inputCommandData53.TrySetAsAxis(RebindableID.SIGNALSCOPE, "signalscope");
		AddInputCommandData(inputCommandData53);
		InputCommandData inputCommandData54 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.SCOPEVIEW);
		inputCommandData54.TrySetAsAxis(RebindableID.SIGNALSCOPE_CHANGEVIEW, "scopeView");
		AddInputCommandData(inputCommandData54);
		InputCommandData inputCommandData55 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.PROBELAUNCH);
		inputCommandData55.TrySetAsAxis(RebindableID.PROBE, "probeLaunch");
		AddInputCommandData(inputCommandData55);
		InputCommandData inputCommandData56 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.PROBERETRIEVE);
		inputCommandData56.TrySetAsAxis(RebindableID.PROBE_RETRIEVE, "probeRetrieve");
		AddInputCommandData(inputCommandData56);
		InputCommandData inputCommandData57 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.TOOL_PRIMARY);
		inputCommandData57.TrySetAsAxis(RebindableID.TOOL_PRIMARY, "toolActionPrimary");
		AddInputCommandData(inputCommandData57);
		InputCommandData inputCommandData58 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.TOOL_SECONDARY);
		inputCommandData58.TrySetAsAxis(RebindableID.TOOL_SECONDARY, "toolActionSecondary");
		AddInputCommandData(inputCommandData58);
		InputCommandData inputCommandData59 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.THRUST_UP);
		inputCommandData59.TrySetAsAxis(RebindableID.FLIGHT_YPOS, "thrustUp");
		AddInputCommandData(inputCommandData59);
		InputCommandData inputCommandData60 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.THRUST_DOWN);
		inputCommandData60.TrySetAsAxis(RebindableID.FLIGHT_YNEG, "thrustDown");
		AddInputCommandData(inputCommandData60);
		InputCommandData inputCommandData61 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.ROLL_MODE);
		inputCommandData61.TrySetAsAxis(RebindableID.FLIGHT_ROLL, "rollMode");
		AddInputCommandData(inputCommandData61);
		InputCommandData inputCommandData62 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.BOOST);
		inputCommandData62.TrySetAsAxis(RebindableID.FLIGHT_BOOST, "boost");
		AddInputCommandData(inputCommandData62);
		InputCommandData inputCommandData63 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.LOCKON);
		inputCommandData63.TrySetAsAxis(RebindableID.FLIGHT_LOCKON, "lockOn");
		AddInputCommandData(inputCommandData63);
		InputCommandData inputCommandData64 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.MATCH_VELOCITY);
		inputCommandData64.TrySetAsAxis(RebindableID.FLIGHT_MATCHV, "matchVelocity");
		AddInputCommandData(inputCommandData64);
		InputCommandData inputCommandData65 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.LANDING_CAMERA);
		inputCommandData65.TrySetAsAxis(RebindableID.SHIP_LANDCAM, "landingCamera");
		AddInputCommandData(inputCommandData65);
		InputCommandData inputCommandData66 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.AUTOPILOT);
		inputCommandData66.TrySetAsAxis(RebindableID.SHIP_AUTOPILOT, "autopilot");
		AddInputCommandData(inputCommandData66);
		InputCommandData inputCommandData67 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.FREELOOK);
		inputCommandData67.TrySetAsAxis(RebindableID.SHIP_FREELOOK, "freeLook");
		AddInputCommandData(inputCommandData67);
		InputCommandData inputCommandData68 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.SWAP_SHIP_LOG_MODE);
		inputCommandData68.TrySetAsAxis(RebindableID.SHIPLOG_CHANGEMODE, "swapShipLogMode");
		AddInputCommandData(inputCommandData68);
		InputCommandData inputCommandData69 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.MARK_ENTRY_ON_HUD);
		inputCommandData69.TrySetAsAxis(RebindableID.SHIPLOG_MARKENTRY, "markEntryOnHud");
		AddInputCommandData(inputCommandData69);
		InputCommandData inputCommandData70 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.SCROLLING_TEXT);
		inputCommandData70.TrySetAsAxis(RebindableID.SHIPLOG_SCROLL, "scrollLogText");
		AddInputCommandData(inputCommandData70);
		InputCommandData inputCommandData71 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.SET_DEFAULTS);
		inputCommandData71.TrySetAsAxis(RebindableID.SET_DEFAULTS, "setDefaults");
		AddInputCommandData(inputCommandData71);
		InputCommandData inputCommandData72 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.TOOL_X);
		inputCommandData72.TrySetAsAxis(RebindableID.TOOL_X, "toolOptionRight", "toolOptionLeft");
		AddInputCommandData(inputCommandData72);
		InputCommandData inputCommandData73 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.TOOL_Y);
		inputCommandData73.TrySetAsAxis(RebindableID.TOOL_Y, "toolOptionUp", "toolOptionDown");
		AddInputCommandData(inputCommandData73);
		InputCommandData inputCommandData74 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.MOVE_X);
		inputCommandData74.TrySetAsAxis(RebindableID.MOVE_X, "moveRight", "moveLeft");
		AddInputCommandData(inputCommandData74);
		InputCommandData inputCommandData75 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.MOVE_Z);
		inputCommandData75.TrySetAsAxis(RebindableID.MOVE_Y, "moveForward", "moveBackward");
		AddInputCommandData(inputCommandData75);
		InputCommandData inputCommandData76 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.LOOK_X);
		inputCommandData76.TrySetAsAxis(RebindableID.LOOK_X, "lookRight", "lookLeft");
		AddInputCommandData(inputCommandData76);
		InputCommandData inputCommandData77 = InputCommandData.CreateCommandData(CommandDataType.Axis, InputConsts.InputCommandType.LOOK_Y);
		inputCommandData77.TrySetAsAxis(RebindableID.LOOK_Y, "lookUp", "lookDown");
		AddInputCommandData(inputCommandData77);
		InputCommandData inputCommandData78 = InputCommandData.CreateCommandData(CommandDataType.Composite, InputConsts.InputCommandType.MOVE_XZ);
		inputCommandData78.TrySetAsComposite(RebindableID.MOVE_Y, "moveForward", "moveBackward", RebindableID.MOVE_X, "moveRight", "moveLeft");
		AddInputCommandData(inputCommandData78);
		InputCommandData inputCommandData79 = InputCommandData.CreateCommandData(CommandDataType.Composite, InputConsts.InputCommandType.LOOK);
		inputCommandData79.TrySetAsComposite(RebindableID.LOOK_Y, "lookUp", "lookDown", RebindableID.LOOK_X, "lookRight", "lookLeft");
		AddInputCommandData(inputCommandData79);
	}

	private static bool AddInputCommandData(InputCommandData data)
	{
		if (data.DataType == CommandDataType.Undefined)
		{
			return false;
		}
		if (InputCommandDataLookup.ContainsKey(data.CommandType))
		{
			Debug.LogError("Input Data already exists for InputCommandType " + data.CommandType);
			return false;
		}
		if (data.HasRebindable)
		{
			if (data.Primary != null && data.Primary.IsRebindable && !RebindableCommandDataLookup.ContainsKey(data.Primary.ID))
			{
				RebindableCommandDataLookup.Add(data.Primary.ID, data);
			}
			if (data.Secondary != null && data.Secondary.IsRebindable && !RebindableCommandDataLookup.ContainsKey(data.Secondary.ID))
			{
				RebindableCommandDataLookup.Add(data.Secondary.ID, data);
			}
		}
		InputCommandDataLookup.Add(data.CommandType, data);
		return true;
	}

	public static bool TryGetInputCommandData(InputConsts.InputCommandType type, out InputCommandData data)
	{
		return InputCommandDataLookup.TryGetValue(type, out data);
	}

	public static bool TryGetInputCommandData(RebindableID id, out InputCommandData data)
	{
		return RebindableCommandDataLookup.TryGetValue(id, out data);
	}
}
