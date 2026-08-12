using UnityEngine;

[RequireComponent(typeof(Light))]
public class TranslatorTargetBeam : MonoBehaviour
{
	private Light _light;

	[SerializeField]
	private float _activateSpeed = 1f;

	private float _initIntensity;

	private bool _active;

	private float _activateTimer;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_initIntensity = _light.intensity;
		_active = false;
		_activateTimer = 0f;
		_light.enabled = false;
	}

	private void Update()
	{
		_activateTimer = Mathf.Clamp01(_activateTimer + Time.deltaTime * _activateSpeed * (_active ? 1f : (-1f)));
		if (_active && _activateTimer == 1f)
		{
			_light.intensity = _initIntensity;
			base.enabled = false;
		}
		else if (!_active && _activateTimer == 0f)
		{
			_light.intensity = 0f;
			base.enabled = false;
			_light.enabled = false;
		}
		else
		{
			_light.intensity = Mathf.PerlinNoise(Time.timeSinceLevelLoad * 30f, 0f) * _activateTimer * _initIntensity;
		}
	}

	public void Activate()
	{
		_active = true;
		base.enabled = true;
		if (_light != null)
		{
			_light.enabled = true;
		}
	}

	public void Deactivate()
	{
		_active = false;
		base.enabled = true;
	}
}
