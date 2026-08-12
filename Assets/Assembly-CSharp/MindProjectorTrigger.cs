using UnityEngine;

public class MindProjectorTrigger : SectoredMonoBehaviour
{
	[Space]
	[SerializeField]
	private bool _startActive;

	[SerializeField]
	private bool _deactivateOnCompletion;

	[SerializeField]
	private Transform _lockOnTransform;

	[Space]
	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	[SerializeField]
	private MindSlideProjector _mindProjector;

	[Header("Custom Fade-In Curve")]
	[SerializeField]
	private bool _useCurve;

	[SerializeField]
	private AnimationCurve _intensityCurve;

	[Header("Projection Beam Effects")]
	[SerializeField]
	private OWRendererFadeController _lightRayFadeController;

	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private OWFlameController _flameController;

	[SerializeField]
	private ParticleSystem[] _particles;

	[Header("Scan Beam Effects (Optional")]
	[SerializeField]
	private Transform _scanBeamTransform;

	[SerializeField]
	private OWRendererFadeController _scanBeamFadeController;

	[SerializeField]
	private OWLightController _scanLightController;

	[SerializeField]
	private OWAudioSource _scanSource;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private OWAudioSource _loopingSource;

	public OWEvent OnBeamStartHitPlayer = new OWEvent(1);

	public OWEvent OnBeamStopHitPlayer = new OWEvent(1);

	public OWEvent OnBeamStartHitPrisoner = new OWEvent(1);

	public OWEvent OnBeamStopHitPrisoner = new OWEvent(1);

	private bool _active;

	private bool _playerLockedOn;

	private float _activeTime;

	private ConeShape _triggerConeShape;

	private Vector3 _triggerConeDimensions;

	private Vector3 _baseScanBeamEulerAngles;

	protected override void Awake()
	{
		base.Awake();
		_triggerVolume.OnEntry += OnTriggerVolumeEntry;
		_triggerVolume.OnExit += OnTriggerVolumeExit;
		_triggerConeShape = _triggerVolume.GetShape() as ConeShape;
		if (_triggerConeShape != null)
		{
			_triggerConeDimensions = new Vector3(_triggerConeShape.topRadius, _triggerConeShape.bottomRadius, _triggerConeShape.height);
		}
		if (_lockOnTransform == null)
		{
			_lockOnTransform = base.transform;
		}
	}

