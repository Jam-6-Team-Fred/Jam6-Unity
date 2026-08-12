using UnityEngine;

public class RiverHazardToggle : SectoredMonoBehaviour
{
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private bool _activePostFlood = true;

	[Space]
	[SerializeField]
	private OWTriggerVolume[] _triggerVolumes = new OWTriggerVolume[0];

	[SerializeField]
	private OWAudioSource[] _audioSources = new OWAudioSource[0];

	[SerializeField]
	private ParticleSystem[] _particles = new ParticleSystem[0];

	[SerializeField]
	private GameObject[] _gameObjects = new GameObject[0];

	[Space]
	[SerializeField]
	private PowerCableAudioController _powerCableAudio;

	[Header("Temp")]
	[SerializeField]
	private Sector _particleSectorOverride;

	private bool _active;

	private bool _playerInSector;

	protected override void Awake()
	{
		base.Awake();
		_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		if (_particleSectorOverride != null)
		{
			_particleSectorOverride.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnParticleSectorOccupantAdded);
			_particleSectorOverride.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnParticleSectorOccupantRemoved);
		}
	}

	private void Start()
	{
		_active = !_activePostFlood;
		for (int i = 0; i < _audioSources.Length; i++)
		{
			_audioSources[i].SetLocalVolume(0f);
		}
		for (int j = 0; j < _triggerVolumes.Length; j++)
		{
			_triggerVolumes[j].SetTriggerActivation(_active);
		}
		for (int k = 0; k < _gameObjects.Length; k++)
		{
			_gameObjects[k].SetActive(_active);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
		if (_particleSectorOverride != null)
		{
			_particleSectorOverride.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnParticleSectorOccupantAdded);
			_particleSectorOverride.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnParticleSectorOccupantRemoved);
		}
	}

	private void OnFloodImpact()
	{
		_active = _activePostFlood;
		for (int i = 0; i < _triggerVolumes.Length; i++)
		{
			_triggerVolumes[i].SetTriggerActivation(_active);
		}
		for (int j = 0; j < _gameObjects.Length; j++)
		{
			_gameObjects[j].SetActive(_active);
		}
		if (_powerCableAudio != null)
		{
			_powerCableAudio.OnFloodImpact();
		}
		UpdateEffects();
	}

	private void UpdateEffects()
	{
		bool flag = (_particleSectorOverride != null && _particleSectorOverride.ContainsOccupant(DynamicOccupant.Player)) || (_particleSectorOverride == null && _playerInSector);
		for (int i = 0; i < _particles.Length; i++)
		{
			if (flag && _active)
			{
				_particles[i].Play();
			}
			else
			{
				_particles[i].Stop();
			}
		}
		for (int j = 0; j < _audioSources.Length; j++)
		{
			if (_playerInSector && _active)
			{
				_audioSources[j].FadeIn(1f, fadeFromNothing: false, randomizePlayhead: true);
			}
			else
			{
				_audioSources[j].FadeOut(1f);
			}
		}
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInSector = true;
			UpdateEffects();
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInSector = false;
			UpdateEffects();
		}
	}

	private void OnParticleSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			UpdateEffects();
		}
	}

	private void OnParticleSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			UpdateEffects();
		}
	}
}
