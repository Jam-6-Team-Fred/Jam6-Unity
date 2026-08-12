using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
public class ExtractShadowMapOnLightTest : MonoBehaviour
{
	private Light _light;

	private void Start()
	{
		_light = GetComponent<Light>();
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.name = "Get Shadow Mask";
		commandBuffer.SetGlobalTexture("_CopiedScreenSpaceShadows", BuiltinRenderTextureType.CurrentActive);
		_light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer);
	}

	private void Update()
	{
	}
}
