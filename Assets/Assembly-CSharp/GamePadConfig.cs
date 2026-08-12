using System;

[Serializable]
public class GamePadConfig
{
	public struct JoystickAxisData
	{
		public AxisIdentifier axisId;

		public int axisNum;

		public float invert;

		public bool isTrigger;
	}

	public struct JoystickButtonData
	{
		public JoystickButton buttonId;

		public int buttonNum;
	}

	public string gamepadHardwareName;

	public InputUtil.GamePadPresetConfig originalPreset;

	public InputUtil.ButtonPromptPresetConfig buttonPromptType;

	public JoystickAxisData[] axisData;

	public JoystickButtonData[] buttonData;

	public GamePadConfig(InputUtil.GamePadPresetConfig configType = InputUtil.GamePadPresetConfig.NONE)
	{
		buttonPromptType = InputUtil.ButtonPromptPresetConfig.NONE;
		BuildAsPreset(configType);
	}

	public GamePadConfig Clone(GamePadConfig configToClone)
	{
		return null;
	}

	public void BuildAsPreset(InputUtil.GamePadPresetConfig configType)
	{
		switch (configType)
		{
		case InputUtil.GamePadPresetConfig.XBOX:
		case InputUtil.GamePadPresetConfig.NONE:
			originalPreset = InputUtil.GamePadPresetConfig.XBOX;
			InitializeAsXbox();
			break;
		case InputUtil.GamePadPresetConfig.PS4:
			originalPreset = InputUtil.GamePadPresetConfig.PS4;
			InitializeAsPS4();
			break;
		}
	}

	private void InitializeAsXbox()
	{
		buttonPromptType = InputUtil.ButtonPromptPresetConfig.XBOX;
		buttonData = new JoystickButtonData[14];
		buttonData[0].buttonId = JoystickButton.FaceRight;
		buttonData[0].buttonNum = 1;
		buttonData[1].buttonId = JoystickButton.FaceDown;
		buttonData[1].buttonNum = 0;
		buttonData[2].buttonId = JoystickButton.FaceUp;
		buttonData[2].buttonNum = 3;
		buttonData[3].buttonId = JoystickButton.FaceLeft;
		buttonData[3].buttonNum = 2;
		buttonData[4].buttonId = JoystickButton.LeftBumper;
		buttonData[4].buttonNum = 4;
		buttonData[5].buttonId = JoystickButton.RightBumper;
		buttonData[5].buttonNum = 5;
		buttonData[6].buttonId = JoystickButton.LeftStickClick;
		buttonData[6].buttonNum = 8;
		buttonData[7].buttonId = JoystickButton.RightStickClick;
		buttonData[7].buttonNum = 9;
		buttonData[8].buttonId = JoystickButton.Start;
		buttonData[8].buttonNum = 7;
		buttonData[9].buttonId = JoystickButton.Select;
		buttonData[9].buttonNum = 6;
		buttonData[10].buttonId = JoystickButton.DPadUp;
		buttonData[10].buttonNum = 12;
		buttonData[11].buttonId = JoystickButton.DPadDown;
		buttonData[11].buttonNum = 13;
		buttonData[12].buttonId = JoystickButton.DPadLeft;
		buttonData[12].buttonNum = 14;
		buttonData[13].buttonId = JoystickButton.DPadRight;
		buttonData[13].buttonNum = 15;
		axisData = new JoystickAxisData[10];
		axisData[0].axisId = AxisIdentifier.CTRLR_LSTICKX;
		axisData[0].axisNum = 1;
		axisData[0].invert = 1f;
		axisData[0].isTrigger = false;
		axisData[1].axisId = AxisIdentifier.CTRLR_LSTICKY;
		axisData[1].axisNum = 2;
		axisData[1].invert = -1f;
		axisData[1].isTrigger = false;
		axisData[2].axisId = AxisIdentifier.CTRLR_RSTICKX;
		axisData[2].axisNum = 4;
		axisData[2].invert = 1f;
		axisData[2].isTrigger = false;
		axisData[3].axisId = AxisIdentifier.CTRLR_RSTICKY;
		axisData[3].axisNum = 5;
		axisData[3].invert = -1f;
		axisData[3].isTrigger = false;
		axisData[4].axisId = AxisIdentifier.CTRLR_DPADX;
		axisData[4].axisNum = 6;
		axisData[4].invert = 1f;
		axisData[4].isTrigger = false;
		axisData[5].axisId = AxisIdentifier.CTRLR_DPADY;
		axisData[5].axisNum = 7;
		axisData[5].invert = 1f;
		axisData[5].isTrigger = false;
		axisData[6].axisId = AxisIdentifier.CTRLR_LTRIGGER;
		axisData[6].axisNum = 9;
		axisData[6].invert = 1f;
		axisData[6].isTrigger = true;
		axisData[7].axisId = AxisIdentifier.CTRLR_RTRIGGER;
		axisData[7].axisNum = 10;
		axisData[7].invert = 1f;
		axisData[7].isTrigger = true;
		axisData[8].axisId = AxisIdentifier.CTRLR_CUSTOM;
		axisData[8].axisNum = 3;
		axisData[8].invert = 1f;
		axisData[8].isTrigger = false;
		axisData[9].axisId = AxisIdentifier.CTRLR_CUSTOM;
		axisData[9].axisNum = 8;
		axisData[9].invert = 1f;
		axisData[9].isTrigger = false;
	}

