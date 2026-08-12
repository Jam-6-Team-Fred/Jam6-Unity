using System;
using System.Collections.Generic;
using UnityEngine;

public static class SettingsUiUtil
{
	private static SettingsLookupTable s_lookupTable;

	private static string s_lookupTableResourcePath = "SettingsLookupTable";

	private static void LoadSettingsLookupTable()
	{
		s_lookupTable = Resources.Load(s_lookupTableResourcePath) as SettingsLookupTable;
	}

	public static void ApplySettingsToUi(MenuOption[] uiOptions, SettingsSave settingsToApply)
	{
		for (int i = 0; i < uiOptions.Length; i++)
		{
			MenuValueOption menuValueOption = uiOptions[i] as MenuValueOption;
			switch (uiOptions[i].GetSettingsID())
			{
			case SettingsID.AUTO_BOOST:
				menuValueOption.Initialize(settingsToApply.autoBoost);
				break;
			case SettingsID.AUTO_EQUIP_TRANSLATOR:
				menuValueOption.Initialize(settingsToApply.autoEquipTranslator);
				break;
			case SettingsID.SHIP_LOG_NOTIFICATION:
				menuValueOption.Initialize(settingsToApply.showShipLogNotifications);
				break;
			case SettingsID.SHIP_LOG_RUMOR_MODE:
				menuValueOption.Initialize(settingsToApply.detectiveModeEnabled);
				break;
			case SettingsID.AUTOPILOT:
				menuValueOption.Initialize(settingsToApply.autopilotEnabled);
				break;
			case SettingsID.BUTTON_PROMPTS:
				menuValueOption.Initialize(settingsToApply.buttonPromptsEnabled);
				break;
			case SettingsID.REDUCED_FRIGHTS:
				menuValueOption.Initialize(settingsToApply.reducedFrights);
				break;
			case SettingsID.LOOK_INVERT:
				menuValueOption.Initialize(settingsToApply.inversionFactor != 1);
				break;
			case SettingsID.FLIGHT_INVERT:
				menuValueOption.Initialize(settingsToApply.shipInversionFactor != 1);
				break;
			case SettingsID.RUMBLE:
				menuValueOption.Initialize(settingsToApply.rumbleEnabled);
				break;
			case SettingsID.LOOK_SENSITIVITY:
				menuValueOption.Initialize(settingsToApply.GetSliderValLookSensitivity());
				break;
			case SettingsID.FLIGHT_SENSITIVITY:
				menuValueOption.Initialize(settingsToApply.GetSliderValFlightSensitivity());
				break;
			case SettingsID.INNER_DEADZN:
				menuValueOption.Initialize(settingsToApply.GetSliderValInnerDeadZone());
				break;
			case SettingsID.OUTER_DEADZN:
				menuValueOption.Initialize(settingsToApply.GetSliderValOuterDeadZone());
				break;
			case SettingsID.LANGUAGE:
				((OptionsSelectorElement)menuValueOption).Initialize((int)settingsToApply.language, SettingsSave.GetLanguageStrings());
				break;
			case SettingsID.TEXT_SPEED:
				((OptionsSelectorElement)menuValueOption).Initialize((int)settingsToApply.textSpeed, SettingsSave.GetTextSpeedStrings());
				break;
			case SettingsID.UI_SIZE:
				((OptionsSelectorElement)menuValueOption).Initialize((int)settingsToApply.uiTextSize, SettingsSave.GetTextSizeStrings());
				break;
			case SettingsID.FREEZE_TIME_TRANSLATOR:
				menuValueOption.Initialize(settingsToApply.freezeTimeWhileReading);
				break;
			case SettingsID.FREEZE_TIME_LOG:
				menuValueOption.Initialize(settingsToApply.freezeTimeWhileReadingShipLog);
				break;
			case SettingsID.FREEZE_TIME_DIALOGUE:
				menuValueOption.Initialize(settingsToApply.freezeTimeWhileReadingConversations);
				break;
			case SettingsID.VOL_MASTER:
				menuValueOption.Initialize(settingsToApply.GetSliderValMasterVol());
				break;
			case SettingsID.VOL_MUSIC:
				menuValueOption.Initialize(settingsToApply.GetSliderValMusicVol());
				break;
			case SettingsID.VOL_SFX:
				menuValueOption.Initialize(settingsToApply.GetSliderValSFXVol());
				break;
			case SettingsID.INPUT_BUTTON_PROMPT:
				((OptionsSelectorElement)menuValueOption).Initialize((int)PlayerData.GetButtonPromptImageSetting(), SettingsSave.GetButtonPromptImageStrings());
				break;
			}
		}
	}

