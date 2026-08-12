using UnityEngine;

public class SparkEffectController : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _sparkParticleSystem;

	[SerializeField]
	private Light _sparkLight;

	[SerializeField]
	private float _minSparkDelay = 1f;

	[SerializeField]
	private float _maxSparkDelay = 3f;

	[SerializeField]
	private int _minSparkCount = 2;

	[SerializeField]
	private int _maxSparkCount = 7;

	[SerializeField]
	private float _sparkLightFadeSpeed = 1f;

	private float _sparkTimer;

	private float _lightIntensity;

	private bool _sparkEnabled;

	private void Awake()
	{
		_sparkTimer = Random.Range(_minSparkDelay, _maxSparkDelay);
		if ((bool)_sparkLight)
		{
			_lightIntensity = _sparkLight.intensity;
			_sparkLight.intensity = 0f;
		}
		_sparkEnabled = true;
	}

	private void Update()
	{
		if (_sparkEnabled)
		{
			_sparkTimer -= Time.deltaTime;
		}
		if ((bool)_sparkLight)
		{
			_sparkLight.intensity = Mathf.MoveTowards(_sparkLight.intensity, 0f, _sparkLightFadeSpeed * Time.deltaTime);
		}
		if (_sparkTimer <= 0f && _sparkParticleSystem != null)
		{
			_sparkParticleSystem.Emit(Random.Range(_minSparkCount, _maxSparkCount));
			if (_sparkLight != null)
			{
				_sparkLight.intensity = _lightIntensity;
			}
			_sparkTimer = Random.Range(_minSparkDelay, _maxSparkDelay);
		}
		if (_sparkEnabled)
		{
			return;
		}
		if (_sparkLight != null)
		{
			if (_sparkLight.intensity == 0f)
			{
				base.enabled = false;
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	public void Enable()
	{
		_sparkEnabled = true;
		base.enabled = true;
	}

	public void Disable()
	{
		_sparkEnabled = false;
	}
}
