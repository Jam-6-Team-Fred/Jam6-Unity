using System;
using System.Runtime.Serialization;
using System.Xml;
using Steamworks;
using UnityEngine;

[Serializable]
public class SettingsSave
{
	[Serializable]
	public struct UserDeviceInfo
	{
		public bool userEnabled;

		public string unityDeviceName;

		public int productId;

		public int vendorId;

		public string manufacturer;

		public string productName;
	}

	public bool autoBoost;

	public bool autoRun = true;

	public bool autoEquipTranslator = true;

	public TextSpeed textSpeed = TextSpeed.Normal;

	public bool freezeTimeWhileReading = true;

	public bool showShipLogNotifications = true;

	public bool detectiveModeEnabled = true;

	public int inversionFactor = 1;

	private static float _sensitivityMin = 0.5f;

	private static float _sensitivityCurveFlatness = 0f;

	public int lookSensitivity = 5;

	public int flightSensitivity = 5;

	[OptionalField(VersionAdded = 2)]
	public Difficulty difficulty = Difficulty.NORMAL;

	[OptionalField(VersionAdded = 2)]
	public bool autopilotEnabled = true;

	[OptionalField(VersionAdded = 2)]
	public bool buttonPromptsEnabled = true;

	[OptionalField(VersionAdded = 7)]
	public bool reducedFrights;

	[OptionalField(VersionAdded = 2)]
	public int shipInversionFactor = 1;

	[OptionalField(VersionAdded = 6)]
	public bool rumbleEnabled = true;

	[OptionalField(VersionAdded = 8)]
	public ButtonPromptImgSet promptImgSet = ButtonPromptImgSet.DEFAULT;

	[OptionalField(VersionAdded = 9)]
	public UserDeviceInfo[] deviceEnabledList;

	[OptionalField(VersionAdded = 3)]
	public TextTranslation.Language language;

	[OptionalField(VersionAdded = 4)]
	public bool freezeTimeWhileReadingShipLog = true;

	[OptionalField(VersionAdded = 4)]
	public bool freezeTimeWhileReadingConversations;

	[OptionalField(VersionAdded = 10)]
	public UITextSize uiTextSize;

	public static float s_inDeadZnMin = 0f;

	public static float s_inDeadZnMax = 1f;

	[OptionalField(VersionAdded = 2)]
	public float innerDeadZone = 0.5f;

	public static float s_outDeadZnMin = 0f;

	public static float s_outDeadZnMax = 1f;

	[OptionalField(VersionAdded = 2)]
	public float outerDeadZone = 0.5f;

	public static float s_masterVolMin = 0f;

	public static float s_masterVolMax = 1f;

	[OptionalField(VersionAdded = 2)]
	public float masterVolume = 1f;

	[OptionalField(VersionAdded = 5)]
	public float musicVolume = 1f;

	[OptionalField(VersionAdded = 7)]
	public float sfxVolume = 1f;

	private static SettingsID[] s_floatValIds = new SettingsID[5]
	{
		SettingsID.VOL_MASTER,
		SettingsID.VOL_MUSIC,
		SettingsID.VOL_SFX,
		SettingsID.INNER_DEADZN,
		SettingsID.OUTER_DEADZN
	};

	private static float[] s_floatValDefaults = new float[5] { 1f, 1f, 1f, 0.5f, 0.5f };

	private static SettingsID[] s_intValIds = new SettingsID[9]
	{
		SettingsID.LANGUAGE,
		SettingsID.TEXT_SPEED,
		SettingsID.DIFFICULTY,
		SettingsID.LOOK_INVERT,
		SettingsID.FLIGHT_INVERT,
		SettingsID.LOOK_SENSITIVITY,
		SettingsID.FLIGHT_SENSITIVITY,
		SettingsID.INPUT_BUTTON_PROMPT,
		SettingsID.UI_SIZE
	};

	private static int[] s_intValDefaults = new int[9] { -1, 1, 1, 1, 1, 5, 5, -1, 0 };

