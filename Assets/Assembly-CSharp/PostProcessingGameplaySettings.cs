using UnityEngine;
using UnityEngine.PostProcessing;

public class PostProcessingGameplaySettings
{
	private PostProcessingProfile _baseProfile;

	private PostProcessingProfile _runtimeProfile;

	public bool antialiasingAvailable;

	private bool _antialiasingEnabled;

	public bool ambientOcclusionAvailable;

	private bool _ambientOcclusionEnabled;

	public bool screenSpaceReflectionAvailable;

	private bool _screenSpaceReflectionEnabled;

	public bool bloomEnabled;

	public BloomModel.BloomSettings bloom;

	public BloomModel.LensDirtSettings lensDirt;

	public BloomModel.FrostSettings frost;

	public BloomModel.BreathFogSettings breathFog;

	public bool colorGradingEnabled;

	public ColorGradingModel.BasicSettings colorGrading;

	public bool userLutEnabled;

	public UserLutModel.Settings userLut;

	public bool chromaticAberrationEnabled;

	public ChromaticAberrationModel.Settings chromaticAberration;

	public bool vignetteEnabled;

	public VignetteModel.Settings vignette;

	public bool ditheringAvailable;

	private bool _ditheringEnabled;

	public bool customGammaAvailable;

	private bool _customGammaEnabled;

	public bool eyeMaskEnabled;

	public EyeMaskModel.Settings eyeMask;

	public bool phosphenesEnabled;

	public PhospheneModel.Settings phosphenes;

	public bool volumeOcclusionAvailable;

	private bool _volumeOcclusionEnabled;

	public BloomModel.BloomSettings bloomDefault => _baseProfile.bloom.settings.bloom;

	public BloomModel.LensDirtSettings lensDirtDefault => _baseProfile.bloom.settings.lensDirt;

	public BloomModel.FrostSettings frostDefault => _baseProfile.bloom.settings.frost;

	public BloomModel.BreathFogSettings breathFogDefault => _baseProfile.bloom.settings.breathFog;

	public ColorGradingModel.BasicSettings colorGradingDefault => _baseProfile.colorGrading.settings.basic;

	public UserLutModel.Settings userLutDefault => _baseProfile.userLut.settings;

	public ChromaticAberrationModel.Settings chromaticAberrationDefault => _baseProfile.chromaticAberration.settings;

	public VignetteModel.Settings vignetteDefault => _baseProfile.vignette.settings;

	public EyeMaskModel.Settings eyeMaskDefault => _baseProfile.eyeMask.settings;

	public PhospheneModel.Settings phosphenesDefault => _baseProfile.phosphenes.settings;

	public PostProcessingGameplaySettings(PostProcessingBehaviour postProcessing)
	{
		_baseProfile = postProcessing.profile;
		_runtimeProfile = Object.Instantiate(postProcessing.profile);
		_runtimeProfile.name = _baseProfile.name + "_Runtime";
		postProcessing.profile = _runtimeProfile;
		antialiasingAvailable = true;
		_antialiasingEnabled = _runtimeProfile.antialiasing.enabled;
		ambientOcclusionAvailable = true;
		_ambientOcclusionEnabled = _runtimeProfile.ambientOcclusion.enabled;
		screenSpaceReflectionAvailable = true;
		_screenSpaceReflectionEnabled = _runtimeProfile.screenSpaceReflection.enabled;
		bloomEnabled = _runtimeProfile.bloom.enabled;
		bloom = _runtimeProfile.bloom.settings.bloom;
		lensDirt = _runtimeProfile.bloom.settings.lensDirt;
		frost = _runtimeProfile.bloom.settings.frost;
		breathFog = _runtimeProfile.bloom.settings.breathFog;
		colorGradingEnabled = _runtimeProfile.colorGrading.enabled;
		colorGrading = _runtimeProfile.colorGrading.settings.basic;
		userLutEnabled = _runtimeProfile.userLut.enabled;
		userLut = _runtimeProfile.userLut.settings;
		chromaticAberrationEnabled = _runtimeProfile.chromaticAberration.enabled;
		chromaticAberration = _runtimeProfile.chromaticAberration.settings;
		vignetteEnabled = _runtimeProfile.vignette.enabled;
		vignette = _runtimeProfile.vignette.settings;
		ditheringAvailable = true;
		_ditheringEnabled = _runtimeProfile.dithering.enabled;
		customGammaAvailable = true;
		_customGammaEnabled = _runtimeProfile.customGamma.enabled;
		eyeMaskEnabled = _runtimeProfile.eyeMask.enabled;
		eyeMask = _runtimeProfile.eyeMask.settings;
		phosphenesEnabled = _runtimeProfile.phosphenes.enabled;
		phosphenes = _runtimeProfile.phosphenes.settings;
		volumeOcclusionAvailable = true;
		_volumeOcclusionEnabled = _runtimeProfile.volumeOcclusion.enabled;
		UpdateGraphicsSettings(PlayerData.GetGraphicSettings());
		GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", UpdateGraphicsSettings);
	}

