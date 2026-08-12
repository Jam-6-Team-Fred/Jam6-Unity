using UnityEngine;

[RequireComponent(typeof(Light))]
public class HeatLightningController : MonoBehaviour
{
	[SerializeField]
	private float _minflashDuration = 0.5f;

	[SerializeField]
	private float _maxflashDuration = 1.5f;

	[SerializeField]
	private float _minIntensity = 0.5f;

	[SerializeField]
	private float _maxIntensity = 2f;

	[SerializeField]
	private float _minFlashInterval = 5f;

	[SerializeField]
	private float _maxFlashInterval = 15f;

	private Light _light;

	private float _startFlashTime;

	private float _flashDuration;

	private bool _flashing;

	private float _manualFlashDuration;

	private bool _manualFlashQueued;

	private void Start()
	{
		_light = GetComponent<Light>();
		ResetFlash();
	}

	public void TriggerFlash(float duration)
	{
		_manualFlashQueued = true;
		_manualFlashDuration = duration;
		if (!_flashing)
		{
			ResetFlash();
		}
	}

	private void ResetFlash()
	{
		_startFlashTime = Time.time + Random.Range(_minFlashInterval, _maxFlashInterval);
		_flashDuration = Random.Range(_minflashDuration, _maxflashDuration);
		_flashing = false;
		_light.intensity = _minIntensity;
		if (_manualFlashQueued)
		{
			_startFlashTime = Time.time;
			_flashDuration = _manualFlashDuration;
			_manualFlashQueued = false;
		}
	}

	private void Update()
	{
		if (!_flashing && Time.time > _startFlashTime)
		{
			_flashing = true;
		}
		if (_flashing)
		{
			float num = Mathf.Clamp01((Time.time - _startFlashTime) / _flashDuration);
			_light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, 0f - Mathf.Pow(2f * num - 1f, 2f) + 1f);
			if (num == 1f)
			{
				ResetFlash();
			}
		}
	}
}
