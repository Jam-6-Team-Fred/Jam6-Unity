using System;
using UnityEngine;

public class PossibilitySphereController : MonoBehaviour
{
	[Serializable]
	private struct ProbabilityParticleSystem
	{
		public ParticleSystem particleSystem;

		public float probability;
	}

	[SerializeField]
	private float _minIntensity = 0.5f;

	[SerializeField]
	private float _fadeInDuration = 0.1f;

	[SerializeField]
	private float _litDuration = 0.1f;

	[SerializeField]
	private float _fadeOutDuration = 0.05f;

	[SerializeField]
	private float _unlitDuration;

	[Space]
	[SerializeField]
	private OWLight[] _lights;

	[SerializeField]
	private ProbabilityParticleSystem[] _particles;

	[SerializeField]
	private Transform[] _randomRotationRoots;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource _ambientSource;

	[Header("Prisoner")]
	[SerializeField]
	private GameObject _prisonerObject;

	[SerializeField]
	private LightSensor _prisonerLightSensor;

	[SerializeField]
	private OWEmissiveRenderer[] _prisonerEyeRenderers;

	private float _midIntensity;

	private float _startIntensity;

	private float _targetIntensity;

	private float _changeStateTime;

	private float _fadeDuration;

	private bool _pause;

	private void Start()
	{
		base.enabled = false;
	}

	public void Activate()
	{
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].SetIntensity(0f);
		}
		_midIntensity = Mathf.Lerp(_minIntensity, 1f, 0.5f);
		_changeStateTime = Time.time;
		_targetIntensity = 1f;
		_fadeDuration = _fadeInDuration;
		GenerateShadows();
		_ambientSource.SetLocalVolume(0f);
		_ambientSource.FadeIn(3f);
		base.enabled = true;
	}

	public void OnCollapse()
	{
		_ambientSource.FadeOut(0.5f);
		_oneShotSource.pitch = 1f;
		_oneShotSource.PlayOneShot(AudioType.EyeSmokeSphereEntry);
		_oneShotSource.PlayOneShot(AudioType.EyeSmokeSphereCollapse);
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].SetIntensity(0f);
		}
		for (int j = 0; j < _particles.Length; j++)
		{
			_particles[j].particleSystem.Clear();
		}
		for (int k = 0; k < _randomRotationRoots.Length; k++)
		{
			_randomRotationRoots[k].gameObject.SetActive(value: false);
		}
		if (_prisonerObject.activeInHierarchy)
		{
			SetPrisonerEyeGlow(0.5f);
		}
		base.enabled = false;
	}

	private void GenerateShadows()
	{
		_oneShotSource.pitch = UnityEngine.Random.Range(0.9f, 1f);
		_oneShotSource.PlayOneShot(AudioType.EyeSmokeSpherePulse);
		for (int i = 0; i < _randomRotationRoots.Length; i++)
		{
			_randomRotationRoots[i].forward = UnityEngine.Random.onUnitSphere;
		}
		for (int j = 0; j < _particles.Length; j++)
		{
			if (UnityEngine.Random.value <= _particles[j].probability)
			{
				_particles[j].particleSystem.Clear();
				_particles[j].particleSystem.Play();
			}
		}
	}

	private void Update()
	{
		if (_pause)
		{
			float num = ((_targetIntensity < _midIntensity) ? _unlitDuration : _litDuration);
			if (Time.time > _changeStateTime + num)
			{
				_startIntensity = _targetIntensity;
				_targetIntensity = ((_targetIntensity < _midIntensity) ? 1f : _minIntensity);
				_fadeDuration = ((_targetIntensity > _midIntensity) ? _fadeInDuration : _fadeOutDuration);
				_changeStateTime = Time.time;
				_pause = false;
			}
		}
		else
		{
			float num2 = Mathf.InverseLerp(_changeStateTime, _changeStateTime + _fadeDuration, Time.time);
			float num3 = Mathf.Lerp(_startIntensity, _targetIntensity, num2);
			for (int i = 0; i < _lights.Length; i++)
			{
				_lights[i].SetIntensity(num3);
			}
			if (_prisonerObject.activeInHierarchy && !_prisonerLightSensor.IsIlluminated())
			{
				SetPrisonerEyeGlow(num3 * 0.8f);
			}
			if (num2 >= 1f)
			{
				if (_targetIntensity < _midIntensity)
				{
					GenerateShadows();
				}
				_changeStateTime = Time.time;
				_pause = true;
			}
		}
		if (_prisonerObject.activeInHierarchy && _prisonerLightSensor.IsIlluminated())
		{
			SetPrisonerEyeGlow(1f);
		}
	}

	private void SetPrisonerEyeGlow(float glow)
	{
		for (int i = 0; i < _prisonerEyeRenderers.Length; i++)
		{
			_prisonerEyeRenderers[i].SetEmissiveScale(glow);
		}
	}
}
