using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class QuantumLockTrigger : MonoBehaviour
{
	[SerializeField]
	private QuantumObject _quantumObject;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
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
			_quantumObject.SetIsQuantum(isQuantum: false);
		}
	}
}
