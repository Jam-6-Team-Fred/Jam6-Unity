using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[AddComponentMenu("Audio/OW Audio Mixer", 200)]
public class OWAudioMixer : MonoBehaviour
{
	public enum TrackName
	{
		Undefined = 0,
		Menu = 1,
		Music = 2,
		Environment = 4,
		Environment_Unfiltered = 5,
		EndTimes_SFX = 8,
		Signal = 16,
		Death = 32,
		Player = 64,
		Player_External = 65,
		Ship = 128,
		Map = 256,
		EndTimes_Music = 512,
		MuffleWhileRafting = 1024,
		MuffleIndoors = 2048,
		SlideReelMusic = 4096
	}

	[SerializeField]
	private AudioMixer _unityMixer;

	[SerializeField]
	private TrackName _trackToFind;

	private Dictionary<int, AudioMixerGroup> s_nameToGroup;

	private List<AudioParameter> _parameterList;

	private bool _initialized;

	private bool _deathMixed;

	private bool _endTimesMixed;

	private bool _dreamWorldMixed;

	private bool _isEndTimePlaying;

	private bool _pauseMixed;

	private bool _sleepingAtCampfire;

	private bool _playerInReverbVolume;

	private int _indoorVolumeCount;

	private AudioParameter _masterVolume;

	private AudioParameter _masterSFXVolume;

	private AudioParameter _masterMusicVolume;

	private AudioParameter _menuVolume;

	private AudioParameter _inGameVolume;

	private AudioParameter _endTimesVolume;

	private AudioParameter _nonEndTimesVolume;

	private AudioParameter _nonMapVolume;

	private AudioParameter _nonSignalVolume;

	private AudioParameter _environmentVolume;

	private AudioParameter _musicVolume;

	private AudioParameter _muffleWhileRaftingVolume;

	private AudioParameter _muffleIndoorsVolume;

	private AudioParameter _slideReelMusicVolume;

	private AudioParameter _environmentLowPass;

	private AudioParameter _environmentReverb;

	private AudioParameter _playerExternalReverb;

	private AudioParameter _atmosphereLowPass;

	private AudioParameter _muffleIndoorsLowPass;

	private void Awake()
	{
		GlobalMessenger.AddListener("EnterShip", OnEnterShip);
		GlobalMessenger.AddListener("ExitShip", OnExitShip);
		GlobalMessenger.AddListener("GamePauseUpdated", OnGamePauseUpdated);
		GlobalMessenger.AddListener("TriggerSupernova", OnTriggerSupernova);
	}

	private void Start()
	{
		if (!_initialized)
		{
			Initialize();
		}
		SetMasterVolume(PlayerData.GetMasterVolume(), 0f);
		SetMasterSFXVolume(PlayerData.GetSFXVolume(), 0f);
		SetMasterMusicVolume(PlayerData.GetMusicVolume(), 0f);
	}

