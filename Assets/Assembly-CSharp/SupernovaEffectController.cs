using UnityEngine;

public class SupernovaEffectController : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] _explosionParticles;

	[SerializeField]
	private MeshRenderer _shockwave;

	[SerializeField]
	private float _shockwaveLength = 5f;

	[SerializeField]
	private AnimationCurve _shockwaveScale = AnimationCurve.Linear(0f, 0f, 1f, 100000f);

	[SerializeField]
	private AnimationCurve _shockwaveAlpha = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[Space]
	[SerializeField]
	private TessellatedSphereRenderer _surface;

	[SerializeField]
	private Material _supernovaMaterial;

	[SerializeField]
	private SupernovaDestructionVolume _supernovaVolume;

	[SerializeField]
	private AnimationCurve _supernovaScale = AnimationCurve.Linear(5f, 0f, 15f, 50000f);

	[SerializeField]
	private AnimationCurve _supernovaAlpha = AnimationCurve.Linear(5f, 1f, 15f, 0f);

	[Space]
	[SerializeField]
	private OWAudioSource _audioSource;

	private float _time;

	private float _currentSupernovaScale;

	private Material _localSupernovaMat;

	private bool _belongsToProxySun;

	private bool _renderingEnabled = true;

	private ParticleSystemRenderer[] _cachedParticleRenderers;

	private SunProxyEffectController _sunProxyEffects;

	private void Awake()
	{
		_cachedParticleRenderers = new ParticleSystemRenderer[_explosionParticles.Length];
		for (int i = 0; i < _explosionParticles.Length; i++)
		{
			_cachedParticleRenderers[i] = _explosionParticles[i].GetComponent<ParticleSystemRenderer>();
		}
	}

	private void OnEnable()
	{
		_shockwave.enabled = _renderingEnabled;
		for (int i = 0; i < _explosionParticles.Length; i++)
		{
			_explosionParticles[i].Play();
			_cachedParticleRenderers[i].enabled = _renderingEnabled;
		}
		_time = 0f;
		_currentSupernovaScale = _supernovaScale.Evaluate(0f);
		_localSupernovaMat = new Material(_supernovaMaterial);
		_surface.sharedMaterial = _localSupernovaMat;
		if (_supernovaVolume != null)
		{
			_supernovaVolume.SetActivation(active: true);
		}
		if (_audioSource != null)
		{
			_audioSource.AssignAudioLibraryClip(AudioType.Sun_SupernovaWall_LP);
			_audioSource.SetLocalVolume(0f);
			_audioSource.Play();
		}
	}

	private void OnDisable()
	{
		_shockwave.enabled = false;
		if (_supernovaVolume != null)
		{
			_supernovaVolume.SetActivation(active: false);
		}
		if (_audioSource != null)
		{
			_audioSource.SetLocalVolume(0f);
			_audioSource.Stop();
		}
	}

	private void FixedUpdate()
	{
		_time += Time.deltaTime;
		float time = Mathf.Clamp01(_time / _shockwaveLength);
		_shockwave.transform.localScale = Vector3.one * _shockwaveScale.Evaluate(time);
		_shockwave.material.color = Color.Lerp(Color.black, _shockwave.sharedMaterial.color, _shockwaveAlpha.Evaluate(time));
		_currentSupernovaScale = _supernovaScale.Evaluate(_time);
		_surface.transform.localScale = Vector3.one * _currentSupernovaScale;
		_localSupernovaMat.color = Color.Lerp(Color.black, _supernovaMaterial.color, _supernovaAlpha.Evaluate(_time));
		if (!_belongsToProxySun)
		{
			_supernovaVolume.transform.localScale = Vector3.one * _currentSupernovaScale;
		}
		float num = Vector3.Distance(base.transform.position, Locator.GetPlayerCamera().transform.position) - GetSupernovaRadius();
		if (PlayerState.InDreamWorld())
		{
			num = 20000f;
		}
		float num2 = Mathf.InverseLerp(12000f, 0f, num);
		float num3 = Mathf.Lerp(0f, 1f, num2 * num2);
		float num4 = Mathf.InverseLerp(0f, 5f, _time);
		if (!_belongsToProxySun)
		{
			_audioSource.SetLocalVolume(num3 * num4);
			RumbleManager.UpdateSupernova(num);
		}
	}

	public float GetSupernovaRadius()
	{
		return _currentSupernovaScale;
	}

	public void SetIsProxy(bool isProxy)
	{
		_belongsToProxySun = isProxy;
	}

	public void SetParticlesVisibility(bool visible)
	{
		for (int i = 0; i < _cachedParticleRenderers.Length; i++)
		{
			_cachedParticleRenderers[i].enabled = visible;
		}
	}

	public void SetRenderingEnabled(bool renderingEnabled)
	{
		_renderingEnabled = renderingEnabled;
		if (base.enabled)
		{
			_shockwave.enabled = renderingEnabled;
			SetParticlesVisibility(renderingEnabled);
		}
	}
}
