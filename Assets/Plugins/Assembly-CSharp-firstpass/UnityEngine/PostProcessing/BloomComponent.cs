namespace UnityEngine.PostProcessing
{
	public sealed class BloomComponent : PostProcessingComponentRenderTexture<BloomModel>
	{
		private static class Uniforms
		{
			internal static readonly int _AutoExposure = Shader.PropertyToID("_AutoExposure");

			internal static readonly int _Threshold = Shader.PropertyToID("_Threshold");

			internal static readonly int _Curve = Shader.PropertyToID("_Curve");

			internal static readonly int _PrefilterOffs = Shader.PropertyToID("_PrefilterOffs");

			internal static readonly int _SampleScale = Shader.PropertyToID("_SampleScale");

			internal static readonly int _BaseTex = Shader.PropertyToID("_BaseTex");

			internal static readonly int _BloomTex = Shader.PropertyToID("_BloomTex");

			internal static readonly int _Bloom_Settings = Shader.PropertyToID("_Bloom_Settings");

			internal static readonly int _Bloom_VisorUVTex = Shader.PropertyToID("_Bloom_VisorUVTex");

			internal static readonly int _Bloom_DirtTex = Shader.PropertyToID("_Bloom_DirtTex");

			internal static readonly int _Bloom_DirtIntensity = Shader.PropertyToID("_Bloom_DirtIntensity");

			internal static readonly int _Bloom_FrostTex = Shader.PropertyToID("_Bloom_FrostTex");

			internal static readonly int _Bloom_FrostSettings = Shader.PropertyToID("_Bloom_FrostSettings");

			internal static readonly int _Bloom_BreathFogTex = Shader.PropertyToID("_Bloom_BreathFogTex");

			internal static readonly int _Bloom_BreathFogTex_ST = Shader.PropertyToID("_Bloom_BreathFogTex_STc");

			internal static readonly int _Bloom_BreathFogSettings = Shader.PropertyToID("_Bloom_BreathFogSettings");
		}

		private const int k_MaxPyramidBlurLevel = 16;

		private readonly RenderTexture[] m_BlurBuffer1 = new RenderTexture[16];

		private readonly RenderTexture[] m_BlurBuffer2 = new RenderTexture[16];

		public override bool active
		{
			get
			{
				if (base.model.enabled && base.model.settings.bloom.intensity > 0f)
				{
					return !context.interrupted;
				}
				return false;
			}
		}

		public void Prepare(RenderTexture source, Material uberMaterial, Texture autoExposure)
		{
			BloomModel.BloomSettings bloom = base.model.settings.bloom;
			BloomModel.LensDirtSettings lensDirt = base.model.settings.lensDirt;
			BloomModel.FrostSettings frost = base.model.settings.frost;
			BloomModel.BreathFogSettings breathFog = base.model.settings.breathFog;
			Material material = context.materialFactory.Get("Hidden/Post FX/Bloom");
			material.shaderKeywords = null;
			material.SetTexture(Uniforms._AutoExposure, autoExposure);
			int width = context.width / 2;
			int num = context.height / 2;
			RenderTextureFormat format = (Application.isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
			float num2 = Mathf.Log(num, 2f) + bloom.radius - 8f;
			int num3 = (int)num2;
			int num4 = Mathf.Clamp(num3, 1, 16);
			float thresholdLinear = bloom.thresholdLinear;
			material.SetFloat(Uniforms._Threshold, thresholdLinear);
			float num5 = thresholdLinear * bloom.softKnee + 1E-05f;
			material.SetVector(value: new Vector3(thresholdLinear - num5, num5 * 2f, 0.25f / num5), nameID: Uniforms._Curve);
			material.SetFloat(Uniforms._PrefilterOffs, bloom.antiFlicker ? (-0.5f) : 0f);
			float num6 = 0.5f + num2 - (float)num3;
			material.SetFloat(Uniforms._SampleScale, num6);
			if (bloom.antiFlicker)
			{
				material.EnableKeyword("ANTI_FLICKER");
			}
			RenderTexture renderTexture = context.renderTextureFactory.Get(width, num, 0, format, context.useDynamicScale);
			Graphics.Blit(source, renderTexture, material, 0);
			RenderTexture renderTexture2 = renderTexture;
			for (int i = 0; i < num4; i++)
			{
				m_BlurBuffer1[i] = context.renderTextureFactory.Get(renderTexture2.width / 2, renderTexture2.height / 2, 0, format, context.useDynamicScale);
				int pass = ((i == 0) ? 1 : 2);
				Graphics.Blit(renderTexture2, m_BlurBuffer1[i], material, pass);
				renderTexture2 = m_BlurBuffer1[i];
			}
			for (int num7 = num4 - 2; num7 >= 0; num7--)
			{
				RenderTexture renderTexture3 = m_BlurBuffer1[num7];
				material.SetTexture(Uniforms._BaseTex, renderTexture3);
				m_BlurBuffer2[num7] = context.renderTextureFactory.Get(renderTexture3.width, renderTexture3.height, 0, format, context.useDynamicScale);
				Graphics.Blit(renderTexture2, m_BlurBuffer2[num7], material, 3);
				renderTexture2 = m_BlurBuffer2[num7];
			}
			RenderTexture renderTexture4 = renderTexture2;
			for (int j = 0; j < 16; j++)
			{
				if (m_BlurBuffer1[j] != null)
				{
					context.renderTextureFactory.Release(m_BlurBuffer1[j]);
				}
				if (m_BlurBuffer2[j] != null && m_BlurBuffer2[j] != renderTexture4)
				{
					context.renderTextureFactory.Release(m_BlurBuffer2[j]);
				}
				m_BlurBuffer1[j] = null;
				m_BlurBuffer2[j] = null;
			}
			context.renderTextureFactory.Release(renderTexture);
			uberMaterial.SetTexture(Uniforms._BloomTex, renderTexture4);
			uberMaterial.SetVector(Uniforms._Bloom_Settings, new Vector2(num6, bloom.intensity));
			if ((lensDirt.intensity > 0f && lensDirt.texture != null) || (frost.frostRamp > 0f && frost.frostTexture != null) || (breathFog.breathFogRamp > 0f && breathFog.breathFogTexture != null))
			{
				uberMaterial.SetTexture(Uniforms._Bloom_VisorUVTex, lensDirt.uvTexture);
				uberMaterial.SetTexture(Uniforms._Bloom_DirtTex, lensDirt.texture);
				uberMaterial.SetFloat(Uniforms._Bloom_DirtIntensity, lensDirt.intensity);
				uberMaterial.SetTexture(Uniforms._Bloom_FrostTex, frost.frostTexture);
				uberMaterial.SetVector(Uniforms._Bloom_FrostSettings, new Vector4(frost.frostIntensity, frost.frostRamp, frost.frostRampWidth, 0f));
				uberMaterial.SetTexture(Uniforms._Bloom_BreathFogTex, breathFog.breathFogTexture);
				uberMaterial.SetVector(Uniforms._Bloom_BreathFogTex_ST, breathFog.breathFogTexture_ScaleOffset);
				uberMaterial.SetVector(Uniforms._Bloom_BreathFogSettings, new Vector4(breathFog.breathFogIntensity, breathFog.breathFogRamp, breathFog.breathFogFade, 0f));
				uberMaterial.EnableKeyword("BLOOM_LENS_DIRT");
			}
			else
			{
				uberMaterial.EnableKeyword("BLOOM");
			}
		}
	}
}
