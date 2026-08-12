using UnityEngine;

[RequireComponent(typeof(OWAudioSource))]
public class ObjectImpactAudio : MonoBehaviour
{
	[SerializeField]
	private AudioType _clipType = AudioType.DefaultPropImpact;

	[SerializeField]
	private ImpactSensor _impactSensor;

	[SerializeField]
	private float _minSpeed = 1f;

	[SerializeField]
	private float _maxSpeed = 10f;

	[SerializeField]
	private float _minPitch = 1f;

	[SerializeField]
	private float _maxPitch = 1f;

	[SerializeField]
	private float _minCollidingMass;

	private OWAudioSource _audioSource;

	private float _lastImpactTime;

	private void Reset()
	{
		_impactSensor = GetComponentInParent<ImpactSensor>();
		OWAudioSource addComponent = base.gameObject.GetAddComponent<OWAudioSource>();
		if (addComponent != null)
		{
			addComponent.SetTrack(OWAudioMixer.TrackName.Environment);
		}
		AudioSource addComponent2 = base.gameObject.GetAddComponent<AudioSource>();
		if (addComponent2 != null)
		{
			addComponent2.loop = false;
			addComponent2.playOnAwake = false;
			addComponent2.dopplerLevel = 0f;
			addComponent2.spatialBlend = 1f;
			addComponent2.rolloffMode = AudioRolloffMode.Linear;
			addComponent2.minDistance = 0f;
			addComponent2.maxDistance = 30f;
		}
	}

	private void Awake()
	{
		if (_impactSensor == null)
		{
			_impactSensor = GetComponentInParent<ImpactSensor>();
		}
		_audioSource = GetComponent<OWAudioSource>();
		_impactSensor.OnImpact += OnImpact;
	}

	private void OnDestroy()
	{
		_impactSensor.OnImpact -= OnImpact;
	}

	private void OnImpact(ImpactData impact)
	{
		if (!(Time.time < _lastImpactTime + 0.2f) && !(impact.otherBody.GetMass() < _minCollidingMass))
		{
			_lastImpactTime = Time.time;
			float volume = Mathf.InverseLerp(_minSpeed, _maxSpeed, impact.speed);
			_audioSource.pitch = Random.Range(_minPitch, _maxPitch);
			_audioSource.PlayOneShot(_clipType, volume);
		}
	}
}
