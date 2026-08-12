using System;

namespace UnityEngine.PostProcessing
{
	public sealed class DepthOfFieldComponent : PostProcessingComponentRenderTexture<DepthOfFieldModel>
	{
		private static class Uniforms
		{
			internal static readonly int _DepthOfFieldTex = Shader.PropertyToID("_DepthOfFieldTex");

			internal static readonly int _Distance = Shader.PropertyToID("_Distance");

			internal static readonly int _LensCoeff = Shader.PropertyToID("_LensCoeff");

			internal static readonly int _MaxCoC = Shader.PropertyToID("_MaxCoC");

			internal static readonly int _RcpMaxCoC = Shader.PropertyToID("_RcpMaxCoC");

			internal static readonly int _RcpAspect = Shader.PropertyToID("_RcpAspect");

			internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");

			internal static readonly int _HistoryCoC = Shader.PropertyToID("_HistoryCoC");

			internal static readonly int _HistoryWeight = Shader.PropertyToID("_HistoryWeight");

			internal static readonly int _DepthOfFieldParams = Shader.PropertyToID("_DepthOfFieldParams");
		}

		private const string k_ShaderString = "Hidden/Post FX/Depth Of Field";

		private RenderTexture m_CoCHistory;

		private RenderBuffer[] m_MRT = new RenderBuffer[2];

		private const float k_FilmHeight = 0.024f;

		public override bool active
		{
			get
			{
				if (base.model.enabled && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf))
				{
					return !context.interrupted;
				}
				return false;
			}
		}

		public override DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth;
		}

		private float CalculateFocalLength()
		{
			DepthOfFieldModel.Settings settings = base.model.settings;
			if (!settings.useCameraFov)
			{
				return settings.focalLength / 1000f;
			}
			float num = context.camera.fieldOfView * ((float)Math.PI / 180f);
			return 0.012f / Mathf.Tan(0.5f * num);
		}

		private float CalculateMaxCoCRadius(int screenHeight)
		{
			float num = (float)base.model.settings.kernelSize * 4f + 6f;
			return Mathf.Min(0.05f, num / (float)screenHeight);
		}

		public void Prepare(RenderTexture source, Material uberMaterial, bool antialiasCoC)
		{
			DepthOfFieldModel.Settings settings = base.model.settings;
			Material material = context.materialFactory.Get("Hidden/Post FX/Depth Of Field");
			material.shaderKeywords = null;
			float focusDistance = settings.focusDistance;
			float num = CalculateFocalLength();
			focusDistance = Mathf.Max(focusDistance, num);
			material.SetFloat(Uniforms._Distance, focusDistance);
			float num2 = num * num / (settings.aperture * (focusDistance - num) * 0.024f * 2f);
			material.SetFloat(Uniforms._LensCoeff, num2);
			float num3 = CalculateMaxCoCRadius(source.height);
			material.SetFloat(Uniforms._MaxCoC, num3);
			material.SetFloat(Uniforms._RcpMaxCoC, 1f / num3);
			float value = (float)source.height / (float)source.width;
			material.SetFloat(Uniforms._RcpAspect, value);
			RenderTexture renderTexture = context.renderTextureFactory.Get(context.width / 2, context.height / 2);
			source.filterMode = FilterMode.Point;
			if (!antialiasCoC)
			{
				Graphics.Blit(source, renderTexture, material, 0);
			}
			else
			{
				bool flag = m_CoCHistory == null || !m_CoCHistory.IsCreated() || m_CoCHistory.width != context.width / 2 || m_CoCHistory.height != context.height / 2;
				RenderTexture temporary = RenderTexture.GetTemporary(context.width / 2, context.height / 2, 0, RenderTextureFormat.RHalf);
				temporary.filterMode = FilterMode.Point;
				temporary.name = "CoC History";
				m_MRT[0] = renderTexture.colorBuffer;
				m_MRT[1] = temporary.colorBuffer;
				material.SetTexture(Uniforms._MainTex, source);
				material.SetTexture(Uniforms._HistoryCoC, m_CoCHistory);
				material.SetFloat(Uniforms._HistoryWeight, flag ? 0f : 0.5f);
				Graphics.SetRenderTarget(m_MRT, renderTexture.depthBuffer);
				GraphicsUtils.Blit(material, 1);
				RenderTexture.ReleaseTemporary(m_CoCHistory);
				m_CoCHistory = temporary;
			}
			RenderTexture renderTexture2 = context.renderTextureFactory.Get(context.width / 2, context.height / 2);
			Graphics.Blit(renderTexture, renderTexture2, material, (int)(2 + settings.kernelSize));
			Graphics.Blit(renderTexture2, renderTexture, material, 6);
			if (context.profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.FocusPlane))
			{
				uberMaterial.SetVector(Uniforms._DepthOfFieldParams, new Vector2(focusDistance, num2));
				uberMaterial.EnableKeyword("DEPTH_OF_FIELD_COC_VIEW");
				context.Interrupt();
			}
			else
			{
				uberMaterial.SetTexture(Uniforms._DepthOfFieldTex, renderTexture);
				uberMaterial.EnableKeyword("DEPTH_OF_FIELD");
			}
			context.renderTextureFactory.Release(renderTexture2);
			source.filterMode = FilterMode.Bilinear;
		}

		public override void OnDisable()
		{
			if (m_CoCHistory != null)
			{
				RenderTexture.ReleaseTemporary(m_CoCHistory);
			}
			m_CoCHistory = null;
		}
	}
}