	public void Destroy()
	{
		GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", UpdateGraphicsSettings);
	}

	public void ApplySettings()
	{
		_runtimeProfile.antialiasing.enabled = antialiasingAvailable && _antialiasingEnabled;
		_runtimeProfile.ambientOcclusion.enabled = ambientOcclusionAvailable && _ambientOcclusionEnabled;
		_runtimeProfile.screenSpaceReflection.enabled = screenSpaceReflectionAvailable && _screenSpaceReflectionEnabled;
		_runtimeProfile.bloom.enabled = bloomEnabled;
		BloomModel.Settings settings = _runtimeProfile.bloom.settings;
		settings.bloom = bloom;
		settings.lensDirt = lensDirt;
		settings.frost = frost;
		settings.breathFog = breathFog;
		_runtimeProfile.bloom.settings = settings;
		if (_runtimeProfile.colorGrading.enabled != colorGradingEnabled)
		{
			_runtimeProfile.colorGrading.enabled = colorGradingEnabled;
		}
		_runtimeProfile.colorGrading.SetBasicSettings(colorGrading);
		_runtimeProfile.userLut.enabled = userLutEnabled;
		_runtimeProfile.userLut.settings = userLut;
		_runtimeProfile.chromaticAberration.enabled = chromaticAberrationEnabled;
		_runtimeProfile.chromaticAberration.settings = chromaticAberration;
		_runtimeProfile.vignette.enabled = vignetteEnabled;
		_runtimeProfile.vignette.settings = vignette;
		_runtimeProfile.dithering.enabled = ditheringAvailable && _ditheringEnabled;
		_runtimeProfile.customGamma.enabled = customGammaAvailable && _customGammaEnabled;
		_runtimeProfile.eyeMask.enabled = eyeMaskEnabled;
		_runtimeProfile.eyeMask.settings = eyeMask;
		_runtimeProfile.phosphenes.enabled = phosphenesEnabled;
		_runtimeProfile.phosphenes.settings = phosphenes;
		_runtimeProfile.volumeOcclusion.enabled = volumeOcclusionAvailable && _volumeOcclusionEnabled;
	}

	public void CopySettingsFrom(PostProcessingGameplaySettings other)
	{
		antialiasingAvailable = other.antialiasingAvailable;
		ambientOcclusionAvailable = other.ambientOcclusionAvailable;
		screenSpaceReflectionAvailable = other.screenSpaceReflectionAvailable;
		bloomEnabled = other.bloomEnabled;
		bloom = other.bloom;
		lensDirt = other.lensDirt;
		frost = other.frost;
		breathFog = other.breathFog;
		colorGradingEnabled = other.colorGradingEnabled;
		colorGrading = other.colorGrading;
		userLutEnabled = other.userLutEnabled;
		userLut = other.userLut;
		chromaticAberrationEnabled = other.chromaticAberrationEnabled;
		chromaticAberration = other.chromaticAberration;
		vignetteEnabled = other.vignetteEnabled;
		vignette = other.vignette;
		ditheringAvailable = other.ditheringAvailable;
		customGammaAvailable = other.customGammaAvailable;
		eyeMaskEnabled = other.eyeMaskEnabled;
		eyeMask = other.eyeMask;
		phosphenesEnabled = other.phosphenesEnabled;
		phosphenes = other.phosphenes;
		volumeOcclusionAvailable = other.volumeOcclusionAvailable;
	}

