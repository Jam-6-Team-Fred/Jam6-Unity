using UnityEngine;

public class GravityCrystalAudioController : MonoBehaviour
{
	private const float FADE_DURATION = 0.5f;

	[SerializeField]
	private OWTriggerVolume _trigger;

	[SerializeField]
	private bool _rolloffOverDistance;

	[SerializeField]
	private float _minLocalVolume = 0.2f;

	private OWAudioSource _audioSource;

	private float _radius = -1f;

	private float _startFadeTime;

	private float _fadeVolume;

	private float _targetFadeVolume;

	private void OnValidate()
	{
		AudioSource component = GetComponent<AudioSource>();
		if (component.playOnAwake)
		{
			component.playOnAwake = false;
		}
		if (!component.loop)
		{
			component.loop = true;
		}
		if (_rolloffOverDistance)
		{
			if (component.spatialBlend < 1f)
			{
				component.spatialBlend = 1f;
				component.rolloffMode = AudioRolloffMode.Custom;
				component.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0f, 1f, 1f, 1f));
			}
		}
		else if (component.spatialBlend > 0f)
		{
			component.spatialBlend = 0f;
		}
	}

	private void Awake()
	{
		_audioSource = GetComponent<OWAudioSource>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		if (_rolloffOverDistance)
		{
			CapsuleCollider component = _trigger.gameObject.GetComponent<CapsuleCollider>();
			if (component != null)
			{
				_radius = component.radius;
				return;
			}
			Debug.LogError("Could not find attached Capsule Collider");
			Debug.Break();
		}
	}

	private void Start()
	{
		_audioSource.SetLocalVolume(0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void Update()
	{
		float num = 1f;
		if (_rolloffOverDistance)
		{
			float magnitude = (Locator.GetPlayerTransform().position - base.transform.position).magnitude;
			float num2 = Mathf.InverseLerp(_radius, 1f, magnitude);
			num = Mathf.Lerp(_minLocalVolume, 1f, num2 * num2);
		}
		_fadeVolume = Mathf.MoveTowards(_fadeVolume, _targetFadeVolume, Time.deltaTime / 0.5f);
		_audioSource.SetLocalVolume(num * _fadeVolume);
		if (_targetFadeVolume <= 0f && _fadeVolume <= 0f)
		{
			base.enabled = false;
			_audioSource.Stop();
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			if (!_audioSource.isPlaying)
			{
				_audioSource.Play();
				_audioSource.RandomizePlayhead();
			}
			_targetFadeVolume = 1f;
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_targetFadeVolume = 0f;
		}
	}
}
