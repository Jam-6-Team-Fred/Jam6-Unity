using UnityEngine;

public class CloakFieldController : MonoBehaviour, SunLightController.ISunOverrider
{
	[SerializeField]
	private OWRenderer _cloakSphereRenderer;

	[SerializeField]
	private Sector _exclusionSector;

	[SerializeField]
	private ReferenceFrameVolume _referenceFrameVolume;

	[Space]
	[SerializeField]
	private SphereShape _cloakSphereShape;

	[SerializeField]
	private OWTriggerVolume _cloakSphereVolume;

	[Space]
	[SerializeField]
	private float _nearCloakRadius = 800f;

	[SerializeField]
	private float _farCloakRadius = 500f;

	[SerializeField]
	private float _innerCloakRadius = 800f;

	[SerializeField]
	private float _cloakScaleDist = 2000f;

	[Space]
	[SerializeField]
	private float _entryFadeOutLength = 1f;

	[SerializeField]
	private float _entryFadeInLength = 1f;

	[SerializeField]
	private float _exitFadeOutLength = 1f;

	[SerializeField]
	private float _exitFadeInLength = 1f;

	[Space]
	[SerializeField]
	private OWRenderer[] _ringworldFadeRenderers = new OWRenderer[0];

	[Space]
	[SerializeField]
	private OWAudioSource _musicAudioSource;

	private int _propID_CloakSpherePosRadius = Shader.PropertyToID("_CloakSpherePosRadius");

	private int _propID_CloakSphereEntryFactor = Shader.PropertyToID("_CloakSphereEntryFactor");

	private int _propID_CloakSphereRevealFactor = Shader.PropertyToID("_CloakSphereRevealFactor");

	private bool _cloakVisualsEnabled;

	private bool _firstUpdate = true;

	private bool _inMapView;

	private bool _playerInsideCloak;

	private bool _shipInsideCloak;

	private bool _probeInsideCloak;

	private float _currentCloakRadius;

	private float _playerCloakFactor;

	private float _worldFadeFactor;

	private float _interiorRevealFactor;

	private float _rendererFade;

	private bool _hasTriggeredMusic;

	private bool _playMusicAfterDelay;

	private float _musicPlayTime;

	public OWEvent OnPlayerEnter = new OWEvent(8);

	public OWEvent OnPlayerExit = new OWEvent(8);

	public OWEvent OnShipEnter = new OWEvent(8);

	public OWEvent OnShipExit = new OWEvent(8);

	public OWEvent OnProbeEnter = new OWEvent(8);

	public OWEvent OnProbeExit = new OWEvent(8);

	public bool isPlayerInsideCloak => _playerInsideCloak;

	public bool isShipInsideCloak => _shipInsideCloak;

	public bool isProbeInsideCloak => _probeInsideCloak;

	public float playerCloakFactor => _playerCloakFactor;

