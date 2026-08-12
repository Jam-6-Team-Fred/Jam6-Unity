using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class AddToVolumeTrigger : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _addToVolume;

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
		if (!_addToVolume.IsTrackingObject(hitObj))
		{
			_addToVolume.AddObjectToVolume(hitObj);
		}
	}
}
