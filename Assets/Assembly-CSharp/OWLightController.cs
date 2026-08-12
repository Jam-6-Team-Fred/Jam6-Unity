using System;
using UnityEngine;

public class OWLightController : MonoBehaviour
{
	[SerializeField]
	private OWLight2[] _lights;

	[SerializeField]
	private OWEmissiveRenderer[] _renderers;

	protected float _intensity = 1f;

	private float _fadeStartTime;

	private float _fadeDuration;

	private float _fadeStartIntensity;

	private float _fadeTargetIntensity;

	public OWLight2[] lights => _lights;

	public OWEmissiveRenderer[] renderers => _renderers;

	protected virtual void Awake()
	{
		base.enabled = false;
	}

	public bool IsFading()
	{
		return base.enabled;
	}

	public float GetIntensity()
	{
		return _intensity;
	}

	public void SetIntensity(float intensity)
	{
		_intensity = intensity;
		UpdateVisuals();
		base.enabled = false;
	}

	public void FadeTo(float intensity, float duration)
	{
		if (duration <= 0f)
		{
			_intensity = intensity;
			UpdateVisuals();
			base.enabled = false;
		}
		else
		{
			_fadeStartIntensity = _intensity;
			_fadeTargetIntensity = intensity;
			_fadeDuration = duration;
			_fadeStartTime = Time.time;
			base.enabled = true;
		}
	}

	protected virtual void UpdateVisuals()
	{
		try
		{
			for (int i = 0; i < _lights.Length; i++)
			{
				_lights[i].SetIntensityScale(_intensity);
			}
			for (int j = 0; j < _renderers.Length; j++)
			{
				_renderers[j].SetEmissiveScale(_intensity);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(_fadeStartTime, _fadeStartTime + _fadeDuration, Time.time);
		_intensity = Mathf.Lerp(_fadeStartIntensity, _fadeTargetIntensity, Mathf.SmoothStep(0f, 1f, num));
		if (num >= 1f)
		{
			base.enabled = false;
		}
		UpdateVisuals();
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer, float maxDistance)
	{
		bool result = false;
		for (int i = 0; i < _lights.Length; i++)
		{
			if (_lights[i].CheckIlluminationAtPoint(point, buffer, maxDistance))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public OWLight2[] GetLights()
	{
		return _lights;
	}
}
