using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : SectoredMonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_EmissionColor;

	private Light _light;

	[SerializeField]
	private Renderer _emissiveRenderer;

	[SerializeField]
	private int _materialIndex;

	[Space(10f)]
	[SerializeField]
	private float _range = 0.1f;

	[SerializeField]
	private float _rate = 0.2f;

	private float _startLightIntensity;

	private float _targetLightIntensity;

	private Color _baseEmissionColor;

	private float _startEmissionIntensity;

	private float _targetEmissionIntensity;

	protected override void Awake()
	{
		base.Awake();
		_light = GetComponent<Light>();
		_startLightIntensity = _light.intensity;
		_targetLightIntensity = _startLightIntensity;
		if (_emissiveRenderer != null)
		{
			if (s_matPropBlock == null)
			{
				s_matPropBlock = new MaterialPropertyBlock();
				if (_emissiveRenderer.sharedMaterials[_materialIndex].HasProperty("_EmissionColor"))
				{
					s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
				}
				else
				{
					s_propID_EmissionColor = Shader.PropertyToID("_Color");
				}
			}
			_baseEmissionColor = _emissiveRenderer.sharedMaterials[_materialIndex].GetColor(s_propID_EmissionColor);
			_startEmissionIntensity = 1f;
		}
		if (_sector != null)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (Mathf.Abs(_light.intensity - _targetLightIntensity) < 0.01f)
		{
			float num = Random.Range(-1f, 1f) * _range;
			_targetLightIntensity = _startLightIntensity + num;
			_targetEmissionIntensity = _startEmissionIntensity + num;
		}
		_light.intensity = Mathf.Lerp(_light.intensity, _targetLightIntensity, _rate);
		if (_emissiveRenderer != null)
		{
			s_matPropBlock.SetColor(s_propID_EmissionColor, Mathf.Lerp(_startEmissionIntensity, _targetEmissionIntensity, _rate) * _baseEmissionColor);
			_emissiveRenderer.SetPropertyBlock(s_matPropBlock);
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}
}
