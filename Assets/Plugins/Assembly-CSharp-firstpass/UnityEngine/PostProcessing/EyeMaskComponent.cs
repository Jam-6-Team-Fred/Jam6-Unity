namespace UnityEngine.PostProcessing
{
	public class EyeMaskComponent : PostProcessingComponentRenderTexture<EyeMaskModel>
	{
		private static class Uniforms
		{
			internal static readonly int _EyeMask = Shader.PropertyToID("_EyeMask");

			internal static readonly int _EyeMask_Settings = Shader.PropertyToID("_EyeMask_Settings");

			internal static readonly int _EyeMask_EdgeColor = Shader.PropertyToID("_EyeMask_EdgeColor");

			internal static readonly int _EyeMask_LinesRamp = Shader.PropertyToID("_EyeMask_LinesRamp");

			internal static readonly int _EyeMask_LinesMask = Shader.PropertyToID("_EyeMask_LinesMask");

			internal static readonly int _EyeMask_LinesSettings = Shader.PropertyToID("_EyeMask_LinesSettings");
		}

		public override bool active
		{
			get
			{
				EyeMaskModel.Settings settings = base.model.settings;
				if (base.model.enabled && settings.eyeMask != null)
				{
					return !context.interrupted;
				}
				return false;
			}
		}

		public override void Prepare(Material uberMaterial)
		{
			EyeMaskModel.Settings settings = base.model.settings;
			uberMaterial.EnableKeyword("EYEMASK");
			uberMaterial.SetTexture(Uniforms._EyeMask, settings.eyeMask);
			uberMaterial.SetVector(Uniforms._EyeMask_Settings, new Vector4(settings.openness, settings.blendWidth, settings.edgeColorMode ? 1f : 0f));
			uberMaterial.SetColor(Uniforms._EyeMask_EdgeColor, settings.edgeColor);
			uberMaterial.SetTexture(Uniforms._EyeMask_LinesRamp, settings.linesRamp);
			uberMaterial.SetTexture(Uniforms._EyeMask_LinesMask, settings.linesMask);
			uberMaterial.SetVector(Uniforms._EyeMask_LinesSettings, new Vector4(settings.linesEnabled ? 1f : 0f, settings.linesProgress, settings.linesWidth));
		}
	}
}
