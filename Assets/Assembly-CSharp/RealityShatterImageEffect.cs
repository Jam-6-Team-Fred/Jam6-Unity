using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class RealityShatterImageEffect : MonoBehaviour
{
	[SerializeField]
	private Shader _realityShatterShader;

	[SerializeField]
	private Texture2D _realityShatterTexture;

	private Material _material;

	private Vector4 _shatterParams = Vector4.zero;

	private void Awake()
	{
		base.enabled = false;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_material == null && _realityShatterShader != null)
		{
			_material = new Material(_realityShatterShader);
			_material.SetTexture("_ShatterTex", _realityShatterTexture);
		}
		if (_material != null)
		{
			_material.SetVector("_Progress", _shatterParams);
			Graphics.Blit(source, destination, _material);
		}
	}

	public void SetShatterParameters(float shatterProgress, float offset, float dissolveWidth, float dissolveProgress)
	{
		_shatterParams.Set(Mathf.Clamp01(shatterProgress), offset, Mathf.Clamp01(dissolveWidth), Mathf.Clamp01(dissolveProgress));
	}
}
