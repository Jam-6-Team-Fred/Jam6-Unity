using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class ReferenceFrameBlockerVolume : MonoBehaviour
{
	private ReferenceFrameTracker _rfTracker;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
	}

	private void Start()
	{
		_rfTracker = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
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
			_rfTracker.AddBlocker();
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_rfTracker.RemoveBlocker();
		}
	}
}