	private void Initialize()
	{
		s_nameToGroup = new Dictionary<int, AudioMixerGroup>
		{
			{
				1,
				FindGroupByName("Menu")
			},
			{
				2,
				FindGroupByName("Music")
			},
			{
				4,
				FindGroupByName("Environment")
			},
			{
				5,
				FindGroupByName("Environment_Unfiltered")
			},
			{
				8,
				FindGroupByName("EndTimes_SFX")
			},
			{
				16,
				FindGroupByName("Signal")
			},
			{
				32,
				FindGroupByName("Death")
			},
			{
				64,
				FindGroupByName("Player")
			},
			{
				65,
				FindGroupByName("Player_External")
			},
			{
				128,
				FindGroupByName("Ship")
			},
			{
				256,
				FindGroupByName("Map")
			},
			{
				512,
				FindGroupByName("EndTimes_Music")
			},
			{
				1024,
				FindGroupByName("MuffleWhileRafting")
			},
			{
				2048,
				FindGroupByName("MuffleIndoors")
			},
			{
				4096,
				FindGroupByName("SlideReelMusic")
			}
		};
		_parameterList = new List<AudioParameter>();
		_masterVolume = CreateParameter("MasterVolume", 1f, convertDecibelsToLinear: true);
		_masterSFXVolume = CreateParameter("MasterSFXVolume", 1f, convertDecibelsToLinear: true);
		_masterMusicVolume = CreateParameter("MasterMusicVolume", 1f, convertDecibelsToLinear: true);
		_menuVolume = CreateParameter("MenuVolume", 1f, convertDecibelsToLinear: true);
		_inGameVolume = CreateParameter("InGameVolume", 1f, convertDecibelsToLinear: true);
		_endTimesVolume = CreateParameter("EndTimesVolume_SFX", "EndTimesVolume_Music", 1f, convertDecibelsToLinear: true);
		_nonEndTimesVolume = CreateParameter("NonEndTimesVolume_SFX", "NonEndTimesVolume_Music", 1f, convertDecibelsToLinear: true);
		_nonMapVolume = CreateParameter("NonMapVolume_SFX", "NonMapVolume_Music", 1f, convertDecibelsToLinear: true);
		_nonSignalVolume = CreateParameter("NonSignalVolume_SFX", "NonSignalVolume_Music", 1f, convertDecibelsToLinear: true);
		_environmentVolume = CreateParameter("EnvironmentVolume", 1f, convertDecibelsToLinear: true);
		_musicVolume = CreateParameter("MusicVolume", 1f, convertDecibelsToLinear: true);
		_muffleWhileRaftingVolume = CreateParameter("MuffleWhileRaftingVolume", 1f, convertDecibelsToLinear: true);
		_muffleIndoorsVolume = CreateParameter("MuffleIndoorsVolume", 1f, convertDecibelsToLinear: true);
		_slideReelMusicVolume = CreateParameter("SlideReelMusicVolume", 1f, convertDecibelsToLinear: true);
		_environmentLowPass = CreateParameter("EnvironmentLowPass", 22000f);
		_environmentReverb = CreateParameter("EnvironmentReverb", 0f, convertDecibelsToLinear: true);
		_playerExternalReverb = CreateParameter("PlayerExternalReverb", 0f, convertDecibelsToLinear: true);
		_atmosphereLowPass = CreateParameter("AtmosphereLowPass", 22000f);
		_muffleIndoorsLowPass = CreateParameter("MuffleIndoorsLowPass", 22000f);
		_atmosphereLowPass.FadeTo(2000f, 0f);
		_initialized = true;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
		GlobalMessenger.RemoveListener("ExitShip", OnExitShip);
		GlobalMessenger.RemoveListener("GamePauseUpdated", OnGamePauseUpdated);
		GlobalMessenger.RemoveListener("TriggerSupernova", OnTriggerSupernova);
	}

	public AudioMixer GetUnityAudioMixer()
	{
		return _unityMixer;
	}

	public AudioMixerGroup GetAudioMixerGroup(TrackName name)
	{
		if (!_initialized)
		{
			Initialize();
		}
		if (s_nameToGroup.TryGetValue((int)name, out var value))
		{
			return value;
		}
		return null;
	}

	public void SetMasterVolume(float linearVolume, float fadeDuration = 1f)
	{
		if (!_initialized)
		{
			Initialize();
		}
		_masterVolume.FadeTo(linearVolume, fadeDuration);
	}

	public void SetMasterSFXVolume(float linearVolume, float fadeDuration = 1f)
	{
		if (!_initialized)
		{
			Initialize();
		}
		_masterSFXVolume.FadeTo(linearVolume, fadeDuration);
		_menuVolume.FadeTo(linearVolume, fadeDuration);
	}

	public void SetMasterMusicVolume(float linearVolume, float fadeDuration = 1f)
	{
		if (!_initialized)
		{
			Initialize();
		}
		_masterMusicVolume.FadeTo(linearVolume, fadeDuration);
	}

	public void OnEnterReverbVolume()
	{
		_environmentReverb.FadeTo(1f, 5f);
		_playerExternalReverb.FadeTo(1f, 5f);
		_playerInReverbVolume = true;
	}

	public void OnExitReverbVolume()
	{
		_environmentReverb.FadeTo(0f, 5f);
		_playerExternalReverb.FadeTo(0f, 5f);
		_playerInReverbVolume = false;
	}

