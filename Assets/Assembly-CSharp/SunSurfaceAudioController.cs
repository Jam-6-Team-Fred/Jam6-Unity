using UnityEngine;

[RequireComponent(typeof(OWAudioSource))]
public class SunSurfaceAudioController : SectoredMonoBehaviour
{
	[SerializeField]
	private SunController _sunController;

	[SerializeField]
	private OWAudioSource _sunStationMusicSource;

	[SerializeField]
	private OWTriggerVolume _sunStationVolume;

	private OWAudioSource _audioSource;

	private float _fade;

	private bool _playerInSunStation;

	private void Start()
	{
		if (_sunStationVolume != null)
		{
			_sunStationVolume.OnEntry += OnEnterSunStation;
			_sunStationVolume.OnExit += OnExitSunStation;
		}
		_audioSource = GetComponent<OWAudioSource>();
		_audioSource.SetLocalVolume(0f);
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_sunStationVolume != null)
		{
			_sunStationVolume.OnEntry -= OnEnterSunStation;
			_sunStationVolume.OnExit -= OnExitSunStation;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = base.enabled;
		base.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
		if (base.enabled && !flag)
		{
			_audioSource.Play();
		}
		else if (!base.enabled && flag)
		{
			_fade = 0f;
			_audioSource.Stop();
		}
	}

	private void OnEnterSunStation(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInSunStation = true;
		}
	}

	private void OnExitSunStation(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInSunStation = false;
		}
	}

	private void Update()
	{
		float target = (_playerInSunStation ? 0.2f : 1f);
		_fade = Mathf.MoveTowards(_fade, target, Time.deltaTime * 0.2f);
		float value = Mathf.Max(0f, Vector3.Distance(Locator.GetPlayerCamera().transform.position, base.transform.position) - _sunController.GetSurfaceRadius());
		float num = Mathf.InverseLerp(1600f, 100f, value);
		_audioSource.SetLocalVolume(num * num * _fade);
	}
}
