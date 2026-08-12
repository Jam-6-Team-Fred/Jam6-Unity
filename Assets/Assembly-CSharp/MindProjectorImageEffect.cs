using UnityEngine;

public class MindProjectorImageEffect : MonoBehaviour
{
	private readonly int _shaderPropID_Openness = Shader.PropertyToID("_EyeOpenness");

	private readonly int _shaderPropID_SlideFade = Shader.PropertyToID("_SlideFade");

	private readonly int _shaderPropID_SlideTex = Shader.PropertyToID("_SlideTex");

	private readonly int _shaderPropID_UnscaledTime = Shader.PropertyToID("_UnscaledTime");

	[SerializeField]
	private Material _material;

	private Material _localMaterial;

	public float eyeOpenness
	{
		set
		{
			_localMaterial.SetFloat(_shaderPropID_Openness, value);
		}
	}

	public float slideFade
	{
		set
		{
			_localMaterial.SetFloat(_shaderPropID_SlideFade, value);
		}
	}

	public Texture slideTexture
	{
		set
		{
			_localMaterial.SetTexture(_shaderPropID_SlideTex, value);
		}
	}

	private void Awake()
	{
		_localMaterial = new Material(_material);
		_localMaterial.name += "_Instance";
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_localMaterial.SetFloat(_shaderPropID_UnscaledTime, Time.unscaledTime);
		Graphics.Blit(source, destination, _localMaterial);
	}
}
