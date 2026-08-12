using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RandomParticleBursts : SectoredMonoBehaviour
{
	[SerializeField]
	private float _minDelay = 1f;

	[SerializeField]
	private float _maxDelay = 3f;

	[SerializeField]
	private bool _looping = true;

	private float _lastBurstTime;

	private float _burstDelay = 1f;

	private OWAudioSource _audioSource;

	private ParticleSystem _particleSystem;

	protected override void Awake()
	{
		base.Awake();
		_audioSource = GetComponent<OWAudioSource>();
		_particleSystem = GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = _particleSystem.main;
		main.loop = _looping;
		base.enabled = false;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (flag && !base.enabled)
		{
			_lastBurstTime = Time.time;
		}
		base.enabled = flag;
	}

	private void Update()
	{
		if (Time.time > _lastBurstTime + _burstDelay)
		{
			_burstDelay = Random.Range(_minDelay, _maxDelay);
			_lastBurstTime = Time.time;
			_particleSystem.Play();
			if (_audioSource != null)
			{
				_audioSource.Play();
			}
		}
	}
}
