using UnityEngine;

[RequireComponent(typeof(Light))]
public class OWLight : MonoBehaviour
{
	private Light _light;

	private float _baseIntensity;

	private float _intensity = 1f;

	private float _flickerIntensity = 1f;

	private float _originalRange;

	private float _fadeStartTime;

	private float _fadeDuration;

	private float _fadeStartIntensity;

	private float _fadeTargetIntensity;

	public float range
	{
		get
		{
			return _light.range;
		}
		set
		{
			_light.range = value;
		}
	}

	private void Awake()
	{
		_light = GetComponent<Light>();
		_baseIntensity = _light.intensity;
		_originalRange = _light.range;
		base.enabled = false;
	}

	public Light GetLight()
	{
		return _light;
	}

	public float GetOriginalRange()
	{
		return _originalRange;
	}

	public void SetRange(float range)
	{
		_light.range = range;
	}

	public void SetIntensity(float intensity)
	{
		_intensity = intensity;
		UpdateLight();
	}

	public void SetFlickerIntensity(float flickerIntensity)
	{
		_flickerIntensity = flickerIntensity;
		UpdateLight();
	}

	public float GetFlickerIntensity()
	{
		return _flickerIntensity;
	}

	public void FadeTo(float intensity, float duration)
	{
		if (duration <= 0f)
		{
			_intensity = intensity;
			UpdateLight();
			return;
		}
		_fadeStartIntensity = _intensity;
		_fadeTargetIntensity = intensity;
		_fadeDuration = duration;
		_fadeStartTime = Time.time;
		base.enabled = true;
	}

	private void UpdateLight()
	{
		_light.intensity = Mathf.Lerp(0f, _baseIntensity, _intensity * _flickerIntensity);
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(_fadeStartTime, _fadeStartTime + _fadeDuration, Time.time);
		_intensity = Mathf.Lerp(_fadeStartIntensity, _fadeTargetIntensity, Mathf.SmoothStep(0f, 1f, num));
		if (num >= 1f)
		{
			base.enabled = false;
		}
		UpdateLight();
	}
}
