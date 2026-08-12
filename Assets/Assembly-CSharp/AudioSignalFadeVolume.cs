using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class AudioSignalFadeVolume : MonoBehaviour
{
	[SerializeField]
	private AudioSignal _audioSignal;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetComponent<OWTriggerVolume>();
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
			_audioSignal.SetSignalActivation(active: false);
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_audioSignal.SetSignalActivation(active: true);
		}
	}
}
