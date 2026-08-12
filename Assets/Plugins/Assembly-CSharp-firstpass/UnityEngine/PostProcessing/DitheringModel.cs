using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class DitheringModel : PostProcessingModel
	{
		[Serializable]
		public struct Settings
		{
			[Range(0f, 1f)]
			[Tooltip("Dithering strength.")]
			public float strength;

			public static Settings defaultSettings
			{
				get
				{
					Settings result = default(Settings);
					result.strength = 1f;
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
