using UnityEngine;

public class DreamWorldAudioController : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource _waveSource;

	[SerializeField]
	private OWAudioSource _solarSailSource;

	[SerializeField]
	private OWAudioSource _libraryMusicSource;

	[SerializeField]
	private AnimationCurve _waveVolumeCurve;

	[SerializeField]
	private AudioVolume _simulationAmbience;

	[SerializeField]
	private AudioVolume _loadingTunnelAmbience;

	[SerializeField]
	private DreamWorldController _dreamWorldController;

	[SerializeField]
	private OWTriggerVolume[] _libraryMusicTriggers;

	[SerializeField]
	private DreamRiverPathAudioController _riverPathAudioController;

	[SerializeField]
	private DreamRiverPathAudioController _lakePathAudioController;

	private OWRingRiverCollider _ringworldRiverCollider;

	private float _waveFadeOutLerp = 1f;

	private float _dreamFireRiverLerpPos = -1f;

	private float _waveFadeFraction;

	private bool _solarSailOpening;

	private bool _hasPlayedLibraryMusic;

	private void Awake()
	{
		Locator.RegisterDreamWorldAudioController(this);
		_dreamWorldController.OnEnterLanternBounds += new OWEvent.OWCallback(OnEnterLanternBounds);
		_dreamWorldController.OnExitLanternBounds += new OWEvent.OWCallback(OnExitLanternBounds);
		for (int i = 0; i < _libraryMusicTriggers.Length; i++)
		{
			_libraryMusicTriggers[i].OnEntry += OnEnterLibraryMusicTrigger;
		}
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void Start()
	{
		base.enabled = false;
		_oneShotSource.SetLocalVolume(0f);
		_waveSource.SetLocalVolume(0f);
		_solarSailSource.SetLocalVolume(0f);
		_simulationAmbience.SetVolumeActivation(active: false);
		_loadingTunnelAmbience.SetVolumeActivation(active: false);
		if (Locator.GetRingRiverFluidVolume() != null)
		{
			_ringworldRiverCollider = Locator.GetRingRiverFluidVolume().GetComponent<OWRingRiverCollider>();
		}
	}

	private void OnDestroy()
	{
		_dreamWorldController.OnEnterLanternBounds -= new OWEvent.OWCallback(OnEnterLanternBounds);
		_dreamWorldController.OnExitLanternBounds -= new OWEvent.OWCallback(OnExitLanternBounds);
		for (int i = 0; i < _libraryMusicTriggers.Length; i++)
		{
			_libraryMusicTriggers[i].OnEntry -= OnEnterLibraryMusicTrigger;
		}
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.RemoveListener("DamBroken", OnDamBroken);
	}

	public RiverPathAudioController GetRiverPathAudioController()
	{
		return _riverPathAudioController;
	}

	public RiverPathAudioController GetLakePathAudioController()
	{
		return _lakePathAudioController;
	}

	public void OnEnterLoadTunnel()
	{
		_oneShotSource.PlayOneShot(AudioType.LoadingZone_Enter);
		_loadingTunnelAmbience.SetVolumeActivation(active: true);
	}

	public void OnExitLoadTunnel(bool exitByFalling = false)
	{
		_oneShotSource.PlayOneShot(exitByFalling ? AudioType.LoadingZone_GlitchOut : AudioType.LoadingZone_Exit);
		_loadingTunnelAmbience.SetVolumeActivation(active: false);
	}

	public void SetWaveAudioProperties(float waveFadeOutDegrees)
	{
		_waveFadeOutLerp = Mathf.InverseLerp(0f, 360f, waveFadeOutDegrees);
	}

	public void PlaySingleAlarmChime(int index, float volume)
	{
		_oneShotSource.PlayOneShot(AudioType.AlarmChime_DW, index, volume);
	}

	public void PlayTowerOneShot(AudioType audioType)
	{
		if (CanHearRingWorldAudio(DreamArrivalPoint.Location.Zone2))
		{
			_oneShotSource.PlayOneShot(audioType);
		}
	}

	public void OnStationLightFlicker()
	{
		if (CanHearRingWorldAudio())
		{
			_oneShotSource.PlayOneShot(AudioType.StationFlicker_DW);
		}
	}

	public void OnSolarSailStart()
	{
		_solarSailOpening = true;
		if (CanHearRingWorldAudio())
		{
			_oneShotSource.PlayOneShot(AudioType.SolarSail_DW_Start);
			_solarSailSource.FadeIn(0.2f);
		}
	}

	public void OnSolarSailStop()
	{
		_solarSailOpening = false;
		_solarSailSource.FadeOut(0.2f);
		if (CanHearRingWorldAudio())
		{
			_oneShotSource.PlayOneShot(AudioType.SolarSail_DW_End);
			RumbleManager.PlayStationShudder(0.4f);
		}
	}

	private bool CanHearRingWorldAudio()
	{
		if (_dreamWorldController.IsInDream())
		{
			return !PlayerState.IsResurrected();
		}
		return false;
	}

	private bool CanHearRingWorldAudio(DreamArrivalPoint.Location location)
	{
		if (CanHearRingWorldAudio())
		{
			return _dreamWorldController.IsPlayerSleepingAtLocation(location);
		}
		return false;
	}

	private void Update()
	{
		if (PlayerState.IsResurrected())
		{
			return;
		}
		if (_ringworldRiverCollider != null && _ringworldRiverCollider.GetFloodLerp() > 0f && _dreamFireRiverLerpPos >= 0f)
		{
			float floodLerp = _ringworldRiverCollider.GetFloodLerp();
			float num = Mathf.Abs(floodLerp - _dreamFireRiverLerpPos);
			if (num > 0.5f)
			{
				num = 1f - num;
			}
			float target = 1f;
			if (floodLerp >= _waveFadeOutLerp || !_dreamWorldController.IsInDream())
			{
				target = 0f;
			}
			_waveFadeFraction = Mathf.MoveTowards(_waveFadeFraction, target, Time.deltaTime / 2f);
			float num2 = _waveVolumeCurve.Evaluate(num) * _waveFadeFraction;
			_waveSource.SetLocalVolume(num2);
			if (num2 > 0f && !_waveSource.isPlaying)
			{
				_waveSource.Play();
			}
			else if (num2 <= 0f && _waveSource.isPlaying)
			{
				_waveSource.Stop();
			}
		}
		if (!_dreamWorldController.IsInDream() && !_waveSource.isPlaying)
		{
			base.enabled = false;
		}
	}

	private void OnDamBroken()
	{
		if (CanHearRingWorldAudio())
		{
			_oneShotSource.PlayOneShot(AudioType.DamBreak_DW_Base);
			float scalar = 0.2f;
			if (_dreamWorldController.IsPlayerSleepingAtLocation(DreamArrivalPoint.Location.Zone1))
			{
				scalar = 0.4f;
			}
			RumbleManager.PlayDamBreak(scalar);
		}
	}

	private void OnEnterLanternBounds()
	{
		if (!_dreamWorldController.IsExitingDream())
		{
			_oneShotSource.PlayOneShot(AudioType.Simulation_Exit);
		}
		_simulationAmbience.SetVolumeActivation(active: false);
	}

	private void OnExitLanternBounds()
	{
		_oneShotSource.PlayOneShot(AudioType.Simulation_Enter);
		_simulationAmbience.SetVolumeActivation(active: true);
	}

	private void OnEnterDreamWorld()
	{
		base.enabled = true;
		_oneShotSource.FadeIn(1f);
		if (_solarSailOpening && CanHearRingWorldAudio())
		{
			_solarSailSource.FadeIn(1f);
		}
		if (_dreamWorldController.GetDreamCampfire() != null && _ringworldRiverCollider != null)
		{
			_dreamFireRiverLerpPos = _ringworldRiverCollider.WorldPositionToRiverLerp(_dreamWorldController.GetDreamCampfire().transform.position);
		}
		GlobalMessenger.AddListener("DamBroken", OnDamBroken);
	}

	private void OnExitDreamWorld()
	{
		_oneShotSource.FadeOut(1f);
		_solarSailSource.FadeOut(0.2f);
		_simulationAmbience.SetVolumeActivation(active: false);
		_loadingTunnelAmbience.SetVolumeActivation(active: false);
		GlobalMessenger.RemoveListener("DamBroken", OnDamBroken);
	}

	private void OnEnterLibraryMusicTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") && !_hasPlayedLibraryMusic && _libraryMusicSource != null && Locator.GetPlayerBody().GetVelocity().y < 0f)
		{
			_libraryMusicSource.Play();
			_hasPlayedLibraryMusic = true;
		}
	}
}
