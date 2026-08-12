using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class NoiseImageEffect : MonoBehaviour
{
	private Camera _camera;

	[SerializeField]
	private Shader _noiseShader;

	private Material _noiseMaterial;

	[SerializeField]
	private Vector2 _resolution = new Vector2(512f, 512f);

	[SerializeField]
	[Range(0f, 1f)]
	private float _strength = 0.1f;

	public Vector2 resolution
	{
		get
		{
			return _resolution;
		}
		set
		{
			_resolution = value;
		}
	}

	public float strength
	{
		get
		{
			return _strength;
		}
		set
		{
			_strength = Mathf.Clamp01(value);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_camera == null)
		{
			_camera = GetComponent<Camera>();
		}
		if (_noiseMaterial == null && _noiseShader != null)
		{
			_noiseMaterial = new Material(_noiseShader);
		}
		if (_noiseMaterial != null)
		{
			_noiseMaterial.SetVector("_NoiseRes", new Vector4(_resolution.x, _resolution.y, 0f, 0f));
			_noiseMaterial.SetFloat("_NoiseStrength", _strength);
			Graphics.Blit(source, destination, _noiseMaterial);
		}
	}
}
