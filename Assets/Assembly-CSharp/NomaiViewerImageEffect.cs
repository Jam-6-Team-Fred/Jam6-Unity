using UnityEngine;

public class NomaiViewerImageEffect : MonoBehaviour
{
	public Material _material;

	private Material _localMaterial;

	private int _propID_Fade;

	private void Awake()
	{
		_localMaterial = new Material(_material);
		_localMaterial.name += "_Instance";
		_propID_Fade = Shader.PropertyToID("_Fade");
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, _localMaterial);
	}

	public void SetFade(float fade)
	{
		_localMaterial.SetFloat(_propID_Fade, Mathf.Clamp01(fade));
	}
}
