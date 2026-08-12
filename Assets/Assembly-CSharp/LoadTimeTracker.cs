using UnityEngine;

public class LoadTimeTracker : MonoBehaviour
{
	private static float _initLoadTime;

	private static float _latestLoadTime;

	private static bool _displayLoadTime;

	private void Awake()
	{
		GlobalMessenger.AddListener("LoadFromMenu", OnLoadFromMenu);
		GlobalMessenger.AddListener("RestartTimeLoop", OnRestartTimeLoop);
		GlobalMessenger.AddListener("ResetSimulation", OnResetSimulation);
		if (_displayLoadTime)
		{
			_displayLoadTime = false;
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("LoadFromMenu", OnLoadFromMenu);
		GlobalMessenger.RemoveListener("RestartTimeLoop", OnRestartTimeLoop);
		GlobalMessenger.RemoveListener("ResetSimulation", OnResetSimulation);
	}

	public static float GetLatestLoadTime()
	{
		return _latestLoadTime;
	}

	private void Update()
	{
		_latestLoadTime = Time.realtimeSinceStartup - _initLoadTime;
		MonoBehaviour.print("LOAD TIME: " + _latestLoadTime + "      (First Update)");
		base.enabled = false;
	}

	private void OnLoadFromMenu()
	{
		_initLoadTime = Time.realtimeSinceStartup;
		_displayLoadTime = true;
	}

	private void OnRestartTimeLoop()
	{
		_initLoadTime = Time.realtimeSinceStartup;
		_displayLoadTime = true;
	}

	private void OnResetSimulation()
	{
		_initLoadTime = Time.realtimeSinceStartup;
		_displayLoadTime = true;
	}
}
