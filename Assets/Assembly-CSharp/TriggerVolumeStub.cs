using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class TriggerVolumeStub : MonoBehaviour
{
	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
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
		hitObj.CompareTag("PlayerDetector");
	}

	private void OnExit(GameObject hitObj)
	{
		hitObj.CompareTag("PlayerDetector");
	}
}
