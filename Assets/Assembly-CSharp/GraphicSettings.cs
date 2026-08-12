using System;
using System.Runtime.Serialization;
using Steamworks;
using UnityEngine;

[Serializable]
public class GraphicSettings
{
	private static string s_twentyoneNineString = "21:9";

	private static string s_sixteenNineString = "16:9";

	private static string s_sixteenTenString = "16:10";

	private static string s_fiveFourString = "5:4";

	private static string s_fourThreeString = "4:3";

	public bool fullScreen = true;

	public int displayNumber;

	public AspectRatio aspectRatio;

	public int displayResWidth;

	public int displayResHeight;

	[OptionalField(VersionAdded = 3)]
	public int refreshRate = 60;

	public AntiAliasType antiAlias = AntiAliasType.SMAA;

	public AntiAliasQuality antiAliasQuality = AntiAliasQuality.HIGH;

	public TextureQuality textureQuality;

	public GenericQuality oceanQuality = GenericQuality.HIGH;

	public ShadowQuality shadowQuality = ShadowQuality.VERY_HIGH;

	public SSAOQuality ambientOcclusionQuality = SSAOQuality.MEDIUM;

	public bool vSyncEnabled;

	[OptionalField(VersionAdded = 7)]
	public GenericQuality lightingQuality = GenericQuality.HIGH;

	[OptionalField(VersionAdded = 8)]
	public PerformanceMode consolePerformanceMode;

	public static float s_gammaMin = 0f;

	public static float s_gammaMax = 1f;

	public float gammaValue = 0.5f;

	public static float s_fovMin = 55f;

	public static float s_fovMax = 105f;

	[OptionalField(VersionAdded = 2)]
	public float fieldOfView = 70f;

	public static float s_ditherMin = 0f;

	public static float s_ditherMax = 1f;

	[OptionalField(VersionAdded = 2)]
	public float dithering = 0.1f;

	private static SettingsID[] s_floatValIds = new SettingsID[3]
	{
		SettingsID.GFX_GAMMA,
		SettingsID.GFX_FOV,
		SettingsID.GFX_DITHER
	};

	private static SettingsID[] s_intValIds = new SettingsID[8]
	{
		SettingsID.GFX_AA_TYPE,
		SettingsID.GFX_AA_QUAL,
		SettingsID.GFX_TEX_QUAL,
		SettingsID.GFX_OCEAN_QUAL,
		SettingsID.GFX_SHADOW_QUAL,
		SettingsID.GFX_AO_QUAL,
		SettingsID.GFX_LIGHTING_QUAL,
		SettingsID.GFX_CONSOLE_PERF_MODE
	};

	private static SettingsID[] s_boolValIds = new SettingsID[1] { SettingsID.GFX_VSYNC };

	private static bool s_defaultsInitialized = false;

	private static float[] s_floatValDefaults;

	private static int[] s_intValDefaults;

	private static bool[] s_boolValDefaults;

	public static string GetAspectRatioString(AspectRatio ar)
	{
		switch (ar)
		{
		case AspectRatio.UNKNOWN:
			return "???";
		case AspectRatio.TWENTYONE_NINE:
			return s_twentyoneNineString;
		case AspectRatio.SIXTEEN_NINE:
			return s_sixteenNineString;
		case AspectRatio.SIXTEEN_TEN:
			return s_sixteenTenString;
		case AspectRatio.FIVE_FOUR:
			return s_fiveFourString;
		case AspectRatio.FOUR_THREE:
			return s_fourThreeString;
		default:
			return "";
		}
	}

	public static AspectRatio GetAspectRatioFromString(string s)
	{
		if (s == s_twentyoneNineString)
		{
			return AspectRatio.TWENTYONE_NINE;
		}
		if (s == s_sixteenNineString)
		{
			return AspectRatio.SIXTEEN_NINE;
		}
		if (s == s_sixteenTenString)
		{
			return AspectRatio.SIXTEEN_TEN;
		}
		if (s == s_fiveFourString)
		{
			return AspectRatio.FIVE_FOUR;
		}
		if (s == s_fourThreeString)
		{
			return AspectRatio.FOUR_THREE;
		}
		return AspectRatio.UNKNOWN;
	}

	public static string[] GetGenericQualityStrings()
	{
		return new string[3]
		{
			UITextLibrary.GetString(UITextType.OptionLow),
			UITextLibrary.GetString(UITextType.OptionMedium),
			UITextLibrary.GetString(UITextType.OptionHigh)
		};
	}

