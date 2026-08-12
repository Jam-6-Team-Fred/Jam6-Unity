using UnityEngine;

[RequireComponent(typeof(Light))]
public class PulsingLight : SectoredMonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_EmissionColor;

	private Light _light;

	[Space(10f)]
	[SerializeField]
	private Renderer _emissiveRenderer;

	[SerializeField]
	private int _materialIndex;

	[Space(10f)]
	[SerializeField]
	private float _pulseRate = 1f;

	[SerializeField]
	private float _intensityFluctuation;

	[SerializeField]
	private float _rangeFluctuation;

	[SerializeField]
	private float _timeOffset;

	private bool _pulsingEnabled;

	private bool _sectorOccupied;

	private float _initLightIntensity;

	private float _initLightRange;

	private Color _initEmissionColor;

	protected override void Awake()
	{
		base.Awake();
		_light = GetComponent<Light>();
		_pulsingEnabled = base.enabled;
		_sectorOccupied = false;
		_initLightIntensity = _light.intensity;
		_initLightRange = _light.range;
		if (_emissiveRenderer != null)
		{
			if (s_matPropBlock == null)
			{
				s_matPropBlock = new MaterialPropertyBlock();
				s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
			}
			_initEmissionColor = _emissiveRenderer.sharedMaterials[_materialIndex].GetColor(s_propID_EmissionColor);
		}
	}

	public void Enable()
	{
		_pulsingEnabled = true;
		if (!base.enabled && (_sectorOccupied || _sector == null))
		{
			base.enabled = true;
		}
	}

	public void Disable()
	{
		_pulsingEnabled = false;
		if (base.enabled)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		float num = Mathf.Sin((Time.time + _timeOffset) * _pulseRate);
		_light.intensity = num * _intensityFluctuation + _initLightIntensity;
		_light.range = num * _rangeFluctuation + _initLightRange;
		if (_emissiveRenderer != null)
		{
			float num2 = Mathf.Max(_light.intensity / _initLightIntensity, 0f);
			s_matPropBlock.SetColor(s_propID_EmissionColor, num2 * num2 * _initEmissionColor);
			_emissiveRenderer.SetPropertyBlock(s_matPropBlock);
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		_sectorOccupied = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (_sectorOccupied && _pulsingEnabled && !base.enabled)
		{
			base.enabled = true;
		}
		else if (!_sectorOccupied && base.enabled)
		{
			base.enabled = false;
		}
	}
}
