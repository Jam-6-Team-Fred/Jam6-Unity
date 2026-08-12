using System;
using UnityEngine;

public class OWLight2 : MonoBehaviour, ILightSource
{
	private bool _initialized;

	private Light _light;

	private bool _gameplayActive;

	private float _gameplayIntensity;

	private float _intensityScale = 1f;

	private float _flickerScale = 1f;

	private bool _lodActive = true;

	private float _lodFade;

	private OWLight2[] _interfaceCompatibleList;

	public float range
	{
		get
		{
			Initialize();
			return _light.range;
		}
		set
		{
			Initialize();
			_light.range = value;
		}
	}

	private void Initialize()
	{
		if (!_initialized)
		{
			_light = this.GetRequiredComponent<Light>();
			_gameplayActive = _light.enabled;
			_gameplayIntensity = _light.intensity;
			_initialized = true;
		}
	}

	public LightSourceType GetLightSourceType()
	{
		return LightSourceType.UNDEFINED;
	}

	public OWLight2[] GetLights()
	{
		if (_interfaceCompatibleList == null)
		{
			_interfaceCompatibleList = new OWLight2[1] { this };
		}
		return _interfaceCompatibleList;
	}

	public Light GetLight()
	{
		Initialize();
		return _light;
	}

	public bool IsActive()
	{
		Initialize();
		return _gameplayActive;
	}

	public void SetActivation(bool active)
	{
		Initialize();
		if (_gameplayActive != active)
		{
			_gameplayActive = active;
			if (_lodActive)
			{
				_light.enabled = _gameplayActive;
			}
		}
	}

	public void SetLODActivation(bool active)
	{
		Initialize();
		if (_lodActive != active)
		{
			_lodActive = active;
			if (_gameplayActive)
			{
				_light.enabled = _lodActive;
			}
		}
	}

	public float GetIntensity()
	{
		Initialize();
		return _gameplayIntensity;
	}

	public float GetIntensityScale()
	{
		Initialize();
		return _intensityScale;
	}

	public float GetFlickerScale()
	{
		Initialize();
		return _flickerScale;
	}

	public void SetIntensity(float intensity)
	{
		Initialize();
		_gameplayIntensity = Mathf.Max(intensity, 0f);
		UpdateIntensity();
	}

	public void SetIntensityScale(float scale)
	{
		Initialize();
		_intensityScale = scale;
		UpdateIntensity();
	}

	public void SetFlickerScale(float scale)
	{
		Initialize();
		_flickerScale = scale;
		UpdateIntensity();
	}

	public void SetLODFade(float lodFade)
	{
		Initialize();
		_lodFade = Mathf.Clamp01(lodFade);
		UpdateIntensity();
	}

	private void UpdateIntensity()
	{
		_light.intensity = _gameplayIntensity * _intensityScale * _flickerScale * (1f - _lodFade);
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer = 0f, float maxDistance = float.PositiveInfinity)
	{
		Initialize();
		float num = _gameplayIntensity * _intensityScale * _flickerScale;
		if (!_gameplayActive || num < 0.0001f)
		{
			return false;
		}
		Vector3 to = point - base.transform.position;
		float magnitude = to.magnitude;
		float num2 = Mathf.Min(_light.range, maxDistance);
		switch (_light.type)
		{
		case LightType.Point:
			return magnitude - buffer <= num2;
		case LightType.Spot:
		{
			float num3 = _light.spotAngle * 0.5f;
			float num4 = Vector3.Angle(base.transform.forward, to);
			if (magnitude - buffer > num2)
			{
				return false;
			}
			if (num4 <= num3)
			{
				return true;
			}
			if (buffer > 0f)
			{
				if (magnitude < buffer)
				{
					return true;
				}
				return magnitude * Mathf.Sin((num4 - num3) * ((float)Math.PI / 180f)) <= buffer;
			}
			return false;
		}
		case LightType.Directional:
		case LightType.Area:
			return true;
		default:
			return false;
		}
	}
}
