using UnityEngine;

public class TimeLoop : MonoBehaviour
{
	public const float LOOP_DURATION_IN_MINUTES = 22f;

	private static int _loopCount;

	private static float _loopDuration = float.PositiveInfinity;

	private static float _timeOffset = 0f;

	private static bool _isTimeFlowing;

	private static bool _timeLoopEnabled = true;

	private static bool _startTimeLoopOnReload = true;

	private static bool _initialized = false;

	private void Awake()
	{
		_loopDuration = 1320f;
		_timeOffset = 0f;
		_initialized = false;
	}

	private void OnEnable()
	{
	}

	private void Start()
	{
		_timeLoopEnabled = LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse;
		_loopCount = PlayerData.LoadLoopCount();
		if (_startTimeLoopOnReload)
		{
			MonoBehaviour.print("========== Start of Time Loop " + _loopCount + " ==========");
			_isTimeFlowing = PlayerData.GetPersistentCondition("LAUNCH_CODES_GIVEN");
			GlobalMessenger<int>.FireEvent("StartOfTimeLoop", _loopCount);
		}
		else
		{
			_isTimeFlowing = true;
			GlobalMessenger.FireEvent("ResumeSimulation");
		}
		_startTimeLoopOnReload = true;
		_initialized = true;
	}

	private void OnDestroy()
	{
		_initialized = false;
	}

	public static void ResetSimulation()
	{
		_startTimeLoopOnReload = false;
		GlobalMessenger.FireEvent("ResetSimulation");
	}

	public static void RestartTimeLoop()
	{
		_loopCount++;
		PlayerData.SaveLoopCount(_loopCount);
		GlobalMessenger.FireEvent("RestartTimeLoop");
	}

	public static bool IsTimeFlowing()
	{
		return _isTimeFlowing;
	}

	public static int GetLoopCount()
	{
		return _loopCount;
	}

	public static float GetLoopDuration()
	{
		return _loopDuration;
	}

	public static float GetSecondsElapsed()
	{
		if (!_isTimeFlowing)
		{
			return 0f;
		}
		return Time.timeSinceLevelLoad + _timeOffset;
	}

	public static float GetMinutesElapsed()
	{
		return GetSecondsElapsed() / 60f;
	}

	public static float GetFractionElapsed()
	{
		return GetSecondsElapsed() / _loopDuration;
	}

	public static float GetSecondsRemaining()
	{
		return _loopDuration - GetSecondsElapsed();
	}

	public static void SetSecondsRemaining(float secondsRemaining)
	{
		_timeOffset = _loopDuration - secondsRemaining - Time.timeSinceLevelLoad;
	}

	public static void SetTimeLoopEnabled(bool enabled)
	{
		if (!_initialized)
		{
			Debug.LogError("Time Loop not yet initialized!");
			Debug.Break();
		}
		_timeLoopEnabled = enabled;
	}

	public static bool IsTimeLoopEnabled()
	{
		if (!_initialized)
		{
			Debug.LogError("Time Loop not yet initialized!");
			Debug.Break();
		}
		return _timeLoopEnabled;
	}

	private void Update()
	{
		if (!(GetSecondsRemaining() <= 0f))
		{
			return;
		}
		QuantumMoon quantumMoon = Locator.GetQuantumMoon();
		if (quantumMoon != null)
		{
			bool flag = quantumMoon.GetStateIndex() == 5 && quantumMoon.IsPlayerInside();
			quantumMoon.OnSunCollapse(flag);
			if (flag)
			{
				base.enabled = false;
				return;
			}
		}
		GlobalMessenger.FireEvent("TriggerSupernova");
		base.enabled = false;
	}
}
