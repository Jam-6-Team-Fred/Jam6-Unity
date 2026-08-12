using UnityEngine;

public static class OWTime
{
	public enum PauseType
	{
		Menu = 0,
		Loading = 1,
		Reading = 2,
		Sleeping = 3,
		Initializing = 4,
		Streaming = 5,
		System = 6
	}

	private const int kNumPauseTypes = 7;

	private static bool[] s_pauseFlags = new bool[7];

	private static bool s_isPaused = false;

	private static float s_timeScale = 1f;

	private static float s_fixedTimestep = 1f / 60f;

	private static float s_maxDeltaTime = 1f / 15f;

	public static event PauseEvent OnPause;

	public static event PauseEvent OnUnpause;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InitializeTimeSettings()
	{
		if (SecretSettings.TryGetInt("PhysicsRate", out var value))
		{
			value = Mathf.Max(value, 1);
			s_fixedTimestep = 1f / (float)value;
			Debug.Log("Physics rate override: " + value + " (" + s_fixedTimestep.ToString("F8") + ")");
		}
		Time.fixedDeltaTime = s_fixedTimestep;
		Time.maximumDeltaTime = s_maxDeltaTime;
		Time.maximumParticleDeltaTime = s_maxDeltaTime;
	}

	public static void Pause(PauseType pauseType)
	{
		s_pauseFlags[(int)pauseType] = true;
		if (!s_isPaused)
		{
			s_isPaused = true;
			Time.timeScale = 0f;
			GlobalMessenger.FireEvent("GamePaused");
		}
		if (OWTime.OnPause != null)
		{
			OWTime.OnPause(pauseType);
		}
		GlobalMessenger.FireEvent("GamePauseUpdated");
	}

	public static void Unpause(PauseType pauseType)
	{
		s_pauseFlags[(int)pauseType] = false;
		bool flag = false;
		for (int i = 0; i < 7; i++)
		{
			flag |= s_pauseFlags[i];
		}
		if (s_isPaused && !flag)
		{
			s_isPaused = false;
			Time.timeScale = s_timeScale;
			GlobalMessenger.FireEvent("GameUnpaused");
		}
		if (OWTime.OnUnpause != null)
		{
			OWTime.OnUnpause(pauseType);
		}
		GlobalMessenger.FireEvent("GamePauseUpdated");
	}

	public static bool IsPaused()
	{
		return s_isPaused;
	}

	public static bool IsPaused(PauseType pauseType)
	{
		return s_pauseFlags[(int)pauseType];
	}

	public static float GetTimeScale()
	{
		return s_timeScale;
	}

	public static void SetTimeScale(float timeScale)
	{
		if (!s_isPaused)
		{
			Time.timeScale = timeScale;
		}
		s_timeScale = timeScale;
	}

	public static float GetFixedTimestep()
	{
		return s_fixedTimestep;
	}

	public static void SetFixedTimestep(float fixedTimestep)
	{
		Time.fixedDeltaTime = fixedTimestep;
		s_fixedTimestep = fixedTimestep;
	}

	public static float GetMaxDeltaTime()
	{
		return s_maxDeltaTime;
	}

	public static void SetMaxDeltaTime(float maxDeltaTime)
	{
		Time.maximumDeltaTime = maxDeltaTime;
		Time.maximumParticleDeltaTime = maxDeltaTime;
		s_maxDeltaTime = maxDeltaTime;
	}
}
