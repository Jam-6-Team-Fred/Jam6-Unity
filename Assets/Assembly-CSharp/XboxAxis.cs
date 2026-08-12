public static class XboxAxis
{
	public static readonly SingleAxis rightStickX = new SingleAxis(AxisIdentifier.CTRLR_RSTICKX, "RightStick_X_PC", "RightStick_X_Mac", 0.2f, 0.05f);

	public static readonly SingleAxis rightStickY = new SingleAxis(AxisIdentifier.CTRLR_RSTICKY, "RightStick_Y_PC", "RightStick_Y_Mac", 0.2f, 0.05f);

	public static readonly SingleAxis leftStickX = new SingleAxis(AxisIdentifier.CTRLR_LSTICKX, "LeftStick_X_PC", "LeftStick_X_Mac", 0.2f, 0.05f);

	public static readonly SingleAxis leftStickY = new SingleAxis(AxisIdentifier.CTRLR_LSTICKY, "LeftStick_Y_PC", "LeftStick_Y_Mac", 0.2f, 0.05f);

	public static readonly SingleAxis rightTrigger = new SingleAxis(AxisIdentifier.CTRLR_RTRIGGER, "RightTrigger_PC", "RightTrigger_Mac", 0.1f, 0.05f);

	public static readonly SingleAxis leftTrigger = new SingleAxis(AxisIdentifier.CTRLR_LTRIGGER, "LeftTrigger_PC", "LeftTrigger_Mac", 0.1f, 0.05f);

	public static readonly SingleAxis dPadX = new SingleAxis(AxisIdentifier.CTRLR_DPADX, "DPad_X_PC", JoystickButton.DPadRight, JoystickButton.DPadLeft);

	public static readonly SingleAxis dPadY = new SingleAxis(AxisIdentifier.CTRLR_DPADY, "DPad_Y_PC", JoystickButton.DPadUp, JoystickButton.DPadDown);

	public static readonly DoubleAxis rightStick = new DoubleAxis(AxisIdentifier.CTRLR_RSTICK, rightStickX, rightStickY, 0.2f, 0.05f);

	public static readonly DoubleAxis leftStick = new DoubleAxis(AxisIdentifier.CTRLR_LSTICK, leftStickX, leftStickY, 0.2f, 0.05f);

	public static readonly SingleAxis[] singleAxisList = new SingleAxis[8] { rightStickX, rightStickY, leftStickX, leftStickY, rightTrigger, leftTrigger, dPadX, dPadY };

	public static readonly string[] axisNames = new string[72]
	{
		"RightStick_X_PC", "RightStick_X_PC_1", "RightStick_X_PC_2", "RightStick_X_PC_3", "RightStick_X_PC_4", "RightStick_X_PC_5", "RightStick_X_PC_6", "RightStick_X_PC_7", "RightStick_X_PC_8", "RightStick_Y_PC",
		"RightStick_Y_PC_1", "RightStick_Y_PC_2", "RightStick_Y_PC_3", "RightStick_Y_PC_4", "RightStick_Y_PC_5", "RightStick_Y_PC_6", "RightStick_Y_PC_7", "RightStick_Y_PC_8", "LeftStick_X_PC", "LeftStick_X_PC_1",
		"LeftStick_X_PC_2", "LeftStick_X_PC_3", "LeftStick_X_PC_4", "LeftStick_X_PC_5", "LeftStick_X_PC_6", "LeftStick_X_PC_7", "LeftStick_X_PC_8", "LeftStick_Y_PC", "LeftStick_Y_PC_1", "LeftStick_Y_PC_2",
		"LeftStick_Y_PC_3", "LeftStick_Y_PC_4", "LeftStick_Y_PC_5", "LeftStick_Y_PC_6", "LeftStick_Y_PC_7", "LeftStick_Y_PC_8", "RightTrigger_PC", "RightTrigger_PC_1", "RightTrigger_PC_2", "RightTrigger_PC_3",
		"RightTrigger_PC_4", "RightTrigger_PC_5", "RightTrigger_PC_6", "RightTrigger_PC_7", "RightTrigger_PC_8", "LeftTrigger_PC", "LeftTrigger_PC_1", "LeftTrigger_PC_2", "LeftTrigger_PC_3", "LeftTrigger_PC_4",
		"LeftTrigger_PC_5", "LeftTrigger_PC_6", "LeftTrigger_PC_7", "LeftTrigger_PC_8", "DPad_X_PC", "DPad_X_PC_1", "DPad_X_PC_2", "DPad_X_PC_3", "DPad_X_PC_4", "DPad_X_PC_5",
		"DPad_X_PC_6", "DPad_X_PC_7", "DPad_X_PC_8", "DPad_Y_PC", "DPad_Y_PC_1", "DPad_Y_PC_2", "DPad_Y_PC_3", "DPad_Y_PC_4", "DPad_Y_PC_5", "DPad_Y_PC_6",
		"DPad_Y_PC_7", "DPad_Y_PC_8"
	};

	public static void UpdateAxes()
	{
		rightStickX.UpdateAxis();
		rightStickY.UpdateAxis();
		leftStickX.UpdateAxis();
		leftStickY.UpdateAxis();
		rightTrigger.UpdateAxis();
		leftTrigger.UpdateAxis();
		dPadX.UpdateAxis();
		dPadY.UpdateAxis();
		rightStick.UpdateAxis();
		leftStick.UpdateAxis();
	}

	public static void ChangeDeadZones(float innerDeadZone, float outerDeadZone)
	{
		rightStickX.SetInnerDeadZoneMultiplier(innerDeadZone);
		rightStickX.SetOuterDeadZoneMultiplier(outerDeadZone);
		rightStickY.SetInnerDeadZoneMultiplier(innerDeadZone);
		rightStickY.SetOuterDeadZoneMultiplier(outerDeadZone);
		leftStickX.SetInnerDeadZoneMultiplier(innerDeadZone);
		leftStickX.SetOuterDeadZoneMultiplier(outerDeadZone);
		leftStickY.SetInnerDeadZoneMultiplier(innerDeadZone);
		leftStickY.SetOuterDeadZoneMultiplier(outerDeadZone);
		rightStick.SetInnerDeadZoneMultiplier(innerDeadZone);
		rightStick.SetOuterDeadZoneMultiplier(outerDeadZone);
		leftStick.SetInnerDeadZoneMultiplier(innerDeadZone);
		leftStick.SetOuterDeadZoneMultiplier(outerDeadZone);
	}
}
