using UnityEngine;

public class FloodImpactEffect : MonoBehaviour
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private ParticleSystem _particleSystem;

	[Space]
	[SerializeField]
	private AudioType _audioType;

	[SerializeField]
	private float _delay;

	private float _playTime;

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	private void Awake()
	{
		if (_sector == null)
		{
			_sector = GetComponentInParent<Sector>();
		}
		_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
	}

	private void OnFloodImpact()
	{
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			if (_delay > 0f)
			{
				base.enabled = true;
				_playTime = Time.time + _delay;
			}
			else
			{
				PlayEffects();
			}
		}
	}

	private void Update()
	{
		if (Time.time >= _playTime)
		{
			if (_sector.ContainsOccupant(DynamicOccupant.Player))
			{
				PlayEffects();
			}
			base.enabled = false;
		}
	}

	private void PlayEffects()
	{
		if (_audioSource != null)
		{
			_audioSource.PlayOneShot(_audioType);
		}
		if (_particleSystem != null)
		{
			_particleSystem.Play();
		}
	}
}
