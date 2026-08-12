using UnityEngine;

public class JetpackThrusterAudio : ThrusterAudio
{
	[SerializeField]
	private OWAudioSource _underwaterSource;

	[SerializeField]
	private OWAudioSource _oxygenSource;

	[SerializeField]
	private OWAudioSource _boostSource;

	private PlayerResources _playerResources;

	private bool _wasBoosting;

	protected override void Start()
	{
		base.Start();
		_playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void Update()
	{
		bool flag = ((JetpackThrusterModel)_thrusterModel).IsBoosterFiring();
		bool flag2 = _playerResources.GetFuel() > 0f;
		float targetVolume = (flag ? 0f : _thrusterModel.GetThrustFraction());
		float targetPan = (0f - _thrusterModel.GetLocalAcceleration().x) / _thrusterModel.GetMaxTranslationalThrust() * 0.4f;
		UpdateTranslationalSource(_translationalSource, targetVolume, targetPan, !_underwater && flag2);
		UpdateTranslationalSource(_underwaterSource, targetVolume, targetPan, _underwater);
		UpdateTranslationalSource(_oxygenSource, targetVolume, targetPan, !_underwater && !flag2);
		if (!_wasBoosting && flag)
		{
			_boostSource.FadeIn(0.3f);
		}
		else if (_wasBoosting && !flag)
		{
			_boostSource.FadeOut(0.3f);
		}
		_wasBoosting = flag;
		if (!_thrustersFiring && !_translationalSource.isPlaying && !_underwaterSource.isPlaying && !_oxygenSource.isPlaying && !flag && !_wasBoosting)
		{
			base.enabled = false;
		}
	}

	private void UpdateTranslationalSource(OWAudioSource source, float targetVolume, float targetPan, bool active)
	{
		if (!active)
		{
			targetVolume = 0f;
			targetPan = 0f;
		}
		if (!source.isPlaying && targetVolume > 0f)
		{
			source.SetLocalVolume(0f);
			source.Play();
		}
		else if (source.isPlaying && source.volume <= 0f)
		{
			source.Stop();
		}
		source.SetLocalVolume(Mathf.MoveTowards(source.GetLocalVolume(), targetVolume, 5f * Time.deltaTime));
		source.panStereo = Mathf.MoveTowards(source.panStereo, targetPan, 5f * Time.deltaTime);
	}
}
