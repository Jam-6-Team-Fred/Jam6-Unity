using System.Collections.Generic;

public class RebindableLookup
{
	public UITextType[] _rebindableStringRepresentation;

	private static UITextType[] s_listV3TextTypeIDs;

	private static UITextType[] s_listV4TextTypeIDs;

	private static UITextType[] s_listV4TextTypeTooltips;

	private static RebindableID[] s_v3IdList;

	private static RebindableID[] s_v4IdList;

	private static InputConsts.InputCommandType[] s_v4NewInputIdList;

	private static bool s_bSerializationLookupInit = false;

	private static RebindableLookup s_instance;

	private static Dictionary<RebindableID, string[]> s_rebindableCompositePartsDictionary = new Dictionary<RebindableID, string[]>
	{
		{
			RebindableID.MOVE_X,
			new string[3] { "left", "right", "Horizontal" }
		},
		{
			RebindableID.MOVE_Y,
			new string[3] { "up", "down", "Vertical" }
		},
		{
			RebindableID.LOOK_X,
			new string[3] { "left", "right", "Horizontal" }
		},
		{
			RebindableID.LOOK_Y,
			new string[3] { "up", "down", "Vertical" }
		},
		{
			RebindableID.MAP_ZOOMOUT,
			new string[1] { "negative" }
		},
		{
			RebindableID.MAP_ZOOMIN,
			new string[1] { "positive" }
		}
	};

