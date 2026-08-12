using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class ButtonPromptLibrary
{
	public delegate void UpdateButtonPromptConfigEvent();

	public static float BUTTONIMAGE_MIN_REFRESH_TIME = 0.1f;

	private static ButtonPromptLibrary s_instance;

	private static bool s_usingSteamdeck = false;

	private static Texture2D s_faceLeft;

	private static Texture2D s_faceTop;

	private static Texture2D s_faceBottom;

	private static Texture2D s_faceRight;

	private static Texture2D s_rightTrigger;

	private static Texture2D s_leftTrigger;

	private static Texture2D s_rightStick;

	private static Texture2D s_rightStickHorz;

	private static Texture2D s_rightStickVert;

	private static Texture2D s_leftStick;

	private static Texture2D s_leftStickHorz;

	private static Texture2D s_leftStickVert;

	private static Texture2D s_rightStickClick;

	private static Texture2D s_leftStickClick;

	private static Texture2D s_start;

	private static Texture2D s_select;

	private static Texture2D s_share;

	private static Texture2D s_rightBumper;

	private static Texture2D s_leftBumper;

	private static Texture2D s_dPadAll;

	private static Texture2D s_dPadUp;

	private static Texture2D s_dPadDown;

	private static Texture2D s_dPadVert;

	private static Texture2D s_dPadLeft;

	private static Texture2D s_dPadRight;

	private static Texture2D s_dPadHorz;

	private static Texture2D s_mouseAll;

	private static Texture2D s_mouseHorz;

	private static Texture2D s_mouseVert;

	private static Texture2D s_mouseWheelAll;

	private static Texture2D s_mouseWheelUp;

	private static Texture2D s_mouseWheelDown;

	private static Texture2D[] s_genericJoystick;

	private static Texture2D s_testButton;

	private static Dictionary<KeyCode, Texture2D> s_keyCodeDict;

	private static Dictionary<Texture2D, AxisIdentifier> s_axisTextureDict;

	private static Dictionary<Texture2D, JoystickButton> s_joystickButtonTextureDict;

	private static bool s_initialized = false;

	public static ButtonPromptLibrary SharedInstance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new ButtonPromptLibrary();
				s_instance.Initialize();
			}
			return s_instance;
		}
	}

	public static event UpdateButtonPromptConfigEvent OnUpdateButtonPromptConfig;

	private void Initialize()
	{
		if (!s_initialized)
		{
			s_keyCodeDict = new Dictionary<KeyCode, Texture2D>();
			string text = "";
			text = "ButtonPrompts/Keyboard & Mouse/Keyboard_Black_";
			s_mouseAll = (Texture2D)Resources.Load(text + "Mouse_Simple");
			s_mouseWheelDown = (Texture2D)Resources.Load(text + "Mouse_Down_Scroll");
			s_mouseWheelUp = (Texture2D)Resources.Load(text + "Mouse_Up_Scroll");
			s_mouseHorz = (Texture2D)Resources.Load(text + "Mouse_Horizontal");
			s_mouseVert = (Texture2D)Resources.Load(text + "Mouse_Vertical");
			s_genericJoystick = new Texture2D[20];
			text = "ButtonPrompts/Generic/Button_";
			for (int i = 0; i < s_genericJoystick.Length; i++)
			{
				s_genericJoystick[i] = (Texture2D)Resources.Load(text + i.ToString("00"));
			}
			s_testButton = (Texture2D)Resources.Load("ButtonPrompts/Keyboard & Mouse/Blanks/Blank_Black_Normal");
			s_usingSteamdeck = SteamUtils.IsSteamRunningOnSteamDeck();
			s_axisTextureDict = new Dictionary<Texture2D, AxisIdentifier>();
			s_joystickButtonTextureDict = new Dictionary<Texture2D, JoystickButton>();
			SetConfigTextures(PlayerData.GetButtonPromptImageSetting());
			s_initialized = true;
		}
	}

	private void OnButtonPresetChanged()
	{
		SetConfigTextures(PlayerData.GetButtonPromptImageSetting());
	}

	public void SetConfigTextures(ButtonPromptImgSet promptImageSet)
	{
		switch (promptImageSet)
		{
		case ButtonPromptImgSet.DUALSHOCK_4:
			s_rightStick = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Right_Stick");
			s_rightStickHorz = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Right_Stick_Horizontal");
			s_rightStickVert = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Right_Stick_Vertical");
			s_faceLeft = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Square");
			s_faceTop = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Triangle");
			s_faceBottom = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Cross");
			s_faceRight = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Circle");
			s_rightTrigger = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_R2");
			s_leftTrigger = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_L2");
			s_leftStick = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Left_Stick");
			s_leftStickHorz = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Left_Stick_Horizontal");
			s_leftStickVert = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Left_Stick_Vertical");
			s_start = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Options");
			s_rightBumper = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_R1");
			s_leftBumper = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_L1");
			s_dPadAll = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Dpad");
			s_dPadHorz = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Dpad_Horizontal");
			s_dPadVert = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Dpad_Vertical");
			s_dPadUp = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Dpad_Up");
			s_dPadDown = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Dpad_Down");
			s_dPadLeft = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Dpad_Left");
			s_dPadRight = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Dpad_Right");
			s_rightStickClick = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Right_Stick_Click");
			s_leftStickClick = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Left_Stick_Click");
			s_select = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Touch_Pad");
			s_share = (Texture2D)Resources.Load("ButtonPrompts/PS4/" + "PS4_Share");
			break;
		case ButtonPromptImgSet.SWITCH_PRO:
			s_rightStick = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Right_Stick");
			s_rightStickHorz = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Right_Stick_Horizontal");
			s_rightStickVert = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Right_Stick_Vertical");
			s_faceLeft = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Y");
			s_faceTop = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_X");
			s_faceBottom = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_B");
			s_faceRight = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_A");
			s_rightTrigger = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_RT");
			s_leftTrigger = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_LT");
			s_leftStick = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Left_Stick");
			s_leftStickHorz = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Left_Stick_Horizontal");
			s_leftStickVert = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Left_Stick_Vertical");
			s_start = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Plus");
			s_rightBumper = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_RB");
			s_leftBumper = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_LB");
			s_dPadAll = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Dpad");
			s_dPadHorz = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Dpad_Horizontal");
			s_dPadVert = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Dpad_Vertical");
			s_dPadUp = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Dpad_Up");
			s_dPadDown = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Dpad_Down");
			s_dPadLeft = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Dpad_Left");
			s_dPadRight = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Dpad_Right");
			s_rightStickClick = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Right_Stick_Click");
			s_leftStickClick = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Left_Stick_Click");
			s_select = (Texture2D)Resources.Load("ButtonPrompts/Switch/" + "Switch_Minus");
			s_share = s_testButton;
			break;
		case ButtonPromptImgSet.XBOX_ONE:
			s_rightStick = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Right_Stick");
			s_rightStickHorz = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Right_Stick_Horizontal");
			s_rightStickVert = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Right_Stick_Vertical");
			s_faceLeft = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_X");
			s_faceTop = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Y");
			s_faceBottom = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_A");
			s_faceRight = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_B");
			s_rightTrigger = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_RT");
			s_leftTrigger = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_LT");
			s_leftStick = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Left_Stick");
			s_leftStickHorz = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Left_Stick_Horizontal");
			s_leftStickVert = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Left_Stick_Vertical");
			s_start = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Menu");
			s_rightBumper = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_RB");
			s_leftBumper = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_LB");
			s_dPadAll = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Dpad");
			s_dPadHorz = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Dpad_Horizontal");
			s_dPadVert = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Dpad_Vertical");
			s_dPadUp = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Dpad_Up");
			s_dPadDown = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Dpad_Down");
			s_dPadLeft = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Dpad_Left");
			s_dPadRight = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Dpad_Right");
			s_rightStickClick = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Right_Stick_Click");
			s_leftStickClick = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Left_Stick_Click");
			s_select = (Texture2D)Resources.Load("ButtonPrompts/Xbox One/" + "XboxOne_Windows");
			s_share = s_testButton;
			break;
		case ButtonPromptImgSet.DUALSENSEPS5:
			s_rightStick = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Right_Stick");
			s_rightStickHorz = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Right_Stick_Horizontal");
			s_rightStickVert = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Right_Stick_Vertical");
			s_faceLeft = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Square");
			s_faceTop = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Triangle");
			s_faceBottom = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Cross");
			s_faceRight = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Circle");
			s_rightTrigger = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_R2");
			s_leftTrigger = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_L2");
			s_leftStick = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Left_Stick");
			s_leftStickHorz = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Left_Stick_Horizontal");
			s_leftStickVert = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Left_Stick_Vertical");
			s_start = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Options");
			s_rightBumper = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_R1");
			s_leftBumper = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_L1");
			s_dPadAll = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Dpad");
			s_dPadHorz = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Dpad_Horizontal");
			s_dPadVert = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Dpad_Vertical");
			s_dPadUp = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Dpad_Up");
			s_dPadDown = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Dpad_Down");
			s_dPadLeft = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Dpad_Left");
			s_dPadRight = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Dpad_Right");
			s_rightStickClick = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Right_Stick_Click");
			s_leftStickClick = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Left_Stick_Click");
			s_select = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Touch_Pad");
			s_share = (Texture2D)Resources.Load("ButtonPrompts/PS5/" + "PS5_Share");
			break;
		case ButtonPromptImgSet.XBOX_SERIES:
			s_rightStick = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Right_Stick");
			s_rightStickHorz = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Right_Stick_Horizontal");
			s_rightStickVert = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Right_Stick_Vertical");
			s_faceLeft = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_X");
			s_faceTop = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Y");
			s_faceBottom = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_A");
			s_faceRight = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_B");
			s_rightTrigger = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_RT");
			s_leftTrigger = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_LT");
			s_leftStick = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Left_Stick");
			s_leftStickHorz = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Left_Stick_Horizontal");
			s_leftStickVert = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Left_Stick_Vertical");
			s_start = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Menu");
			s_rightBumper = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_RB");
			s_leftBumper = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_LB");
			s_dPadAll = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Dpad");
			s_dPadHorz = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Dpad_Horizontal");
			s_dPadVert = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Dpad_Vertical");
			s_dPadUp = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Dpad_Up");
			s_dPadDown = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Dpad_Down");
			s_dPadLeft = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Dpad_Left");
			s_dPadRight = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Dpad_Right");
			s_rightStickClick = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Right_Stick_Click");
			s_leftStickClick = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_Left_Stick_Click");
			s_select = (Texture2D)Resources.Load("ButtonPrompts/Xbox Series/" + "XboxSeriesX_View");
			s_share = s_testButton;
			break;
		case ButtonPromptImgSet.STEAMDECK:
			s_faceLeft = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_X");
			s_faceTop = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Y");
			s_faceBottom = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_A");
			s_faceRight = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_B");
			s_dPadAll = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Dpad");
			s_dPadHorz = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Dpad_Horizontal");
			s_dPadVert = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Dpad_Vertical");
			s_dPadUp = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Dpad_Up");
			s_dPadDown = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Dpad_Down");
			s_dPadLeft = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Dpad_Left");
			s_dPadRight = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Dpad_Right");
			s_start = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Menu");
			s_select = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck/" + "SteamDeck_Square");
			s_rightTrigger = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_R2");
			s_leftTrigger = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_L2");
			s_rightBumper = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_R1");
			s_leftBumper = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_L1");
			s_leftStick = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_Left_Stick");
			s_leftStickHorz = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_Left_Stick_Horizontal");
			s_leftStickVert = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_Left_Stick_Vertical");
			s_rightStick = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_Right_Stick");
			s_rightStickHorz = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_Right_Stick_Horizontal");
			s_rightStickVert = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_Right_Stick_Vertical");
			s_rightStickClick = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_Right_Stick_Click");
			s_leftStickClick = (Texture2D)Resources.Load("ButtonPrompts/Steam Deck JANK/" + "SteamDeck_Left_Stick_Click");
			s_share = s_testButton;
			break;
		}
		s_axisTextureDict.Clear();
		s_axisTextureDict.Add(s_rightStick, AxisIdentifier.CTRLR_RSTICK);
		s_axisTextureDict.Add(s_rightStickHorz, AxisIdentifier.CTRLR_RSTICKX);
		s_axisTextureDict.Add(s_rightStickVert, AxisIdentifier.CTRLR_RSTICKY);
		s_axisTextureDict.Add(s_leftStick, AxisIdentifier.CTRLR_LSTICK);
		s_axisTextureDict.Add(s_leftStickHorz, AxisIdentifier.CTRLR_LSTICKX);
		s_axisTextureDict.Add(s_leftStickVert, AxisIdentifier.CTRLR_LSTICKY);
		s_axisTextureDict.Add(s_rightTrigger, AxisIdentifier.CTRLR_RTRIGGER);
		s_axisTextureDict.Add(s_leftTrigger, AxisIdentifier.CTRLR_LTRIGGER);
		s_axisTextureDict.Add(s_dPadRight, AxisIdentifier.CTRLR_DPADX);
		s_axisTextureDict.Add(s_dPadLeft, AxisIdentifier.CTRLR_DPADX);
		s_axisTextureDict.Add(s_dPadHorz, AxisIdentifier.CTRLR_DPADX);
		s_axisTextureDict.Add(s_dPadUp, AxisIdentifier.CTRLR_DPADY);
		s_axisTextureDict.Add(s_dPadDown, AxisIdentifier.CTRLR_DPADY);
		s_axisTextureDict.Add(s_dPadVert, AxisIdentifier.CTRLR_DPADY);
		s_axisTextureDict.Add(s_dPadAll, AxisIdentifier.CTRLR_FULLDPAD);
		s_axisTextureDict.Add(s_mouseAll, AxisIdentifier.KEYBD_MOUSE);
		s_axisTextureDict.Add(s_mouseWheelDown, AxisIdentifier.KEYBD_MOUSEWHEEL);
		s_axisTextureDict.Add(s_mouseWheelUp, AxisIdentifier.KEYBD_MOUSEWHEEL);
		s_axisTextureDict.Add(s_mouseHorz, AxisIdentifier.KEYBD_MOUSEX);
		s_axisTextureDict.Add(s_mouseVert, AxisIdentifier.KEYBD_MOUSEY);
		s_joystickButtonTextureDict.Clear();
		s_joystickButtonTextureDict.Add(s_faceLeft, JoystickButton.FaceLeft);
		s_joystickButtonTextureDict.Add(s_faceTop, JoystickButton.FaceUp);
		s_joystickButtonTextureDict.Add(s_faceBottom, JoystickButton.FaceDown);
		s_joystickButtonTextureDict.Add(s_faceRight, JoystickButton.FaceRight);
		s_joystickButtonTextureDict.Add(s_start, JoystickButton.Start);
		s_joystickButtonTextureDict.Add(s_rightBumper, JoystickButton.RightBumper);
		s_joystickButtonTextureDict.Add(s_leftBumper, JoystickButton.LeftBumper);
		s_joystickButtonTextureDict.Add(s_dPadUp, JoystickButton.DPadUp);
		s_joystickButtonTextureDict.Add(s_dPadDown, JoystickButton.DPadDown);
		s_joystickButtonTextureDict.Add(s_dPadLeft, JoystickButton.DPadLeft);
		s_joystickButtonTextureDict.Add(s_dPadRight, JoystickButton.DPadRight);
		s_joystickButtonTextureDict.Add(s_rightStickClick, JoystickButton.RightStickClick);
		s_joystickButtonTextureDict.Add(s_leftStickClick, JoystickButton.LeftStickClick);
		s_joystickButtonTextureDict.Add(s_rightStick, JoystickButton.RightStick);
		s_joystickButtonTextureDict.Add(s_leftStick, JoystickButton.LeftStick);
		s_joystickButtonTextureDict.Add(s_select, JoystickButton.Select);
		s_joystickButtonTextureDict.Add(s_share, JoystickButton.DS4_Share);
		if (ButtonPromptLibrary.OnUpdateButtonPromptConfig != null)
		{
			ButtonPromptLibrary.OnUpdateButtonPromptConfig();
		}
	}

	public void RefineUITextureListForDisplay(in List<Texture2D> textureList)
	{
		if (textureList.Count == 2)
		{
			if (textureList[0] == SharedInstance.GetAxisTexture(AxisIdentifier.CTRLR_RTRIGGER) && textureList[1] == SharedInstance.GetAxisTexture(AxisIdentifier.CTRLR_LTRIGGER))
			{
				Texture2D value = textureList[0];
				textureList[0] = textureList[1];
				textureList[1] = value;
			}
			else if (textureList[0] == SharedInstance.GetButtonTexture(JoystickButton.RightBumper) && textureList[1] == SharedInstance.GetButtonTexture(JoystickButton.LeftBumper))
			{
				Texture2D value2 = textureList[0];
				textureList[0] = textureList[1];
				textureList[1] = value2;
			}
			else if (textureList[0] == textureList[1])
			{
				AxisIdentifier[] array = new AxisIdentifier[4]
				{
					AxisIdentifier.CTRLR_LSTICKX,
					AxisIdentifier.CTRLR_LSTICKY,
					AxisIdentifier.CTRLR_RSTICKX,
					AxisIdentifier.CTRLR_RSTICKY
				};
				for (int i = 0; i < array.Length; i++)
				{
					if (textureList[0] == SharedInstance.GetAxisTexture(array[i]))
					{
						textureList.Clear();
						textureList.Add(SharedInstance.GetAxisTexture(array[i]));
						break;
					}
				}
			}
		}
		if (textureList.Count == 4 && textureList.Contains(SharedInstance.GetButtonTexture(JoystickButton.DPadUp)) && textureList.Contains(SharedInstance.GetButtonTexture(JoystickButton.DPadDown)) && textureList.Contains(SharedInstance.GetButtonTexture(JoystickButton.DPadLeft)) && textureList.Contains(SharedInstance.GetButtonTexture(JoystickButton.DPadRight)))
		{
			textureList.Clear();
			textureList.Add(SharedInstance.GetAxisTexture(AxisIdentifier.CTRLR_FULLDPAD));
		}
	}

	public Texture2D GetButtonTexture(JoystickButton button)
	{
		switch (button)
		{
		case JoystickButton.RightStick:
			return s_rightStick;
		case JoystickButton.FaceLeft:
			return s_faceLeft;
		case JoystickButton.FaceUp:
			return s_faceTop;
		case JoystickButton.FaceDown:
			return s_faceBottom;
		case JoystickButton.FaceRight:
			return s_faceRight;
		case JoystickButton.RightTrigger:
			return s_rightTrigger;
		case JoystickButton.LeftTrigger:
			return s_leftTrigger;
		case JoystickButton.LeftStick:
			return s_leftStick;
		case JoystickButton.Start:
			return s_start;
		case JoystickButton.RightBumper:
			return s_rightBumper;
		case JoystickButton.LeftBumper:
			return s_leftBumper;
		case JoystickButton.DPadUp:
			return s_dPadUp;
		case JoystickButton.DPadDown:
			return s_dPadDown;
		case JoystickButton.DPadLeft:
			return s_dPadLeft;
		case JoystickButton.DPadRight:
			return s_dPadRight;
		case JoystickButton.RightStickClick:
			return s_rightStickClick;
		case JoystickButton.LeftStickClick:
			return s_leftStickClick;
		case JoystickButton.Select:
			return s_select;
		case JoystickButton.DS4_Share:
			return s_share;
		default:
			return s_testButton;
		}
	}

	public Texture2D GetButtonTexture(KeyCode key)
	{
		if (s_keyCodeDict.ContainsKey(key))
		{
			return s_keyCodeDict[key];
		}
		KeyCode keyCode = KeyCode.None;
		if (key >= KeyCode.Joystick1Button0 && key <= KeyCode.Joystick8Button19)
		{
			int num = (int)(key - 350) / 20;
			keyCode = (KeyCode)(330 + num);
		}
		if (key >= KeyCode.JoystickButton0 && key <= KeyCode.JoystickButton19)
		{
			keyCode = key;
		}
		if (keyCode != 0)
		{
			JoystickButton joystickButton = JoystickButton.None;
			switch (joystickButton)
			{
			case JoystickButton.Custom:
			{
				int num2 = (int)(keyCode - 330);
				return s_genericJoystick[num2];
			}
			default:
				return GetButtonTexture(joystickButton);
			case JoystickButton.None:
				break;
			}
		}
		if (key >= KeyCode.Mouse3 && key <= KeyCode.Mouse6)
		{
			int num3 = (int)(key - 323);
			return s_genericJoystick[num3];
		}
		string text = "ButtonPrompts/Keyboard & Mouse/Keyboard_Black_";
		string keyCodeString = GetKeyCodeString(key);
		text = ((!(keyCodeString == string.Empty)) ? (text + keyCodeString) : "ButtonPrompts/Keyboard & Mouse/Blanks/Blank_Black_Normal");
		Texture2D texture2D = (Texture2D)Resources.Load(text);
		s_keyCodeDict.Add(key, texture2D);
		return texture2D;
	}

	public Texture2D GetAxisTexture(AxisIdentifier axis0, AxisIdentifier axis1)
	{
		if (axis0 == AxisIdentifier.CTRLR_DPADX && axis1 == AxisIdentifier.CTRLR_DPADY)
		{
			return s_dPadAll;
		}
		if (axis0 == AxisIdentifier.CTRLR_DPADX && axis1 == AxisIdentifier.CTRLR_DPADX)
		{
			return s_dPadHorz;
		}
		if (axis0 == AxisIdentifier.CTRLR_DPADY && axis1 == AxisIdentifier.CTRLR_DPADY)
		{
			return s_dPadVert;
		}
		return s_testButton;
	}

	public Texture2D GetCustomAxisTexture(string axisName)
	{
		string s = axisName.Remove(0, 12);
		int result = -1;
		if (int.TryParse(s, out result))
		{
			return s_genericJoystick[result];
		}
		return null;
	}

	public Texture2D GetAxisTexture(AxisIdentifier axisId, int axisDirection = 0)
	{
		switch (axisId)
		{
		case AxisIdentifier.CTRLR_RSTICK:
			return s_rightStick;
		case AxisIdentifier.CTRLR_RSTICKX:
			return s_rightStickHorz;
		case AxisIdentifier.CTRLR_RSTICKY:
			return s_rightStickVert;
		case AxisIdentifier.CTRLR_LSTICK:
			return s_leftStick;
		case AxisIdentifier.CTRLR_LSTICKX:
			return s_leftStickHorz;
		case AxisIdentifier.CTRLR_LSTICKY:
			return s_leftStickVert;
		case AxisIdentifier.CTRLR_RTRIGGER:
			return s_rightTrigger;
		case AxisIdentifier.CTRLR_LTRIGGER:
			return s_leftTrigger;
		case AxisIdentifier.CTRLR_DPADX:
			switch (axisDirection)
			{
			case 1:
				return s_dPadRight;
			case -1:
				return s_dPadLeft;
			default:
				return s_dPadHorz;
			}
		case AxisIdentifier.CTRLR_DPADY:
			switch (axisDirection)
			{
			case 1:
				return s_dPadUp;
			case -1:
				return s_dPadDown;
			default:
				return s_dPadVert;
			}
		case AxisIdentifier.CTRLR_FULLDPAD:
			return s_dPadAll;
		case AxisIdentifier.KEYBD_MOUSE:
			return s_mouseAll;
		case AxisIdentifier.KEYBD_MOUSEWHEEL:
			if (axisDirection == -1)
			{
				return s_mouseWheelDown;
			}
			return s_mouseWheelUp;
		case AxisIdentifier.KEYBD_MOUSEX:
			return s_mouseHorz;
		case AxisIdentifier.KEYBD_MOUSEY:
			return s_mouseVert;
		default:
			return s_testButton;
		}
	}

	public static AxisIdentifier GetTextureAxisIdentifier(Texture2D texture)
	{
		if (s_axisTextureDict.ContainsKey(texture))
		{
			return s_axisTextureDict[texture];
		}
		return AxisIdentifier.NONE;
	}

	public static JoystickButton GetTextureJoystickButton(Texture2D texture)
	{
		if (s_joystickButtonTextureDict.ContainsKey(texture))
		{
			return s_joystickButtonTextureDict[texture];
		}
		return JoystickButton.None;
	}

	public static Vector2 AdjustButtonImageSize(Texture2D buttonTexture, float normalLayoutHeight, bool reduceAxisImages = false)
	{
		Vector2 result = new Vector2(normalLayoutHeight, normalLayoutHeight);
		Vector2 one = Vector2.one;
		AxisIdentifier textureAxisIdentifier = GetTextureAxisIdentifier(buttonTexture);
		float num = ((textureAxisIdentifier != 0 && (uint)(textureAxisIdentifier - 12) > 3u) ? ((float)SharedInstance.GetButtonTexture(JoystickButton.FaceRight).height) : ((float)SharedInstance.GetButtonTexture(KeyCode.A).height));
		one.y = (float)buttonTexture.height / num;
		one.x = (float)buttonTexture.width / num;
		if (reduceAxisImages)
		{
			float num2 = 1f;
			switch (textureAxisIdentifier)
			{
			case AxisIdentifier.KEYBD_MOUSE:
			case AxisIdentifier.KEYBD_MOUSEX:
			case AxisIdentifier.KEYBD_MOUSEY:
			case AxisIdentifier.KEYBD_MOUSEWHEEL:
				num2 = 0.8f;
				break;
			case AxisIdentifier.CTRLR_RSTICK:
			case AxisIdentifier.CTRLR_RSTICKX:
			case AxisIdentifier.CTRLR_RSTICKY:
			case AxisIdentifier.CTRLR_LSTICK:
			case AxisIdentifier.CTRLR_LSTICKX:
			case AxisIdentifier.CTRLR_LSTICKY:
				num2 = 0.9f;
				break;
			}
			if (num2 == 1f)
			{
				switch (GetTextureJoystickButton(buttonTexture))
				{
				case JoystickButton.RightStickClick:
				case JoystickButton.LeftStickClick:
					num2 = 0.8f;
					break;
				case JoystickButton.RightStick:
				case JoystickButton.LeftStick:
					num2 = 0.9f;
					break;
				}
			}
			one.Scale(new Vector2(num2, num2));
		}
		result.Scale(one);
		return result;
	}

	private static string GetKeyCodeString(KeyCode key)
	{
		string result;
		switch (key)
		{
		case KeyCode.Alpha0:
		case KeyCode.Keypad0:
			result = "0";
			break;
		case KeyCode.Alpha1:
		case KeyCode.Keypad1:
			result = "1";
			break;
		case KeyCode.Alpha2:
		case KeyCode.Keypad2:
			result = "2";
			break;
		case KeyCode.Alpha3:
		case KeyCode.Keypad3:
			result = "3";
			break;
		case KeyCode.Alpha4:
		case KeyCode.Keypad4:
			result = "4";
			break;
		case KeyCode.Alpha5:
		case KeyCode.Keypad5:
			result = "5";
			break;
		case KeyCode.Alpha6:
		case KeyCode.Keypad6:
			result = "6";
			break;
		case KeyCode.Alpha7:
		case KeyCode.Keypad7:
			result = "7";
			break;
		case KeyCode.Alpha8:
		case KeyCode.Keypad8:
			result = "8";
			break;
		case KeyCode.Alpha9:
		case KeyCode.Keypad9:
			result = "9";
			break;
		case KeyCode.A:
			result = "A";
			break;
		case KeyCode.B:
			result = "B";
			break;
		case KeyCode.C:
			result = "C";
			break;
		case KeyCode.D:
			result = "D";
			break;
		case KeyCode.E:
			result = "E";
			break;
		case KeyCode.F:
			result = "F";
			break;
		case KeyCode.G:
			result = "G";
			break;
		case KeyCode.H:
			result = "H";
			break;
		case KeyCode.I:
			result = "I";
			break;
		case KeyCode.J:
			result = "J";
			break;
		case KeyCode.K:
			result = "K";
			break;
		case KeyCode.L:
			result = "L";
			break;
		case KeyCode.M:
			result = "M";
			break;
		case KeyCode.N:
			result = "N";
			break;
		case KeyCode.O:
			result = "O";
			break;
		case KeyCode.P:
			result = "P";
			break;
		case KeyCode.Q:
			result = "Q";
			break;
		case KeyCode.R:
			result = "R";
			break;
		case KeyCode.S:
			result = "S";
			break;
		case KeyCode.T:
			result = "T";
			break;
		case KeyCode.U:
			result = "U";
			break;
		case KeyCode.V:
			result = "V";
			break;
		case KeyCode.W:
			result = "W";
			break;
		case KeyCode.X:
			result = "X";
			break;
		case KeyCode.Y:
			result = "Y";
			break;
		case KeyCode.Z:
			result = "Z";
			break;
		case KeyCode.UpArrow:
			result = "Arrow_Up";
			break;
		case KeyCode.DownArrow:
			result = "Arrow_Down";
			break;
		case KeyCode.LeftArrow:
			result = "Arrow_Left";
			break;
		case KeyCode.RightArrow:
			result = "Arrow_Right";
			break;
		case KeyCode.LeftShift:
			result = "Left_Shift";
			break;
		case KeyCode.RightShift:
			result = "Right_Shift";
			break;
		case KeyCode.LeftAlt:
			result = "Left_Alt";
			break;
		case KeyCode.RightAlt:
			result = "Right_Alt";
			break;
		case KeyCode.LeftControl:
			result = "Left_Ctrl";
			break;
		case KeyCode.RightControl:
			result = "Right_Ctrl";
			break;
		case KeyCode.Tab:
			result = "Tab";
			break;
		case KeyCode.Space:
			result = "Space";
			break;
		case KeyCode.F1:
			result = "F1";
			break;
		case KeyCode.F2:
			result = "F2";
			break;
		case KeyCode.F3:
			result = "F3";
			break;
		case KeyCode.F4:
			result = "F4";
			break;
		case KeyCode.F5:
			result = "F5";
			break;
		case KeyCode.F6:
			result = "F6";
			break;
		case KeyCode.F7:
			result = "F7";
			break;
		case KeyCode.F8:
			result = "F8";
			break;
		case KeyCode.F9:
			result = "F9";
			break;
		case KeyCode.F10:
			result = "F10";
			break;
		case KeyCode.F11:
			result = "F11";
			break;
		case KeyCode.F12:
			result = "F12";
			break;
		case KeyCode.F13:
			result = "F13";
			break;
		case KeyCode.F14:
			result = "F14";
			break;
		case KeyCode.F15:
			result = "F15";
			break;
		case KeyCode.Mouse0:
			result = "Mouse_Left";
			break;
		case KeyCode.Mouse1:
			result = "Mouse_Right";
			break;
		case KeyCode.Mouse2:
			result = "Mouse_Middle";
			break;
		case KeyCode.Escape:
			result = "Esc";
			break;
		case KeyCode.Return:
			result = "Enter";
			break;
		case KeyCode.KeypadEnter:
			result = "Enter_Tall";
			break;
		case KeyCode.Numlock:
			result = "Num_Lock";
			break;
		case KeyCode.Plus:
			result = "Plus";
			break;
		case KeyCode.Minus:
		case KeyCode.KeypadMinus:
			result = "Minus";
			break;
		case KeyCode.KeypadPlus:
			result = "Plus_Tall";
			break;
		case KeyCode.Asterisk:
		case KeyCode.KeypadMultiply:
			result = "Asterisk";
			break;
		case KeyCode.Backspace:
			result = "Backspace_Alt";
			break;
		case KeyCode.LeftBracket:
			result = "Bracket_Left";
			break;
		case KeyCode.RightBracket:
			result = "Bracket_Right";
			break;
		case KeyCode.CapsLock:
			result = "Caps_Lock";
			break;
		case KeyCode.Delete:
			result = "Del";
			break;
		case KeyCode.End:
			result = "End";
			break;
		case KeyCode.Home:
			result = "Home";
			break;
		case KeyCode.PageDown:
			result = "Page_Down";
			break;
		case KeyCode.PageUp:
			result = "Page_Up";
			break;
		case KeyCode.Insert:
			result = "Insert";
			break;
		case KeyCode.Print:
		case KeyCode.SysReq:
			result = "Print_Screen";
			break;
		case KeyCode.Quote:
			result = "Quote";
			break;
		case KeyCode.Semicolon:
			result = "Semicolon";
			break;
		case KeyCode.Slash:
		case KeyCode.KeypadDivide:
			result = "Slash";
			break;
		case KeyCode.Backslash:
			result = "Backslash";
			break;
		case KeyCode.Greater:
			result = "Mark_Right";
			break;
		case KeyCode.Less:
			result = "Mark_Left";
			break;
		case KeyCode.Question:
			result = "Question";
			break;
		case KeyCode.LeftWindows:
		case KeyCode.RightWindows:
			result = "Win";
			break;
		case KeyCode.RightCommand:
		case KeyCode.LeftCommand:
			result = ((!OWUtilities.RunningOnMac()) ? "Win" : "Command");
			break;
		case KeyCode.None:
			result = string.Empty;
			break;
		default:
			switch ((int)key)
			{
			case 37:
			case 123:
			case 124:
			case 125:
				result = string.Empty;
				Debug.LogWarning("KeyCode " + key.ToString() + " does not have corresponding string");
				break;
			case 126:
				result = "Tilda";
				break;
			default:
				result = string.Empty;
				Debug.LogWarning("KeyCode " + key.ToString() + " does not have corresponding string");
				break;
			}
			break;
		}
		return result;
	}
}
