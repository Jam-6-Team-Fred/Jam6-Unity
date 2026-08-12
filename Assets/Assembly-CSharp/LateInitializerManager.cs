using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LateInitializerManager : MonoBehaviour
{
	public enum Priority
	{
		Low = 1,
		Normal = 8,
		High = 33
	}

	private static LateInitializerManager s_instance = null;

	private static List<ILateInitializer> s_lateInitializers = new List<ILateInitializer>(1024);

	private static Priority s_priority = Priority.Normal;

	private static Stopwatch s_stopwatch = new Stopwatch();

	private static bool s_pauseOnInitialization = false;

	private static bool s_paused = false;

	public static Priority priority
	{
		get
		{
			return s_priority;
		}
		set
		{
			s_priority = value;
		}
	}

	public static bool isDoneInitializing => s_lateInitializers.Count == 0;

	public static bool pauseOnInitialization
	{
		get
		{
			return s_pauseOnInitialization;
		}
		set
		{
			if (s_pauseOnInitialization != value)
			{
				s_pauseOnInitialization = value;
				if (s_lateInitializers.Count > 0 && s_pauseOnInitialization && !s_paused)
				{
					OWTime.Pause(OWTime.PauseType.Initializing);
					SpinnerUI.Show();
					s_paused = true;
				}
				else if (!s_pauseOnInitialization && s_paused)
				{
					OWTime.Unpause(OWTime.PauseType.Initializing);
					SpinnerUI.Hide();
					s_paused = false;
				}
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InitializeOnSceneLoad()
	{
		if (!(s_instance != null))
		{
			GameObject obj = new GameObject("LateInitializer");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.hideFlags = HideFlags.NotEditable;
			s_instance = obj.AddComponent<LateInitializerManager>();
		}
	}

	public static void RegisterLateInitializer(ILateInitializer lateInitializer)
	{
		s_lateInitializers.Add(lateInitializer);
		if (s_instance != null && !s_instance.enabled)
		{
			s_instance.enabled = true;
		}
		if (s_pauseOnInitialization && !s_paused)
		{
			OWTime.Pause(OWTime.PauseType.Initializing);
			SpinnerUI.Show();
			s_paused = true;
		}
	}

	public static void UnregisterLateInitializer(ILateInitializer lateInitializer)
	{
		s_lateInitializers.Remove(lateInitializer);
		if (s_lateInitializers.Count == 0 && s_instance != null && s_instance.enabled)
		{
			s_instance.enabled = false;
		}
		if (s_lateInitializers.Count == 0 && s_paused)
		{
			OWTime.Unpause(OWTime.PauseType.Initializing);
			SpinnerUI.Hide();
			s_paused = false;
		}
	}

	private void Update()
	{
		if (s_lateInitializers.Count == 0)
		{
			base.enabled = false;
			if (s_paused)
			{
				OWTime.Unpause(OWTime.PauseType.Initializing);
				SpinnerUI.Hide();
				s_paused = false;
			}
			return;
		}
		s_stopwatch.Reset();
		s_stopwatch.Start();
		while (s_lateInitializers.Count > 0 && s_stopwatch.Elapsed.TotalMilliseconds < (double)s_priority)
		{
			ILateInitializer lateInitializer = s_lateInitializers[0];
			s_lateInitializers.RemoveAt(0);
			try
			{
				lateInitializer.LateInitialize();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		s_stopwatch.Stop();
	}
}
