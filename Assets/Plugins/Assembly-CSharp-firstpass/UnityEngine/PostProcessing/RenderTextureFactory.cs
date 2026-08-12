using System;
using System.Collections.Generic;

namespace UnityEngine.PostProcessing
{
	public sealed class RenderTextureFactory : IDisposable
	{
		private HashSet<RenderTexture> m_TemporaryRTs;

		public RenderTextureFactory()
		{
			m_TemporaryRTs = new HashSet<RenderTexture>();
		}

		public RenderTexture Get(RenderTexture baseRenderTexture)
		{
			return Get(baseRenderTexture.width, baseRenderTexture.height, baseRenderTexture.depth, baseRenderTexture.format, baseRenderTexture.useDynamicScale, baseRenderTexture.filterMode, baseRenderTexture.wrapMode);
		}

		public RenderTexture Get(int width, int height, int depthBuffer = 0, RenderTextureFormat format = RenderTextureFormat.ARGBHalf, bool useDynamicScale = false, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp, string name = "FactoryTempTexture")
		{
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, depthBuffer, format, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, VRTextureUsage.None, useDynamicScale);
			temporary.filterMode = filterMode;
			temporary.wrapMode = wrapMode;
			temporary.name = name;
			m_TemporaryRTs.Add(temporary);
			return temporary;
		}

		public void Release(RenderTexture rt)
		{
			if (!(rt == null))
			{
				if (!m_TemporaryRTs.Contains(rt))
				{
					throw new ArgumentException($"Attempting to remove a RenderTexture that was not allocated: {rt}");
				}
				m_TemporaryRTs.Remove(rt);
				RenderTexture.ReleaseTemporary(rt);
			}
		}

		public void ReleaseAll()
		{
			HashSet<RenderTexture>.Enumerator enumerator = m_TemporaryRTs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				RenderTexture.ReleaseTemporary(enumerator.Current);
			}
			m_TemporaryRTs.Clear();
		}

		public void Dispose()
		{
			ReleaseAll();
		}
	}
}
