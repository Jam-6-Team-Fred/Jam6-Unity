using System;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class CloudLightning : MonoBehaviour
{
	[Serializable]
	public class AnimSettings
	{
		public AnimationCurve radiusScale;

		public AnimationCurve intensityScale;
	}

	public delegate void CloudLightningEvent(CloudLightning cloudLightning);

	private Light _light;

	[SerializeField]
	private float _lightLength = 1f;

	[SerializeField]
	private float _lightRadius = 100f;

	[SerializeField]
	private float _lightIntensity = 5f;

	[SerializeField]
	private AnimSettings _lightAnimSettings;

	private float _lightTimer;

	public Color lightColor
	{
		get
		{
			return _light.color;
		}
		set
		{
			_light.color = value;
		}
	}

	public float lightLength
	{
		get
		{
			return _lightLength;
		}
		set
		{
			_lightLength = value;
		}
	}

	public float lightRadius
	{
		get
		{
			return _lightRadius;
		}
		set
		{
			_lightRadius = value;
		}
	}

	public float lightIntensity
	{
		get
		{
			return _lightIntensity;
		}
		set
		{
			_lightIntensity = value;
		}
	}

	public AnimSettings lightAnimSettings
	{
		get
		{
			return _lightAnimSettings;
		}
		set
		{
			_lightAnimSettings = value;
		}
	}

	public event CloudLightningEvent OnComplete;

	private void Awake()
	{
		_light = GetComponent<Light>();
		ResetLightning();
	}

	private void Update()
	{
		_lightTimer -= Time.deltaTime;
		float time = 1f - Mathf.Clamp01(_lightTimer / _lightLength);
		_light.range = _lightAnimSettings.radiusScale.Evaluate(time) * _lightRadius;
		_light.intensity = _lightAnimSettings.intensityScale.Evaluate(time) * _lightIntensity;
		if (_lightTimer <= 0f && this.OnComplete != null)
		{
			this.OnComplete(this);
		}
	}

	public void ResetLightning()
	{
		_lightTimer = _lightLength;
		_light.intensity = 0f;
		_light.range = 0f;
	}
}
