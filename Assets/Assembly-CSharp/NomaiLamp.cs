using UnityEngine;

public class NomaiLamp : MonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_EmissionColor;

	private Color[] _emission;

	[SerializeField]
	private bool _startOn = true;

	[SerializeField]
	private Renderer[] _emissiveRenderers;

	[SerializeField]
	private int _materialIndex;

	private Light[] _lights;

	private float[] _intensities;

	private float _lightFraction;

	private float _targetLightFraction;

	private float _fadeDuration;

	private void Awake()
	{
		base.enabled = false;
		_lightFraction = (_targetLightFraction = (_startOn ? 1f : 0f));
		_lights = GetComponentsInChildren<Light>();
		_intensities = new float[_lights.Length];
		for (int i = 0; i < _lights.Length; i++)
		{
			_intensities[i] = _lights[i].intensity;
			_lights[i].intensity *= _lightFraction;
		}
		_emission = new Color[_emissiveRenderers.Length];
		for (int j = 0; j < _emissiveRenderers.Length; j++)
		{
			if (!(_emissiveRenderers[j] == null))
			{
				if (s_matPropBlock == null)
				{
					s_matPropBlock = new MaterialPropertyBlock();
					s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
				}
				_emission[j] = _emissiveRenderers[j].sharedMaterials[_materialIndex].GetColor(s_propID_EmissionColor);
				s_matPropBlock.SetColor(s_propID_EmissionColor, _emission[j] * _lightFraction);
				_emissiveRenderers[j].SetPropertyBlock(s_matPropBlock);
			}
		}
	}

	public void FadeTo(float lightFraction, float fadeDuration = 1f)
	{
		if (_lightFraction != lightFraction)
		{
			_targetLightFraction = lightFraction;
			_fadeDuration = fadeDuration;
			base.enabled = true;
		}
	}

	private void Update()
	{
		float maxDelta = Time.deltaTime / _fadeDuration;
		_lightFraction = Mathf.MoveTowards(_lightFraction, _targetLightFraction, maxDelta);
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].intensity = Mathf.SmoothStep(0f, _intensities[i], _lightFraction);
			_lights[i].enabled = _lightFraction > 0f;
		}
		for (int j = 0; j < _emissiveRenderers.Length; j++)
		{
			if (_emissiveRenderers[j] != null)
			{
				s_matPropBlock.SetColor(s_propID_EmissionColor, _emission[j] * _lightFraction);
				_emissiveRenderers[j].SetPropertyBlock(s_matPropBlock);
			}
		}
		base.enabled = _targetLightFraction != _lightFraction;
	}
}
