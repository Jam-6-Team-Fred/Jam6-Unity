using UnityEngine;

public class RingWorldController : MonoBehaviour
{
	[SerializeField]
	private OWRigidbody _ringWorldBody;

	[SerializeField]
	private OWRigidbody _staticRingBody;

	[SerializeField]
	private OWTriggerVolume[] _enterOnWakeVolumes;

	[SerializeField]
	private OWTriggerVolume _insideRingWorldVolume;

	[SerializeField]
	private RingRiverController _riverController;

	[SerializeField]
	private RingRiverPathAudioController _riverPathAudioController;

	[Header("Timers")]
	[SerializeField]
	private float _sailDeployTime = 400f;

	[SerializeField]
	private float _lightFlickerTime = 401f;

	[SerializeField]
	private float _departTime = 405f;

	[SerializeField]
	private float _damDamageTime = 410f;

	[SerializeField]
	private float _damBreakTime = 780f;

	[SerializeField]
	private float _lighthouseCollapseTime = 1220f;

	[Header("Solar Sail")]
	[SerializeField]
	private Animation[] _solarSailAnimations;

	[SerializeField]
	private OWCollider _solarSailClosedCollider;

	[SerializeField]
	private OWCollider _solarSailOpenCollider;

	[SerializeField]
	private GameObject _solarSailClosedProxy;

	[SerializeField]
	private GameObject _solarSailOpenProxy;

	[SerializeField]
	private float _sailDeployDuration = 12f;

	[SerializeField]
	private Vector3 _departDirection = Vector3.up;

	[SerializeField]
	private float _departAcceleration = 10f;

	[SerializeField]
	private float _interiorAccelFactor;

	[Header("Dam")]
	[SerializeField]
	private RingWorldFlickerController _flickerController;

	[SerializeField]
	private RingWorldScreenController _screenController;

	[SerializeField]
	private DamDestructionController _damController;

	[SerializeField]
	private LighthouseController _lighthouseController;

	[SerializeField]
	private OWTriggerVolume _zone1TriggerVolume;

	[SerializeField]
	private float _damLifeExtendDuration = 60f;

	[Header("AudioSources")]
	[SerializeField]
	private OWAudioSource _solarSailOneShot;

	[SerializeField]
	private OWAudioSource _solarSailLooping;

	[SerializeField]
	private OWAudioSource _flickerOneShot;

	[SerializeField]
	private OWAudioSource _damOneShotAudio_Far;

	private DetachableFragment[] _damFragments;

	private bool _playerInsideRingWorld;

	private bool _probeInsideRingWorld;

	private bool _sailsDeploying;

	private bool _sailsDeployed;

	private bool _departing;

	private bool _damDamaged;

	private bool _lightFlickeringOut;

	private float _lightResetTime;

	private bool _lightFlickeringIn;

	private bool _damBroken;

	private bool _lighthouseCollapsed;

	public OWEvent OnDamDamaged = new OWEvent(16);

	public OWEvent OnDamBreak = new OWEvent(16);

	public OWEvent OnPlayerEnter = new OWEvent(8);

	public OWEvent OnPlayerExit = new OWEvent(8);

	public OWEvent OnProbeEnter = new OWEvent(8);

	public OWEvent OnProbeExit = new OWEvent(8);

	public float sailDeployTime => _sailDeployTime;

	public float departTime => _departTime;

	public float damDamageTime => _damDamageTime;

	public float damBreakTime => _damBreakTime;

	public bool isPlayerInside => _playerInsideRingWorld;

	public bool isProbeInside => _probeInsideRingWorld;

	public bool areSailsDeployed => _sailsDeployed;

	public bool isDeparting => _departing;

	public bool isDamDamaged => _damDamaged;

	public bool isDamBroken => _damBroken;

	public bool hasLighthouseCollapsed => _lighthouseCollapsed;

	public OWRigidbody GetRingWorldBody()
	{
		return _ringWorldBody;
	}

	public LighthouseController GetLighthouseController()
	{
		return _lighthouseController;
	}

	public RiverPathAudioController GetRiverPathAudioController()
	{
		return _riverPathAudioController;
	}

	private void Awake()
	{
		Locator.RegisterRingWorldController(this);
		_insideRingWorldVolume.OnEntry += OnEnterInsideVolume;
		_insideRingWorldVolume.OnExit += OnExitInsideVolume;
		_departDirection.Normalize();
	}

