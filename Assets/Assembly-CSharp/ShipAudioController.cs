using UnityEngine;

public class ShipAudioController : MonoBehaviour
{
	[Space(10f)]
	[SerializeField]
	private ShipElectricalComponent _shipElectrics;

	[Space(10f)]
	[SerializeField]
	private OWAudioSource _alarmSource;

	[SerializeField]
	private OWAudioSource _ambientSource;

	[SerializeField]
	private OWAudioSource _fluidVolumeSource;

	[SerializeField]
	private OWAudioSource _loudShipSource;

	[SerializeField]
	private OWAudioSource _hatchSource;

	[SerializeField]
	private OWAudioSource _cockpitSource;

	[SerializeField]
	private OWAudioSource _glassCrackSource;

	[SerializeField]
	private OWAudioSource _probeScreenSource;

	[SerializeField]
	private OWAudioSource _signalscopeSource;

	[SerializeField]
	private OWAudioSource _cockpitInstrumentsAudioSource;

	[SerializeField]
	private OWAudioSource _cockpitInstrumentsAudioSource2;

	[SerializeField]
	private OWAudioSource _ejectCoverSource;

	[Space]
	[SerializeField]
	private OWAudioSource[] _hullImpactSources;

	private AudioManager _audioManager;

	private FluidDetector _fluidDetector;

	private AudioClip _consoleReadoutStart;

	private AudioClip _consoleReadoutLoop;

	private float _lastImpactTime;

	private void Awake()
	{
		GlobalMessenger.AddListener("EnterShip", PlayShipAmbient);
		GlobalMessenger.AddListener("ExitShip", StopShipAmbient);
		if (_shipElectrics != null)
		{
			_shipElectrics.OnDamaged += OnElectricsDamaged;
			_shipElectrics.OnRepaired += OnElectricsRepaired;
		}
	}

	protected void Start()
	{
		_audioManager = Locator.GetAudioManager();
		_alarmSource.clip = _audioManager.GetSingleAudioClip(AudioType.ShipCockpitMasterAlarm_LP);
		_ambientSource.AssignAudioLibraryClip(AudioType.ShipCabinAmbience);
		_alarmSource.loop = true;
		_ambientSource.loop = true;
		_fluidDetector = Locator.GetShipDetector().GetComponent<FluidDetector>();
		_fluidDetector.OnEnterFluidType += OnEnterFluidType;
		_fluidDetector.OnExitFluidType += OnExitFluidType;
		_consoleReadoutStart = _audioManager.GetSingleAudioClip(AudioType.ShipCockpitConsoleReadout_In);
		_consoleReadoutLoop = _audioManager.GetSingleAudioClip(AudioType.ShipCockpitConsoleReadout_LP);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterShip", PlayShipAmbient);
		GlobalMessenger.RemoveListener("ExitShip", StopShipAmbient);
		_fluidDetector.OnEnterFluidType -= OnEnterFluidType;
		_fluidDetector.OnExitFluidType -= OnExitFluidType;
	}

	public void PlaySignalscopeInPosition()
	{
		_signalscopeSource.PlayOneShot(AudioType.ShipCockpitScopeScreenKachunk);
	}

	public void PlayBuckle()
	{
		_cockpitSource.PlayOneShot(AudioType.ShipCockpitBuckleUp);
	}

	public void PlayUnbuckle()
	{
		_cockpitSource.PlayOneShot(AudioType.ShipCockpitUnbuckle);
	}

	public void PlayEject()
	{
		_cockpitSource.PlayOneShot(AudioType.ShipCockpitEject);
	}

	public void PlayRaiseEjectCover()
	{
		_ejectCoverSource.PlayOneShot(AudioType.ShipCockpitLandingCamActivate);
	}

	public void PlayProbeScreenMotor()
	{
		_probeScreenSource.clip = _audioManager.GetSingleAudioClip(AudioType.ShipCockpitProbeCameraScreenRotation);
		_probeScreenSource.Play();
	}

	public void StopProbeScreenMotor()
	{
		_probeScreenSource.Stop();
	}

	public void PlaySigScopeSlide()
	{
		_signalscopeSource.clip = _audioManager.GetSingleAudioClip(AudioType.ShipCockpitScopeScreenSlide_LP);
		_signalscopeSource.Play();
	}

	public void StopSigScopeSlide()
	{
		_signalscopeSource.Stop();
	}

	public void PlayHeadlightOn()
	{
		_cockpitSource.PlayOneShot(AudioType.ShipCockpitHeadlightsOn);
	}

	public void PlayHeadlightOff()
	{
		_cockpitSource.PlayOneShot(AudioType.ShipCockpitHeadlightsOff);
	}

	public void PlayAutopilotOn()
	{
		_cockpitSource.PlayOneShot(AudioType.ShipCockpitAutopilotActivate);
	}

	public void PlayAutopilotOff()
	{
		_cockpitSource.PlayOneShot(AudioType.ShipCockpitAutopilotDeactivate);
	}

	public void PlayLandingCamOn(AudioType initAmbientAudioType)
	{
		if (_cockpitInstrumentsAudioSource.clip != _audioManager.GetSingleAudioClip(initAmbientAudioType))
		{
			_cockpitInstrumentsAudioSource.clip = _audioManager.GetSingleAudioClip(initAmbientAudioType);
		}
		_cockpitInstrumentsAudioSource.PlayOneShot(AudioType.ShipCockpitLandingCamActivate);
	}

