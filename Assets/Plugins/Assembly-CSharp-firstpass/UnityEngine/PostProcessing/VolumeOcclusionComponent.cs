using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing
{
	public class VolumeOcclusionComponent : PostProcessingComponentCommandBuffer<VolumeOcclusionModel>
	{
		private static class Uniforms
		{
			internal static readonly int _VolumeOcclusionTexture = Shader.PropertyToID("_VolumeOcclusionTexture");

			internal static readonly int _OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");

			internal static readonly int _OcclusionLightDirRange = Shader.PropertyToID("_OcclusionLightDirRange");

			internal static readonly int _OcclusionLightIntensity = Shader.PropertyToID("_OcclusionLightIntensity");

			internal static readonly int _WorldToOcclusionLight = Shader.PropertyToID("_WorldToOcclusionLight");

			internal static readonly int _OcclusionLightParams = Shader.PropertyToID("_OcclusionLightParams");

			internal static readonly int _OcclusionLightTex = Shader.PropertyToID("_OcclusionLightTex");

			internal static readonly int _OcclusionLightTexMipCount = Shader.PropertyToID("_OcclusionLightTexMipCount");

			internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");
		}

		private const string k_ClearBlitDepthShaderString = "Hidden/Post FX/ClearBlitDepth";

		private const string k_VolumeOcclusionShaderString = "Hidden/Post FX/Volume Occlusion";

		private const string k_LightShaderString = "Hidden/Post FX/Light";

		private const string k_CompositeShaderString = "Hidden/Post FX/CompositeAlpha";

		private static readonly bool k_SupportsRHalf = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf);

		private static readonly Plane[] s_frustumPlanes = new Plane[6];

		public override bool active
		{
			get
			{
				if (base.model.enabled && context.isGBufferAvailable && !context.interrupted)
				{
					return VolumeOcclusionManager.HasActiveOcclusionVolumes();
				}
				return false;
			}
		}

		public override string GetName()
		{
			return "Volume Occlusion";
		}

		public override CameraEvent GetCameraEvent()
		{
			return CameraEvent.BeforeLighting;
		}

		public override void PopulateCommandBuffer(CommandBuffer cb)
		{
			VolumeOcclusionModel.Settings settings = base.model.settings;
			GeometryUtility.CalculateFrustumPlanes(context.camera, s_frustumPlanes);
			List<VolumeOcclusionRenderer> culledOcclusionVolumeList = VolumeOcclusionManager.GetCulledOcclusionVolumeList(s_frustumPlanes);
			if (culledOcclusionVolumeList.Count == 0)
			{
				return;
			}
			List<VolumeOcclusionLight> culledLightList = VolumeOcclusionManager.GetCulledLightList(s_frustumPlanes);
			Material mat = context.materialFactory.Get("Hidden/Post FX/ClearBlitDepth");
			Material material = context.materialFactory.Get("Hidden/Post FX/Volume Occlusion");
			Material material2 = context.materialFactory.Get("Hidden/Post FX/Light");
			Material material3 = context.materialFactory.Get("Hidden/Post FX/CompositeAlpha");
			int num = ((!settings.downsampling) ? 1 : 2);
			RenderTextureFormat format = (k_SupportsRHalf ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32);
			cb.GetTemporaryRT(Uniforms._VolumeOcclusionTexture, context.width / num, context.height / num, 24, FilterMode.Bilinear, format, RenderTextureReadWrite.Linear, 1, enableRandomWrite: false, RenderTextureMemoryless.None, context.useDynamicScale);
			cb.Blit(null, Uniforms._VolumeOcclusionTexture, mat);
			cb.SetRenderTarget(Uniforms._VolumeOcclusionTexture);
			for (int i = 0; i < culledOcclusionVolumeList.Count; i++)
			{
				VolumeOcclusionRenderer volumeOcclusionRenderer = culledOcclusionVolumeList[i];
				cb.SetGlobalFloat(Uniforms._OcclusionStrength, volumeOcclusionRenderer.occlusionStrength);
				cb.DrawMesh(volumeOcclusionRenderer.mesh, volumeOcclusionRenderer.localToWorldMatrix, material);
			}
			Vector4 value3 = default(Vector4);
			for (int j = 0; j < culledLightList.Count; j++)
			{
				VolumeOcclusionLight volumeOcclusionLight = culledLightList[j];
				Vector3 lightDirection = volumeOcclusionLight.lightDirection;
				float range = volumeOcclusionLight.range;
				Matrix4x4 localToWorldMatrix = volumeOcclusionLight.localToWorldMatrix;
				Matrix4x4 inverse = localToWorldMatrix.inverse;
				Vector2 startSize = volumeOcclusionLight.startSize;
				Vector2 endSize = volumeOcclusionLight.endSize;
				Texture2D texture2D = volumeOcclusionLight.cookie;
				float value = 0f;
				if (volumeOcclusionLight.distanceBlur && texture2D != null)
				{
					value = Mathf.Max(texture2D.mipmapCount - 2, 0);
				}
				if (texture2D == null)
				{
					texture2D = Texture2D.whiteTexture;
				}
				Vector4 value2 = new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 1f / range);
				value3.x = 1f / (startSize.x * 0.5f);
				value3.y = 1f / (startSize.y * 0.5f);
				value3.z = endSize.x / startSize.x;
				value3.w = endSize.y / startSize.y;
				cb.SetGlobalVector(Uniforms._OcclusionLightDirRange, value2);
				cb.SetGlobalFloat(Uniforms._OcclusionLightIntensity, volumeOcclusionLight.intensity);
				cb.SetGlobalMatrix(Uniforms._WorldToOcclusionLight, inverse);
				cb.SetGlobalVector(Uniforms._OcclusionLightParams, value3);
				cb.SetGlobalTexture(Uniforms._OcclusionLightTex, texture2D);
				cb.SetGlobalFloat(Uniforms._OcclusionLightTexMipCount, value);
				cb.DrawMesh(volumeOcclusionLight.mesh, localToWorldMatrix, material2);
			}
			cb.SetGlobalTexture(Uniforms._MainTex, Uniforms._VolumeOcclusionTexture);
			cb.SetRenderTarget(BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.CameraTarget);
			cb.DrawMesh(GraphicsUtils.quad, Matrix4x4.identity, material3);
			cb.ReleaseTemporaryRT(Uniforms._VolumeOcclusionTexture);
		}
	}
}