	public static void ApplyGraphicSettingsToUi(MenuValueOption[] uiOptions, GraphicSettings settingsToApply)
	{
		MenuValueOption menuValueOption = null;
		MenuValueOption menuValueOption2 = null;
		for (int i = 0; i < uiOptions.Length; i++)
		{
			switch (uiOptions[i].GetSettingsID())
			{
			case SettingsID.GFX_FULLSCREEN:
				uiOptions[i].Initialize(settingsToApply.fullScreen);
				break;
			case SettingsID.GFX_DISPLAY_NUM:
			{
				string[] array = new string[settingsToApply.GetNumberOfDisplays()];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = "Display " + (j + 1);
				}
				((OptionsSelectorElement)uiOptions[i]).Initialize(settingsToApply.displayNumber, array);
				break;
			}
			case SettingsID.GFX_AA_TYPE:
				((OptionsSelectorElement)uiOptions[i]).Initialize((int)settingsToApply.antiAlias, Enum.GetNames(typeof(AntiAliasType)));
				break;
			case SettingsID.GFX_AA_QUAL:
				((OptionsSelectorElement)uiOptions[i]).Initialize((int)settingsToApply.antiAliasQuality, GraphicSettings.GetAntiAliasQualityStrings());
				break;
			case SettingsID.GFX_TEX_QUAL:
				((OptionsSelectorElement)uiOptions[i]).Initialize((int)settingsToApply.textureQuality, GraphicSettings.GetTextureQualityStrings());
				break;
			case SettingsID.GFX_SHADOW_QUAL:
				((OptionsSelectorElement)uiOptions[i]).Initialize((int)settingsToApply.shadowQuality, GraphicSettings.GetShadowQualityStrings());
				break;
			case SettingsID.GFX_AO_QUAL:
				((OptionsSelectorElement)uiOptions[i]).Initialize((int)settingsToApply.ambientOcclusionQuality, GraphicSettings.GetSSAOQualityStrings());
				break;
			case SettingsID.GFX_OCEAN_QUAL:
				((OptionsSelectorElement)uiOptions[i]).Initialize((int)settingsToApply.oceanQuality, GraphicSettings.GetGenericQualityStrings());
				break;
			case SettingsID.GFX_LIGHTING_QUAL:
				((OptionsSelectorElement)uiOptions[i]).Initialize((int)settingsToApply.lightingQuality, GraphicSettings.GetGenericQualityStrings());
				break;
			case SettingsID.GFX_VSYNC:
				uiOptions[i].Initialize(settingsToApply.vSyncEnabled);
				break;
			case SettingsID.GFX_GAMMA:
			{
				uiOptions[i].Initialize(settingsToApply.GetSliderValGamma());
				float num = settingsToApply.gammaValue * 2f - 1f;
				uiOptions[i].GetSecondaryTextField().text = ((num > 0f) ? ("+" + num.ToString("F1")) : num.ToString("F1"));
				break;
			}
			case SettingsID.GFX_FOV:
				uiOptions[i].Initialize(settingsToApply.GetSliderValFOV());
				uiOptions[i].GetSecondaryTextField().text = settingsToApply.fieldOfView.ToString("F0");
				break;
			case SettingsID.GFX_DITHER:
				uiOptions[i].Initialize(settingsToApply.GetSliderValDither());
				break;
			case SettingsID.GFX_ASPECT_RATIO:
				menuValueOption = uiOptions[i];
				break;
			case SettingsID.GFX_RESOLUTION:
				menuValueOption2 = uiOptions[i];
				break;
			case SettingsID.GFX_CONSOLE_PERF_MODE:
				((OptionsSelectorElement)uiOptions[i]).Initialize((int)settingsToApply.consolePerformanceMode, GraphicSettings.GetPerformanceModeStrings());
				break;
			}
		}
		if (!(menuValueOption != null) || !(menuValueOption2 != null))
		{
			return;
		}
		AspectRatio[] availableAspectRatioList = SystemDisplay.GetAvailableAspectRatioList();
		string[] array2 = new string[availableAspectRatioList.Length];
		for (int k = 0; k < array2.Length; k++)
		{
			array2[k] = GraphicSettings.GetAspectRatioString(availableAspectRatioList[k]);
		}
		Resolution resolution = default(Resolution);
		resolution.width = settingsToApply.displayResWidth;
		resolution.height = settingsToApply.displayResHeight;
		AspectRatio aspectRatio;
		if (SystemDisplay.IsResolutionAvailable(resolution))
		{
			((OptionsSelectorElement)menuValueOption).Initialize(GetIndexFromAspectRatio(settingsToApply.aspectRatio, array2), array2);
			aspectRatio = settingsToApply.aspectRatio;
		}
		else
		{
			resolution = SystemDisplay.GetDefaultResolution();
			AspectRatio aspectRatioFromResolution = SystemDisplay.GetAspectRatioFromResolution(resolution);
			((OptionsSelectorElement)menuValueOption).Initialize(GetIndexFromAspectRatio(aspectRatioFromResolution, array2), array2);
			aspectRatio = aspectRatioFromResolution;
		}
		int index = 0;
		Resolution[] resolutionsWithAspect = SystemDisplay.GetResolutionsWithAspect(aspectRatio);
		string[] array3 = new string[resolutionsWithAspect.Length];
		for (int l = 0; l < resolutionsWithAspect.Length; l++)
		{
			if (resolution.width == resolutionsWithAspect[l].width && resolution.height == resolutionsWithAspect[l].height)
			{
				index = l;
			}
			array3[l] = resolutionsWithAspect[l].width + "x" + resolutionsWithAspect[l].height;
		}
		((OptionsSelectorElement)menuValueOption2).Initialize(index, array3);
	}

	private static int GetIndexFromAspectRatio(AspectRatio ar, string[] aspectRatioDisplayStrings)
	{
		for (int i = 0; i < aspectRatioDisplayStrings.Length; i++)
		{
			if (GraphicSettings.GetAspectRatioFromString(aspectRatioDisplayStrings[i]) == ar)
			{
				return i;
			}
		}
		return -1;
	}

	public static void UpdateAllSettingsDataFromUi(SettingsMenuData[] settingsUiOptions, SettingsSave settingsToUpdate, Resolution[] availResolutions = null, GraphicSettings gfxSettingsToUpdate = null)
	{
		for (int i = 0; i < settingsUiOptions.Length; i++)
		{
			UpdateAnySettingDataFromUi(settingsUiOptions[i], settingsToUpdate, availResolutions, gfxSettingsToUpdate);
		}
	}

	public static void UpdateAnySettingDataFromUi(SettingsMenuData settingsUiOption, SettingsSave settingsToUpdate, Resolution[] availResolutions, GraphicSettings gfxSettingsToUpdate)
	{
		MenuValueOption menuValueOption = settingsUiOption.uiMenuOption as MenuValueOption;
		switch (settingsUiOption.id)
		{
		case SettingsID.AUTO_BOOST:
			settingsToUpdate.autoBoost = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.AUTO_EQUIP_TRANSLATOR:
			settingsToUpdate.autoEquipTranslator = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.SHIP_LOG_NOTIFICATION:
			settingsToUpdate.showShipLogNotifications = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.SHIP_LOG_RUMOR_MODE:
			settingsToUpdate.detectiveModeEnabled = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.AUTOPILOT:
			settingsToUpdate.autopilotEnabled = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.BUTTON_PROMPTS:
			settingsToUpdate.buttonPromptsEnabled = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.REDUCED_FRIGHTS:
			settingsToUpdate.reducedFrights = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.INPUT_BUTTON_PROMPT:
			settingsToUpdate.promptImgSet = (ButtonPromptImgSet)menuValueOption.GetValue();
			break;
		case SettingsID.LOOK_INVERT:
			settingsToUpdate.inversionFactor = ((!menuValueOption.GetValueAsBool()) ? 1 : (-1));
			break;
		case SettingsID.FLIGHT_INVERT:
			settingsToUpdate.shipInversionFactor = ((!menuValueOption.GetValueAsBool()) ? 1 : (-1));
			break;
		case SettingsID.RUMBLE:
			settingsToUpdate.rumbleEnabled = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.LOOK_SENSITIVITY:
			settingsToUpdate.SetSliderValLookSensitivity(menuValueOption.GetValue());
			break;
		case SettingsID.FLIGHT_SENSITIVITY:
			settingsToUpdate.SetSliderValFlightSensitivity(menuValueOption.GetValue());
			break;
		case SettingsID.INNER_DEADZN:
			settingsToUpdate.SetSliderValInnerDeadZone(menuValueOption.GetValue());
			break;
		case SettingsID.OUTER_DEADZN:
			settingsToUpdate.SetSliderValOuterDeadZone(menuValueOption.GetValue());
			break;
		case SettingsID.LANGUAGE:
			settingsToUpdate.language = (TextTranslation.Language)menuValueOption.GetValue();
			break;
		case SettingsID.TEXT_SPEED:
			settingsToUpdate.textSpeed = (TextSpeed)menuValueOption.GetValue();
			break;
		case SettingsID.UI_SIZE:
			settingsToUpdate.uiTextSize = (UITextSize)menuValueOption.GetValue();
			break;
		case SettingsID.FREEZE_TIME_TRANSLATOR:
			settingsToUpdate.freezeTimeWhileReading = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.FREEZE_TIME_LOG:
			settingsToUpdate.freezeTimeWhileReadingShipLog = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.FREEZE_TIME_DIALOGUE:
			settingsToUpdate.freezeTimeWhileReadingConversations = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.VOL_MASTER:
			settingsToUpdate.SetSliderValMasterVol(((SliderElement)menuValueOption).GetFloatValue());
			break;
		case SettingsID.VOL_MUSIC:
			settingsToUpdate.SetSliderValMusicVol(((SliderElement)menuValueOption).GetFloatValue());
			break;
		case SettingsID.VOL_SFX:
			settingsToUpdate.SetSliderValSFXVol(((SliderElement)menuValueOption).GetFloatValue());
			break;
		case SettingsID.GFX_FULLSCREEN:
			gfxSettingsToUpdate.fullScreen = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.GFX_ASPECT_RATIO:
			gfxSettingsToUpdate.aspectRatio = GraphicSettings.GetAspectRatioFromString(((OptionsSelectorElement)menuValueOption).GetSelectedOption());
			break;
		case SettingsID.GFX_RESOLUTION:
			gfxSettingsToUpdate.displayResWidth = availResolutions[menuValueOption.GetValue()].width;
			gfxSettingsToUpdate.displayResHeight = availResolutions[menuValueOption.GetValue()].height;
			break;
		case SettingsID.GFX_DISPLAY_NUM:
			gfxSettingsToUpdate.displayNumber = menuValueOption.GetValue();
			break;
		case SettingsID.GFX_AA_TYPE:
			gfxSettingsToUpdate.antiAlias = (AntiAliasType)menuValueOption.GetValue();
			break;
		case SettingsID.GFX_AA_QUAL:
			gfxSettingsToUpdate.antiAliasQuality = (AntiAliasQuality)menuValueOption.GetValue();
			break;
		case SettingsID.GFX_TEX_QUAL:
			gfxSettingsToUpdate.textureQuality = (TextureQuality)menuValueOption.GetValue();
			break;
		case SettingsID.GFX_OCEAN_QUAL:
			gfxSettingsToUpdate.oceanQuality = (GenericQuality)menuValueOption.GetValue();
			break;
		case SettingsID.GFX_LIGHTING_QUAL:
			gfxSettingsToUpdate.lightingQuality = (GenericQuality)menuValueOption.GetValue();
			break;
		case SettingsID.GFX_SHADOW_QUAL:
			gfxSettingsToUpdate.shadowQuality = (ShadowQuality)menuValueOption.GetValue();
			break;
		case SettingsID.GFX_AO_QUAL:
			gfxSettingsToUpdate.ambientOcclusionQuality = (SSAOQuality)menuValueOption.GetValue();
			break;
		case SettingsID.GFX_VSYNC:
			gfxSettingsToUpdate.vSyncEnabled = menuValueOption.GetValueAsBool();
			break;
		case SettingsID.GFX_GAMMA:
			gfxSettingsToUpdate.SetSliderValGamma(menuValueOption.GetValue());
			break;
		case SettingsID.GFX_FOV:
			gfxSettingsToUpdate.SetSliderValFOV(menuValueOption.GetValue());
			break;
		case SettingsID.GFX_DITHER:
			gfxSettingsToUpdate.SetSliderValDither(menuValueOption.GetValue());
			break;
		case SettingsID.GFX_CONSOLE_PERF_MODE:
			gfxSettingsToUpdate.consolePerformanceMode = (PerformanceMode)menuValueOption.GetValue();
			break;
		default:
			Debug.LogWarning("SettingsMenuModel not setup to save Menu Value option with ID: " + settingsUiOption.id, settingsUiOption.uiMenuOption.gameObject);
			break;
		}
	}

	public static void InitMenuOptionTextLabels(MenuOption[] settings, MenuValueOption confirmToggleOption = null)
	{
		if (s_lookupTable == null)
		{
			LoadSettingsLookupTable();
		}
		Dictionary<SettingsID, SettingsLookupTable.SettingsEntry> dictionary = new Dictionary<SettingsID, SettingsLookupTable.SettingsEntry>();
		for (int i = 0; i < s_lookupTable.settingsEntries.Length; i++)
		{
			try
			{
				SettingsLookupTable.SettingsEntry value = s_lookupTable.settingsEntries[i];
				if (value.settingsId != 0 && value.settingsId != SettingsID.REBINDABLE_OPTION)
				{
					dictionary.Add(value.settingsId, value);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
			}
		}
		foreach (MenuOption menuOption in settings)
		{
			SettingsID settingsID = menuOption.GetSettingsID();
			if (!dictionary.ContainsKey(settingsID))
			{
				continue;
			}
			if (menuOption.GetLabelField() != null)
			{
				LocalizedText addComponent = menuOption.GetLabelField().gameObject.GetAddComponent<LocalizedText>();
				UITextType overrideUITextType = menuOption.GetOverrideUITextType();
				if (overrideUITextType != 0)
				{
					addComponent.SetTextID(overrideUITextType);
				}
				else
				{
					addComponent.SetTextID(dictionary[settingsID].labelTextType);
				}
			}
			menuOption.SetTooltipText(dictionary[settingsID].tooltipTextType);
		}
		if (!(confirmToggleOption != null))
		{
			return;
		}
		SettingsID settingsID2 = confirmToggleOption.GetSettingsID();
		if (dictionary.ContainsKey(settingsID2))
		{
			LocalizedText addComponent2 = confirmToggleOption.GetLabelField().gameObject.GetAddComponent<LocalizedText>();
			UITextType overrideUITextType2 = confirmToggleOption.GetOverrideUITextType();
			if (overrideUITextType2 != 0)
			{
				addComponent2.SetTextID(overrideUITextType2);
			}
			else
			{
				addComponent2.SetTextID(dictionary[settingsID2].labelTextType);
			}
			confirmToggleOption.SetTooltipText(dictionary[settingsID2].tooltipTextType);
		}
	}

	public static void InitRebindableOptionTextLabels(KeyRebindingElement[] rebindableOptions)
	{
		if (s_lookupTable == null)
		{
			LoadSettingsLookupTable();
		}
		Dictionary<RebindableID, SettingsLookupTable.SettingsEntry> dictionary = new Dictionary<RebindableID, SettingsLookupTable.SettingsEntry>();
		for (int i = 0; i < s_lookupTable.settingsEntries.Length; i++)
		{
			try
			{
				SettingsLookupTable.SettingsEntry value = s_lookupTable.settingsEntries[i];
				if (value.settingsId != 0 && value.settingsId == SettingsID.REBINDABLE_OPTION)
				{
					dictionary.Add(value.rebindableId, value);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
			}
		}
		foreach (KeyRebindingElement keyRebindingElement in rebindableOptions)
		{
			if (keyRebindingElement.GetSettingsID() != SettingsID.REBINDABLE_OPTION)
			{
				continue;
			}
			RebindableID rebindableID = keyRebindingElement.GetRebindableID();
			if (dictionary.ContainsKey(rebindableID))
			{
				LocalizedText addComponent = keyRebindingElement.GetLabelField().gameObject.GetAddComponent<LocalizedText>();
				UITextType overrideUITextType = keyRebindingElement.GetOverrideUITextType();
				if (overrideUITextType != 0)
				{
					addComponent.SetTextID(overrideUITextType);
				}
				else
				{
					addComponent.SetTextID(dictionary[rebindableID].labelTextType);
				}
				keyRebindingElement.SetTooltipText(dictionary[rebindableID].tooltipTextType);
			}
		}
	}
}
