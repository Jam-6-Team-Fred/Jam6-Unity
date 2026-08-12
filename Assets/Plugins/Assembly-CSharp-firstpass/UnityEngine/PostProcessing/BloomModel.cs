using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class BloomModel : PostProcessingModel
	{
		[Serializable]
		public struct BloomSettings
		{
			[Min(0f)]
			[Tooltip("Blend factor of the result image.")]
			public float intensity;

			[Min(0f)]
			[Tooltip("Filters out pixels under this level of brightness.")]
			public float threshold;

			[Range(0f, 1f)]
			[Tooltip("Makes transition between under/over-threshold gradual (0 = hard threshold, 1 = soft threshold).")]
			public float softKnee;

			[Range(1f, 7f)]
			[Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
			public float radius;

			[Tooltip("Reduces flashing noise with an additional filter.")]
			public bool antiFlicker;

			public float thresholdLinear
			{
				get
				{
					return Mathf.GammaToLinearSpace(threshold);
				}
				set
				{
					threshold = Mathf.LinearToGammaSpace(value);
				}
			}

			public static BloomSettings defaultSettings
			{
				get
				{
					BloomSettings result = default(BloomSettings);
					result.intensity = 0.5f;
					result.threshold = 1.1f;
					result.softKnee = 0.5f;
					result.radius = 4f;
					result.antiFlicker = false;
					return result;
				}
			}
		}

		[Serializable]
		public struct LensDirtSettings
		{
			[Tooltip("Texture that provides UVs (red/green), blend mask (blue), and fade (alpha) for the dirt and ice textures.")]
			public Texture uvTexture;

			[Tooltip("Dirtiness texture to add smudges or dust to the lens.")]
			public Texture texture;

			[Min(0f)]
			[Tooltip("Amount of lens dirtiness.")]
			public float intensity;

			public static LensDirtSettings defaultSettings
			{
				get
				{
					LensDirtSettings result = default(LensDirtSettings);
					result.uvTexture = null;
					result.texture = null;
					result.intensity = 3f;
					return result;
				}
			}
		}

		[Serializable]
		public struct FrostSettings
		{
			[Tooltip("Frost texture to add bloom behind the visor ice.")]
			public Texture frostTexture;

			[Min(0f)]
			[Tooltip("Strength of the frost effect.")]
			public float frostIntensity;

			[Range(0f, 1f)]
			[Tooltip("Frost ramp based on the uvTexture blend mask.")]
			public float frostRamp;

			[Range(0f, 1f)]
			[Tooltip("Width of the frost ramp.")]
			public float frostRampWidth;

			public static FrostSettings defaultSettings
			{
				get
				{
					FrostSettings result = default(FrostSettings);
					result.frostTexture = null;
					result.frostIntensity = 30f;
					result.frostRamp = 0f;
					result.frostRampWidth = 0.5f;
					return result;
				}
			}
		}

		[Serializable]
		public struct BreathFogSettings
		{
			[Tooltip("Breath fog ramp texture.")]
			public Texture breathFogTexture;

			[Tooltip("UV offset for the breath fog texture.")]
			public Vector4 breathFogTexture_ScaleOffset;

			[Min(0f)]
			[Tooltip("Strength of the breath fog.")]
			public float breathFogIntensity;

			[Range(0f, 1f)]
			[Tooltip("Ramps the breath fog in and out based on the texture ramp.")]
			public float breathFogRamp;

			[Min(0f)]
			[Tooltip("How much to fade the world color to white in the fogged area.")]
			public float breathFogFade;

			public static BreathFogSettings defaultSettings
			{
				get
				{
					BreathFogSettings result = default(BreathFogSettings);
					result.breathFogTexture = null;
					result.breathFogTexture_ScaleOffset = new Vector4(1f, 1f, 0f, 0f);
					result.breathFogIntensity = 3f;
					result.breathFogRamp = 0f;
					result.breathFogFade = 0.01f;
					return result;
				}
			}
		}

		[Serializable]
		public struct Settings
		{
			public BloomSettings bloom;

			public LensDirtSettings lensDirt;

			public FrostSettings frost;

			public BreathFogSettings breathFog;

			public static Settings defaultSettings
			{
				get
				{
					Settings result = default(Settings);
					result.bloom = BloomSettings.defaultSettings;
					result.lensDirt = LensDirtSettings.defaultSettings;
					result.frost = FrostSettings.defaultSettings;
					result.breathFog = BreathFogSettings.defaultSettings;
					return result;
				}
			}
		}

		[SerializeField]
		private Settings m_Settings = Settings.defaultSettings;

		public Settings settings
		{
			get
			{
				return m_Settings;
			}
			set
			{
				m_Settings = value;
			}
		}

		public override void Reset()
		{
			m_Settings = Settings.defaultSettings;
		}
	}
}
