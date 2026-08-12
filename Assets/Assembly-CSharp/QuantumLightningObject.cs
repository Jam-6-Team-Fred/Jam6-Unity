using UnityEngine;

public class QuantumLightningObject : MonoBehaviour
{
	[SerializeField]
	private Light _light;

	[SerializeField]
	private GameObject[] _models;

	[SerializeField]
	private ParticleSystem[] _particleSystems;

	[SerializeField]
	private OWAudioSource _audioSource;

	private float _flashDuration = 0.5f;

	private float _startFlashTime;

	private bool _flashing;

	private int _index;

	private void Start()
	{
		SetActivation(active: false);
	}

	public void SetActivation(bool active)
	{
		base.enabled = active;
		if (active)
		{
			ResetFlash();
			return;
		}
		_light.enabled = false;
		for (int i = 0; i < _models.Length; i++)
		{
			_models[i].SetActive(value: false);
			_particleSystems = GetComponentsInChildren<ParticleSystem>();
		}
	}

	private void ResetFlash()
	{
		_startFlashTime = Time.time + Random.Range(5f, 15f);
		_flashing = false;
		_light.intensity = 0f;
		_light.enabled = true;
		_models[_index].SetActive(value: false);
		_index = Random.Range(0, _models.Length);
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			_particleSystems[i].Stop();
		}
	}

	private void Update()
	{
		if (!_flashing && Time.time > _startFlashTime)
		{
			_flashing = true;
			_models[_index].SetActive(value: true);
			for (int i = 0; i < _particleSystems.Length; i++)
			{
				_particleSystems[i].Play();
			}
			_audioSource.PlayOneShot(AudioType.EyeLightning);
			if (Vector3.Distance(base.transform.position, Locator.GetPlayerTransform().position) < 40f)
			{
				RumbleManager.PulseQuantumLightning();
			}
		}
		if (_flashing)
		{
			float num = Mathf.Clamp01((Time.time - _startFlashTime) / _flashDuration);
			_light.intensity = (0f - Mathf.Pow(2f * num - 1f, 2f) + 1f) * 2f;
			if (num == 1f)
			{
				ResetFlash();
			}
		}
	}
}
