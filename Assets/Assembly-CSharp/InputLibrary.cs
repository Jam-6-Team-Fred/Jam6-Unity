public static class InputLibrary
{
	public static InputConsts.InputCommandType[] submitCommandTypes = new InputConsts.InputCommandType[3]
	{
		InputConsts.InputCommandType.MENU_CONFIRM,
		InputConsts.InputCommandType.ENTER,
		InputConsts.InputCommandType.ENTER2
	};

	public static IInputCommands select => GetInputCommand(InputConsts.InputCommandType.SELECT);

	public static IInputCommands menuConfirm => GetInputCommand(InputConsts.InputCommandType.MENU_CONFIRM);

	public static IInputCommands enter => GetInputCommand(InputConsts.InputCommandType.ENTER);

	public static IInputCommands enter2 => GetInputCommand(InputConsts.InputCommandType.ENTER2);

	public static IInputCommands cancel => GetInputCommand(InputConsts.InputCommandType.CANCEL);

	public static IInputCommands cancelRebinding1 => GetInputCommand(InputConsts.InputCommandType.CANCEL_REBINDING1);

	public static IInputCommands cancelRebinding2 => GetInputCommand(InputConsts.InputCommandType.CANCEL_REBINDING2);

	public static IInputCommands escape => GetInputCommand(InputConsts.InputCommandType.ESCAPE);

	public static IInputCommands setDefaults => GetInputCommand(InputConsts.InputCommandType.SET_DEFAULTS);

	public static IInputCommands confirm => GetInputCommand(InputConsts.InputCommandType.CONFIRM);

	public static IInputCommands confirm2 => GetInputCommand(InputConsts.InputCommandType.CONFIRM2);

	public static IInputCommands up => GetInputCommand(InputConsts.InputCommandType.UP);

	public static IInputCommands down => GetInputCommand(InputConsts.InputCommandType.DOWN);

	public static IInputCommands right => GetInputCommand(InputConsts.InputCommandType.RIGHT);

	public static IInputCommands left => GetInputCommand(InputConsts.InputCommandType.LEFT);

	public static IInputCommands menuRight => GetInputCommand(InputConsts.InputCommandType.MENU_RIGHT);

	public static IInputCommands menuLeft => GetInputCommand(InputConsts.InputCommandType.MENU_LEFT);

	public static IInputCommands submenuRight => GetInputCommand(InputConsts.InputCommandType.SUBMENU_RIGHT);

	public static IInputCommands submenuLeft => GetInputCommand(InputConsts.InputCommandType.SUBMENU_LEFT);

	public static IInputCommands up2 => GetInputCommand(InputConsts.InputCommandType.UP2);

	public static IInputCommands down2 => GetInputCommand(InputConsts.InputCommandType.DOWN2);

	public static IInputCommands right2 => GetInputCommand(InputConsts.InputCommandType.RIGHT2);

	public static IInputCommands left2 => GetInputCommand(InputConsts.InputCommandType.LEFT2);

	public static IInputCommands tab => GetInputCommand(InputConsts.InputCommandType.TAB);

	public static IInputCommands tabL => GetInputCommand(InputConsts.InputCommandType.TABL);

	public static IInputCommands tabR => GetInputCommand(InputConsts.InputCommandType.TABR);

	public static IInputCommands tabL2 => GetInputCommand(InputConsts.InputCommandType.TABL2);

	public static IInputCommands tabR2 => GetInputCommand(InputConsts.InputCommandType.TABR2);

	public static IInputCommands shiftL => GetInputCommand(InputConsts.InputCommandType.SHIFTL);

	public static IInputCommands shiftR => GetInputCommand(InputConsts.InputCommandType.SHIFTR);

	public static IInputCommands pause => GetInputCommand(InputConsts.InputCommandType.PAUSE);

	public static IInputCommands faceUp => GetInputCommand(InputConsts.InputCommandType.FACE_UP);

	public static IInputCommands faceDown => GetInputCommand(InputConsts.InputCommandType.FACE_DOWN);

	public static IInputCommands faceLeft => GetInputCommand(InputConsts.InputCommandType.FACE_LEFT);

	public static IInputCommands faceRight => GetInputCommand(InputConsts.InputCommandType.FACE_RIGHT);

	public static IInputCommands swapShipLogMode => GetInputCommand(InputConsts.InputCommandType.INTERACT_SECONDARY);

	public static IInputCommands markEntryOnHUD => GetInputCommand(InputConsts.InputCommandType.JUMP);

	public static IInputCommands scrollLogText => GetInputCommand(InputConsts.InputCommandType.LOOK_Y);

	public static IInputCommands map => GetInputCommand(InputConsts.InputCommandType.MAP);

	public static IInputCommands mapZoomIn => GetInputCommand(InputConsts.InputCommandType.THRUST_UP);

	public static IInputCommands mapZoomOut => GetInputCommand(InputConsts.InputCommandType.THRUST_DOWN);

	public static IInputCommands moveXZ => GetInputCommand(InputConsts.InputCommandType.MOVE_XZ);

	public static IInputCommands look => GetInputCommand(InputConsts.InputCommandType.LOOK);

	public static IInputCommands jump => GetInputCommand(InputConsts.InputCommandType.JUMP);

	public static IInputCommands interact => GetInputCommand(InputConsts.InputCommandType.INTERACT);

	public static IInputCommands interactSecondary => GetInputCommand(InputConsts.InputCommandType.INTERACT_SECONDARY);

	public static IInputCommands flashlight => GetInputCommand(InputConsts.InputCommandType.FLASHLIGHT);

	public static IInputCommands extendStick => GetInputCommand(InputConsts.InputCommandType.THRUST_UP);

	public static IInputCommands toolActionPrimary => GetInputCommand(InputConsts.InputCommandType.TOOL_PRIMARY);

	public static IInputCommands toolActionSecondary => GetInputCommand(InputConsts.InputCommandType.TOOL_SECONDARY);

	public static IInputCommands toolOptionX => GetInputCommand(InputConsts.InputCommandType.TOOL_X);

	public static IInputCommands toolOptionY => GetInputCommand(InputConsts.InputCommandType.TOOL_Y);

	public static IInputCommands toolOptionRight => GetInputCommand(InputConsts.InputCommandType.TOOL_RIGHT);

	public static IInputCommands toolOptionLeft => GetInputCommand(InputConsts.InputCommandType.TOOL_LEFT);

	public static IInputCommands toolOptionUp => GetInputCommand(InputConsts.InputCommandType.TOOL_UP);

	public static IInputCommands toolOptionDown => GetInputCommand(InputConsts.InputCommandType.TOOL_DOWN);

	public static IInputCommands signalscope => GetInputCommand(InputConsts.InputCommandType.SIGNALSCOPE);

	public static IInputCommands probeLaunch => GetInputCommand(InputConsts.InputCommandType.TOOL_PRIMARY);

	public static IInputCommands probeRetrieve => GetInputCommand(InputConsts.InputCommandType.PROBERETRIEVE);

	public static IInputCommands rollMode => GetInputCommand(InputConsts.InputCommandType.ROLL_MODE);

	public static IInputCommands boost => GetInputCommand(InputConsts.InputCommandType.BOOST);

	public static IInputCommands thrustX => GetInputCommand(InputConsts.InputCommandType.MOVE_X);

	public static IInputCommands thrustZ => GetInputCommand(InputConsts.InputCommandType.MOVE_Z);

	public static IInputCommands thrustUp => GetInputCommand(InputConsts.InputCommandType.THRUST_UP);

	public static IInputCommands thrustDown => GetInputCommand(InputConsts.InputCommandType.THRUST_DOWN);

	public static IInputCommands yaw => GetInputCommand(InputConsts.InputCommandType.LOOK_X);

	public static IInputCommands pitch => GetInputCommand(InputConsts.InputCommandType.LOOK_Y);

	public static IInputCommands lockOn => GetInputCommand(InputConsts.InputCommandType.LOCKON);

	public static IInputCommands matchVelocity => GetInputCommand(InputConsts.InputCommandType.MATCH_VELOCITY);

	public static IInputCommands landingCamera => GetInputCommand(InputConsts.InputCommandType.LANDING_CAMERA);

	public static IInputCommands autopilot => GetInputCommand(InputConsts.InputCommandType.AUTOPILOT);

	public static IInputCommands freeLook => GetInputCommand(InputConsts.InputCommandType.FREELOOK);

	public static IInputCommands takeScreenshot => GetInputCommand(InputConsts.InputCommandType.TAKE_SCREENSHOT);

	public static IInputCommands take2xScreenshot => GetInputCommand(InputConsts.InputCommandType.TAKE_2XSCREENSHOT);

	public static IInputCommands slowDownTime => GetInputCommand(InputConsts.InputCommandType.SLOWDOWNTIME);

	public static IInputCommands speedUpTime => GetInputCommand(InputConsts.InputCommandType.SPEEDUPTIME);

	public static IInputCommands GetInputCommand(InputConsts.InputCommandType type)
	{
		InputCommandManager.MappedInputActions.TryGetValue(type, out var value);
		return value;
	}
}
