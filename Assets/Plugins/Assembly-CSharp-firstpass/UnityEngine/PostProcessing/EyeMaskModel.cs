using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class EyeMaskModel : PostProcessingModel
	{
		[Serializable]
		public struct Settings
		{
			[Tooltip("Eye mask texture.")]
			public Texture2D eyeMask;

			[Range(0f, 1f)]
			[Tooltip("Openness of the eyes.")]
			public float openness;

			[Range(0.01f, 1f)]
			[Tooltip("How far along the alpha ramp to blend out.")]
			public float blendWidth;

			[Tooltip("Whether to use the eye mask color or to fade to a color along the edge.")]
			public bool edgeColorMode;

			[ColorUsage(false, true)]
			[Tooltip("The color of the edge color band.")]
			public Color edgeColor;

			[Tooltip("Whether to overlay the Nomai memory recall lines.")]
			public bool linesEnabled;

			[Tooltip("Nomai recall lines ramp.")]
			public Texture2D linesRamp;

			[Tooltip("Nomai recall lines mask.")]
			public Texture2D linesMask;

			[Range(0f, 1f)]
			[Tooltip("How far along the lines ramp to blend out.")]
			public float linesProgress;

			[Range(0.01f, 1f)]
			[Tooltip("The width of the line trails in the lines ramp.")]
			public float linesWidth;

			public static Settings defaultSettings
			{
				get
				{
					Settings result = default(Settings);
					result.eyeMask = null;
					result.openness = 1f;
					result.blendWidth = 0.1f;
					result.edgeColorMode = false;
					result.edgeColor = Color.white;
					result.linesEnabled = false;
					result.linesRamp = null;
					result.linesMask = null;
					result.linesProgress = 0f;
					result.linesWidth = 0.1f;
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
