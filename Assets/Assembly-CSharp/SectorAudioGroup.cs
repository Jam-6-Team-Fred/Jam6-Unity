using UnityEngine;

[AddComponentMenu("Sectors/Sector Audio Group", 200)]
public class SectorAudioGroup : MonoBehaviour, ISectorGroup
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private float _fadeDuration = 1f;

	private OWAudioSource[] _audioSources;

	private bool _playerInSector;

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	private void Awake()
	{
		_audioSources = GetComponentsInChildren<OWAudioSource>();
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("SectorCullGroup has no specified Sector!", this);
		}
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
	}

	private void Start()
	{
		for (int i = 0; i < _audioSources.Length; i++)
		{
			if (_audioSources[i].playOnAwake)
			{
				Debug.LogError("Sectorized audio sources should not be set to play on awake", _audioSources[i]);
				Debug.Break();
			}
			_audioSources[i].SetLocalVolume(0f);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
	}

	public Sector GetSector()
	{
		return _sector;
	}

	public void SetSector(Sector sector)
	{
		if (!(_sector == sector))
		{
			if ((bool)_sector)
			{
				_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
			}
			_playerInSector = false;
			_sector = sector;
			if ((bool)_sector)
			{
				_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
			}
			OnSectorOccupantsUpdated();
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		if ((bool)_sector)
		{
			bool playerInSector = _playerInSector;
			_playerInSector = _sector.ContainsOccupant(DynamicOccupant.Player);
			if (_playerInSector != playerInSector)
			{
				SetAudioActivation(_playerInSector);
			}
		}
	}

	private void OnStartFastForward()
	{
		for (int i = 0; i < _audioSources.Length; i++)
		{
			_audioSources[i].Stop();
		}
	}

	private void OnEndFastForward()
	{
		if (_sector == null || _sector.ContainsOccupant(DynamicOccupant.Player))
		{
			SetAudioActivation(active: true);
		}
	}

	private void SetAudioActivation(bool active)
	{
		for (int i = 0; i < _audioSources.Length; i++)
		{
			if (active)
			{
				_audioSources[i].FadeIn(_fadeDuration, fadeFromNothing: false, _audioSources[i].GetRandomizePlayheadOnAwake());
			}
			else
			{
				_audioSources[i].FadeOut(_fadeDuration);
			}
		}
	}
}