	public void PlayLandingCamOff()
	{
		_cockpitInstrumentsAudioSource.Stop();
		_cockpitInstrumentsAudioSource.PlayOneShot(AudioType.ShipCockpitLandingCamDeactivate);
	}

	public void PlayLandingCamAmbient(float delay = 0f)
	{
		_cockpitInstrumentsAudioSource.loop = true;
		_cockpitInstrumentsAudioSource.clip = _audioManager.GetSingleAudioClip(AudioType.ShipCockpitLandingCamAmbient_LP);
		_cockpitInstrumentsAudioSource.PlayDelayed(delay);
	}

	public void PlayLandingCamStatic(float delay = 0f)
	{
		_cockpitInstrumentsAudioSource.loop = true;
		_cockpitInstrumentsAudioSource.clip = _audioManager.GetSingleAudioClip(AudioType.ShipCockpitLandingCamStatic_LP);
		_cockpitInstrumentsAudioSource.PlayDelayed(delay);
	}

	public void PlayOpenHatch()
	{
		_hatchSource.Stop();
		_hatchSource.clip = _audioManager.GetSingleAudioClip(AudioType.ShipHatchOpen);
		_hatchSource.Play();
	}

	public void PlayCloseHatch()
	{
		_hatchSource.Stop();
		_hatchSource.clip = _audioManager.GetSingleAudioClip(AudioType.ShipHatchClose);
		_hatchSource.Play();
	}

	public void PlayLandingPadImpact(OWAudioSource padSource, float impactSpeed)
	{
		if (!(impactSpeed >= 30f))
		{
			padSource.pitch = 1f + Random.Range(-0.2f, 0.2f);
			float volume = Mathf.InverseLerp(0f, 15f, impactSpeed);
			padSource.PlayOneShot(AudioType.Ship_LandingPad_Hard, volume);
		}
	}

	public void StartCockpitTextAudio()
	{
		if (!IsCockpitTextSFXPlaying())
		{
			_cockpitInstrumentsAudioSource.clip = _consoleReadoutStart;
			_cockpitInstrumentsAudioSource2.clip = _consoleReadoutLoop;
			_cockpitInstrumentsAudioSource.loop = false;
			_cockpitInstrumentsAudioSource2.loop = true;
			_cockpitInstrumentsAudioSource.Play();
			_cockpitInstrumentsAudioSource2.PlayDelayed(0.8f * _consoleReadoutStart.length);
		}
	}

	public void StopCockpitTextAudio()
	{
		_cockpitInstrumentsAudioSource.Stop();
		_cockpitInstrumentsAudioSource2.Stop();
	}

	public bool IsCockpitTextSFXPlaying()
	{
		if (!(_cockpitInstrumentsAudioSource.clip == _consoleReadoutStart) || !_cockpitInstrumentsAudioSource.isPlaying)
		{
			if (_cockpitInstrumentsAudioSource2.clip == _consoleReadoutLoop)
			{
				return _cockpitInstrumentsAudioSource2.isPlaying;
			}
			return false;
		}
		return true;
	}

	public void PlayAlarm()
	{
		if (_alarmSource != null && !_alarmSource.isPlaying)
		{
			_alarmSource.Play();
		}
	}

	public void StopAlarm()
	{
		if (_alarmSource != null)
		{
			_alarmSource.Stop();
		}
	}

	public void OnElectricsDamaged(ShipComponent shipComponent)
	{
		StopShipAmbient();
	}

	public void OnElectricsRepaired(ShipComponent shipComponent)
	{
		if (PlayerState.IsInsideShip())
		{
			PlayShipAmbient();
		}
	}

	public void PlayGlassCrackClip()
	{
		_glassCrackSource.PlayOneShot(AudioType.ShipDamageCockpitGlassCrack);
	}

	public float PlayShipExplodeClip()
	{
		return _loudShipSource.PlayOneShot(AudioType.ShipDamageShipExplosion).length;
	}

	public void PlayImpactAtPosition(AudioType audioType, float volume, float pitch, Vector3 worldPos)
	{
		if (Time.time - _lastImpactTime < 0.5f)
		{
			return;
		}
		for (int i = 0; i < _hullImpactSources.Length; i++)
		{
			if (!_hullImpactSources[i].isPlaying)
			{
				_lastImpactTime = Time.time;
				_hullImpactSources[i].transform.position = worldPos;
				_hullImpactSources[i].SetLocalVolume(volume);
				_hullImpactSources[i].pitch = pitch;
				_hullImpactSources[i].clip = _audioManager.GetSingleAudioClip(audioType);
				_hullImpactSources[i].Play();
				break;
			}
		}
	}

	private void PlayShipAmbient()
	{
		if (_ambientSource != null && !_shipElectrics.isDamaged)
		{
			_ambientSource.FadeIn(1f, fadeFromNothing: true);
		}
	}

	private void StopShipAmbient()
	{
		if (_ambientSource != null)
		{
			_ambientSource.FadeOut(1f);
		}
	}

	private void OnEnterFluidType(FluidVolume.Type type)
	{
		if (type == FluidVolume.Type.SAND)
		{
			_fluidVolumeSource.FadeIn(0.5f);
		}
	}

	private void OnExitFluidType(FluidVolume.Type type)
	{
		if (type == FluidVolume.Type.SAND)
		{
			_fluidVolumeSource.FadeOut(0.5f);
		}
	}
}
