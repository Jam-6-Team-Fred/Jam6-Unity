using UnityEngine;

public class OWVolumeOcclusionLight : MonoBehaviour
{
	private bool _initialized;

	private VolumeOcclusionLight _occlusionLight;

	private bool _gameplayActive;

	private float _gameplayIntensity;

	private float _intensityScale = 1f;

	private bool _lodActive = true;

	private float _lodFade;

	private void Initialize()
	{
		if (!_initialized)
		{
			_occlusionLight = this.GetRequiredComponent<VolumeOcclusionLight>();
			_gameplayActive = _occlusionLight.enabled;
			_gameplayIntensity = _occlusionLight.intensity;
			_initialized = true;
		}
	}

	public VolumeOcclusionLight GetLight()
	{
		Initialize();
		return _occlusionLight;
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
				_occlusionLight.enabled = _gameplayActive;
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
				_occlusionLight.enabled = _lodActive;
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

	public void SetLODFade(float lodFade)
	{
		Initialize();
		_lodFade = Mathf.Clamp01(lodFade);
		UpdateIntensity();
	}

	private void UpdateIntensity()
	{
		_occlusionLight.intensity = _gameplayIntensity * _intensityScale * (1f - _lodFade);
	}
}