	private void Start()
	{
		for (int i = 0; i < _solarSailAnimations.Length; i++)
		{
			_solarSailAnimations[i].Play();
			_solarSailAnimations[i].Sample();
			_solarSailAnimations[i].Stop();
		}
		_solarSailClosedCollider.SetActivation(active: true);
		_solarSailOpenCollider.SetActivation(active: false);
		_solarSailClosedProxy.SetActive(value: true);
		_solarSailOpenProxy.SetActive(value: false);
		_solarSailLooping.SetLocalVolume(0f);
		if (!Locator.GetShipLogManager().IsFactRevealed("IP_RING_WORLD_X1") && _zone1TriggerVolume != null)
		{
			_zone1TriggerVolume.OnEntry += OnZone1Entry;
		}
	}

	private void OnDestroy()
	{
		_insideRingWorldVolume.OnEntry -= OnEnterInsideVolume;
		_insideRingWorldVolume.OnExit -= OnExitInsideVolume;
		if (_zone1TriggerVolume != null)
		{
			_zone1TriggerVolume.OnEntry -= OnZone1Entry;
		}
	}

	public void OnExitDreamWorld()
	{
		for (int i = 0; i < _enterOnWakeVolumes.Length; i++)
		{
			_enterOnWakeVolumes[i].AddObjectToVolume(Locator.GetPlayerDetector());
			_enterOnWakeVolumes[i].AddObjectToVolume(Locator.GetPlayerCameraDetector());
		}
	}

	public void ApplyDepartureAcceleration(OWRigidbody body)
	{
		if (_departing)
		{
			Vector3 acceleration = _ringWorldBody.transform.TransformDirection(_departDirection) * _departAcceleration;
			if (body.GetAttachedForceDetector().CompareNameMask(Detector.Name.Player | Detector.Name.Probe | Detector.Name.Ship))
			{
				acceleration *= _interiorAccelFactor;
			}
			body.AddAcceleration(acceleration);
		}
	}

	private void Update()
	{
		if (!_sailsDeploying && !_sailsDeployed && TimeLoop.GetSecondsElapsed() > _sailDeployTime)
		{
			BeginDeploySails();
		}
		else if (!_lightFlickeringOut && TimeLoop.GetSecondsElapsed() > _lightFlickerTime)
		{
			BeginLightFlicker();
		}
		else if (!_departing && _sailsDeployed && TimeLoop.GetSecondsElapsed() > _departTime)
		{
			BeginDepartSolarSystem();
		}
		else if (!_damDamaged && !_damBroken && TimeLoop.GetSecondsElapsed() > _damDamageTime)
		{
			DamageDam();
		}
		else if (!_damBroken && _damDamaged && TimeLoop.GetSecondsElapsed() > _damBreakTime)
		{
			BreakDam();
		}
		else if (!_lighthouseCollapsed && TimeLoop.GetSecondsElapsed() > _lighthouseCollapseTime)
		{
			CollapseLighthouse();
		}
		if (_sailsDeploying && !_sailsDeployed && TimeLoop.GetSecondsElapsed() >= _sailDeployTime + _sailDeployDuration)
		{
			if (_playerInsideRingWorld)
			{
				_solarSailOneShot.PlayOneShot(AudioType.SolarSail_RW_End);
				_solarSailLooping.FadeOut(0.2f);
				_damOneShotAudio_Far.PlayOneShot(AudioType.StationShudder_DW);
				if (Locator.GetPlayerController().GetGroundBody() == _ringWorldBody)
				{
					RumbleManager.PlayStationShudder(1f);
				}
			}
			if (Locator.GetDreamWorldAudioController() != null)
			{
				Locator.GetDreamWorldAudioController().OnSolarSailStop();
			}
			_solarSailClosedCollider.SetActivation(active: false);
			_solarSailOpenCollider.SetActivation(active: true);
			_solarSailClosedProxy.SetActive(value: false);
			_solarSailOpenProxy.SetActive(value: true);
			_sailsDeployed = true;
		}
		if (!_lightFlickeringOut || _lightFlickeringIn || !(Time.timeSinceLevelLoad >= _lightResetTime))
		{
			return;
		}
		_lightFlickeringIn = true;
		if (_flickerController != null)
		{
			_flickerController.SetFlickerScale(0.5f);
			_flickerController.Flicker(1f, 2f, 0.1f, 0.2f, 0.3f);
			DreamWorldStarsController componentInChildren = Locator.GetSkyboxTransform().GetComponentInChildren<DreamWorldStarsController>();
			if (componentInChildren != null)
			{
				componentInChildren.StartFlicker(_flickerController);
			}
		}
	}