	public void UpdateGraphicsSettings(GraphicSettings graphicsSettings)
	{
		if (_baseProfile.antialiasing.enabled)
		{
			AntialiasingModel.Settings settings = _baseProfile.antialiasing.settings;
			switch (graphicsSettings.antiAlias)
			{
			case AntiAliasType.FXAA:
				_antialiasingEnabled = true;
				settings.method = AntialiasingModel.Method.Fxaa;
				switch (graphicsSettings.antiAliasQuality)
				{
				case AntiAliasQuality.LOW:
					settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Performance;
					break;
				case AntiAliasQuality.MEDIUM:
					settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Default;
					break;
				case AntiAliasQuality.HIGH:
					settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Quality;
					break;
				case AntiAliasQuality.VERY_HIGH:
					settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.ExtremeQuality;
					break;
				default:
					settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Default;
					break;
				}
				break;
			case AntiAliasType.SMAA:
				_antialiasingEnabled = true;
				settings.method = AntialiasingModel.Method.Smaa;
				switch (graphicsSettings.antiAliasQuality)
				{
				case AntiAliasQuality.LOW:
					settings.smaaSettings.preset = AntialiasingModel.SmaaPreset.LowQuality;
					break;
				case AntiAliasQuality.MEDIUM:
					settings.smaaSettings.preset = AntialiasingModel.SmaaPreset.MediumQuality;
					break;
				case AntiAliasQuality.HIGH:
					settings.smaaSettings.preset = AntialiasingModel.SmaaPreset.HighQuality;
					break;
				case AntiAliasQuality.VERY_HIGH:
					settings.smaaSettings.preset = AntialiasingModel.SmaaPreset.UltraQuality;
					break;
				default:
					settings.smaaSettings.preset = AntialiasingModel.SmaaPreset.MediumQuality;
					break;
				}
				break;
			default:
				_antialiasingEnabled = false;
				break;
			}
			_runtimeProfile.antialiasing.enabled = antialiasingAvailable && _antialiasingEnabled;
			_runtimeProfile.antialiasing.settings = settings;
		}
		if (_baseProfile.ambientOcclusion.enabled)
		{
			_ambientOcclusionEnabled = graphicsSettings.ambientOcclusionQuality != SSAOQuality.OFF;
			AmbientOcclusionModel.Settings settings2 = _baseProfile.ambientOcclusion.settings;
			settings2.downsampling = graphicsSettings.ambientOcclusionQuality != SSAOQuality.VERY_HIGH;
			switch (graphicsSettings.ambientOcclusionQuality)
			{
			case SSAOQuality.LOW:
				settings2.sampleCount = AmbientOcclusionModel.SampleCount.Lowest;
				break;
			case SSAOQuality.MEDIUM:
				settings2.sampleCount = AmbientOcclusionModel.SampleCount.Medium;
				break;
			case SSAOQuality.HIGH:
				settings2.sampleCount = AmbientOcclusionModel.SampleCount.High;
				break;
			case SSAOQuality.VERY_HIGH:
				settings2.sampleCount = AmbientOcclusionModel.SampleCount.High;
				break;
			default:
				settings2.sampleCount = AmbientOcclusionModel.SampleCount.Medium;
				break;
			}
			_runtimeProfile.ambientOcclusion.enabled = ambientOcclusionAvailable && _ambientOcclusionEnabled;
			_runtimeProfile.ambientOcclusion.settings = settings2;
		}
		if (_baseProfile.screenSpaceReflection.enabled)
		{
			ScreenSpaceReflectionModel.Settings settings3 = _baseProfile.screenSpaceReflection.settings;
			switch (graphicsSettings.lightingQuality)
			{
			case GenericQuality.LOW:
				settings3.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.Low;
				settings3.reflection.smoothnessCutoff = 0.9f;
				settings3.reflection.reflectionBlur = 0.666f;
				break;
			case GenericQuality.MEDIUM:
				settings3.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.High;
				settings3.reflection.smoothnessCutoff = 0.9f;
				settings3.reflection.reflectionBlur = 1f;
				break;
			case GenericQuality.HIGH:
				settings3.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.High;
				settings3.reflection.smoothnessCutoff = 0f;
				settings3.reflection.reflectionBlur = 1f;
				break;
			default:
				settings3.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.High;
				settings3.reflection.smoothnessCutoff = 0.9f;
				settings3.reflection.reflectionBlur = 1f;
				break;
			}
			_runtimeProfile.screenSpaceReflection.enabled = screenSpaceReflectionAvailable && _screenSpaceReflectionEnabled;
			_runtimeProfile.screenSpaceReflection.settings = settings3;
		}
		if (_baseProfile.volumeOcclusion.enabled)
		{
			VolumeOcclusionModel.Settings settings4 = _baseProfile.volumeOcclusion.settings;
			settings4.downsampling = graphicsSettings.lightingQuality == GenericQuality.LOW;
			_runtimeProfile.volumeOcclusion.enabled = volumeOcclusionAvailable && _volumeOcclusionEnabled;
			_runtimeProfile.volumeOcclusion.settings = settings4;
		}
		if (_baseProfile.customGamma.enabled)
		{
			_customGammaEnabled = true;
			CustomGammaModel.Settings settings5 = _baseProfile.customGamma.settings;
			settings5.gamma = 2f / Mathf.Pow(2f, 0.4f + 2f * graphicsSettings.gammaValue * 0.6f);
			_runtimeProfile.customGamma.enabled = customGammaAvailable && _customGammaEnabled;
			_runtimeProfile.customGamma.settings = settings5;
		}
		if (_baseProfile.dithering.enabled)
		{
			_ditheringEnabled = graphicsSettings.dithering > 0f;
			DitheringModel.Settings settings6 = _baseProfile.dithering.settings;
			settings6.strength = Mathf.Clamp01(graphicsSettings.dithering);
			_runtimeProfile.dithering.enabled = ditheringAvailable && _ditheringEnabled;
			_runtimeProfile.dithering.settings = settings6;
		}
	}
}
