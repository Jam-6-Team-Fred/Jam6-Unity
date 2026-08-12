using UnityEngine;

public class ShipThrusterAudio : ThrusterAudio
{
	[Space]
	[SerializeField]
	private OWAudioSource _ignitionSource;

	[SerializeField]
	private OWAudioSource _rightTranslationalSource;

	[SerializeField]
	private OWAudioSource _rightUnderwaterSource;

	[SerializeField]
	private OWAudioSource _leftTranslationalSource;

	[SerializeField]
	private OWAudioSource _leftUnderwaterSource;

	[SerializeField]
	private AnimationCurve _thrustToVolumeCurve;

	[Space]
	[SerializeField]
	private AudioLowPassFilter _rotationalSourceLowpass;

	private bool _isIgnitionPlaying;

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger.AddListener("StartShipIgnition", OnStartShipIgnition);
		GlobalMessenger.AddListener("CompleteShipIgnition", OnCompleteShipIgnition);
		GlobalMessenger.AddListener("CancelShipIgnition", OnCancelShipIgnition);
	}

	protected override void Start()
	{
		base.Start();
		_leftTranslationalSource.SetLocalVolume(0f);
		_leftUnderwaterSource.SetLocalVolume(0f);
		_rightTranslationalSource.SetLocalVolume(0f);
		_rightUnderwaterSource.SetLocalVolume(0f);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger.RemoveListener("StartShipIgnition", OnStartShipIgnition);
		GlobalMessenger.RemoveListener("CompleteShipIgnition", OnCompleteShipIgnition);
		GlobalMessenger.RemoveListener("CancelShipIgnition", OnCancelShipIgnition);
	}

	protected override void Update()
	{
		Vector3 localAcceleration = _thrusterModel.GetLocalAcceleration();
		localAcceleration.y *= 0.5f;
		localAcceleration.z *= 0.5f;
		Vector3 vector = (_thrusterModel.IsThrusterBankEnabled(ThrusterBank.Left) ? localAcceleration : Vector3.zero);
		vector.x = Mathf.Max(0f, vector.x);
		Vector3 vector2 = (_thrusterModel.IsThrusterBankEnabled(ThrusterBank.Right) ? localAcceleration : Vector3.zero);
		vector2.x = Mathf.Min(0f, vector2.x);
		float maxTranslationalThrust = _thrusterModel.GetMaxTranslationalThrust();
		UpdateTranslationalSourceVolume(_leftTranslationalSource, _thrustToVolumeCurve.Evaluate(vector.magnitude / maxTranslationalThrust), !_underwater);
		UpdateTranslationalSourceVolume(_rightTranslationalSource, _thrustToVolumeCurve.Evaluate(vector2.magnitude / maxTranslationalThrust), !_underwater);
		UpdateTranslationalSourceVolume(_leftUnderwaterSource, _thrustToVolumeCurve.Evaluate(vector.magnitude / maxTranslationalThrust), _underwater);
		UpdateTranslationalSourceVolume(_rightUnderwaterSource, _thrustToVolumeCurve.Evaluate(vector2.magnitude / maxTranslationalThrust), _underwater);
		if (!_thrustersFiring && !_leftTranslationalSource.isPlaying && !_rightTranslationalSource.isPlaying && !_leftUnderwaterSource.isPlaying && !_rightUnderwaterSource.isPlaying)
		{
			base.enabled = false;
		}
	}

	protected override void UpdateUnderwaterSettings()
	{
		_rotationalSourceLowpass.enabled = !_underwater;
	}

	private void UpdateTranslationalSourceVolume(OWAudioSource source, float targetVolume, bool active)
	{
		if (_isIgnitionPlaying || !active)
		{
			targetVolume = 0f;
		}
		if (source.isPlaying && targetVolume <= 0f && source.volume <= 0f)
		{
			source.Stop();
		}
		else if (!source.isPlaying && targetVolume > 0f)
		{
			source.Play();
			source.RandomizePlayhead();
		}
		float localVolume = Mathf.MoveTowards(source.GetLocalVolume(), targetVolume, 2f * Time.deltaTime);
		source.SetLocalVolume(localVolume);
	}

	private void OnStartShipIgnition()
	{
		_ignitionSource.Stop();
		_isIgnitionPlaying = true;
		_ignitionSource.PlayOneShot(AudioType.ShipThrustIgnition);
	}

	private void OnCompleteShipIgnition()
	{
		_isIgnitionPlaying = false;
	}

	private void OnCancelShipIgnition()
	{
		_ignitionSource.Stop();
		_isIgnitionPlaying = false;
	}
}
