using UnityEngine;

public class ThrusterAudio : MonoBehaviour
{
	[SerializeField]
	protected OWAudioSource _translationalSource;

	[SerializeField]
	private OWAudioSource _rotationalSource;

	[Space]
	[SerializeField]
	protected AudioType _rotationClip;

	[SerializeField]
	protected AudioType _underwaterRotationClip;

	protected AudioManager _audioManager;

	protected ThrusterModel _thrusterModel;

	protected FluidDetector _fluidDetector;

	protected bool _thrustersFiring;

	protected bool _underwater;

	private float _lastRotationalThrustTime;

	protected virtual void Awake()
	{
		_thrusterModel = base.gameObject.GetAttachedOWRigidbody().GetRequiredComponent<ThrusterModel>();
		_thrusterModel.OnStartTranslationalThrust += OnStartTranslationalThrust;
		_thrusterModel.OnStopTranslationalThrust += OnStopTranslationalThrust;
		_thrusterModel.OnFireRotationalThruster += OnFireRotationalThruster;
	}

	protected virtual void Start()
	{
		_audioManager = Locator.GetAudioManager();
		_fluidDetector = base.gameObject.GetAttachedOWRigidbody().GetComponentInChildren<FluidDetector>();
		if (_fluidDetector != null)
		{
			_fluidDetector.OnEnterFluidType += OnEnterFluidType;
			_fluidDetector.OnExitFluidType += OnExitFluidType;
		}
		if (_translationalSource != null)
		{
			_translationalSource.SetLocalVolume(0f);
		}
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		_thrusterModel.OnStartTranslationalThrust -= OnStartTranslationalThrust;
		_thrusterModel.OnStopTranslationalThrust -= OnStopTranslationalThrust;
		_thrusterModel.OnFireRotationalThruster -= OnFireRotationalThruster;
		if (_fluidDetector != null)
		{
			_fluidDetector.OnEnterFluidType -= OnEnterFluidType;
			_fluidDetector.OnExitFluidType -= OnExitFluidType;
		}
	}

	protected virtual void Update()
	{
		float thrustFraction = _thrusterModel.GetThrustFraction();
		float localVolume = _translationalSource.GetLocalVolume();
		float num = ((thrustFraction > localVolume) ? 5f : 5f);
		if (_thrustersFiring && !_translationalSource.isPlaying)
		{
			_translationalSource.SetLocalVolume(0f);
			_translationalSource.Play();
		}
		else if (!_thrustersFiring && _translationalSource.volume <= 0f)
		{
			_translationalSource.Stop();
		}
		_translationalSource.SetLocalVolume(Mathf.MoveTowards(localVolume, thrustFraction, num * Time.deltaTime));
		if (!_thrustersFiring && !_translationalSource.isPlaying)
		{
			base.enabled = false;
		}
	}

	protected virtual void UpdateUnderwaterSettings()
	{
	}

	private void OnFireRotationalThruster()
	{
		if (_thrusterModel.GetRotationalThrustFraction() > 0f && Time.time > _lastRotationalThrustTime + 0.2f)
		{
			_rotationalSource.PlayOneShot(_underwater ? _underwaterRotationClip : _rotationClip);
			_lastRotationalThrustTime = Time.time;
		}
	}

	private void OnStartTranslationalThrust()
	{
		base.enabled = true;
		_thrustersFiring = true;
	}

	private void OnStopTranslationalThrust()
	{
		_thrustersFiring = false;
	}

	private void OnEnterFluidType(FluidVolume.Type type)
	{
		_underwater = _fluidDetector.InFluidType(FluidVolume.Type.WATER);
		UpdateUnderwaterSettings();
	}

	private void OnExitFluidType(FluidVolume.Type type)
	{
		_underwater = _fluidDetector.InFluidType(FluidVolume.Type.WATER);
		UpdateUnderwaterSettings();
	}
}