	private void Start()
	{
		_active = _startActive;
		_lightRayFadeController.SetFade(_active ? 1f : 0f);
		_lightController.SetIntensity(_active ? 1f : 0f);
		if (_flameController != null)
		{
			_flameController.SetIntensity(_active ? 1f : 0f);
		}
		_triggerVolume.SetTriggerActivation(_active ? true : false);
		if (_scanBeamTransform != null)
		{
			_baseScanBeamEulerAngles = _scanBeamTransform.localEulerAngles;
			_scanBeamTransform.localEulerAngles = new Vector3(-30f, _baseScanBeamEulerAngles.y, _baseScanBeamEulerAngles.z);
			_scanBeamFadeController.SetFade(_active ? 1f : 0f);
			_scanLightController.SetIntensity(_active ? 1f : 0f);
		}
		if (_loopingSource != null)
		{
			_loopingSource.SetLocalVolume(0f);
		}
		if (_scanSource != null)
		{
			_scanSource.SetLocalVolume(0f);
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_mindProjector.OnProjectionStart -= new OWEvent.OWCallback(OnProjectionStart);
		_mindProjector.OnProjectionComplete -= new OWEvent.OWCallback(OnProjectionComplete);
		_triggerVolume.OnEntry -= OnTriggerVolumeEntry;
		_triggerVolume.OnExit -= OnTriggerVolumeExit;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		UpdateParticlesState();
		if (_loopingSource != null)
		{
			bool flag = _loopingSource.isPlaying && !_loopingSource.IsFadingOut();
			bool flag2 = _sector.ContainsOccupant(DynamicOccupant.Player);
			if (flag && !flag2)
			{
				_loopingSource.FadeOut(1f);
			}
			else if (_active && !flag && flag2)
			{
				_loopingSource.FadeIn(1f);
			}
		}
	}

	public bool IsActive()
	{
		return _active;
	}

	public void SetProjectorActive(bool active)
	{
		if (_active == active)
		{
			return;
		}
		_active = active;
		if (_active)
		{
			if (_scanBeamTransform == null || _scanLightController.GetIntensity() <= 0f)
			{
				_activeTime = Time.time;
			}
			base.enabled = _useCurve || _scanBeamTransform != null;
		}
		if (_active && _useCurve)
		{
			_lightController.SetIntensity(0f);
		}
		else
		{
			_lightController.FadeTo(active ? 1f : 0f, 1f);
		}
		_lightRayFadeController.FadeTo(active ? 1f : 0f, 1f);
		if (_flameController != null)
		{
			_flameController.FadeTo(_active ? 1f : 0f, 1f);
		}
		if (_scanBeamTransform != null)
		{
			if (active && _scanLightController.GetIntensity() <= 0f)
			{
				_scanBeamTransform.localEulerAngles = new Vector3(-30f, _baseScanBeamEulerAngles.y, _baseScanBeamEulerAngles.z);
			}
			_scanBeamFadeController.FadeTo(active ? 1f : 0f, 0.3f);
			_scanLightController.FadeTo(active ? 1f : 0f, 0.3f);
		}
		_audioSource.PlayOneShot(active ? AudioType.VisionTorch_ProjectionOn : AudioType.VisionTorch_ProjectionOff);
		if (_loopingSource != null)
		{
			if (_active)
			{
				_loopingSource.FadeIn(0.5f);
			}
			else
			{
				_loopingSource.FadeOut(0.5f);
			}
		}
		_triggerVolume.SetTriggerActivation(active);
		UpdateParticlesState();
	}

	private void UpdateParticlesState()
	{
		for (int i = 0; i < _particles.Length; i++)
		{
			ParticleSystem.EmissionModule emission = _particles[i].emission;
			emission.enabled = _active;
		}
	}

	private void Update()
	{
		float num = Time.time - _activeTime;
		if (_useCurve)
		{
			float time = Mathf.Clamp01(num);
			_lightController.SetIntensity(_intensityCurve.Evaluate(time));
		}
		if (_scanBeamTransform != null)
		{
			float x = Mathf.Cos(num) * -30f;
			_scanBeamTransform.localEulerAngles = new Vector3(x, _baseScanBeamEulerAngles.y, _baseScanBeamEulerAngles.z);
		}
		bool num2 = !_useCurve || num >= 1f;
		bool flag = _scanBeamTransform == null || (!_active && _scanLightController.GetIntensity() <= 0f);
		if (_scanSource != null)
		{
			float num3 = (((_mindProjector.IsPlaying() && !_mindProjector.IsOpeningEyes()) || !_active) ? 0f : 1f);
			if (!_scanSource.isPlaying && num3 > 0f)
			{
				_scanSource.Play();
				_scanSource.RandomizePlayhead();
			}
			float localVolume = _scanSource.GetLocalVolume();
			_scanSource.SetLocalVolume(Mathf.MoveTowards(localVolume, num3, Time.unscaledDeltaTime));
			if (_scanSource.GetLocalVolume() <= 0f && num3 <= 0f)
			{
				_scanSource.Stop();
			}
		}
		if (num2 && flag && (_scanSource == null || !_scanSource.isPlaying))
		{
			base.enabled = false;
		}
	}

	private void OnTriggerVolumeEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerCameraDetector"))
		{
			OnBeamStartHitPlayer.Invoke();
			_mindProjector.Play(reset: true);
			_mindProjector.OnProjectionStart += new OWEvent.OWCallback(OnProjectionStart);
			_mindProjector.OnProjectionComplete += new OWEvent.OWCallback(OnProjectionComplete);
			if (_triggerConeShape != null)
			{
				_triggerConeShape.topRadius = _triggerConeDimensions.x + 0.15f;
				_triggerConeShape.bottomRadius = _triggerConeDimensions.y + 0.15f;
				_triggerConeShape.height = _triggerConeDimensions.z + 0.3f;
			}
			Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(_lockOnTransform, Vector3.zero);
			_playerLockedOn = true;
			if (Locator.GetToolModeSwapper().GetToolMode() == ToolMode.SignalScope && Locator.GetToolModeSwapper().GetSignalScope().InZoomMode())
			{
				Locator.GetToolModeSwapper().UnequipTool();
			}
			OWInput.ChangeInputMode(InputMode.None);
		}
		else if (hitObj.CompareTag("PrisonerDetector"))
		{
			OnBeamStartHitPrisoner.Invoke();
			_mindProjector.Play(reset: true);
			_mindProjector.OnProjectionStart += new OWEvent.OWCallback(OnProjectionStart);
			_mindProjector.OnProjectionComplete += new OWEvent.OWCallback(OnProjectionComplete);
			Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(hitObj.transform, Vector3.zero);
			_playerLockedOn = true;
		}
	}

	private void OnTriggerVolumeExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerCameraDetector") || hitObj.CompareTag("PrisonerDetector"))
		{
			if (hitObj.CompareTag("PlayerCameraDetector"))
			{
				OnBeamStopHitPlayer.Invoke();
			}
			else
			{
				OnBeamStopHitPrisoner.Invoke();
			}
			_mindProjector.Stop();
			_mindProjector.OnProjectionStart -= new OWEvent.OWCallback(OnProjectionStart);
			_mindProjector.OnProjectionComplete -= new OWEvent.OWCallback(OnProjectionComplete);
			if (OWInput.IsInputMode(InputMode.NomaiRemoteCam | InputMode.None))
			{
				OWInput.ChangeInputMode(InputMode.Character);
			}
			if (_playerLockedOn)
			{
				_playerLockedOn = false;
				Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
			}
			if (_triggerConeShape != null)
			{
				_triggerConeShape.topRadius = _triggerConeDimensions.x;
				_triggerConeShape.bottomRadius = _triggerConeDimensions.y;
				_triggerConeShape.height = _triggerConeDimensions.z;
			}
		}
	}

	private void OnProjectionStart()
	{
		_mindProjector.OnProjectionStart -= new OWEvent.OWCallback(OnProjectionStart);
		if (OWInput.IsInputMode(InputMode.None))
		{
			OWInput.ChangeInputMode(InputMode.NomaiRemoteCam);
		}
		if (_playerLockedOn)
		{
			_playerLockedOn = false;
			Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
		}
	}

	private void OnProjectionComplete()
	{
		_mindProjector.OnProjectionComplete -= new OWEvent.OWCallback(OnProjectionComplete);
		if (_deactivateOnCompletion)
		{
			SetProjectorActive(active: false);
		}
	}
}
