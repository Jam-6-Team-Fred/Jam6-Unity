using UnityEngine;
using UnityEngine.Rendering;

public class TitleAnimRenderer : MonoBehaviour
{
	[SerializeField]
	private Material _logoMaterial;

	[SerializeField]
	private CanvasGroup _logoGroup;

	private Material _logoMaterialClone;

	private Color _logoColor;

	private void Awake()
	{
		_logoMaterialClone = new Material(_logoMaterial);
		_logoMaterialClone.name += "_LocalCopy";
		_logoColor = _logoMaterialClone.color;
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.name = "AnimatedTitleUI";
		Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			commandBuffer.DrawRenderer(componentsInChildren[i], _logoMaterialClone);
		}
	}

	private void LateUpdate()
	{
		_logoColor.a = _logoGroup.alpha;
		_logoMaterialClone.color = _logoColor;
	}
}
