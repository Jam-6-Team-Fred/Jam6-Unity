namespace UnityEngine.PostProcessing
{
	public sealed class SmaaComponent : PostProcessingComponentRenderTexture<AntialiasingModel>
	{
		private static class Uniforms
		{
			public static int _MainTex = Shader.PropertyToID("_MainTex");

			public static int _MainTex_ScaledTexelSize = Shader.PropertyToID("_MainTex_ScaledTexelSize");

			public static int _AreaTex = Shader.PropertyToID("_AreaTex");

			public static int _SearchTex = Shader.PropertyToID("_SearchTex");

			public static int _BlendTex = Shader.PropertyToID("_BlendTex");
		}

		private enum Pass
		{
			EdgeDetection = 0,
			BlendWeights = 4,
			NeighborhoodBlending = 8
		}

		private const string k_ShaderString = "Hidden/Post FX/SubpixelMorphologicalAntialiasing";

		private static readonly bool k_SupportsRG16 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RG16);

		private bool lutsLoaded;

		private Texture2D lutArea;

		private Texture2D lutSearch;

		public override bool active
		{
			get
			{
				if (base.model.enabled && base.model.settings.method == AntialiasingModel.Method.Smaa)
				{
					return !context.interrupted;
				}
				return false;
			}
		}

		public override void OnDisable()
		{
			lutsLoaded = false;
			lutArea = null;
			lutSearch = null;
		}

		private void LoadLUTs()
		{
			lutArea = Resources.Load<Texture2D>("SMAA/AreaTex");
			lutSearch = Resources.Load<Texture2D>("SMAA/SearchTex");
			lutsLoaded = true;
		}

		private void BlitFullscreenTriangle(Texture source, RenderTexture destination, Material material, int pass, bool clearColor = false, bool clearDepth = false)
		{
			if (clearColor || clearDepth)
			{
				GL.Clear(clearDepth, clearColor, Color.clear);
			}
			GL.PushMatrix();
			GL.LoadOrtho();
			material.SetTexture(Uniforms._MainTex, source);
			material.SetPass(pass);
			GL.Begin(4);
			GL.Vertex3(-1f, -1f, 0f);
			GL.Vertex3(-1f, 3f, 0f);
			GL.Vertex3(3f, -1f, 0f);
			GL.End();
			GL.PopMatrix();
		}

		public void Render(RenderTexture source, RenderTexture destination)
		{
			AntialiasingModel.SmaaSettings smaaSettings = base.model.settings.smaaSettings;
			Material material = context.materialFactory.Get("Hidden/Post FX/SubpixelMorphologicalAntialiasing");
			int num = 1;
			switch (smaaSettings.preset)
			{
			case AntialiasingModel.SmaaPreset.LowQuality:
				num = 0;
				break;
			case AntialiasingModel.SmaaPreset.MediumQuality:
				num = 1;
				break;
			case AntialiasingModel.SmaaPreset.HighQuality:
				num = 2;
				break;
			case AntialiasingModel.SmaaPreset.UltraQuality:
				num = 3;
				break;
			}
			if (!lutsLoaded)
			{
				LoadLUTs();
			}
			material.SetTexture(Uniforms._AreaTex, lutArea);
			material.SetTexture(Uniforms._SearchTex, lutSearch);
			if (context.useDynamicScale)
			{
				material.EnableKeyword("OW_DYNAMIC_SCALE");
				material.SetVector(Uniforms._MainTex_ScaledTexelSize, new Vector4(1f / (float)context.scaledWidth, 1f / (float)context.scaledHeight, context.scaledWidth, context.scaledHeight));
			}
			else
			{
				material.DisableKeyword("OW_DYNAMIC_SCALE");
			}
			RenderTextureFormat format = (k_SupportsRG16 ? RenderTextureFormat.RG16 : RenderTextureFormat.ARGB32);
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 24, format, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, VRTextureUsage.None, source.useDynamicScale);
			RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, VRTextureUsage.None, source.useDynamicScale);
			Graphics.SetRenderTarget(temporary);
			BlitFullscreenTriangle(source, temporary, material, num, clearColor: true, clearDepth: true);
			Graphics.SetRenderTarget(temporary2.colorBuffer, temporary.depthBuffer);
			BlitFullscreenTriangle(temporary, temporary2, material, 4 + num, clearColor: true);
			material.SetTexture(Uniforms._BlendTex, temporary2);
			Graphics.SetRenderTarget(destination);
			BlitFullscreenTriangle(source, destination, material, 8);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
		}
	}
}
