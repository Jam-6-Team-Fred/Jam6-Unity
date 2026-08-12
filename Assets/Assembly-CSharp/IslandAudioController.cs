using UnityEngine;

public class IslandAudioController : SectoredMonoBehaviour
{
	[SerializeField]
	private OWAudioSource _islandAudioSource;

	[SerializeField]
	private GameObject _shoreAudioRoot;

	[SerializeField]
	private AudioVolume[] _audioVolumes;

	private IslandController _islandController;

	private OWAudioSource[] _shorelineAudio;

	private bool _playerInSector;

	private bool _islandAirborne;

	private bool _shoreAudioPlaying;

	protected override void Awake()
	{
		base.Awake();
		if (_shoreAudioRoot != null)
		{
			_shorelineAudio = _shoreAudioRoot.GetComponentsInChildren<OWAudioSource>();
		}
		else
		{
			_shorelineAudio = new OWAudioSource[0];
		}
		_islandController = this.GetRequiredComponent<IslandController>();
		_islandController.OnIslandSplashEvent += OnIslandSplashEvent;
		_islandController.OnIslandEnteredTornadoEvent += OnIslandEnteredTornado;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_islandController.OnIslandSplashEvent -= OnIslandSplashEvent;
		_islandController.OnIslandEnteredTornadoEvent -= OnIslandEnteredTornado;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		_playerInSector = _sector.ContainsOccupant(DynamicOccupant.Player);
		UpdateShoreAudio();
	}

	private void UpdateShoreAudio()
	{
		bool flag = !_islandAirborne && _playerInSector;
		if (flag == _shoreAudioPlaying)
		{
			return;
		}
		_shoreAudioPlaying = flag;
		for (int i = 0; i < _shorelineAudio.Length; i++)
		{
			if (flag)
			{
				_shorelineAudio[i].FadeIn(1f);
			}
			else
			{
				_shorelineAudio[i].FadeOut(1f);
			}
		}
	}

	private void OnIslandEnteredTornado()
	{
		if (_islandAudioSource != null)
		{
			_islandAudioSource.PlayOneShot(AudioType.GD_IslandLiftedByTornado);
		}
		for (int i = 0; i < _audioVolumes.Length; i++)
		{
			_audioVolumes[i].SetVolumeActivation(active: false);
		}
		_islandAirborne = true;
		UpdateShoreAudio();
	}

	private void OnIslandSplashEvent()
	{
		if (_islandAudioSource != null)
		{
			_islandAudioSource.PlayOneShot(AudioType.GD_IslandSplash);
		}
		for (int i = 0; i < _audioVolumes.Length; i++)
		{
			_audioVolumes[i].SetVolumeActivation(active: true);
		}
		_islandAirborne = false;
		UpdateShoreAudio();
	}
}
