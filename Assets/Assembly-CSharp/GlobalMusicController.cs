using UnityEngine;

public class GlobalMusicController : MonoBehaviour
{
	public const float secondsBeforeSupernovaPlayTime = 85f;

	[SerializeField]
	private OWAudioSource _travelSource;

	[SerializeField]
	private OWAudioSource _darkBrambleSource;

	[SerializeField]
	private OWAudioSource _endTimesSource;

	[Space]
	[SerializeField]
	private OWAudioSource _finalEndTimesIntroSource;

	[SerializeField]
	private OWAudioSource _finalEndTimesLoopSource;

	[SerializeField]
	private OWAudioSource _finalEndTimesDarkBrambleSource;

	private bool _playingEndTimes;

	private bool _playingFinalEndTimes;

	private bool _finalEndTimesInsideDarkBramble;

	private void Awake()
	{
		GlobalMessenger.AddListener("TriggerSupernova", OnTriggerSupernova);
		GlobalMessenger<OWRigidbody>.AddListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
		GlobalMessenger.AddListener("PlayerEnterBrambleDimension", OnPlayerEnterBrambleDimension);
		GlobalMessenger.AddListener("PlayerExitBrambleDimension", OnPlayerExitBrambleDimension);
		GlobalMessenger.AddListener("GamePaused", OnGamePaused);
		GlobalMessenger.AddListener("StartVesselWarp", OnStartVesselWarp);
		GlobalMessenger<bool>.AddListener("StartSleepingAtCampfire", OnStartSleeping);
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void Start()
	{
		if (LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse)
		{
			Object.Destroy(this);
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("TriggerSupernova", OnTriggerSupernova);
		GlobalMessenger<OWRigidbody>.RemoveListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
		GlobalMessenger.RemoveListener("PlayerEnterBrambleDimension", OnPlayerEnterBrambleDimension);
		GlobalMessenger.RemoveListener("PlayerExitBrambleDimension", OnPlayerExitBrambleDimension);
		GlobalMessenger.RemoveListener("GamePaused", OnGamePaused);
		GlobalMessenger.RemoveListener("StartVesselWarp", OnStartVesselWarp);
		GlobalMessenger<bool>.RemoveListener("StartSleepingAtCampfire", OnStartSleeping);
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
	}

	public bool IsEndTimesPlaying()
	{
		if (!_playingEndTimes)
		{
			return _playingFinalEndTimes;
		}
		return true;
	}

	private void Update()
	{
		UpdateTravelMusic();
		UpdateBrambleMusic();
		UpdateEndTimesMusic();
	}

	private void UpdateTravelMusic()
	{
		bool flag = PlayerState.AtFlightConsole() && !PlayerState.IsHullBreached() && Locator.GetPlayerRulesetDetector().AllowTravelMusic() && !_playingFinalEndTimes;
		bool flag2 = _travelSource.isPlaying && !_travelSource.IsFadingOut();
		if (flag && !flag2)
		{
			_travelSource.FadeIn(5f);
		}
		else if (!flag && flag2)
		{
			_travelSource.FadeOut(5f, OWAudioSource.FadeOutCompleteAction.PAUSE);
		}
	}

	private void UpdateBrambleMusic()
	{
		bool flag = Locator.GetPlayerSectorDetector().InBrambleDimension() && !Locator.GetPlayerSectorDetector().InVesselDimension() && PlayerState.AtFlightConsole() && !PlayerState.IsHullBreached() && !_playingFinalEndTimes;
		bool flag2 = _darkBrambleSource.isPlaying && !_darkBrambleSource.IsFadingOut();
		if (flag && !flag2)
		{
			_darkBrambleSource.FadeIn(5f);
		}
		else if (!flag && flag2)
		{
			_darkBrambleSource.FadeOut(5f);
		}
	}

	private void UpdateEndTimesMusic()
	{
		if (_playingFinalEndTimes)
		{
			if (TimeLoop.IsTimeLoopEnabled())
			{
				_playingFinalEndTimes = false;
				_finalEndTimesIntroSource.FadeOut(5f);
				_finalEndTimesLoopSource.FadeOut(5f);
				if (TimeLoop.GetSecondsRemaining() > 85f)
				{
					Locator.GetAudioMixer().UnmixEndTimes(5f);
				}
			}
			if (_finalEndTimesInsideDarkBramble)
			{
				bool flag = PlayerState.IsInsideShip() || !Locator.GetPlayerSectorDetector().InVesselDimension();
				if (flag && (!_finalEndTimesDarkBrambleSource.isPlaying || _finalEndTimesDarkBrambleSource.IsFadingOut()))
				{
					_finalEndTimesDarkBrambleSource.FadeIn(5f);
					_finalEndTimesLoopSource.FadeOut(10f);
				}
				else if (!flag && _finalEndTimesDarkBrambleSource.isPlaying && !_finalEndTimesDarkBrambleSource.IsFadingOut())
				{
					_finalEndTimesDarkBrambleSource.FadeOut(5f);
					_finalEndTimesLoopSource.FadeIn(5f, fadeFromNothing: false, randomizePlayhead: false, 0.5f);
				}
			}
		}
		if (!_playingEndTimes && !OWTime.IsPaused() && !PlayerState.IsSleepingAtCampfire() && TimeLoop.IsTimeLoopEnabled() && TimeLoop.GetSecondsRemaining() < 85f)
		{
			_playingEndTimes = true;
			float num = 85f - TimeLoop.GetSecondsRemaining();
			if (num < _endTimesSource.clip.length)
			{
				Locator.GetAudioMixer().MixEndTimes(5f);
				_endTimesSource.FadeInToLibraryVolume(2f);
				_endTimesSource.time = num;
			}
		}
		else if (_playingEndTimes && !TimeLoop.IsTimeLoopEnabled())
		{
			_playingEndTimes = false;
			_endTimesSource.FadeOut(5f);
		}
	}

	private void OnExitTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player") && !TimeLoop.IsTimeLoopEnabled() && !_playingFinalEndTimes)
		{
			Locator.GetAudioMixer().MixEndTimes(5f);
			_finalEndTimesLoopSource.GetAudioSource().PlayScheduled(AudioSettings.dspTime + (double)_finalEndTimesIntroSource.clip.length);
			_finalEndTimesIntroSource.Stop();
			_finalEndTimesIntroSource.FadeIn(2f);
			_playingFinalEndTimes = true;
		}
	}

	private void OnTriggerSupernova()
	{
		_endTimesSource.FadeOut(5f);
		_finalEndTimesIntroSource.FadeOut(5f);
		_finalEndTimesLoopSource.FadeOut(5f);
		_finalEndTimesDarkBrambleSource.FadeOut(5f);
		base.enabled = false;
	}

	private void OnPlayerEnterBrambleDimension()
	{
		if (_playingFinalEndTimes)
		{
			Locator.GetAudioMixer().MixBrambleEndTimes(5f);
			_finalEndTimesIntroSource.FadeOut(5f);
			_finalEndTimesLoopSource.FadeOut(5f);
			_finalEndTimesDarkBrambleSource.FadeIn(5f);
			_finalEndTimesInsideDarkBramble = true;
		}
	}

	private void OnPlayerExitBrambleDimension()
	{
		if (_playingFinalEndTimes)
		{
			Locator.GetAudioMixer().UnmixBrambleEndTimes(5f);
			_finalEndTimesLoopSource.FadeIn(5f);
			_finalEndTimesDarkBrambleSource.FadeOut(5f);
			_finalEndTimesInsideDarkBramble = false;
		}
	}

	private void OnGamePaused()
	{
		if (_playingEndTimes)
		{
			_playingEndTimes = false;
			_endTimesSource.FadeOut(0.5f);
		}
	}

	private void OnStartVesselWarp()
	{
		_finalEndTimesInsideDarkBramble = false;
		_finalEndTimesLoopSource.FadeOut(0.5f);
		_finalEndTimesDarkBrambleSource.FadeOut(0.5f);
	}

	private void OnStartSleeping(bool isDreamCampfire)
	{
		if (_playingEndTimes)
		{
			_playingEndTimes = false;
			_endTimesSource.FadeOut(3f);
		}
	}

	private void OnEnterDreamWorld()
	{
		if (_playingFinalEndTimes)
		{
			_finalEndTimesIntroSource.Stop();
			_finalEndTimesLoopSource.Stop();
			_finalEndTimesDarkBrambleSource.FadeIn(1f);
		}
		else
		{
			_endTimesSource.Stop();
			_endTimesSource.AssignAudioLibraryClip(AudioType.EndOfTime_Dream);
			_playingEndTimes = false;
		}
	}

	private void OnExitDreamWorld()
	{
		if (_playingFinalEndTimes)
		{
			_finalEndTimesLoopSource.FadeIn(1f);
			_finalEndTimesDarkBrambleSource.Stop();
		}
		else
		{
			_endTimesSource.Stop();
			_endTimesSource.AssignAudioLibraryClip(AudioType.EndOfTime);
			_playingEndTimes = false;
		}
	}
}
