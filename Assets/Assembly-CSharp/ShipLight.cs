using UnityEngine;

[RequireComponent(typeof(Light))]
public class ShipLight : ElectricalComponent, ILightSource
{
	private Light _light;

	private OWLight2 _owLight;

	private OWLight2[] _interfaceCompatibleList;

	private MaterialPropertyBlock _matPropBlock;

	private int _propID_EmissionColor;

	[SerializeField]
	private Renderer _emissiveRenderer;

	[SerializeField]
	private int _materialIndex;

	[Space(10f)]
	[SerializeField]
	private float _fadeLength = 1f;

	private float _baseIntensity;

	private float _intensityScale;

	private Color _baseEmission;

	private bool _on;

	private bool _damaged;

	private float _fadeStartTime;

	private float _startIntensity;

	private float _targetIntensity;

	private float _mainIntensity;

	private float _extraIntensityScale = 1f;

	private LightSourceVolume _lightSourceVol;

	protected override void Awake()
	{
		base.Awake();
		_light = GetComponent<Light>();
		_owLight = GetComponent<OWLight2>();
		_baseIntensity = _light.intensity;
		_intensityScale = 1f;
		_on = _light.enabled;
		_damaged = false;
		_lightSourceVol = GetComponentInChildren<LightSourceVolume>();
		if (_lightSourceVol != null)
		{
			_lightSourceVol.LinkLightSource(this);
		}
		_fadeStartTime = 0f;
		_startIntensity = ((_on && _powered) ? _baseIntensity : 0f);
		_targetIntensity = _startIntensity;
		_mainIntensity = _targetIntensity;
		UpdateLightIntensity();
		_light.enabled = _light.intensity > 0f;
		if (_emissiveRenderer != null)
		{
			_matPropBlock = new MaterialPropertyBlock();
			_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
			_baseEmission = _emissiveRenderer.sharedMaterials[_materialIndex].GetColor(_propID_EmissionColor);
			float num = _light.intensity / _baseIntensity;
			_matPropBlock.SetColor(_propID_EmissionColor, _baseEmission * num);
			_emissiveRenderer.SetPropertyBlock(_matPropBlock);
		}
		base.enabled = false;
	}

	private void Start()
	{
		if (_lightSourceVol != null)
		{
			_lightSourceVol.SetVolumeActivation(_on);
		}
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - _fadeStartTime) / _fadeLength);
		_mainIntensity = Mathf.SmoothStep(_startIntensity, _targetIntensity, num);
		UpdateLightIntensity();
		_light.enabled = _light.intensity > 0f;
		if (_emissiveRenderer != null)
		{
			float num2 = _light.intensity / _baseIntensity;
			_matPropBlock.SetColor(_propID_EmissionColor, _baseEmission * num2);
			_emissiveRenderer.SetPropertyBlock(_matPropBlock);
		}
		if (num == 1f)
		{
			base.enabled = false;
		}
	}

	private void UpdateLightIntensity()
	{
		_light.intensity = _mainIntensity * _extraIntensityScale;
		if (_owLight != null)
		{
			_owLight.SetIntensity(_light.intensity);
		}
	}

	public bool IsOn()
	{
		return _on;
	}

	public LightSourceType GetLightSourceType()
	{
		return LightSourceType.FLASHLIGHT;
	}

	public OWLight2[] GetLights()
	{
		if (_interfaceCompatibleList == null)
		{
			if (_owLight == null)
			{
				_interfaceCompatibleList = new OWLight2[0];
			}
			else
			{
				_interfaceCompatibleList = new OWLight2[1] { _owLight };
			}
		}
		return _interfaceCompatibleList;
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer = 0f, float maxDistance = float.PositiveInfinity)
	{
		if (_owLight == null)
		{
			return false;
		}
		if (!_on)
		{
			return false;
		}
		return _owLight.CheckIlluminationAtPoint(point, buffer, maxDistance);
	}

	public bool IsDamaged()
	{
		return _damaged;
	}

	public bool IsEmittingLight()
	{
		return _light.intensity > 0f;
	}

	public float GetIntensityScale()
	{
		return _intensityScale;
	}

	public void SetExtraIntensityScale(float scale)
	{
		_extraIntensityScale = scale;
		UpdateLightIntensity();
	}

	public void SetOn(bool on)
	{
		if (_on != on)
		{
			_on = on;
			_startIntensity = _light.intensity;
			_targetIntensity = ((_on && !_damaged && _powered) ? (_baseIntensity * _intensityScale) : 0f);
			_fadeStartTime = Time.time;
			if (_lightSourceVol != null)
			{
				_lightSourceVol.SetVolumeActivation(_on);
			}
			base.enabled = true;
		}
	}

	public void SetDamaged(bool damaged)
	{
		if (_damaged != damaged)
		{
			_damaged = damaged;
			_startIntensity = _light.intensity;
			_targetIntensity = ((_on && !_damaged && _powered) ? (_baseIntensity * _intensityScale) : 0f);
			_fadeStartTime = Time.time;
			base.enabled = true;
		}
	}

	public override void SetPowered(bool powered)
	{
		if (_powered != powered)
		{
			base.SetPowered(powered);
			_startIntensity = _light.intensity;
			_targetIntensity = ((_on && !_damaged && _powered) ? (_baseIntensity * _intensityScale) : 0f);
			_fadeStartTime = Time.time;
			base.enabled = true;
		}
	}

	public void SetIntensityScale(float scale)
	{
		if (_intensityScale != scale)
		{
			_intensityScale = scale;
			_startIntensity = _light.intensity;
			_targetIntensity = ((_on && !_damaged && _powered) ? (_baseIntensity * _intensityScale) : 0f);
			_fadeStartTime = Time.time;
			base.enabled = true;
		}
	}
}