	private void FixedUpdate()
	{
		if (_departing)
		{
			Vector3 acceleration = _ringWorldBody.transform.TransformDirection(_departDirection) * _departAcceleration;
			_ringWorldBody.AddAcceleration(acceleration);
			_staticRingBody.AddAcceleration(acceleration);
		}
	}

	private void BeginDeploySails()
	{
		_sailsDeploying = true;
		for (int i = 0; i < _solarSailAnimations.Length; i++)
		{
			_solarSailAnimations[i].Play();
			_solarSailAnimations[i].Sample();
		}
		if (_playerInsideRingWorld)
		{
			_solarSailOneShot.PlayOneShot(AudioType.SolarSail_RW_Start);
			_solarSailLooping.FadeInToLibraryVolume(0.2f);
		}
		if (Locator.GetDreamWorldAudioController() != null)
		{
			Locator.GetDreamWorldAudioController().OnSolarSailStart();
		}
	}

	private void BeginLightFlicker()
	{
		float num = 0.6f;
		if (_flickerController != null)
		{
			_flickerController.Flicker(0.5f, num, 0.1f, 0.04f, 0.08f);
			DreamWorldStarsController componentInChildren = Locator.GetSkyboxTransform().GetComponentInChildren<DreamWorldStarsController>();
			if (componentInChildren != null)
			{
				componentInChildren.StartFlicker(_flickerController);
			}
		}
		if (_screenController != null)
		{
			_screenController.BeginFlicker();
		}
		_lightFlickeringOut = true;
		_lightResetTime = Time.timeSinceLevelLoad + num;
		if (_playerInsideRingWorld)
		{
			_flickerOneShot.PlayOneShot(AudioType.StationFlicker_RW);
		}
		if (Locator.GetDreamWorldAudioController() != null)
		{
			Locator.GetDreamWorldAudioController().OnStationLightFlicker();
		}
	}

	private void BeginDepartSolarSystem()
	{
		_departing = true;
	}

	private void DamageDam()
	{
		_damDamaged = true;
		if (_damController != null)
		{
			_damController.StartLeak();
		}
		OnDamDamaged.Invoke();
	}

	private void BreakDam()
	{
		_damBroken = true;
		if (_damController != null)
		{
			_damController.StartCollapse();
		}
		if (_playerInsideRingWorld && _damOneShotAudio_Far != null)
		{
			_damOneShotAudio_Far.PlayOneShot(AudioType.DamBreak_RW_Base);
			if (Locator.GetPlayerController().GetGroundBody() == _ringWorldBody)
			{
				float value = Vector3.Distance(Locator.GetPlayerTransform().position, _damOneShotAudio_Far.transform.position);
				float num = Mathf.InverseLerp(_damOneShotAudio_Far.maxDistance, _damOneShotAudio_Far.minDistance, value);
				RumbleManager.PlayDamBreak(num * num);
			}
		}
		OnDamBreak.Invoke();
		GlobalMessenger.FireEvent("DamBroken");
		if (_riverController != null)
		{
			_riverController.StartFlood();
		}
	}

	private void CollapseLighthouse()
	{
		_lighthouseCollapsed = true;
		if (_lighthouseController != null)
		{
			_lighthouseController.StartCollapse();
		}
	}

	private void OnEnterInsideVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInsideRingWorld = true;
			OnPlayerEnter.Invoke();
			if (_sailsDeploying && !_sailsDeployed)
			{
				_solarSailLooping.FadeInToLibraryVolume(2f);
			}
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInsideRingWorld = true;
			OnProbeEnter.Invoke();
		}
	}

	private void OnExitInsideVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInsideRingWorld = false;
			OnPlayerExit.Invoke();
			if (_solarSailLooping.isPlaying)
			{
				_solarSailLooping.FadeOut(2f);
			}
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInsideRingWorld = false;
			OnProbeExit.Invoke();
		}
	}

	private void OnZone1Entry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_damBreakTime += _damLifeExtendDuration;
			_zone1TriggerVolume.OnEntry -= OnZone1Entry;
		}
	}
}
