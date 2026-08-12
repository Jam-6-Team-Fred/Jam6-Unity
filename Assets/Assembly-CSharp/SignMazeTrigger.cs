using UnityEngine;

public class SignMazeTrigger : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _trigger;

	[SerializeField]
	private GameObject _activateObject;

	[SerializeField]
	private OWTriggerVolume _activateTrigger;

	[SerializeField]
	private AudioSignal _activateSignal;

	[SerializeField]
	private AudioSignal _deactivateSignal;

	private void Start()
	{
		if (_activateObject != null)
		{
			_activateObject.SetActive(value: false);
		}
		if (_activateTrigger != null)
		{
			_activateTrigger.SetTriggerActivation(active: false);
		}
		_trigger.OnEntry += OnEntry;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			if (_activateObject != null)
			{
				_activateObject.SetActive(value: true);
			}
			if (_activateTrigger != null)
			{
				_activateTrigger.SetTriggerActivation(active: true);
			}
			_activateSignal.SetSignalActivation(active: true);
			_deactivateSignal.SetSignalActivation(active: false);
			_trigger.SetTriggerActivation(active: false);
		}
	}
}
