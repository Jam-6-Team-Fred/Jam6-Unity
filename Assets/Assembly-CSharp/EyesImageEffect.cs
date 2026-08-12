using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class EyesImageEffect : MonoBehaviour
{
	private Camera _camera;

	[SerializeField]
	private Shader _eyesShader;

	[SerializeField]
	private Texture2D _eyesTexture;

	private Material _eyesMaterial;

	[SerializeField]
	[Range(0f, 1f)]
	private float _openness = 1f;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float _blendWidth = 0.1f;

	public float openness
	{
		get
		{
			return _openness;
		}
		set
		{
			_openness = Mathf.Clamp01(value);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_camera == null)
		{
			_camera = GetComponent<Camera>();
		}
		if (_eyesMaterial == null && _eyesShader != null)
		{
			_eyesMaterial = new Material(_eyesShader);
			_eyesMaterial.SetTexture("_EyeTex", _eyesTexture);
		}
		if (_eyesMaterial != null)
		{
			_eyesMaterial.SetFloat("_EyeOpenness", _openness);
			_eyesMaterial.SetFloat("_BlendWidth", _blendWidth);
			Graphics.Blit(source, destination, _eyesMaterial);
		}
	}
}
