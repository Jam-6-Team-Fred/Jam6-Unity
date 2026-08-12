using UnityEngine;

public class MiniGalaxy : MonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_Fade;

	private static int s_propID_ImpactTime;

	private static int s_propID_ImpactPosition;

	private static int s_propID_ImpactVelocity;

	[SerializeField]
	private ParticleSystem _deathParticles;

	[SerializeField]
	private ParticleSystem[] _galaxySpiralParticles;

	[SerializeField]
	private Renderer[] _galaxyRenderers;

	[SerializeField]
	private OWLight2 _light;

	[SerializeField]
	private OWTriggerVolume _trigger;

	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private bool _randomRotation = true;

	[SerializeField]
	private float _expandedLightRange = 40f;

	private AudioType _deathClip;

	private bool _alive;

	private bool _waitingToAppear;

	private bool _waitingToDie;

	private bool _impacted;

	private bool _playDeathParticles;

	private float _alpha;

	private float _startAlpha;

	private float _targetAlpha;

	private float _fadeDuration;

	private float _fadeStartTime;

	private float _appearTime;

	private float _deathTime;

	private bool _expandLightRange;

	private float _origLightRange;

	private float _startExpandTime;

	private Vector3 _localImpactPos;

	private Vector3 _localImpactVel;

	private float _flickerOnDuration;

	private void Awake()
	{
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_Fade = Shader.PropertyToID("_Fade");
			s_propID_ImpactTime = Shader.PropertyToID("_ImpactTime");
			s_propID_ImpactPosition = Shader.PropertyToID("_ImpactPosition");
			s_propID_ImpactVelocity = Shader.PropertyToID("_ImpactVelocity");
		}
		for (int i = 0; i < _galaxySpiralParticles.Length; i++)
		{
			ParticleSystem.ShapeModule shape = _galaxySpiralParticles[i].shape;
			shape.arcSpread = Random.Range(0.15f, 0.23f);
		}
		_targetAlpha = 0f;
		_origLightRange = _light.range;
		if (_randomRotation)
		{
			base.transform.rotation = Random.rotation;
			_deathParticles.transform.forward = Vector3.up;
		}
		_trigger.OnEntry += OnEntry;
	}

	private void Start()
	{
		UpdateVisuals();
		base.enabled = _waitingToAppear;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
	}

	public void AppearAfterSeconds(float seconds)
	{
		if (!_alive)
		{
			base.enabled = true;
			_waitingToAppear = true;
			_waitingToDie = false;
			_appearTime = Time.time + seconds;
		}
	}

	public void DieAfterSeconds(float seconds, bool playDeathParticles, AudioType deathClip)
	{
		if (_alive || _waitingToAppear)
		{
			_deathTime = Time.time + seconds;
			_waitingToDie = true;
			_playDeathParticles = playDeathParticles;
			_deathClip = deathClip;
			base.enabled = true;
		}
	}

	public void ExpandLightRange()
	{
		_expandLightRange = true;
		_startExpandTime = Time.time;
	}

	private void Update()
	{
		if (_waitingToAppear && Time.time > _appearTime)
		{
			_alive = true;
			_waitingToAppear = false;
			FadeTo(1f, 2f);
		}
		if (_waitingToDie && Time.time > _deathTime)
		{
			_alive = false;
			_waitingToDie = false;
			_audioSource.PlayOneShot(_deathClip);
			if (_playDeathParticles)
			{
				_deathParticles.Play();
			}
			FadeTo(0f, 3f);
		}
		if (_alpha != _targetAlpha)
		{
			float t = Mathf.InverseLerp(_fadeStartTime, _fadeStartTime + _fadeDuration, Time.time);
			_alpha = Mathf.Lerp(_startAlpha, _targetAlpha, t);
			if (_alpha == 0f && !_alive)
			{
				base.enabled = false;
			}
		}
		UpdateVisuals();
	}

	private void FadeTo(float alpha, float duration)
	{
		_startAlpha = _alpha;
		_targetAlpha = alpha;
		_fadeDuration = duration;
		_fadeStartTime = Time.time;
	}

	private void UpdateVisuals()
	{
		_light.SetIntensityScale(_alpha);
		if (_expandLightRange)
		{
			float num = Mathf.InverseLerp(_startExpandTime, _startExpandTime + 8f, Time.time);
			_light.range = Mathf.Lerp(_origLightRange, _expandedLightRange, Mathf.SmoothStep(0f, 1f, num));
			if (num >= 1f)
			{
				_expandLightRange = false;
			}
		}
		s_matPropBlock.SetFloat(s_propID_Fade, 1f - _alpha);
		if (_impacted)
		{
			s_matPropBlock.SetFloat(s_propID_ImpactTime, Mathf.Max(Time.time - _deathTime, 0f));
			s_matPropBlock.SetVector(s_propID_ImpactPosition, base.transform.TransformPoint(_localImpactPos));
			s_matPropBlock.SetVector(s_propID_ImpactVelocity, base.transform.TransformVector(_localImpactVel));
		}
		else
		{
			s_matPropBlock.SetFloat(s_propID_ImpactTime, 0f);
			s_matPropBlock.SetVector(s_propID_ImpactPosition, Vector3.zero);
			s_matPropBlock.SetVector(s_propID_ImpactVelocity, Vector3.zero);
		}
		for (int i = 0; i < _galaxyRenderers.Length; i++)
		{
			_galaxyRenderers[i].SetPropertyBlock(s_matPropBlock);
		}
	}

	private void OnEntry(GameObject obj)
	{
		if ((_alive && obj.CompareTag("PlayerDetector")) || obj.CompareTag("ProbeDetector"))
		{
			_impacted = true;
			Vector3 position = base.transform.position + Vector3.Normalize(obj.transform.position - base.transform.position);
			_localImpactPos = base.transform.InverseTransformPoint(position);
			Vector3 relativeVelocity = this.GetAttachedOWRigidbody().GetRelativeVelocity(obj.GetAttachedOWRigidbody());
			_localImpactVel = base.transform.InverseTransformVector(relativeVelocity);
			DieAfterSeconds(0f, playDeathParticles: false, AudioType.EyeGalaxyBlowAway);
		}
	}
}
