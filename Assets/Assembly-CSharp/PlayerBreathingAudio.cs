using UnityEngine;

public class PlayerBreathingAudio : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource _breathingSource;

	[SerializeField]
	private OWAudioSource _breathingLowOxygenSource;

	[SerializeField]
	private OWAudioSource _drowningSource;

	[SerializeField]
	private OWAudioSource _asphyxiationSource;

	private PlayerResources _playerResources;

	private bool _wearingHelmet;

	private bool _playingSuffocation;

	private bool _playingBreathing;

	private bool _playGaspAfterDelay;

	private float _playGaspTime;

	private bool _playResurrectGaspAfterDelay;

	private float _resurrectGaspTime;

	private void Awake()
	{
		GlobalMessenger.AddListener("HelmetHUDActivated", OnHelmetHUDActivated);
		GlobalMessenger.AddListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger.AddListener("WakeUp", OnWakeUp);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("PlayerResurrection", OnPlayerResurrection);
	}

	private void Start()
	{
		_playerResources = Locator.GetPlayerBody().GetComponent<PlayerResources>();
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("HelmetHUDActivated", OnHelmetHUDActivated);
		GlobalMessenger.RemoveListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger.RemoveListener("WakeUp", OnWakeUp);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("PlayerResurrection", OnPlayerResurrection);
	}

	private void Update()
	{
		if (_playGaspAfterDelay && Time.time > _playGaspTime)
		{
			_playGaspAfterDelay = false;
			Locator.GetAudioMixer().UnmixWakeUp();
			PlayWakeGasp();
		}
		if (_playResurrectGaspAfterDelay && Time.time > _resurrectGaspTime)
		{
			_playResurrectGaspAfterDelay = false;
			_oneShotSource.PlayOneShot(AudioType.PlayerGasp_Medium);
		}
		bool flag = _playerResources.IsSuffocating();
		bool flag2 = !_playerResources.IsOxygenPresent() && _wearingHelmet && !flag;
		bool flag3 = _playerResources.GetOxygenInSeconds() <= _playerResources.GetCriticalOxygenInSeconds();
		if (flag && !_playingSuffocation)
		{
			_playingSuffocation = true;
			_asphyxiationSource.AssignAudioLibraryClip(_wearingHelmet ? AudioType.Asphyxiate_Start_Suit : AudioType.Asphyxiate_Start_NoSuit);
		}
		else if (!flag && _playingSuffocation)
		{
			_playingSuffocation = false;
			_drowningSource.FadeOut(0.2f);
			_asphyxiationSource.FadeOut(0.2f);
			if (!_wearingHelmet)
			{
				_oneShotSource.PlayOneShot(AudioType.PlayerGasp_StopSuffocating);
			}
			else
			{
				_oneShotSource.PlayOneShot(AudioType.PlayerGasp_StopSuffocating_Suit);
			}
		}
		if (_playingSuffocation)
		{
			bool flag4 = !_wearingHelmet && _playerResources.IsUnderwater();
			if (!_drowningSource.isPlaying && flag4)
			{
				_drowningSource.FadeIn(0.2f);
				_asphyxiationSource.FadeOut(0.2f);
			}
			else if (!_asphyxiationSource.isPlaying && !flag4)
			{
				_drowningSource.FadeOut(0.2f);
				_asphyxiationSource.FadeIn(0.2f);
			}
		}
		if (flag2 && !_playingBreathing)
		{
			_playingBreathing = true;
			if (flag3)
			{
				_breathingLowOxygenSource.FadeIn(0.5f);
			}
			else
			{
				_breathingSource.FadeIn(1f);
			}
		}
		else if (!flag2 && _playingBreathing)
		{
			_playingBreathing = false;
			_breathingSource.FadeOut(0.5f);
			_breathingLowOxygenSource.FadeOut(0.5f);
		}
		if (_playingBreathing)
		{
			if (flag3 && !_breathingLowOxygenSource.isPlaying)
			{
				_breathingSource.FadeOut(1f);
				_breathingLowOxygenSource.FadeIn(1f);
			}
			else if (!flag3 && !_breathingSource.isPlaying)
			{
				_breathingSource.FadeIn(1f);
				_breathingLowOxygenSource.FadeOut(1f);
			}
		}
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		base.enabled = false;
		if (_playerResources.IsSuffocating())
		{
			AudioType type = (_wearingHelmet ? AudioType.Asphyxiate_End_Suit : (_playerResources.IsUnderwater() ? AudioType.Drowing_End : AudioType.Asphyxiate_End_NoSuit));
			_oneShotSource.PlayOneShot(type);
		}
		else if (deathType == DeathType.Energy || deathType == DeathType.BigBang || deathType == DeathType.Supernova || deathType == DeathType.Lava)
		{
			_breathingSource.FadeOut(0.2f);
			_breathingLowOxygenSource.FadeOut(0.2f);
		}
	}

	private void OnPlayerResurrection()
	{
		base.enabled = true;
		_playResurrectGaspAfterDelay = true;
		_resurrectGaspTime = Time.time + 0.5f;
	}

	private void OnHelmetHUDActivated()
	{
		_wearingHelmet = true;
	}

	private void OnRemoveHelmet()
	{
		_wearingHelmet = false;
	}

	private void OnWakeUp()
	{
		_playGaspAfterDelay = true;
		_playGaspTime = Time.time + 0.1f;
		Locator.GetAudioMixer().MixWakeUp();
	}

	private void PlayWakeGasp()
	{
		if (TimeLoop.GetLoopCount() == 1 || PlayerData.GetLastDeathType() == DeathType.Supernova || PlayerData.GetLastDeathType() == DeathType.Meditation)
		{
			_oneShotSource.PlayOneShot(AudioType.PlayerGasp_Light);
		}
		else if (PlayerData.GetLastDeathType() == DeathType.Asphyxiation)
		{
			_oneShotSource.PlayOneShot(AudioType.PlayerGasp_Heavy);
		}
		else
		{
			_oneShotSource.PlayOneShot(AudioType.PlayerGasp_Medium);
		}
	}
}
