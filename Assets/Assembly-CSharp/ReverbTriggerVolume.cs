using UnityEngine;

[AddComponentMenu("Audio/Reverb Trigger Volume", 400)]
[RequireComponent(typeof(OWTriggerVolume))]
public class ReverbTriggerVolume : MonoBehaviour
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
		if (hitObj.CompareTag("PlayerCameraDetector"))
		{
			Locator.GetAudioMixer().OnEnterReverbVolume();
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerCameraDetector"))
		{
			Locator.GetAudioMixer().OnExitReverbVolume();
		}
	}
}
