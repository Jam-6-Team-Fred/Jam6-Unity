using UnityEngine;

public class PlaytestEnterStationVolume : MonoBehaviour
{
	private bool _initialized;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_initialized = true;
	}

	private void OnDestroy()
	{
		if (_initialized)
		{
			_trigger.OnEntry -= OnEntry;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			PlaytestDiscoveryPrompt instance = PlaytestDiscoveryPrompt.instance;
			if (instance != null && instance.enterSatStationLoop < 0)
			{
				instance.enterSatStationLoop = TimeLoop.GetLoopCount();
			}
		}
	}
}
