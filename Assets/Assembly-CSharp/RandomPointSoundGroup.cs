using UnityEngine;

public class RandomPointSoundGroup : MonoBehaviour
{
	[SerializeField]
	private AudioType _pointAudioType;

	[SerializeField]
	private Vector2 _delayRange;

	[SerializeField]
	private float _minPlayDistance;

	[Space]
	[SerializeField]
	private AudioVolume _audioVolume;

	[SerializeField]
	private OWAudioSource[] _pointSources;

	private float _nextPlayTime;

	[ContextMenu("Populate Point Sources", false)]
	private void PopulatePointSources()
	{
		_pointSources = GetComponentsInChildren<OWAudioSource>();
	}

	private void Awake()
	{
		if (_audioVolume != null)
		{
			_audioVolume.OnAudioPlay += new OWEvent.OWCallback(OnAudioVolumePlay);
			_audioVolume.OnAudioStop += new OWEvent.OWCallback(OnAudioVolumeStop);
		}
	}

	private void Start()
	{
		if (_audioVolume != null)
		{
			base.enabled = _audioVolume.IsActive();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnDestroy()
	{
		if (_audioVolume != null)
		{
			_audioVolume.OnAudioPlay -= new OWEvent.OWCallback(OnAudioVolumePlay);
			_audioVolume.OnAudioStop -= new OWEvent.OWCallback(OnAudioVolumeStop);
		}
	}

	private void Update()
	{
		if (Time.time > _nextPlayTime)
		{
			OWAudioSource oWAudioSource = _pointSources[Random.Range(0, _pointSources.Length)];
			if ((oWAudioSource.transform.position - Locator.GetPlayerCamera().transform.position).sqrMagnitude > _minPlayDistance * _minPlayDistance)
			{
				oWAudioSource.PlayOneShot(_pointAudioType);
				_nextPlayTime = Time.time + Random.Range(_delayRange.x, _delayRange.y);
			}
		}
	}

	private void OnAudioVolumePlay()
	{
		_nextPlayTime = Time.time + Random.Range(_delayRange.x, _delayRange.y);
		base.enabled = true;
	}

	private void OnAudioVolumeStop()
	{
		base.enabled = false;
	}
}
