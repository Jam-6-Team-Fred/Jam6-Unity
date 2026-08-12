using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class VolumeOcclusionModel : PostProcessingModel
	{
		[Serializable]
		public struct Settings
		{
			[Tooltip("Halves the resolution of the effect to increase performance.")]
			public bool downsampling;

			public static Settings defaultSettings
			{
				get
				{
					Settings result = default(Settings);
					result.downsampling = false;
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
