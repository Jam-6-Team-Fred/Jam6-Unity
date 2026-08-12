using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class CustomGammaModel : PostProcessingModel
	{
		[Serializable]
		public struct Settings
		{
			[Range(0.25f, 4f)]
			[Tooltip("Gamma correction exponent.")]
			public float gamma;

			public static Settings defaultSettings
			{
				get
				{
					Settings result = default(Settings);
					result.gamma = 1f;
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