	private static SettingsID[] s_boolValIds = new SettingsID[12]
	{
		SettingsID.AUTO_BOOST,
		SettingsID.AUTO_RUN,
		SettingsID.AUTO_EQUIP_TRANSLATOR,
		SettingsID.SHIP_LOG_NOTIFICATION,
		SettingsID.SHIP_LOG_RUMOR_MODE,
		SettingsID.AUTOPILOT,
		SettingsID.FREEZE_TIME_TRANSLATOR,
		SettingsID.FREEZE_TIME_LOG,
		SettingsID.FREEZE_TIME_DIALOGUE,
		SettingsID.BUTTON_PROMPTS,
		SettingsID.RUMBLE,
		SettingsID.REDUCED_FRIGHTS
	};

	private static bool[] s_boolValDefaults = new bool[12]
	{
		false, false, true, true, true, true, true, true, false, true,
		true, false
	};

	public static string[] GetTextSpeedStrings()
	{
		return new string[4]
		{
			UITextLibrary.GetString(UITextType.OptionSlow),
			UITextLibrary.GetString(UITextType.OptionNormal),
			UITextLibrary.GetString(UITextType.OptionFast),
			UITextLibrary.GetString(UITextType.OptionInstant)
		};
	}

	public static string[] GetLanguageStrings()
	{
		return new string[12]
		{
			"English", "Español", "Deutsch", "Français", "Italiano", "Polski", "Português", "日本語", "Pусский", "简化字",
			"한국어", "Türkçe"
		};
	}

	public static string[] GetButtonPromptImageStrings()
	{
		return new string[6] { "Xbox One", "PS4", "Switch Pro", "PS5", "Xbox Series X|S", "Steam Deck" };
	}

	public int GetSliderValLookSensitivity()
	{
		return Mathf.RoundToInt(lookSensitivity);
	}

	public void SetSliderValLookSensitivity(float value)
	{
		lookSensitivity = Mathf.RoundToInt(value);
	}

	public float GetLookSensitivity()
	{
		return _sensitivityMin + Mathf.Pow(((float)lookSensitivity + _sensitivityCurveFlatness) / (5f + _sensitivityCurveFlatness), 2f) * (1f - _sensitivityMin);
	}

	public int GetSliderValFlightSensitivity()
	{
		return Mathf.RoundToInt(flightSensitivity);
	}

	public void SetSliderValFlightSensitivity(float value)
	{
		flightSensitivity = Mathf.RoundToInt(value);
	}

	public float GetFlightSensitivity()
	{
		return _sensitivityMin + Mathf.Pow(((float)flightSensitivity + _sensitivityCurveFlatness) / (5f + _sensitivityCurveFlatness), 2f) * (1f - _sensitivityMin);
	}

	public static string[] GetTextSizeStrings()
	{
		return new string[3]
		{
			UITextLibrary.GetString(UITextType.UiSizeOptionAuto),
			UITextLibrary.GetString(UITextType.UiSizeOptionReg),
			UITextLibrary.GetString(UITextType.UiSizeOptionLrg)
		};
	}

	public bool IsLargeTextSize()
	{
		if (uiTextSize == UITextSize.AUTO)
		{
			if (language == TextTranslation.Language.JAPANESE || language == TextTranslation.Language.CHINESE_SIMPLE || language == TextTranslation.Language.KOREAN)
			{
				return true;
			}
			return SteamUtils.IsSteamRunningOnSteamDeck();
		}
		return uiTextSize == UITextSize.LARGE;
	}

	public int GetSliderValInnerDeadZone()
	{
		return Mathf.RoundToInt(Mathf.InverseLerp(s_inDeadZnMin, s_inDeadZnMax, innerDeadZone) * 10f);
	}

	public void SetSliderValInnerDeadZone(float value)
	{
		innerDeadZone = Mathf.Lerp(s_inDeadZnMin, s_inDeadZnMax, value * 0.1f);
	}

	public float GetInnerDeadZone()
	{
		return innerDeadZone * 2f;
	}

	public int GetSliderValOuterDeadZone()
	{
		return Mathf.RoundToInt(Mathf.InverseLerp(s_outDeadZnMin, s_outDeadZnMax, outerDeadZone) * 10f);
	}

	public void SetSliderValOuterDeadZone(float value)
	{
		outerDeadZone = Mathf.Lerp(s_outDeadZnMin, s_outDeadZnMax, value * 0.1f);
	}

	public float GetOuterDeadZone()
	{
		return outerDeadZone * 2f;
	}

	public int GetSliderValMasterVol()
	{
		return Mathf.RoundToInt(Mathf.InverseLerp(s_masterVolMin, s_masterVolMax, masterVolume) * 10f);
	}

