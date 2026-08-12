using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class StencilPreviewImageEffect : MonoBehaviour
{
	private enum Flag
	{
		Underwater = 1,
		ShipInterior = 2
	}

	private enum ComparisonFunction
	{
		Greater = 0,
		GreaterOrEqual = 1,
		Less = 2,
		LessOrEqual = 3,
		Equal = 4,
		NotEqual = 5,
		Always = 6,
		Never = 7
	}

	private Camera _camera;

	private Shader _shader;

	private Material _material;

	[SerializeField]
	private Flag _flag;

	private void Init()
	{
		_camera = GetComponent<Camera>();
		_shader = Shader.Find("Hidden/OW_ImageEffectStencilPreview");
		_material = new Material(_shader);
		_material.hideFlags = HideFlags.HideAndDontSave;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_camera == null)
		{
			Init();
		}
		if (_material != null)
		{
			_material.SetInt("_StencilFlag", (int)_flag);
			Graphics.Blit(source, destination, _material);
		}
	}
}