	public static RebindableLookup SharedInstance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new RebindableLookup();
				s_instance.InitStringLookupTable();
			}
			return s_instance;
		}
	}

	private void InitStringLookupTable()
	{
		s_listV4TextTypeIDs = new UITextType[40]
		{
			UITextType.RebindPause,
			UITextType.KeyRebindingConfirm,
			UITextType.RebindCancel,
			UITextType.KeyRebindingMoveXAxis,
			UITextType.KeyRebindingMoveYAxis,
			UITextType.KeyRebindingLookXAxis,
			UITextType.KeyRebindingLookYAxis,
			UITextType.KeyRebindingInteractPrimary,
			UITextType.JumpPrompt,
			UITextType.FlashlightPrompt,
			UITextType.MapPrompt,
			UITextType.KeyRebindingMapZoomIn,
			UITextType.KeyRebindingMapZoomOut,
			UITextType.TranslatorUsePrompt,
			UITextType.RebindSignalscope,
			UITextType.RebindScopeZoom,
			UITextType.KeyRebindingLaunchRetrieveScout,
			UITextType.KeyRebindingToolPrimary,
			UITextType.KeyRebindingToolSecondary,
			UITextType.KeyRebindingToolXAxis,
			UITextType.KeyRebindingToolYAxis,
			UITextType.UpPrompt,
			UITextType.DownPrompt,
			UITextType.KeyRebindingRollMode,
			UITextType.KeyRebindingJetpackBoost,
			UITextType.LockOnPrompt,
			UITextType.MatchVelocityPrompt,
			UITextType.KeyRebindingLandingCamera,
			UITextType.RebindAutopilot,
			UITextType.KeyRebindingCockpitFreeLook,
			UITextType.KeyRebindingChangeShipLogMode,
			UITextType.KeyRebindingMarkEntryOnHUD,
			UITextType.KeyRebindingScrollShipLogText,
			UITextType.CampfireDozeOff,
			UITextType.MenuResetToDefault,
			UITextType.SuitMenuTitle,
			UITextType.KeyRebindingInteractSecondary,
			UITextType.KeyRebindingRetrieveScout,
			UITextType.KeyRebindingToolXAxis,
			UITextType.KeyRebindingToolYAxis
		};
		s_v4IdList = new RebindableID[38]
		{
			RebindableID.PAUSE,
			RebindableID.MENU_CONFIRM,
			RebindableID.MENU_CANCEL,
			RebindableID.MOVE_X,
			RebindableID.MOVE_Y,
			RebindableID.LOOK_X,
			RebindableID.LOOK_Y,
			RebindableID.INTERACT,
			RebindableID.JUMP,
			RebindableID.FLASHLIGHT,
			RebindableID.MAP,
			RebindableID.MAP_ZOOMIN,
			RebindableID.MAP_ZOOMOUT,
			RebindableID.DEPRECATED,
			RebindableID.SIGNALSCOPE,
			RebindableID.DEPRECATED,
			RebindableID.PROBE,
			RebindableID.TOOL_PRIMARY,
			RebindableID.TOOL_SECONDARY,
			RebindableID.TOOL_X,
			RebindableID.TOOL_Y,
			RebindableID.FLIGHT_YPOS,
			RebindableID.FLIGHT_YNEG,
			RebindableID.FLIGHT_ROLL,
			RebindableID.FLIGHT_BOOST,
			RebindableID.FLIGHT_LOCKON,
			RebindableID.FLIGHT_MATCHV,
			RebindableID.SHIP_LANDCAM,
			RebindableID.SHIP_AUTOPILOT,
			RebindableID.SHIP_FREELOOK,
			RebindableID.SHIPLOG_CHANGEMODE,
			RebindableID.SHIPLOG_MARKENTRY,
			RebindableID.SHIPLOG_SCROLL,
			RebindableID.CAMPFIRE_SLEEP,
			RebindableID.SET_DEFAULTS,
			RebindableID.SUIT_MENU,
			RebindableID.INTERACT_SECONDARY,
			RebindableID.PROBE_RETRIEVE
		};
		s_listV4TextTypeTooltips = new UITextType[38]
		{
			UITextType.Tooltip_Rebind_Pause,
			UITextType.Tooltip_Rebind_Confirm,
			UITextType.Tooltip_Rebind_Cancel,
			UITextType.Tooltip_Rebind_MoveX,
			UITextType.Tooltip_Rebind_MoveY,
			UITextType.Tooltip_Rebind_LookX,
			UITextType.Tooltip_Rebind_LookY,
			UITextType.Tooltip_Rebind_Interact,
			UITextType.Tooltip_Rebind_Jump,
			UITextType.Tooltip_Rebind_Flashlight,
			UITextType.Tooltip_Rebind_Map,
			UITextType.Tooltip_Rebind_MapZoomIn,
			UITextType.Tooltip_Rebind_MapZoomOut,
			UITextType.None,
			UITextType.Tooltip_Rebind_Signalscope,
			UITextType.None,
			UITextType.Tooltip_Rebind_Scout,
			UITextType.Tooltip_Rebind_ToolPrimary,
			UITextType.Tooltip_Rebind_ToolSecondary,
			UITextType.Tooltip_Rebind_ToolX,
			UITextType.Tooltip_Rebind_ToolY,
			UITextType.Tooltip_Rebind_ThrustUp,
			UITextType.Tooltip_Rebind_ThrustDown,
			UITextType.Tooltip_Rebind_Roll,
			UITextType.Tooltip_Rebind_Boost,
			UITextType.Tooltip_Rebind_LockOn,
			UITextType.Tooltip_Rebind_MatchV,
			UITextType.Tooltip_Rebind_LandingCam,
			UITextType.Tooltip_Rebind_Autopilot,
			UITextType.Tooltip_Rebind_FreeLook,
			UITextType.Tooltip_Rebind_ShipLogMode,
			UITextType.Tooltip_Rebind_ShipLogMarker,
			UITextType.Tooltip_Rebind_ShipLogScroll,
			UITextType.None,
			UITextType.None,
			UITextType.None,
			UITextType.None,
			UITextType.None
		};
		s_v4NewInputIdList = new InputConsts.InputCommandType[38]
		{
			InputConsts.InputCommandType.PAUSE,
			InputConsts.InputCommandType.MENU_CONFIRM,
			InputConsts.InputCommandType.CANCEL,
			InputConsts.InputCommandType.MOVE_X,
			InputConsts.InputCommandType.MOVE_Z,
			InputConsts.InputCommandType.LOOK_X,
			InputConsts.InputCommandType.LOOK_Y,
			InputConsts.InputCommandType.INTERACT,
			InputConsts.InputCommandType.JUMP,
			InputConsts.InputCommandType.FLASHLIGHT,
			InputConsts.InputCommandType.MAP,
			InputConsts.InputCommandType.MAP_ZOOM,
			InputConsts.InputCommandType.MAP_ZOOM,
			InputConsts.InputCommandType.DEPRECATED,
			InputConsts.InputCommandType.SIGNALSCOPE,
			InputConsts.InputCommandType.DEPRECATED,
			InputConsts.InputCommandType.PROBELAUNCH,
			InputConsts.InputCommandType.TOOL_PRIMARY,
			InputConsts.InputCommandType.TOOL_SECONDARY,
			InputConsts.InputCommandType.TOOL_RIGHT,
			InputConsts.InputCommandType.TOOL_UP,
			InputConsts.InputCommandType.THRUST_UP,
			InputConsts.InputCommandType.THRUST_DOWN,
			InputConsts.InputCommandType.ROLL_MODE,
			InputConsts.InputCommandType.BOOST,
			InputConsts.InputCommandType.LOCKON,
			InputConsts.InputCommandType.MATCH_VELOCITY,
			InputConsts.InputCommandType.LANDING_CAMERA,
			InputConsts.InputCommandType.AUTOPILOT,
			InputConsts.InputCommandType.FREELOOK,
			InputConsts.InputCommandType.SWAP_SHIP_LOG_MODE,
			InputConsts.InputCommandType.MARK_ENTRY_ON_HUD,
			InputConsts.InputCommandType.SCROLLING_TEXT,
			InputConsts.InputCommandType.DEPRECATED,
			InputConsts.InputCommandType.SET_DEFAULTS,
			InputConsts.InputCommandType.DEPRECATED,
			InputConsts.InputCommandType.INTERACT_SECONDARY,
			InputConsts.InputCommandType.PROBERETRIEVE
		};
		_rebindableStringRepresentation = s_listV4TextTypeIDs;
	}

	public void InitSerializationLookup()
	{
		s_bSerializationLookupInit = true;
		s_listV3TextTypeIDs = new UITextType[36]
		{
			UITextType.RebindPause,
			UITextType.KeyRebindingConfirm,
			UITextType.RebindCancel,
			UITextType.KeyRebindingMoveXAxis,
			UITextType.KeyRebindingMoveYAxis,
			UITextType.KeyRebindingLookXAxis,
			UITextType.KeyRebindingLookYAxis,
			UITextType.RebindX,
			UITextType.JumpPrompt,
			UITextType.FlashlightPrompt,
			UITextType.MapPrompt,
			UITextType.KeyRebindingMapZoomIn,
			UITextType.KeyRebindingMapZoomOut,
			UITextType.TranslatorUsePrompt,
			UITextType.RebindSignalscope,
			UITextType.RebindScopeZoom,
			UITextType.KeyRebindingLaunchScoutForwardSnapshot,
			UITextType.ProbeRetrievePrompt,
			UITextType.RebindRearSnap,
			UITextType.KeyRebindingToolXAxis,
			UITextType.KeyRebindingToolYAxis,
			UITextType.UpPrompt,
			UITextType.DownPrompt,
			UITextType.KeyRebindingRollMode,
			UITextType.KeyRebindingJetpackBoost,
			UITextType.LockOnPrompt,
			UITextType.MatchVelocityPrompt,
			UITextType.KeyRebindingLandingCamera,
			UITextType.RebindAutopilot,
			UITextType.KeyRebindingCockpitFreeLook,
			UITextType.KeyRebindingChangeShipLogMode,
			UITextType.KeyRebindingMarkEntryOnHUD,
			UITextType.KeyRebindingScrollShipLogText,
			UITextType.CampfireDozeOff,
			UITextType.MenuResetToDefault,
			UITextType.SuitMenuTitle
		};
		s_v3IdList = new RebindableID[36]
		{
			RebindableID.PAUSE,
			RebindableID.MENU_CONFIRM,
			RebindableID.MENU_CANCEL,
			RebindableID.MOVE_X,
			RebindableID.MOVE_Y,
			RebindableID.LOOK_X,
			RebindableID.LOOK_Y,
			RebindableID.INTERACT,
			RebindableID.JUMP,
			RebindableID.FLASHLIGHT,
			RebindableID.MAP,
			RebindableID.MAP_ZOOMIN,
			RebindableID.MAP_ZOOMOUT,
			RebindableID.TRANSLATOR,
			RebindableID.SIGNALSCOPE,
			RebindableID.SIGNALSCOPE_CHANGEVIEW,
			RebindableID.PROBE,
			RebindableID.PROBE_RETRIEVE,
			RebindableID.PROBE_REVERSE,
			RebindableID.TOOL_RIGHT,
			RebindableID.TOOL_UP,
			RebindableID.FLIGHT_YPOS,
			RebindableID.FLIGHT_YNEG,
			RebindableID.FLIGHT_ROLL,
			RebindableID.FLIGHT_BOOST,
			RebindableID.FLIGHT_LOCKON,
			RebindableID.FLIGHT_MATCHV,
			RebindableID.SHIP_LANDCAM,
			RebindableID.SHIP_AUTOPILOT,
			RebindableID.SHIP_FREELOOK,
			RebindableID.SHIPLOG_CHANGEMODE,
			RebindableID.SHIPLOG_MARKENTRY,
			RebindableID.SHIPLOG_SCROLL,
			RebindableID.CAMPFIRE_SLEEP,
			RebindableID.SET_DEFAULTS,
			RebindableID.SUIT_MENU
		};
	}

	public UITextType LookupRebindableMenuString(RebindableID rebindableId)
	{
		int num = -1;
		for (int i = 0; i < s_v4IdList.Length; i++)
		{
			if (s_v4IdList[i] == rebindableId)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return UITextType.None;
		}
		return s_listV4TextTypeIDs[num];
	}

	public UITextType LookupRebindableMenuTooltip(RebindableID rebindableId)
	{
		int num = -1;
		for (int i = 0; i < s_v4IdList.Length; i++)
		{
			if (s_v4IdList[i] == rebindableId)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return UITextType.None;
		}
		return s_listV4TextTypeTooltips[num];
	}

	public InputConsts.InputCommandType LookupInputCommandType(RebindableID rebindableId)
	{
		int num = -1;
		for (int i = 0; i < s_v4IdList.Length; i++)
		{
			if (s_v4IdList[i] == rebindableId)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return InputConsts.InputCommandType.UNDEFINED;
		}
		return s_v4NewInputIdList[num];
	}

	public string[] LookupInputCompositeParts(RebindableID rebindableId)
	{
		if (s_rebindableCompositePartsDictionary.TryGetValue(rebindableId, out var value))
		{
			return value;
		}
		return null;
	}

	public AxisIdentifier LookupPatch6ToPatch7AxisIdentifier(AxisIdentifier p6AxisIdentifier)
	{
		if (p6AxisIdentifier >= AxisIdentifier.CTRLR_FULLDPAD)
		{
			return p6AxisIdentifier + 1;
		}
		return p6AxisIdentifier;
	}

	public RebindableID LookupV3ToV4RebindableID(UITextType v3TextType)
	{
		if (!s_bSerializationLookupInit)
		{
			InitSerializationLookup();
		}
		int num = -1;
		for (int i = 0; i < s_listV3TextTypeIDs.Length; i++)
		{
			if (s_listV3TextTypeIDs[i] == v3TextType)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return RebindableID.UNDEFINED;
		}
		return s_v4IdList[num];
	}

	public RebindableID LookupV3RebindableID(UITextType v3TextType)
	{
		if (!s_bSerializationLookupInit)
		{
			InitSerializationLookup();
		}
		int num = -1;
		for (int i = 0; i < s_listV3TextTypeIDs.Length; i++)
		{
			if (s_listV3TextTypeIDs[i] == v3TextType)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return RebindableID.UNDEFINED;
		}
		return s_v3IdList[num];
	}

	public static bool IsXAxisRebindable(RebindableID id)
	{
		if (id != RebindableID.MOVE_X && id != RebindableID.LOOK_X)
		{
			return id == RebindableID.TOOL_X;
		}
		return true;
	}
}