	public static string[] GetShadowQualityStrings()
	{
		return new string[4]
		{
			UITextLibrary.GetString(UITextType.OptionLow),
			UITextLibrary.GetString(UITextType.OptionMedium),
			UITextLibrary.GetString(UITextType.OptionHigh),
			UITextLibrary.GetString(UITextType.OptionVHigh)
		};
	}

	public static string[] GetSSAOQualityStrings()
	{
		return new string[5]
		{
			UITextLibrary.GetString(UITextType.OptionOff),
			UITextLibrary.GetString(UITextType.OptionLow),
			UITextLibrary.GetString(UITextType.OptionMedium),
			UITextLibrary.GetString(UITextType.OptionHigh),
			UITextLibrary.GetString(UITextType.OptionVHigh)
		};
	}

	public static string[] GetTextureQualityStrings()
	{
		return new string[4]
		{
			UITextLibrary.GetString(UITextType.OptionFull),
			UITextLibrary.GetString(UITextType.OptionHalf),
			UITextLibrary.GetString(UITextType.OptionQuarter),
			UITextLibrary.GetString(UITextType.OptionEighth)
		};
	}

	public static string[] GetAntiAliasQualityStrings()
	{
		return new string[4]
		{
			UITextLibrary.GetString(UITextType.OptionLow),
			UITextLibrary.GetString(UITextType.OptionMedium),
			UITextLibrary.GetString(UITextType.OptionHigh),
			UITextLibrary.GetString(UITextType.OptionVHigh)
		};
	}

	public static string[] GetPerformanceModeStrings()
	{
		return new string[3]
		{
			UITextLibrary.GetString(UITextType.OptionFramerate),
			UITextLibrary.GetString(UITextType.OptionResolution),
			UITextLibrary.GetString(UITextType.OptionBalanced)
		};
	}

	public int GetSliderValGamma()
	{
		return Mathf.RoundToInt(Mathf.InverseLerp(s_gammaMin, s_gammaMax, gammaValue) * 10f);
	}

	public void SetSliderValGamma(float value)
	{
		gammaValue = Mathf.Lerp(s_gammaMin, s_gammaMax, value * 0.1f);
	}

	public int GetSliderValFOV()
	{
		return Mathf.RoundToInt(Mathf.InverseLerp(s_fovMin, s_fovMax, fieldOfView) * 10f);
	}

	public void SetSliderValFOV(float value)
	{
		fieldOfView = Mathf.Lerp(s_fovMin, s_fovMax, value * 0.1f);
	}

	public int GetSliderValDither()
	{
		return Mathf.RoundToInt(Mathf.InverseLerp(s_ditherMin, s_ditherMax, dithering) * 10f);
	}

	public void SetSliderValDither(float value)
	{
		dithering = Mathf.Lerp(s_ditherMin, s_ditherMax, value * 0.1f);
	}

	public GraphicSettings(bool init)
	{
		if (init)
		{
			SetDefaultSettings();
		}
	}

	[OnDeserializing]
	private void SetDefaultValuesOnDeserializing(StreamingContext context)
	{
		refreshRate = 60;
		lightingQuality = GenericQuality.HIGH;
		consolePerformanceMode = PerformanceMode.FRAMERATE;
	}

	[OnDeserialized]
	private void SetDefaultValuesOnDeserialized(StreamingContext context)
	{
	}

	public static void InitializeDefaultSettings()
	{
		if (!s_defaultsInitialized)
		{
			new GraphicSettings(init: true);
		}
	}