	private void Awake()
	{
		Locator.RegisterCloakFieldController(this);
		if (_exclusionSector != null)
		{
			_exclusionSector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_referenceFrameVolume.gameObject.SetActive(value: false);
		_cloakSphereVolume.OnEntry += OnEnterCloakSphereVolume;
		_cloakSphereVolume.OnExit += OnExitCloakSphereVolume;
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		GlobalMessenger.AddListener("GamePaused", OnGamePaused);
	}

	private void Start()
	{
		_playerInsideCloak = Vector3.Distance(Locator.GetPlayerCamera().transform.position, base.transform.position) < _nearCloakRadius;
		_playerCloakFactor = (_playerInsideCloak ? 1f : 0f);
		_worldFadeFactor = (_playerInsideCloak ? 1f : 0f);
		_interiorRevealFactor = (_playerInsideCloak ? 1f : 0f);
		_rendererFade = (_playerInsideCloak ? 0f : 1f);
		_hasTriggeredMusic = Locator.GetShipLogManager().IsFactRevealed("IP_RING_WORLD_X1");
	}

	private void OnDestroy()
	{
		if (_exclusionSector != null)
		{
			_exclusionSector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_cloakSphereVolume.OnEntry -= OnEnterCloakSphereVolume;
		_cloakSphereVolume.OnExit -= OnExitCloakSphereVolume;
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		GlobalMessenger.RemoveListener("GamePaused", OnGamePaused);
		GlobalMessenger.RemoveListener("GameUnpaused", OnGameUnpaused);
	}

	private void OnEnable()
	{
		SunLightController.RegisterSunOverrider(this, 900);
		UpdateCloakVisualsState();
	}

	private void OnDisable()
	{
		SunLightController.UnregisterSunOverrider(this);
		UpdateCloakVisualsState();
	}

	public bool CheckBodyInsideCloak(OWRigidbody body)
	{
		if (body.GetType() == typeof(ShipBody))
		{
			return _shipInsideCloak;
		}
		return Vector3.SqrMagnitude(body.GetPosition() - base.transform.position) < _currentCloakRadius * _currentCloakRadius;
	}

	public bool IsReferenceFrameVolumeActive()
	{
		return _referenceFrameVolume.gameObject.activeInHierarchy;
	}

	public void SetReferenceFrameVolumeActive(bool active)
	{
		if (!active)
		{
			ReferenceFrameTracker component = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
			if (component.GetReferenceFrame() == _referenceFrameVolume.GetReferenceFrame())
			{
				component.UntargetReferenceFrame();
			}
		}
		_referenceFrameVolume.gameObject.SetActive(active);
	}

	public SunLightController.SunOverrideSettings ApplySunOverrides(OWCamera owCamera, SunLightController.SunOverrideSettings settings)
	{
		if (owCamera == Locator.GetPlayerCamera() && _worldFadeFactor > 0f && _interiorRevealFactor < 1f)
		{
			settings.sunIntensity *= 1f - (_worldFadeFactor - _interiorRevealFactor);
		}
		return settings;
	}

	private void OnSectorOccupantsUpdated()
	{
		bool flag = Locator.GetDreamWorldController() != null && Locator.GetDreamWorldController().IsInDream();
		bool flag2 = _exclusionSector.ContainsOccupant(DynamicOccupant.Player);
		base.enabled = !flag && !flag2;
		if (flag2 && !_playerInsideCloak)
		{
			Debug.Log("Player entered cloak exclusion sector before entering cloak, adding player to manually");
			_playerInsideCloak = true;
			_playerCloakFactor = 1f;
			_worldFadeFactor = 1f;
			_interiorRevealFactor = 1f;
			_rendererFade = 0f;
		}
	}

	private void OnEnterMapView()
	{
		_inMapView = true;
		UpdateCloakVisualsState();
	}

	private void OnExitMapView()
	{
		_inMapView = false;
		UpdateCloakVisualsState();
	}

	private void UpdateCloakVisualsState()
	{
		bool flag = base.enabled && !_inMapView;
		if (flag && !_cloakVisualsEnabled)
		{
			_cloakSphereRenderer.SetActivation(active: true);
			Shader.EnableKeyword("_CLOAKINGFIELDENABLED");
			_cloakVisualsEnabled = true;
		}
		else if (!flag && _cloakVisualsEnabled)
		{
			_cloakSphereRenderer.SetActivation(active: false);
			Shader.DisableKeyword("_CLOAKINGFIELDENABLED");
			_cloakVisualsEnabled = false;
		}
	}

	private void FixedUpdate()
	{
		if (!OWMath.ApproxEquals(_cloakSphereShape.radius, _currentCloakRadius))
		{
			_cloakSphereShape.radius = _currentCloakRadius;
		}
	}

	private void LateUpdate()
	{
		if (_firstUpdate && _playerInsideCloak)
		{
			OnPlayerEnter.Invoke();
			GlobalMessenger.FireEvent("EnterCloak");
		}
		float num = Vector3.Distance(Locator.GetPlayerCamera().transform.position, base.transform.position);
		if (!_playerInsideCloak)
		{
			if (num < _nearCloakRadius)
			{
				_worldFadeFactor += Time.deltaTime / _entryFadeOutLength;
				if (_worldFadeFactor >= 1f)
				{
					_worldFadeFactor = 1f;
					_playerInsideCloak = true;
					OnPlayerEnter.Invoke();
					GlobalMessenger.FireEvent("EnterCloak");
					Locator.GetPlayerAudioController().OnRingWorldCloakEnter();
					if (_musicAudioSource != null && !_hasTriggeredMusic)
					{
						_hasTriggeredMusic = true;
						_playMusicAfterDelay = true;
						_musicPlayTime = Time.time + 3f;
					}
				}
			}
			else if (_interiorRevealFactor > 0f)
			{
				_interiorRevealFactor -= Time.deltaTime / _exitFadeInLength;
				if (_interiorRevealFactor <= 0f)
				{
					_interiorRevealFactor = 0f;
					OnPlayerExit.Invoke();
					GlobalMessenger.FireEvent("ExitCloak");
					Locator.GetPlayerAudioController().OnRingWorldCloakExit();
					if (_probeInsideCloak)
					{
						Locator.GetProbe().ExternalRetrieve();
					}
				}
			}
			else if (_worldFadeFactor > 0f)
			{
				_worldFadeFactor -= Time.deltaTime / _exitFadeOutLength;
				if (_worldFadeFactor <= 0f)
				{
					_worldFadeFactor = 0f;
				}
			}
		}
		else if (num > _innerCloakRadius)
		{
			_playerInsideCloak = false;
			_playMusicAfterDelay = false;
			if (_playMusicAfterDelay || (_musicAudioSource.isPlaying && _musicAudioSource.time < 25f))
			{
				_hasTriggeredMusic = false;
				_musicAudioSource.FadeOut(5f);
			}
		}
		else if (_interiorRevealFactor <= 1f)
		{
			_interiorRevealFactor += Time.deltaTime / _entryFadeInLength;
			if (_interiorRevealFactor >= 1f)
			{
				_interiorRevealFactor = 1f;
			}
		}
		_playerCloakFactor = Mathf.Clamp01((_worldFadeFactor + _interiorRevealFactor) * 0.5f);
		float num2;
		if (_playerInsideCloak)
		{
			num2 = _innerCloakRadius;
		}
		else
		{
			float value = Mathf.Max(0f, num - _nearCloakRadius);
			float num3 = Mathf.InverseLerp(_cloakScaleDist, 0f, value);
			num2 = Mathf.Lerp(_farCloakRadius, _nearCloakRadius, num3 * num3);
		}
		_currentCloakRadius = num2;
		Vector3 position = base.transform.position;
		Shader.SetGlobalVector(_propID_CloakSpherePosRadius, new Vector4(position.x, position.y, position.z, num2));
		Shader.SetGlobalFloat(_propID_CloakSphereEntryFactor, _worldFadeFactor);
		Shader.SetGlobalFloat(_propID_CloakSphereRevealFactor, _interiorRevealFactor);
		float num4 = 1f - _interiorRevealFactor;
		if (!OWMath.ApproxEquals(_rendererFade, num4) || _firstUpdate)
		{
			_rendererFade = num4;
			for (int i = 0; i < _ringworldFadeRenderers.Length; i++)
			{
				_ringworldFadeRenderers[i].SetFade(num4);
			}
		}
		if (_firstUpdate)
		{
			_firstUpdate = false;
		}
		if (_playMusicAfterDelay && Time.time >= _musicPlayTime)
		{
			_playMusicAfterDelay = false;
			_musicAudioSource.Stop();
			_musicAudioSource.SetLocalVolume(1f);
			_musicAudioSource.Play();
		}
	}

	private void OnGamePaused()
	{
		if (_musicAudioSource.isPlaying && !_musicAudioSource.IsFadingOut())
		{
			_musicAudioSource.FadeOut(0.5f, OWAudioSource.FadeOutCompleteAction.PAUSE);
			GlobalMessenger.AddListener("GameUnpaused", OnGameUnpaused);
		}
	}

	private void OnGameUnpaused()
	{
		_musicAudioSource.FadeIn(0.5f);
		GlobalMessenger.RemoveListener("GameUnpaused", OnGameUnpaused);
	}

	private void OnEnterCloakSphereVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("ShipDetector"))
		{
			_shipInsideCloak = true;
			OnShipEnter.Invoke();
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInsideCloak = true;
			OnProbeEnter.Invoke();
		}
	}

	private void OnExitCloakSphereVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("ShipDetector"))
		{
			_shipInsideCloak = false;
			OnShipExit.Invoke();
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInsideCloak = false;
			OnProbeExit.Invoke();
		}
	}
}
