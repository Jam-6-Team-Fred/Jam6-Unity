using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class PhospheneModel : PostProcessingModel
	{
		[Serializable]
		public struct Settings
		{
			[Tooltip("Phosphene texture.")]
			public Texture2D phospheneTex;

			[Tooltip("Phosphene texture scale and offset.")]
			public Vector4 phospheneTex_ScaleOffset;

			[Range(0f, 1f)]
			[Tooltip("Visibility of the effect.")]
			public float visibility;

			[Range(0f, 1f)]
			[Tooltip("Brightness of the effect.")]
			public float brightness;

			public static Settings defaultSettings
			{
				get
				{
					Settings result = default(Settings);
					result.phospheneTex = null;
					result.phospheneTex_ScaleOffset = new Vector4(1f, 1f, 0f, 0f);
					result.visibility = 0f;
					result.brightness = 1f;
					return result;
				}
			}

			public void RandomizeTextureOffset()
			{
				phospheneTex_ScaleOffset.z = Random.value;
				phospheneTex_ScaleOffset.w = Random.value;
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