	private void InitializeAsPS4()
	{
		buttonPromptType = InputUtil.ButtonPromptPresetConfig.PS4;
		buttonData = new JoystickButtonData[10];
		buttonData[0].buttonId = JoystickButton.FaceRight;
		buttonData[0].buttonNum = 2;
		buttonData[1].buttonId = JoystickButton.FaceDown;
		buttonData[1].buttonNum = 1;
		buttonData[2].buttonId = JoystickButton.FaceUp;
		buttonData[2].buttonNum = 3;
		buttonData[3].buttonId = JoystickButton.FaceLeft;
		buttonData[3].buttonNum = 0;
		buttonData[4].buttonId = JoystickButton.LeftBumper;
		buttonData[4].buttonNum = 4;
		buttonData[5].buttonId = JoystickButton.RightBumper;
		buttonData[5].buttonNum = 5;
		buttonData[6].buttonId = JoystickButton.LeftStickClick;
		buttonData[6].buttonNum = 10;
		buttonData[7].buttonId = JoystickButton.RightStickClick;
		buttonData[7].buttonNum = 11;
		buttonData[8].buttonId = JoystickButton.Start;
		buttonData[8].buttonNum = 9;
		buttonData[9].buttonId = JoystickButton.Select;
		buttonData[9].buttonNum = 13;
		axisData = new JoystickAxisData[10];
		axisData[0].axisId = AxisIdentifier.CTRLR_LSTICKX;
		axisData[0].axisNum = 1;
		axisData[0].invert = 1f;
		axisData[0].isTrigger = false;
		axisData[1].axisId = AxisIdentifier.CTRLR_LSTICKY;
		axisData[1].axisNum = 2;
		axisData[1].invert = -1f;
		axisData[1].isTrigger = false;
		axisData[2].axisId = AxisIdentifier.CTRLR_RSTICKX;
		axisData[2].axisNum = 3;
		axisData[2].invert = 1f;
		axisData[2].isTrigger = false;
		axisData[3].axisId = AxisIdentifier.CTRLR_RSTICKY;
		axisData[3].axisNum = 6;
		axisData[3].invert = -1f;
		axisData[3].isTrigger = false;
		axisData[4].axisId = AxisIdentifier.CTRLR_DPADX;
		axisData[4].axisNum = 7;
		axisData[4].invert = 1f;
		axisData[4].isTrigger = false;
		axisData[5].axisId = AxisIdentifier.CTRLR_DPADY;
		axisData[5].axisNum = 8;
		axisData[5].invert = 1f;
		axisData[5].isTrigger = false;
		axisData[6].axisId = AxisIdentifier.CTRLR_LTRIGGER;
		axisData[6].axisNum = 4;
		axisData[6].invert = 1f;
		axisData[6].isTrigger = true;
		axisData[7].axisId = AxisIdentifier.CTRLR_RTRIGGER;
		axisData[7].axisNum = 5;
		axisData[7].invert = 1f;
		axisData[7].isTrigger = true;
		axisData[8].axisId = AxisIdentifier.CTRLR_CUSTOM;
		axisData[8].axisNum = 9;
		axisData[8].invert = 1f;
		axisData[8].isTrigger = false;
		axisData[9].axisId = AxisIdentifier.CTRLR_CUSTOM;
		axisData[9].axisNum = 10;
		axisData[9].invert = 1f;
		axisData[9].isTrigger = false;
	}
}
