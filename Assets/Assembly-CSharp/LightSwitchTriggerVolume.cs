using UnityEngine;

public class LightSwitchTriggerVolume : LightSwitch
{
	private OWTriggerVolume _trigger;

	private void Start()
	{
		TurnOff();
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			TurnOn();
		}
	}
}
