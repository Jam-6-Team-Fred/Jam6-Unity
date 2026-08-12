using UnityEngine;

public class FlashlightPromptTrigger : MonoBehaviour
{
	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetAddComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			GlobalMessenger.FireEvent("EnterFlashlightPromptTrigger");
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			GlobalMessenger.FireEvent("ExitFlashlightPromptTrigger");
		}
	}
}
