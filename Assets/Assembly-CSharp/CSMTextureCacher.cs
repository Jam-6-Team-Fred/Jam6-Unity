using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class CSMTextureCacher : MonoBehaviour
{
	private Light _light;

	private CommandBuffer _commandBuffer;

	private int _propID_RawCSMAvailable;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_commandBuffer = new CommandBuffer();
		_commandBuffer.name = "CSMTexCacher";
		_commandBuffer.SetGlobalTexture("_CascadedShadowMap", BuiltinRenderTextureType.CurrentActive);
		_commandBuffer.SetGlobalFloat("_RawCSMAvailable", 1f);
		_light.AddCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);
		_propID_RawCSMAvailable = Shader.PropertyToID("_RawCSMAvailable");
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnAnyCameraPostRender));
	}

	private void OnAnyCameraPostRender(Camera camera)
	{
		Shader.SetGlobalFloat(_propID_RawCSMAvailable, 0f);
	}

	private void OnDestroy()
	{
		if (_light != null && _commandBuffer != null)
		{
			_light.RemoveCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);
		}
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnAnyCameraPostRender));
		Shader.SetGlobalFloat(_propID_RawCSMAvailable, 0f);
	}
}
