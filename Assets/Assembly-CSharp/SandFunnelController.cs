using UnityEngine;

public class SandFunnelController : MonoBehaviour
{
	[SerializeField]
	private float _growAfterMinutes = 1f;

	[SerializeField]
	private float _shrinkAfterMinutes = 16f;

	[Space]
	[SerializeField]
	private Transform _scaleRoot;

	[SerializeField]
	private SandLevelController _caveTwinSandLevel;

	[SerializeField]
	private SandLevelController _towerTwinSandLevel;

	[SerializeField]
	private GameObject[] _sandGeoObjects;

	[SerializeField]
	private Transform _caveTwinEffectsRoot;

	[SerializeField]
	private Transform _towerTwinEffectsRoot;

	[SerializeField]
	private OWAudioSource[] _audioSources;

	[Space]
	[SerializeField]
	private OWTriggerVolume _lakebedCaveVolume;

	[SerializeField]
	private OWRenderer[] _sandRenderers;

	[SerializeField]
	private Transform _hgtTowerTransform;

	[SerializeField]
	private Sector _particleCullSector;

	private bool _hasStartedGrowing;

	private bool _hasStartedShrinking;

	private float _initScaleTime;

	private float _initScale;

	private float _targetScale;

	private float _scaleDuration;

	private bool _volumesAndEffectsActive;

	private bool _particlesPlaying;

	private bool _isScaling;

	private bool _playerInDarkZone;

	private bool _playerInTimeLoop;

	private FluidVolume _fluidVolume;

	private HazardVolume _hazardVolume;

	private ParticleSystem[] _funnelParticles;

	private int _propID_FadeTT;

	private Vector4 _baseFadeTT;

	public Transform scaleRoot => _scaleRoot;

	public GameObject[] sandGeoObjects => _sandGeoObjects;

	private void Start()
	{
		_fluidVolume = GetComponentInChildren<FluidVolume>();
		_hazardVolume = GetComponentInChildren<HazardVolume>();
		_funnelParticles = GetComponentsInChildren<ParticleSystem>();
		_propID_FadeTT = Shader.PropertyToID("_FadeTT");
		_baseFadeTT = _sandRenderers[0].sharedMaterial.GetVector(_propID_FadeTT);
		SetGeoActive(active: false);
		_scaleRoot.localScale = new Vector3(0.001f, 0.001f, 1f);
		CheckVolumeAndEffectActivation(forceUpdate: true);
		_lakebedCaveVolume.OnEntry += OnLakebedCaveEntry;
		_lakebedCaveVolume.OnExit += OnLakebedCaveExit;
		_particleCullSector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		GlobalMessenger<OWRigidbody>.AddListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
		GlobalMessenger<OWRigidbody>.AddListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
	}

	private void OnDestroy()
	{
		_lakebedCaveVolume.OnEntry -= OnLakebedCaveEntry;
		_lakebedCaveVolume.OnExit -= OnLakebedCaveExit;
		_particleCullSector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
		GlobalMessenger<OWRigidbody>.RemoveListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
	}

	public void SetPlayerInDarkZone(bool playerInDarkZone)
	{
		_playerInDarkZone = playerInDarkZone;
		CheckVolumeAndEffectActivation();
	}

	private void OnEnterTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player"))
		{
			_playerInTimeLoop = true;
			CheckVolumeAndEffectActivation();
		}
	}

	private void OnExitTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player"))
		{
			_playerInTimeLoop = false;
			CheckVolumeAndEffectActivation();
		}
	}

	private void CheckVolumeAndEffectActivation(bool forceUpdate = false)
	{
		bool flag = !_playerInDarkZone && !_playerInTimeLoop && _hasStartedGrowing && !_hasStartedShrinking;
		if (!(flag != _volumesAndEffectsActive || forceUpdate))
		{
			return;
		}
		_volumesAndEffectsActive = flag;
		_fluidVolume.SetVolumeActivation(flag);
		_hazardVolume.SetVolumeActivation(flag);
		CheckParticleActivation();
		for (int i = 0; i < _audioSources.Length; i++)
		{
			if (flag)
			{
				_audioSources[i].FadeIn(5f);
			}
			else
			{
				_audioSources[i].FadeOut(5f);
			}
		}
	}

	private void CheckParticleActivation()
	{
		bool flag = _volumesAndEffectsActive && _particleCullSector.ContainsAnyOccupants(DynamicOccupant.Player);
		if (flag == _particlesPlaying)
		{
			return;
		}
		_particlesPlaying = flag;
		for (int i = 0; i < _funnelParticles.Length; i++)
		{
			if (flag)
			{
				_funnelParticles[i].Play();
			}
			else
			{
				_funnelParticles[i].Stop();
			}
		}
	}

	private void ScaleTo(float targetScale, float scaleDuration)
	{
		_targetScale = targetScale;
		_scaleDuration = scaleDuration;
		_initScaleTime = Time.time;
		_initScale = _scaleRoot.localScale.x;
		_isScaling = true;
	}

	private void Update()
	{
		if (!_hasStartedGrowing && TimeLoop.GetMinutesElapsed() >= _growAfterMinutes)
		{
			_hasStartedGrowing = true;
			CheckVolumeAndEffectActivation();
			ScaleTo(1f, 10f);
			SetGeoActive(active: true);
		}
		else if (!_hasStartedShrinking && TimeLoop.GetMinutesElapsed() >= _shrinkAfterMinutes)
		{
			_hasStartedShrinking = true;
			CheckVolumeAndEffectActivation();
			ScaleTo(0.001f, 10f);
		}
		float num = Vector3.Distance(_caveTwinSandLevel.transform.position, base.transform.position) - _caveTwinSandLevel.GetRadius();
		_caveTwinEffectsRoot.localPosition = Vector3.forward * num;
		_towerTwinEffectsRoot.localPosition = Vector3.forward * _towerTwinSandLevel.GetRadius();
		if (_isScaling)
		{
			float num2 = Mathf.InverseLerp(_initScaleTime, _initScaleTime + _scaleDuration, Time.time);
			float num3 = Mathf.Lerp(_initScale, _targetScale, Mathf.SmoothStep(0f, 1f, num2));
			_scaleRoot.localScale = new Vector3(num3, num3, 1f);
			if (num2 >= 1f)
			{
				_isScaling = false;
				if (num3 < 0.1f)
				{
					SetGeoActive(active: false);
				}
			}
		}
		float value = Vector3.Angle(_scaleRoot.forward, _hgtTowerTransform.up);
		float y = Mathf.Lerp(_baseFadeTT.y * 0.1f, _baseFadeTT.y, Mathf.InverseLerp(10f, 30f, value));
		for (int i = 0; i < _sandRenderers.Length; i++)
		{
			_sandRenderers[i].SetMaterialProperty(_propID_FadeTT, new Vector4(_baseFadeTT.x, y, _baseFadeTT.z, _baseFadeTT.w));
		}
	}

	private void SetGeoActive(bool active)
	{
		for (int i = 0; i < _sandGeoObjects.Length; i++)
		{
			_sandGeoObjects[i].SetActive(active);
		}
	}

	private void OnLakebedCaveEntry(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _sandRenderers.Length; i++)
			{
				_sandRenderers[i].SetActivation(active: false);
			}
		}
	}

	private void OnLakebedCaveExit(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _sandRenderers.Length; i++)
			{
				_sandRenderers[i].SetActivation(active: true);
			}
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		CheckParticleActivation();
	}
}
