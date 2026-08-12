using UnityEngine;

public class ImpactAudioPool : MonoBehaviour
{
	[SerializeField]
	private AudioType _clipType = AudioType.DefaultPropImpact;

	[SerializeField]
	private float _minMass = 0.1f;

	[SerializeField]
	private float _minSpeed = 1f;

	[SerializeField]
	private float _maxSpeed = 10f;

	[SerializeField]
	private ImpactSensor _impactSensor;

	[SerializeField]
	private OWAudioSource[] _audioSources;

	private int _sourceIndex;

	private float _lastImpactTime;

	private void Reset()
	{
		_impactSensor = GetComponentInParent<ImpactSensor>();
	}

	private void Awake()
	{
		_impactSensor.OnImpact += OnImpact;
	}

	private void OnDestroy()
	{
		_impactSensor.OnImpact -= OnImpact;
	}

	private void OnImpact(ImpactData impact)
	{
		if (Time.time < _lastImpactTime + 0.2f || impact.otherBody.GetMass() < _minMass)
		{
			return;
		}
		_lastImpactTime = Time.time;
		if (_audioSources.Length != 0)
		{
			float volume = Mathf.InverseLerp(_minSpeed, _maxSpeed, impact.speed);
			_audioSources[_sourceIndex].transform.position = impact.point;
			_audioSources[_sourceIndex].PlayOneShot(_clipType, volume);
			_sourceIndex++;
			if (_sourceIndex >= _audioSources.Length)
			{
				_sourceIndex = 0;
			}
		}
	}
}