	public void OnEnterAtmosphere()
	{
		_atmosphereLowPass.FadeTo(22000f, 0.2f);
		if (_playerInReverbVolume)
		{
			_environmentReverb.FadeTo(1f, 0.2f);
			_playerExternalReverb.FadeTo(1f, 0.2f);
		}
	}

	public void OnExitAtmosphere()
	{
		_atmosphereLowPass.FadeTo(2000f, 0.2f);
		if (_playerInReverbVolume)
		{
			_environmentReverb.FadeTo(0f, 0.2f);
			_playerExternalReverb.FadeTo(0f, 0.2f);
		}
	}

	public void OnEnterMindSlideProjector()
	{
		_environmentVolume.FadeTo(0.2f, 1f);
		if (!_endTimesMixed)
		{
			_musicVolume.FadeTo(0f, 1f);
		}
	}

	public void OnExitMindSlideProjector()
	{
		_environmentVolume.FadeTo(1f, 1f);
		if (!_endTimesMixed)
		{
			_musicVolume.FadeTo(1f, 1f);
		}
	}

	public void MixWakeUp()
	{
		if (!_initialized)
		{
			Initialize();
		}
		_environmentVolume.FadeTo(0f, 0f);
	}

	public void UnmixWakeUp()
	{
		_environmentVolume.FadeTo(1f, 5f);
	}

	public void MixSleepAtCampfire(float fadeDuration)
	{
		_sleepingAtCampfire = true;
		_inGameVolume.FadeTo(0f, fadeDuration);
	}

	public void UnmixSleepAtCampfire(float fadeDuration)
	{
		_sleepingAtCampfire = false;
		_inGameVolume.FadeTo(1f, fadeDuration);
	}

	public void MixRemoteCameraPlatform(float fadeDuration)
	{
		if (!_endTimesMixed)
		{
			_musicVolume.FadeTo(0f, fadeDuration);
		}
	}

	public void UnmixRemoteCameraPlatform(float fadeDuration)
	{
		if (!_endTimesMixed)
		{
			_musicVolume.FadeTo(1f, fadeDuration);
		}
	}

	public void MixMap()
	{
		_nonMapVolume.FadeTo(0.25f, 0.5f);
	}

	public void UnmixMap()
	{
		_nonMapVolume.FadeTo(1f, 0.5f);
	}

	public void MixSignal()
	{
		_nonSignalVolume.FadeTo(0.2f, 0.5f);
	}

	public void UnmixSignal()
	{
		_nonSignalVolume.FadeTo(1f, 1f);
	}

	public void MixEndTimes(float fadeDuration)
	{
		if (!_deathMixed)
		{
			_endTimesMixed = true;
			_musicVolume.FadeTo(0f, fadeDuration);
			_slideReelMusicVolume.FadeTo(0f, fadeDuration);
			_muffleWhileRaftingVolume.FadeTo(1f, fadeDuration);
			if (!_dreamWorldMixed)
			{
				_nonEndTimesVolume.FadeTo(0.5f, fadeDuration);
			}
		}
		_isEndTimePlaying = true;
	}

	public void UnmixEndTimes(float fadeDuration)
	{
		_endTimesMixed = false;
		_musicVolume.FadeTo(1f, fadeDuration);
		_slideReelMusicVolume.FadeTo(1f, fadeDuration);
		_nonEndTimesVolume.FadeTo(1f, fadeDuration);
		_isEndTimePlaying = false;
	}

	public void MixBrambleEndTimes(float fadeDuration)
	{
		if (_endTimesMixed)
		{
			_nonEndTimesVolume.FadeTo(1f, fadeDuration);
		}
	}

	public void UnmixBrambleEndTimes(float fadeDuration)
	{
		if (_endTimesMixed)
		{
			MixEndTimes(fadeDuration);
		}
	}

	public void MixDreamWorld()
	{
		if (_isEndTimePlaying && !_endTimesMixed)
		{
			_endTimesMixed = true;
			_musicVolume.FadeTo(0f, 1f);
			_slideReelMusicVolume.FadeTo(0f, 1f);
		}
		_dreamWorldMixed = true;
		float duration = (_deathMixed ? 0.5f : 1f);
		_deathMixed = false;
		_nonEndTimesVolume.FadeTo(1f, duration);
		_endTimesVolume.FadeTo(1f, duration);
	}

