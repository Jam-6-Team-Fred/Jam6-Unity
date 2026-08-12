namespace UnityEngine.PostProcessing
{
	public class PhospheneComponent : PostProcessingComponentRenderTexture<PhospheneModel>
	{
		private static class Uniforms
		{
			internal static readonly int _PhospheneTex = Shader.PropertyToID("_PhospheneTex");

			internal static readonly int _PhospheneTex_ST = Shader.PropertyToID("_PhospheneTex_ST");

			internal static readonly int _Phosphene_Settings = Shader.PropertyToID("_Phosphene_Settings");
		}

		public override bool active
		{
			get
			{
				PhospheneModel.Settings settings = base.model.settings;
				if (base.model.enabled && settings.phospheneTex != null)
				{
					return !context.interrupted;
				}
				return false;
			}
		}

		public override void Prepare(Material uberMaterial)
		{
			PhospheneModel.Settings settings = base.model.settings;
			uberMaterial.EnableKeyword("PHOSPHENES");
			uberMaterial.SetTexture(Uniforms._PhospheneTex, settings.phospheneTex);
			uberMaterial.SetVector(Uniforms._PhospheneTex_ST, settings.phospheneTex_ScaleOffset);
			uberMaterial.SetVector(Uniforms._Phosphene_Settings, new Vector4(settings.visibility, settings.brightness));
		}
	}
}
