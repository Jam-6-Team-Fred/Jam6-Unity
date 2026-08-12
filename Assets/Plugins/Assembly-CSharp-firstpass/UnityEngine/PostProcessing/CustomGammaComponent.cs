namespace UnityEngine.PostProcessing
{
	public class CustomGammaComponent : PostProcessingComponentRenderTexture<CustomGammaModel>
	{
		private static class Uniforms
		{
			internal static readonly int _CustomGamma = Shader.PropertyToID("_CustomGamma");
		}

		public override bool active
		{
			get
			{
				if (base.model.enabled)
				{
					return !context.interrupted;
				}
				return false;
			}
		}

		public override void Prepare(Material uberMaterial)
		{
			CustomGammaModel.Settings settings = base.model.settings;
			uberMaterial.EnableKeyword("CUSTOM_GAMMA");
			uberMaterial.SetFloat(Uniforms._CustomGamma, settings.gamma);
		}
	}
}