	public void SetDefaultSettings()
	{
		gammaValue = 0.5f;
		bool flag = false;
		if (SteamUtils.IsSteamRunningOnSteamDeck())
		{
			flag = true;
		}
		fieldOfView = (flag ? 75f : 70f);
		dithering = 0.2f;
		vSyncEnabled = QualitySettings.vSyncCount > 0;
		consolePerformanceMode = PerformanceMode.FRAMERATE;
		if (SteamUtils.IsSteamRunningOnSteamDeck())
		{
			ApplySteamDeckPlatformSettings();
		}
		else
		{
			SetPreset(GraphicSettingsPreset.ULTRA);
		}
		Resolution defaultResolution = SystemDisplay.GetDefaultResolution();
		aspectRatio = SystemDisplay.GetAspectRatioFromResolution(defaultResolution);
		if (!s_defaultsInitialized)
		{
			s_floatValDefaults = new float[3] { gammaValue, fieldOfView, dithering };
			s_intValDefaults = new int[8]
			{
				(int)antiAlias,
				(int)antiAliasQuality,
				(int)textureQuality,
				(int)oceanQuality,
				(int)shadowQuality,
				(int)ambientOcclusionQuality,
				(int)lightingQuality,
				(int)consolePerformanceMode
			};
			s_boolValDefaults = new bool[1] { vSyncEnabled };
		}
		if (PlayerPrefs.HasKey("UnitySelectMonitor"))
		{
			displayNumber = PlayerPrefs.GetInt("UnitySelectMonitor");
		}
		if (PlayerPrefs.HasKey("Screenmanager Resolution Width"))
		{
			displayResWidth = PlayerPrefs.GetInt("Screenmanager Resolution Width");
		}
		else
		{
			displayResWidth = defaultResolution.width;
		}
		if (PlayerPrefs.HasKey("Screenmanager Resolution Height"))
		{
			displayResHeight = PlayerPrefs.GetInt("Screenmanager Resolution Height");
		}
		else
		{
			displayResHeight = defaultResolution.height;
		}
	}