	public void SetSliderValMasterVol(float value)
	{
		masterVolume = Mathf.Lerp(s_masterVolMin, s_masterVolMax, value * 0.1f);
	}

	public int GetSliderValMusicVol()
	{
		return Mathf.RoundToInt(Mathf.InverseLerp(s_masterVolMin, s_masterVolMax, musicVolume) * 10f);
	}

	public void SetSliderValMusicVol(float value)
	{
		musicVolume = Mathf.Lerp(s_masterVolMin, s_masterVolMax, value * 0.1f);
	}

	public int GetSliderValSFXVol()
	{
		return Mathf.RoundToInt(Mathf.InverseLerp(s_masterVolMin, s_masterVolMax, sfxVolume) * 10f);
	}

	public void SetSliderValSFXVol(float value)
	{
		sfxVolume = Mathf.Lerp(s_masterVolMin, s_masterVolMax, value * 0.1f);
	}

	public SettingsSave()
	{
		SetToDefaults(this, s_boolValIds);
		SetToDefaults(this, s_intValIds);
		SetToDefaults(this, s_floatValIds);
		deviceEnabledList = new UserDeviceInfo[0];
	}

	[OnDeserializing]
	private void SetDefaultValuesOnDeserializing(StreamingContext context)
	{
		rumbleEnabled = true;
		reducedFrights = false;
		sfxVolume = 1f;
		promptImgSet = ButtonPromptImgSet.DEFAULT;
		deviceEnabledList = new UserDeviceInfo[0];
		uiTextSize = UITextSize.AUTO;
	}

