using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class ExtractScreenSpaceShadowsEffectTest : MonoBehaviour
{
	public RenderTexture rTex;

	public BuiltinRenderTextureType rType;

	private Material _mat;

	private void Start()
	{
		_mat = new Material(Shader.Find("Hidden/DrawShadowMapTest"));
	}

	private void Update()
	{
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(rTex, destination, _mat);
	}
}
