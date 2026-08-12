using UnityEngine;

public class StatueIslandAudioController : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _caveTriggerVolume;

	[SerializeField]
	private AudioVolume[] _audioVolumes;

	private IslandController _islandController;

	private bool _playerInside;

	private bool _islandAirborne;

	private bool _audioPlaying;

	private void Awake()
	{
		_islandController = this.GetRequiredComponent<IslandController>();
		_islandController.OnIslandSplashEvent += OnIslandSplashEvent;
		_islandController.OnIslandEnteredTornadoEvent += OnIslandEnteredTornado;
		_caveTriggerVolume.OnEntry += OnEntry;
		_caveTriggerVolume.OnExit += OnExit;
	}

	private void Start()
	{
		for (int i = 0; i < _audioVolumes.Length; i++)
		{
			_audioVolumes[i].SetVolumeActivation(active: false);
		}
	}

	private void OnDestroy()
	{
		_islandController.OnIslandSplashEvent -= OnIslandSplashEvent;
		_islandController.OnIslandEnteredTornadoEvent -= OnIslandEnteredTornado;
		_caveTriggerVolume.OnEntry -= OnEntry;
		_caveTriggerVolume.OnExit -= OnExit;
	}

	private void UpdateAudio()
	{
		bool flag = _playerInside && !_islandAirborne;
		if (flag != _audioPlaying)
		{
			_audioPlaying = flag;
			for (int i = 0; i < _audioVolumes.Length; i++)
			{
				_audioVolumes[i].SetVolumeActivation(flag);
			}
		}
	}

	private void OnIslandEnteredTornado()
	{
		_islandAirborne = true;
		UpdateAudio();
	}

	private void OnIslandSplashEvent()
	{
		_islandAirborne = false;
		UpdateAudio();
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerCameraDetector"))
		{
			_playerInside = true;
			UpdateAudio();
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerCameraDetector"))
		{
			_playerInside = false;
			UpdateAudio();
		}
	}
}
