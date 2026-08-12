using System;

namespace TerrainBaker
{
	[Serializable]
	public struct Settings
	{
		public bool exportNormals;

		public bool exportTangents;

		public bool exportColors;

		public ChannelData uv0Data;

		public ChannelData uv1Data;

		public ChannelData uv2Data;

		public ChannelData uv3Data;

		public UVProjector uv0Projector;

		public UVProjector uv1Projector;

		public UVProjector uv2Projector;

		public UVProjector uv3Projector;

		public static Settings defaultSettings
		{
			get
			{
				Settings result = default(Settings);
				result.exportNormals = true;
				result.exportTangents = true;
				result.exportColors = false;
				result.uv0Data = ChannelData.ExistingUV0;
				result.uv1Data = ChannelData.None;
				result.uv2Data = ChannelData.None;
				result.uv3Data = ChannelData.None;
				result.uv0Projector = null;
				result.uv1Projector = null;
				result.uv2Projector = null;
				result.uv3Projector = null;
				return result;
			}
		}

		public static Settings colliderSettings
		{
			get
			{
				Settings result = default(Settings);
				result.exportNormals = false;
				result.exportTangents = false;
				result.exportColors = false;
				result.uv0Data = ChannelData.None;
				result.uv1Data = ChannelData.None;
				result.uv2Data = ChannelData.None;
				result.uv3Data = ChannelData.None;
				result.uv0Projector = null;
				result.uv1Projector = null;
				result.uv2Projector = null;
				result.uv3Projector = null;
				return result;
			}
		}
	}
}