	public static void SetToDefaults(GraphicSettings targetSave, SettingsID[] idsToSet)
	{
		InitializeDefaultSettings();
		foreach (SettingsID settingsID in idsToSet)
		{
			bool flag = false;
			for (int j = 0; j < s_boolValIds.Length; j++)
			{
				if (settingsID == s_boolValIds[j])
				{
					if (settingsID == SettingsID.GFX_VSYNC)
					{
						targetSave.vSyncEnabled = s_boolValDefaults[j];
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
				if (settingsID == s_intValIds[k])
				{
					switch (settingsID)
					{
					case SettingsID.GFX_AA_TYPE:
						targetSave.antiAlias = (AntiAliasType)s_intValDefaults[k];
						break;
					case SettingsID.GFX_AA_QUAL:
						targetSave.antiAliasQuality = (AntiAliasQuality)s_intValDefaults[k];
						break;
					case SettingsID.GFX_TEX_QUAL:
						targetSave.textureQuality = (TextureQuality)s_intValDefaults[k];
						break;
					case SettingsID.GFX_OCEAN_QUAL:
						targetSave.oceanQuality = (GenericQuality)s_intValDefaults[k];
						break;
					case SettingsID.GFX_SHADOW_QUAL:
						targetSave.shadowQuality = (ShadowQuality)s_intValDefaults[k];
						break;
					case SettingsID.GFX_AO_QUAL:
						targetSave.ambientOcclusionQuality = (SSAOQuality)s_intValDefaults[k];
						break;
					case SettingsID.GFX_LIGHTING_QUAL:
						targetSave.lightingQuality = (GenericQuality)s_intValDefaults[k];
						break;
					case SettingsID.GFX_CONSOLE_PERF_MODE:
						targetSave.consolePerformanceMode = (PerformanceMode)s_intValDefaults[k];
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
			for (int l = 0; l < s_floatValIds.Length; l++)
			{
				if (settingsID == s_floatValIds[l])
				{
					switch (settingsID)
					{
					case SettingsID.GFX_GAMMA:
						targetSave.gammaValue = s_floatValDefaults[l];
						break;
					case SettingsID.GFX_FOV:
						targetSave.fieldOfView = s_floatValDefaults[l];
						break;
					case SettingsID.GFX_DITHER:
						targetSave.dithering = s_floatValDefaults[l];
						break;
					}
					break;
				}
			}
		}
	}

	public void ApplySteamDeckPlatformSettings()
	{
		antiAlias = AntiAliasType.SMAA;
		antiAliasQuality = AntiAliasQuality.MEDIUM;
		textureQuality = TextureQuality.FULL;
		oceanQuality = GenericQuality.MEDIUM;
		shadowQuality = ShadowQuality.MEDIUM;
		ambientOcclusionQuality = SSAOQuality.MEDIUM;
		lightingQuality = GenericQuality.MEDIUM;
		vSyncEnabled = true;
		Resolution currentResolution = Screen.currentResolution;
		aspectRatio = SystemDisplay.GetAspectRatioFromResolution(currentResolution);
		displayResWidth = currentResolution.width;
		displayResHeight = currentResolution.height;
	}

	public void ApplyAllGraphicSettings()
	{
		GlobalMessenger<GraphicSettings>.FireEvent("GraphicSettingsUpdated", this);
		QualitySettings.masterTextureLimit = Convert.ToInt32(textureQuality);
		if (vSyncEnabled)
		{
			if (SecretSettings.TryGetInt("VSyncCount", out var value))
			{
				value = Mathf.Clamp(value, 0, 4);
				Debug.Log("VSync count override: " + value);
				QualitySettings.vSyncCount = value;
			}
			else
			{
				QualitySettings.vSyncCount = 1;
			}
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
		switch (shadowQuality)
		{
		case ShadowQuality.LOW:
			QualitySettings.shadowDistance = 50f;
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			ProxyShadowSettings.shadowDistance = 1200f;
			ProxyShadowSettings.cascadeDivisions = new ProxyShadowCascade.Division[2]
			{
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Final, 0.3333f),
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Final, 1f)
			};
			ProxyShadowSettings.shadowTextureSquareSize = 512;
			break;
		case ShadowQuality.MEDIUM:
			QualitySettings.shadowDistance = 100f;
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			ProxyShadowSettings.shadowDistance = 1200f;
			ProxyShadowSettings.cascadeDivisions = new ProxyShadowCascade.Division[2]
			{
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Near, 0.3333f),
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Final, 1f)
			};
			ProxyShadowSettings.shadowTextureSquareSize = 1024;
			break;
		case ShadowQuality.HIGH:
			QualitySettings.shadowDistance = 200f;
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowResolution = ShadowResolution.High;
			ProxyShadowSettings.shadowDistance = 1200f;
			ProxyShadowSettings.cascadeDivisions = new ProxyShadowCascade.Division[2]
			{
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Near, 0.3333f),
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Final, 1f)
			};
			ProxyShadowSettings.shadowTextureSquareSize = 1024;
			break;
		case ShadowQuality.VERY_HIGH:
			QualitySettings.shadowDistance = 200f;
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
			ProxyShadowSettings.shadowDistance = 1200f;
			ProxyShadowSettings.cascadeDivisions = new ProxyShadowCascade.Division[4]
			{
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Near, 0.3333f),
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Mid, 0.5f),
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Far, 0.75f),
				new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Final, 1f)
			};
			ProxyShadowSettings.shadowTextureSquareSize = 2048;
			break;
		}
		Shader shader = Shader.Find("Outer Wilds/Environment/Giant's Deep/Ocean");
		Shader shader2 = Shader.Find("Outer Wilds/Utility/Ocean Stencil");
		Shader shader3 = Shader.Find("Outer Wilds/Environment/Giant's Deep/Current");
		Shader shader4 = Shader.Find("Outer Wilds/Environment/Invisible Planet/Ringworld/River");
		Shader shader5 = Shader.Find("Outer Wilds/Environment/Invisible Planet/Ringworld/River Proxy");
		Shader shader6 = Shader.Find("Outer Wilds/Environment/Invisible Planet/Ringworld/River Stencil");
		switch (oceanQuality)
		{
		case GenericQuality.LOW:
			if (shader != null)
			{
				shader.maximumLOD = 400;
			}
			if (shader2 != null)
			{
				shader2.maximumLOD = 100;
			}
			if (shader3 != null)
			{
				shader3.maximumLOD = 200;
			}
			if (shader4 != null)
			{
				shader4.maximumLOD = 400;
			}
			if (shader5 != null)
			{
				shader5.maximumLOD = 200;
			}
			if (shader6 != null)
			{
				shader6.maximumLOD = 100;
			}
			break;
		case GenericQuality.MEDIUM:
			if (shader != null)
			{
				shader.maximumLOD = 500;
			}
			if (shader2 != null)
			{
				shader2.maximumLOD = 200;
			}
			if (shader3 != null)
			{
				shader3.maximumLOD = 300;
			}
			if (shader4 != null)
			{
				shader4.maximumLOD = 500;
			}
			if (shader5 != null)
			{
				shader5.maximumLOD = 300;
			}
			if (shader6 != null)
			{
				shader6.maximumLOD = 200;
			}
			break;
		case GenericQuality.HIGH:
			if (shader != null)
			{
				shader.maximumLOD = -1;
			}
			if (shader2 != null)
			{
				shader2.maximumLOD = -1;
			}
			if (shader3 != null)
			{
				shader3.maximumLOD = -1;
			}
			if (shader4 != null)
			{
				shader4.maximumLOD = -1;
			}
			if (shader5 != null)
			{
				shader5.maximumLOD = -1;
			}
			if (shader6 != null)
			{
				shader6.maximumLOD = -1;
			}
			break;
		}
		if (displayNumber >= Display.displays.Length)
		{
			displayNumber = 0;
			Debug.Log("Index out of range. Display index set to 0");
		}
		PlayerPrefs.SetInt("UnitySelectMonitor", displayNumber);
		if (!SystemDisplay.IsResolutionAvailable(displayResWidth, displayResHeight))
		{
			displayResWidth = SystemDisplay.GetDefaultResolution().width;
			displayResHeight = SystemDisplay.GetDefaultResolution().height;
		}
		aspectRatio = SystemDisplay.GetAspectRatioFromResolution(displayResWidth, displayResHeight);
		PlayerPrefs.SetInt("Screenmanager Resolution Width", displayResWidth);
		PlayerPrefs.SetInt("Screenmanager Resolution Height", displayResHeight);
		Screen.SetResolution(displayResWidth, displayResHeight, fullScreen);
	}

	public bool DisplaySettingsEqual(GraphicSettings settings)
	{
		if (this == settings)
		{
			return true;
		}
		if (settings == null)
		{
			return false;
		}
		if (settings.fullScreen == fullScreen && settings.aspectRatio == aspectRatio && settings.displayResWidth == displayResWidth && settings.displayResHeight == displayResHeight)
		{
			return settings.displayNumber == displayNumber;
		}
		return false;
	}

	public int GetNumberOfDisplays()
	{
		return Display.displays.Length;
	}

	public GraphicSettingsPreset EqualsPreset()
	{
		if (antiAlias == AntiAliasType.NONE && antiAliasQuality == AntiAliasQuality.LOW && textureQuality == TextureQuality.HALF && oceanQuality == GenericQuality.LOW && shadowQuality == ShadowQuality.LOW && ambientOcclusionQuality == SSAOQuality.OFF && lightingQuality == GenericQuality.LOW)
		{
			return GraphicSettingsPreset.LOW;
		}
		if (antiAlias == AntiAliasType.SMAA && antiAliasQuality == AntiAliasQuality.MEDIUM && textureQuality == TextureQuality.FULL && oceanQuality == GenericQuality.MEDIUM && shadowQuality == ShadowQuality.MEDIUM && ambientOcclusionQuality == SSAOQuality.MEDIUM && lightingQuality == GenericQuality.MEDIUM)
		{
			return GraphicSettingsPreset.MEDIUM;
		}
		if (antiAlias == AntiAliasType.SMAA && antiAliasQuality == AntiAliasQuality.HIGH && textureQuality == TextureQuality.FULL && oceanQuality == GenericQuality.HIGH && shadowQuality == ShadowQuality.HIGH && ambientOcclusionQuality == SSAOQuality.HIGH && lightingQuality == GenericQuality.HIGH)
		{
			return GraphicSettingsPreset.HIGH;
		}
		if (antiAlias == AntiAliasType.SMAA && antiAliasQuality == AntiAliasQuality.VERY_HIGH && textureQuality == TextureQuality.FULL && oceanQuality == GenericQuality.HIGH && shadowQuality == ShadowQuality.VERY_HIGH && ambientOcclusionQuality == SSAOQuality.VERY_HIGH && lightingQuality == GenericQuality.HIGH)
		{
			return GraphicSettingsPreset.ULTRA;
		}
		return GraphicSettingsPreset.NONE;
	}

	public void SetPreset(GraphicSettingsPreset preset)
	{
		switch (preset)
		{
		case GraphicSettingsPreset.LOW:
			antiAlias = AntiAliasType.NONE;
			antiAliasQuality = AntiAliasQuality.LOW;
			textureQuality = TextureQuality.HALF;
			oceanQuality = GenericQuality.LOW;
			shadowQuality = ShadowQuality.LOW;
			ambientOcclusionQuality = SSAOQuality.OFF;
			lightingQuality = GenericQuality.LOW;
			break;
		case GraphicSettingsPreset.MEDIUM:
			antiAlias = AntiAliasType.SMAA;
			antiAliasQuality = AntiAliasQuality.MEDIUM;
			textureQuality = TextureQuality.FULL;
			oceanQuality = GenericQuality.MEDIUM;
			shadowQuality = ShadowQuality.MEDIUM;
			ambientOcclusionQuality = SSAOQuality.MEDIUM;
			lightingQuality = GenericQuality.MEDIUM;
			break;
		case GraphicSettingsPreset.HIGH:
			antiAlias = AntiAliasType.SMAA;
			antiAliasQuality = AntiAliasQuality.HIGH;
			textureQuality = TextureQuality.FULL;
			oceanQuality = GenericQuality.HIGH;
			shadowQuality = ShadowQuality.HIGH;
			ambientOcclusionQuality = SSAOQuality.HIGH;
			lightingQuality = GenericQuality.HIGH;
			break;
		case GraphicSettingsPreset.ULTRA:
			antiAlias = AntiAliasType.SMAA;
			antiAliasQuality = AntiAliasQuality.VERY_HIGH;
			textureQuality = TextureQuality.FULL;
			oceanQuality = GenericQuality.HIGH;
			shadowQuality = ShadowQuality.VERY_HIGH;
			ambientOcclusionQuality = SSAOQuality.VERY_HIGH;
			lightingQuality = GenericQuality.HIGH;
			break;
		}
	}

	public string ToJson()
	{
		return JsonUtility.ToJson(this);
	}

	public static GraphicSettings FromJson(string json)
	{
		try
		{
			return JsonUtility.FromJson<GraphicSettings>(json);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not load graphics settings: " + ex.Message);
			return null;
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is GraphicSettings graphicSettings))
		{
			return false;
		}
		if (true && graphicSettings.fullScreen == fullScreen && graphicSettings.aspectRatio == aspectRatio && graphicSettings.displayResWidth == displayResWidth && graphicSettings.displayResHeight == displayResHeight && graphicSettings.displayNumber == displayNumber && graphicSettings.antiAlias == antiAlias && graphicSettings.antiAliasQuality == antiAliasQuality && graphicSettings.textureQuality == textureQuality && graphicSettings.oceanQuality == oceanQuality && graphicSettings.shadowQuality == shadowQuality && graphicSettings.ambientOcclusionQuality == ambientOcclusionQuality && graphicSettings.lightingQuality == lightingQuality && graphicSettings.consolePerformanceMode == consolePerformanceMode && graphicSettings.vSyncEnabled == vSyncEnabled && graphicSettings.gammaValue == gammaValue && graphicSettings.fieldOfView == fieldOfView)
		{
			return graphicSettings.dithering == dithering;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public GraphicSettings Clone()
	{
		return new GraphicSettings(init: false)
		{
			fullScreen = fullScreen,
			aspectRatio = aspectRatio,
			displayResWidth = displayResWidth,
			displayResHeight = displayResHeight,
			refreshRate = refreshRate,
			displayNumber = displayNumber,
			antiAlias = antiAlias,
			antiAliasQuality = antiAliasQuality,
			textureQuality = textureQuality,
			oceanQuality = oceanQuality,
			shadowQuality = shadowQuality,
			ambientOcclusionQuality = ambientOcclusionQuality,
			lightingQuality = lightingQuality,
			consolePerformanceMode = consolePerformanceMode,
			gammaValue = gammaValue,
			vSyncEnabled = vSyncEnabled,
			fieldOfView = fieldOfView,
			dithering = dithering
		};
	}

	public void CopyTo(GraphicSettings targetSettings)
	{
		targetSettings.fullScreen = fullScreen;
		targetSettings.aspectRatio = aspectRatio;
		targetSettings.displayResWidth = displayResWidth;
		targetSettings.displayResHeight = displayResHeight;
		targetSettings.refreshRate = refreshRate;
		targetSettings.displayNumber = displayNumber;
		targetSettings.antiAlias = antiAlias;
		targetSettings.antiAliasQuality = antiAliasQuality;
		targetSettings.textureQuality = textureQuality;
		targetSettings.oceanQuality = oceanQuality;
		targetSettings.shadowQuality = shadowQuality;
		targetSettings.ambientOcclusionQuality = ambientOcclusionQuality;
		targetSettings.lightingQuality = lightingQuality;
		targetSettings.consolePerformanceMode = consolePerformanceMode;
		targetSettings.gammaValue = gammaValue;
		targetSettings.vSyncEnabled = vSyncEnabled;
		targetSettings.fieldOfView = fieldOfView;
		targetSettings.dithering = dithering;
	}
}