	public void UnmixDreamWorld()
	{
		_dreamWorldMixed = false;
		_endTimesVolume.FadeTo(1f, 1f);
		if (_isEndTimePlaying)
		{
			MixEndTimes(1f);
		}
	}

	public void MixDeath(float fadeDuration)
	{
		_deathMixed = true;
		_endTimesVolume.FadeTo(0f, 1f);
		_nonEndTimesVolume.FadeTo(0f, fadeDuration);
	}

	public void MixMemoryUplink(float duration)
	{
		MixDeath(duration);
	}

	public void UnmixMemoryUplink()
	{
		if (!_initialized)
		{
			Initialize();
		}
		_nonEndTimesVolume.FadeTo(0f, 0f);
		_nonEndTimesVolume.FadeTo(1f, 5f);
	}

	public void MixRaftMusic()
	{
		if (!_endTimesMixed)
		{
			_muffleWhileRaftingVolume.FadeTo(0.6f, 5f);
		}
	}

	public void UnmixRaftMusic()
	{
		if (!_endTimesMixed)
		{
			_muffleWhileRaftingVolume.FadeTo(1f, 5f);
		}
	}

	public void MixSimulation()
	{
		_muffleWhileRaftingVolume.FadeTo(0.1f, 2f);
	}

	public void UnmixSimulation()
	{
		_muffleWhileRaftingVolume.FadeTo(1f, 2f);
	}

	public void OnEnterIndoorVolume()
	{
		_indoorVolumeCount++;
		if (_indoorVolumeCount == 1)
		{
			_muffleIndoorsLowPass.FadeTo(5000f, 1f);
			_muffleIndoorsVolume.FadeTo(0.5f, 1f);
		}
	}

	public void OnExitIndoorVolume()
	{
		_indoorVolumeCount = Mathf.Max(_indoorVolumeCount - 1, 0);
		if (_indoorVolumeCount == 0)
		{
			_muffleIndoorsLowPass.FadeToOriginal(1f);
			_muffleIndoorsVolume.FadeTo(1f, 1f);
		}
	}

	private void OnEnterShip()
	{
		_environmentVolume.FadeTo(0.35f, 1f);
		_environmentLowPass.FadeTo(5000f, 1f);
	}

	private void OnExitShip()
	{
		_environmentVolume.FadeTo(1f, 1f);
		_environmentLowPass.FadeToOriginal(1f);
	}

	private void OnGamePauseUpdated()
	{
		if (!_initialized)
		{
			Initialize();
		}
		if (!_pauseMixed && !_sleepingAtCampfire && OWTime.IsPaused(OWTime.PauseType.Menu))
		{
			_pauseMixed = true;
			_inGameVolume.FadeTo(0f, 1f);
		}
		else if (_pauseMixed && !_sleepingAtCampfire && !OWTime.IsPaused(OWTime.PauseType.Menu))
		{
			_pauseMixed = false;
			_inGameVolume.FadeTo(1f, 1f);
		}
	}

	private void OnTriggerSupernova()
	{
		_slideReelMusicVolume.FadeTo(1f, 5f);
	}

	private AudioParameter CreateParameter(string name, float initialValue, bool convertDecibelsToLinear = false)
	{
		AudioParameter audioParameter = new AudioParameter(new string[1] { name }, initialValue, _unityMixer, convertDecibelsToLinear);
		_parameterList.Add(audioParameter);
		return audioParameter;
	}

	private AudioParameter CreateParameter(string name1, string name2, float initialValue, bool convertDecibelsToLinear = false)
	{
		AudioParameter audioParameter = new AudioParameter(new string[2] { name1, name2 }, initialValue, _unityMixer, convertDecibelsToLinear);
		_parameterList.Add(audioParameter);
		return audioParameter;
	}

	private void Update()
	{
		for (int i = 0; i < _parameterList.Count; i++)
		{
			_parameterList[i].Update();
		}
	}

	private AudioMixerGroup FindGroupByName(string name)
	{
		AudioMixerGroup[] array = _unityMixer.FindMatchingGroups(name);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == name)
			{
				return array[i];
			}
		}
		return null;
	}
}
