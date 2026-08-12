using UnityEngine;

[RequireComponent(typeof(Light))]
public class FadeLight : MonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_EmissionColor;

	private Light _light;

	[SerializeField]
	private Renderer _emissiveRenderer;

	[SerializeField]
	private int _materialIndex;

	private float _startLightIntensity;

	private float _targetLightIntensity;

	private Color _baseEmissionColor;

	private float _startEmissionIntensity;

	private float _targetEmissionIntensity;

	private float _startFadeTime;

	private float _fadeLength;

	private void Awake()
	{
		_light = GetComponent<Light>();
		if (_emissiveRenderer != null)
		{
			if (s_matPropBlock == null)
			{
				s_matPropBlock = new MaterialPropertyBlock();
				s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
			}
			_baseEmissionColor = _emissiveRenderer.sharedMaterials[_materialIndex].GetColor(s_propID_EmissionColor);
			_startEmissionIntensity = 1f;
		}
		base.enabled = false;
	}

	public void StartFade(float targetLightIntensity, float fadeLength, float targetEmissionIntensity = 0f)
	{
		_startLightIntensity = _light.intensity;
		_targetLightIntensity = targetLightIntensity;
		if (_emissiveRenderer != null)
		{
			_startEmissionIntensity = ((Color)s_matPropBlock.GetVector(s_propID_EmissionColor)).maxColorComponent / _baseEmissionColor.maxColorComponent;
			_targetEmissionIntensity = targetEmissionIntensity;
		}
		_startFadeTime = Time.time;
		_fadeLength = fadeLength;
		base.enabled = true;
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - _startFadeTime) / _fadeLength);
		_light.intensity = Mathf.Lerp(_startLightIntensity, _targetLightIntensity, num);
		if (_emissiveRenderer != null)
		{
			s_matPropBlock.SetColor(s_propID_EmissionColor, Mathf.Lerp(_startEmissionIntensity, _targetEmissionIntensity, num) * _baseEmissionColor);
			_emissiveRenderer.SetPropertyBlock(s_matPropBlock);
		}
		if (num >= 1f)
		{
			base.enabled = false;
		}
	}
}
