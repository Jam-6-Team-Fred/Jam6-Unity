using System;
using UnityEngine;

public class DamageEffect : MonoBehaviour
{
	public enum ParticleMode
	{
		Burst = 0,
		Continuous = 1
	}

	[SerializeField]
	private OWRenderer[] _decalRenderers = new OWRenderer[0];

	[SerializeField]
	private SkinnedMeshRenderer[] _blendShapeRenderers = new SkinnedMeshRenderer[0];

	[SerializeField]
	private OWLight2[] _glowLights = new OWLight2[0];

	[Space]
	[SerializeField]
	private OWRenderer _patchRenderer;

	[Space]
	[SerializeField]
	private OWLight2 _damageLight;

	[SerializeField]
	private OWRenderer _damageLightRenderer;

	[SerializeField]
	private float _damageLightPulseSpeed = 1f;

	[Space]
	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private ParticleMode _particleMode;

	[SerializeField]
	private Vector2 _particleBurstCount = new Vector2(10f, 30f);

	[SerializeField]
	private Vector2 _particleBurstDelay = new Vector2(0.1f, 1f);

	[Space]
	[SerializeField]
	private OWAudioSource _particleAudioSource;

	protected float _blend;

	private float _damageLightIntensity;

	private Color _damageLightRendererColor;

	private float _burstTimer;

	protected bool _passiveEffectsOnly;

	private bool _bAwakeDisable = true;

	protected virtual void Awake()
	{
		if ((bool)_damageLight)
		{
			_damageLightIntensity = _damageLight.GetIntensity();
		}
		if ((bool)_damageLightRenderer)
		{
			_damageLightRendererColor = _damageLightRenderer.GetOriginalEmissionColor();
		}
		SetEffectBlend(0f);
		if ((bool)_patchRenderer)
		{
			_patchRenderer.SetActivation(active: false);
		}
	}

	protected virtual void OnEnable()
	{
		for (int i = 0; i < _decalRenderers.Length; i++)
		{
			_decalRenderers[i].SetActivation(active: true);
		}
		for (int j = 0; j < _glowLights.Length; j++)
		{
			_glowLights[j].SetActivation(active: true);
		}
		if ((bool)_patchRenderer)
		{
			_patchRenderer.SetActivation(active: false);
		}
		if ((bool)_damageLight)
		{
			_damageLight.SetActivation(active: true);
		}
		if ((bool)_particleSystem)
		{
			_particleSystem.Play();
			if (_particleMode == ParticleMode.Continuous && (bool)_particleAudioSource)
			{
				_particleAudioSource.Play();
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (!_passiveEffectsOnly)
		{
			for (int i = 0; i < _decalRenderers.Length; i++)
			{
				_decalRenderers[i].SetActivation(active: false);
			}
			for (int j = 0; j < _glowLights.Length; j++)
			{
				_glowLights[j].SetActivation(active: false);
			}
			if ((bool)_patchRenderer)
			{
				_patchRenderer.SetActivation(active: true);
			}
		}
		if ((bool)_damageLight)
		{
			_damageLight.SetActivation(active: false);
		}
		if ((bool)_damageLightRenderer)
		{
			_damageLightRenderer.SetEmissionColor(Color.black);
		}
		if ((bool)_particleSystem)
		{
			_particleSystem.Stop();
			if (_bAwakeDisable)
			{
				_bAwakeDisable = false;
			}
			else if (_particleMode == ParticleMode.Continuous && (bool)_particleAudioSource)
			{
				_particleAudioSource.Stop();
			}
		}
	}

	public virtual void SetEffectBlend(float blend)
	{
		_blend = Mathf.Clamp01(blend);
		for (int i = 0; i < _decalRenderers.Length; i++)
		{
			_decalRenderers[i].SetColor(new Color(1f, 1f, 1f, _blend));
		}
		for (int j = 0; j < _blendShapeRenderers.Length; j++)
		{
			if (_blendShapeRenderers[j].sharedMesh != null)
			{
				_blendShapeRenderers[j].SetBlendShapeWeight(0, _blend * 100f);
			}
		}
		for (int k = 0; k < _glowLights.Length; k++)
		{
			_glowLights[k].SetIntensityScale(_blend);
		}
		if (_blend <= 0f && base.enabled)
		{
			base.enabled = false;
		}
		else if (_blend > 0f && !base.enabled)
		{
			base.enabled = true;
		}
	}

	public virtual void DisableActiveEffects()
	{
		_passiveEffectsOnly = true;
		base.enabled = false;
	}

	protected virtual void Update()
	{
		float num = Mathf.Abs(Mathf.Sin(Time.time * (float)Math.PI * _damageLightPulseSpeed));
		if ((bool)_damageLight)
		{
			_damageLight.SetIntensity(_damageLightIntensity * num);
		}
		if ((bool)_damageLightRenderer)
		{
			_damageLightRenderer.SetEmissionColor(_damageLightRendererColor * num * num);
		}
		if (!_particleSystem || _particleMode != 0)
		{
			return;
		}
		_burstTimer -= Time.deltaTime;
		if (_burstTimer <= 0f)
		{
			_particleSystem.Emit(UnityEngine.Random.Range((int)_particleBurstCount.x, (int)_particleBurstCount.y));
			if ((bool)_particleAudioSource)
			{
				_particleAudioSource.PlayOneShot();
			}
			_burstTimer = UnityEngine.Random.Range(_particleBurstDelay.x, _particleBurstDelay.y);
		}
	}
}