	public override string ToString()
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Empty + "Inversion Factor " + inversionFactor, "Rumble ", rumbleEnabled.ToString()), " Look Sensitivity ", lookSensitivity), " Flight Sensitivity ", flightSensitivity), " Auto Boost ", autoBoost.ToString()), " Auto Run ", autoRun.ToString()), " Auto Equip Translator ", autoEquipTranslator.ToString()), " Show Ship Log Notifications ", showShipLogNotifications.ToString()), " Detective Mode ", detectiveModeEnabled.ToString()), " Reduced Frights ", reducedFrights.ToString()), " Freeze Time While Reading", freezeTimeWhileReading.ToString()), " Freeze Time While Reading", freezeTimeWhileReadingShipLog.ToString()), " Freeze Time While Reading", freezeTimeWhileReadingConversations.ToString()), " Text Speed ", textSpeed.ToString()), " UI Text Size ", uiTextSize.ToString()), " Language ", language.ToString());
	}

	public string ToJson()
	{
		return JsonUtility.ToJson(this);
	}

	public static SettingsSave FromJson(string json)
	{
		try
		{
			return JsonUtility.FromJson<SettingsSave>(json);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not load settings: " + ex.Message);
			return null;
		}
	}

	public SettingsSave Clone()
	{
		return new SettingsSave
		{
			autoBoost = autoBoost,
			autoRun = autoRun,
			autoEquipTranslator = autoEquipTranslator,
			textSpeed = textSpeed,
			uiTextSize = uiTextSize,
			freezeTimeWhileReading = freezeTimeWhileReading,
			freezeTimeWhileReadingShipLog = freezeTimeWhileReadingShipLog,
			freezeTimeWhileReadingConversations = freezeTimeWhileReadingConversations,
			showShipLogNotifications = showShipLogNotifications,
			detectiveModeEnabled = detectiveModeEnabled,
			inversionFactor = inversionFactor,
			lookSensitivity = lookSensitivity,
			flightSensitivity = flightSensitivity,
			difficulty = difficulty,
			autopilotEnabled = autopilotEnabled,
			buttonPromptsEnabled = buttonPromptsEnabled,
			reducedFrights = reducedFrights,
			promptImgSet = promptImgSet,
			deviceEnabledList = deviceEnabledList,
			shipInversionFactor = shipInversionFactor,
			rumbleEnabled = rumbleEnabled,
			innerDeadZone = innerDeadZone,
			outerDeadZone = outerDeadZone,
			masterVolume = masterVolume,
			musicVolume = musicVolume,
			sfxVolume = sfxVolume,
			language = language
		};
	}

	public void CopyTo(SettingsSave targetSettings)
	{
		targetSettings.autoBoost = autoBoost;
		targetSettings.autoRun = autoRun;
		targetSettings.autoEquipTranslator = autoEquipTranslator;
		targetSettings.textSpeed = textSpeed;
		targetSettings.uiTextSize = uiTextSize;
		targetSettings.freezeTimeWhileReading = freezeTimeWhileReading;
		targetSettings.freezeTimeWhileReadingShipLog = freezeTimeWhileReadingShipLog;
		targetSettings.freezeTimeWhileReadingConversations = freezeTimeWhileReadingConversations;
		targetSettings.showShipLogNotifications = showShipLogNotifications;
		targetSettings.detectiveModeEnabled = detectiveModeEnabled;
		targetSettings.inversionFactor = inversionFactor;
		targetSettings.lookSensitivity = lookSensitivity;
		targetSettings.flightSensitivity = flightSensitivity;
		targetSettings.difficulty = difficulty;
		targetSettings.autopilotEnabled = autopilotEnabled;
		targetSettings.buttonPromptsEnabled = buttonPromptsEnabled;
		targetSettings.reducedFrights = reducedFrights;
		targetSettings.promptImgSet = promptImgSet;
		targetSettings.deviceEnabledList = deviceEnabledList;
		targetSettings.shipInversionFactor = shipInversionFactor;
		targetSettings.rumbleEnabled = rumbleEnabled;
		targetSettings.innerDeadZone = innerDeadZone;
		targetSettings.outerDeadZone = outerDeadZone;
		targetSettings.masterVolume = masterVolume;
		targetSettings.musicVolume = musicVolume;
		targetSettings.sfxVolume = sfxVolume;
		targetSettings.language = language;
	}

	public static SettingsSave GetDefault()
	{
		return new SettingsSave();
	}

	public static void SetToDefaults(SettingsSave targetSave, SettingsID[] idsToSet)
	{
		foreach (SettingsID settingsID in idsToSet)
		{
			bool flag = false;
			for (int j = 0; j < s_boolValIds.Length; j++)
			{
				if (settingsID == s_boolValIds[j])
				{
					switch (settingsID)
					{
					case SettingsID.AUTO_BOOST:
						targetSave.autoBoost = s_boolValDefaults[j];
						break;
					case SettingsID.AUTO_EQUIP_TRANSLATOR:
						targetSave.autoEquipTranslator = s_boolValDefaults[j];
						break;
					case SettingsID.SHIP_LOG_NOTIFICATION:
						targetSave.showShipLogNotifications = s_boolValDefaults[j];
						break;
					case SettingsID.SHIP_LOG_RUMOR_MODE:
						targetSave.detectiveModeEnabled = s_boolValDefaults[j];
						break;
					case SettingsID.AUTOPILOT:
						targetSave.autopilotEnabled = s_boolValDefaults[j];
						break;
					case SettingsID.FREEZE_TIME_TRANSLATOR:
						targetSave.freezeTimeWhileReading = s_boolValDefaults[j];
						break;
					case SettingsID.FREEZE_TIME_LOG:
						targetSave.freezeTimeWhileReadingShipLog = s_boolValDefaults[j];
						break;
					case SettingsID.FREEZE_TIME_DIALOGUE:
						targetSave.freezeTimeWhileReadingConversations = s_boolValDefaults[j];
						break;
					case SettingsID.BUTTON_PROMPTS:
						targetSave.buttonPromptsEnabled = s_boolValDefaults[j];
						break;
					case SettingsID.REDUCED_FRIGHTS:
						targetSave.reducedFrights = s_boolValDefaults[j];
						break;
					case SettingsID.RUMBLE:
						targetSave.rumbleEnabled = s_boolValDefaults[j];
						break;
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			for (int k = 0; k < s_intValIds.Length; k++)
			{
				if (settingsID != s_intValIds[k])
				{
					continue;
				}
				switch (settingsID)
				{
				case SettingsID.LANGUAGE:
					if (TextTranslation.Get() == null)
					{
						targetSave.language = TextTranslation.Language.UNKNOWN;
					}
					else
					{
						targetSave.language = TextTranslation.Get().GetSystemLanguage();
					}
					if (targetSave.language == TextTranslation.Language.UNKNOWN)
					{
						targetSave.language = TextTranslation.Language.ENGLISH;
					}
					break;
				case SettingsID.TEXT_SPEED:
					targetSave.textSpeed = (TextSpeed)s_intValDefaults[k];
					break;
				case SettingsID.UI_SIZE:
					targetSave.uiTextSize = (UITextSize)s_intValDefaults[k];
					break;
				case SettingsID.LOOK_INVERT:
					targetSave.inversionFactor = s_intValDefaults[k];
					break;
				case SettingsID.FLIGHT_INVERT:
					targetSave.shipInversionFactor = s_intValDefaults[k];
					break;
				case SettingsID.LOOK_SENSITIVITY:
					targetSave.lookSensitivity = s_intValDefaults[k];
					break;
				case SettingsID.FLIGHT_SENSITIVITY:
					targetSave.flightSensitivity = s_intValDefaults[k];
					break;
				case SettingsID.INPUT_BUTTON_PROMPT:
					targetSave.promptImgSet = (ButtonPromptImgSet)s_intValDefaults[k];
					break;
				}
				flag = true;
				break;
			}
			if (flag)
			{
				continue;
			}
			for (int l = 0; l < s_floatValIds.Length; l++)
			{
				if (settingsID == s_floatValIds[l])
				{
					switch (settingsID)
					{
					case SettingsID.VOL_MASTER:
						targetSave.masterVolume = s_floatValDefaults[l];
						break;
					case SettingsID.VOL_MUSIC:
						targetSave.musicVolume = s_floatValDefaults[l];
						break;
					case SettingsID.VOL_SFX:
						targetSave.sfxVolume = s_floatValDefaults[l];
						break;
					case SettingsID.INNER_DEADZN:
						targetSave.innerDeadZone = s_floatValDefaults[l];
						break;
					case SettingsID.OUTER_DEADZN:
						targetSave.outerDeadZone = s_floatValDefaults[l];
						break;
					}
					break;
				}
			}
		}
	}

	public void ApplyAllSettings()
	{
		if (Locator.GetAudioMixer() != null)
		{
			Locator.GetAudioMixer().SetMasterVolume(masterVolume);
			Locator.GetAudioMixer().SetMasterMusicVolume(musicVolume);
			Locator.GetAudioMixer().SetMasterSFXVolume(sfxVolume);
		}
		if (TextTranslation.Get() != null)
		{
			TextTranslation.Get().SetLanguage(language);
		}
		RumbleManager.SetEnabled(rumbleEnabled);
		OWInputProcessorUtil.SetDeadZoneMultipliers(GetInnerDeadZone(), GetOuterDeadZone());
		InputLibrary.look.Sensitivity = GetLookSensitivity();
		InputLibrary.yaw.Sensitivity = GetFlightSensitivity();
		InputLibrary.pitch.Sensitivity = GetFlightSensitivity();
		OWInput.InitializeConnectedDevices(deviceEnabledList);
		OWInput.UpdateInversion();
		if (Locator.GetUISizeManager() != null)
		{
			Locator.GetUISizeManager().OnUiSizeSettingChanged();
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SettingsSave settingsSave))
		{
			return false;
		}
		if (true && settingsSave.autoBoost == autoBoost && settingsSave.autoRun == autoRun && settingsSave.autoEquipTranslator == autoEquipTranslator && settingsSave.textSpeed == textSpeed && settingsSave.uiTextSize == uiTextSize && settingsSave.freezeTimeWhileReading == freezeTimeWhileReading && settingsSave.freezeTimeWhileReadingShipLog == freezeTimeWhileReadingShipLog && settingsSave.freezeTimeWhileReadingConversations == freezeTimeWhileReadingConversations && settingsSave.showShipLogNotifications == showShipLogNotifications && settingsSave.detectiveModeEnabled == detectiveModeEnabled && settingsSave.inversionFactor == inversionFactor && settingsSave.lookSensitivity == lookSensitivity && settingsSave.flightSensitivity == flightSensitivity && settingsSave.difficulty == difficulty && settingsSave.autopilotEnabled == autopilotEnabled && settingsSave.buttonPromptsEnabled == buttonPromptsEnabled && settingsSave.reducedFrights == reducedFrights && settingsSave.promptImgSet == promptImgSet && settingsSave.deviceEnabledList == deviceEnabledList && settingsSave.shipInversionFactor == shipInversionFactor && settingsSave.rumbleEnabled == rumbleEnabled && settingsSave.innerDeadZone == innerDeadZone && settingsSave.outerDeadZone == outerDeadZone && settingsSave.masterVolume == masterVolume && settingsSave.musicVolume == musicVolume && settingsSave.sfxVolume == sfxVolume)
		{
			return settingsSave.language == language;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public XmlDocument GetXmlDocument()
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlNode xmlNode = xmlDocument.CreateNode("GameSettings");
		XmlNode xmlNode2 = xmlDocument.CreateNode("InversionFactor");
		xmlNode2.SetValue(XmlConvert.ToString(inversionFactor));
		XmlNode xmlNode3 = xmlDocument.CreateNode("Rumble");
		xmlNode3.SetValue(XmlConvert.ToString(rumbleEnabled));
		XmlNode xmlNode4 = xmlDocument.CreateNode("LookSensitivity");
		xmlNode4.SetValue(XmlConvert.ToString(lookSensitivity));
		XmlNode xmlNode5 = xmlDocument.CreateNode("FlightSensitivity");
		xmlNode5.SetValue(XmlConvert.ToString(flightSensitivity));
		XmlNode xmlNode6 = xmlDocument.CreateNode("AutoBoost");
		xmlNode6.SetValue(XmlConvert.ToString(autoBoost));
		XmlNode xmlNode7 = xmlDocument.CreateNode("AutoRun");
		xmlNode7.SetValue(XmlConvert.ToString(autoRun));
		XmlNode xmlNode8 = xmlDocument.CreateNode("AutoEquipTranslator");
		xmlNode8.SetValue(XmlConvert.ToString(autoEquipTranslator));
		XmlNode xmlNode9 = xmlDocument.CreateNode("ShowShipLogNotifications");
		xmlNode9.SetValue(XmlConvert.ToString(showShipLogNotifications));
		XmlNode xmlNode10 = xmlDocument.CreateNode("DetectiveMode");
		xmlNode10.SetValue(XmlConvert.ToString(detectiveModeEnabled));
		XmlNode xmlNode11 = xmlDocument.CreateNode("FreezeTimeWhileReading");
		xmlNode11.SetValue(XmlConvert.ToString(freezeTimeWhileReading));
		XmlNode xmlNode12 = xmlDocument.CreateNode("FreezeTimeWhileReadingShipLog");
		xmlNode12.SetValue(XmlConvert.ToString(freezeTimeWhileReadingShipLog));
		XmlNode xmlNode13 = xmlDocument.CreateNode("FreezeTimeWhileReadingConversations");
		xmlNode13.SetValue(XmlConvert.ToString(freezeTimeWhileReadingConversations));
		XmlNode xmlNode14 = xmlDocument.CreateNode("TextSpeed");
		xmlNode14.SetValue(XmlConvert.ToString(Convert.ToInt32(textSpeed)));
		XmlNode xmlNode15 = xmlDocument.CreateNode("AudioVolume");
		xmlNode15.SetValue(XmlConvert.ToString(masterVolume));
		xmlDocument.CreateNode("MusicVolume").SetValue(XmlConvert.ToString(musicVolume));
		xmlDocument.CreateNode("SFXVolume").SetValue(XmlConvert.ToString(sfxVolume));
		XmlNode xmlNode16 = xmlDocument.CreateNode("Language");
		xmlNode16.SetValue(XmlConvert.ToString((int)language));
		XmlNode xmlNode17 = xmlDocument.CreateNode("OuterDeadZone");
		xmlNode17.SetValue(XmlConvert.ToString(outerDeadZone));
		XmlNode xmlNode18 = xmlDocument.CreateNode("InnerDeadZone");
		xmlNode18.SetValue(XmlConvert.ToString(innerDeadZone));
		xmlNode.AppendChild(xmlNode2);
		xmlNode.AppendChild(xmlNode3);
		xmlNode.AppendChild(xmlNode4);
		xmlNode.AppendChild(xmlNode5);
		xmlNode.AppendChild(xmlNode6);
		xmlNode.AppendChild(xmlNode7);
		xmlNode.AppendChild(xmlNode8);
		xmlNode.AppendChild(xmlNode9);
		xmlNode.AppendChild(xmlNode10);
		xmlNode.AppendChild(xmlNode11);
		xmlNode.AppendChild(xmlNode12);
		xmlNode.AppendChild(xmlNode13);
		xmlNode.AppendChild(xmlNode14);
		xmlNode.AppendChild(xmlNode15);
		xmlNode.AppendChild(xmlNode16);
		xmlNode.AppendChild(xmlNode17);
		xmlNode.AppendChild(xmlNode18);
		xmlDocument.AppendChild(xmlNode);
		return xmlDocument;
	}

	public void SetXmlDocumentData(XmlDocument document)
	{
		XmlNode xmlNode = document.SelectSingleNode("GameSettings");
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("InversionFactor");
		inversionFactor = Convert.ToInt32(xmlNode2.GetValue());
		XmlNode xmlNode3 = xmlNode.SelectSingleNode("Rumble");
		rumbleEnabled = (XmlConvert.ToBoolean(xmlNode3.GetValue()) ? true : false);
		XmlNode xmlNode4 = xmlNode.SelectSingleNode("LookSensitivity");
		lookSensitivity = Convert.ToInt32(xmlNode4.GetValue());
		XmlNode xmlNode5 = xmlNode.SelectSingleNode("FlightSensitivity");
		flightSensitivity = Convert.ToInt32(xmlNode5.GetValue());
		XmlNode xmlNode6 = xmlNode.SelectSingleNode("AutoBoost");
		autoBoost = (XmlConvert.ToBoolean(xmlNode6.GetValue()) ? true : false);
		XmlNode xmlNode7 = xmlNode.SelectSingleNode("AutoRun");
		autoRun = (XmlConvert.ToBoolean(xmlNode7.GetValue()) ? true : false);
		XmlNode xmlNode8 = xmlNode.SelectSingleNode("AutoEquipTranslator");
		autoEquipTranslator = (XmlConvert.ToBoolean(xmlNode8.GetValue()) ? true : false);
		XmlNode xmlNode9 = xmlNode.SelectSingleNode("ShowShipLogNotifications");
		showShipLogNotifications = (XmlConvert.ToBoolean(xmlNode9.GetValue()) ? true : false);
		XmlNode xmlNode10 = xmlNode.SelectSingleNode("DetectiveMode");
		detectiveModeEnabled = (XmlConvert.ToBoolean(xmlNode10.GetValue()) ? true : false);
		XmlNode xmlNode11 = xmlNode.SelectSingleNode("FreezeTimeWhileReading");
		freezeTimeWhileReading = (XmlConvert.ToBoolean(xmlNode11.GetValue()) ? true : false);
		XmlNode xmlNode12 = xmlNode.SelectSingleNode("FreezeTimeWhileReadingShipLog");
		freezeTimeWhileReadingShipLog = (XmlConvert.ToBoolean(xmlNode12.GetValue()) ? true : false);
		XmlNode xmlNode13 = xmlNode.SelectSingleNode("FreezeTimeWhileReadingConversations");
		freezeTimeWhileReadingConversations = (XmlConvert.ToBoolean(xmlNode13.GetValue()) ? true : false);
		textSpeed = (TextSpeed)Enum.ToObject(value: Convert.ToInt32(xmlNode.SelectSingleNode("TextSpeed").GetValue()), enumType: typeof(TextSpeed));
		XmlNode xmlNode14 = xmlNode.SelectSingleNode("AudioVolume");
		masterVolume = (float)XmlConvert.ToDouble(xmlNode14.GetValue());
		XmlNode xmlNode15 = xmlNode.SelectSingleNode("MusicVolume");
		musicVolume = (float)XmlConvert.ToDouble(xmlNode15.GetValue());
		XmlNode xmlNode16 = xmlNode.SelectSingleNode("SFXVolume");
		sfxVolume = (float)XmlConvert.ToDouble(xmlNode16.GetValue());
		XmlNode xmlNode17 = xmlNode.SelectSingleNode("Language");
		language = (TextTranslation.Language)XmlConvert.ToInt32(xmlNode17.GetValue());
		XmlNode xmlNode18 = xmlNode.SelectSingleNode("OuterDeadZone");
		outerDeadZone = (float)XmlConvert.ToDouble(xmlNode18.GetValue());
		XmlNode xmlNode19 = xmlNode.SelectSingleNode("InnerDeadZone");
		innerDeadZone = (float)XmlConvert.ToDouble(xmlNode19.GetValue());
	}
}
