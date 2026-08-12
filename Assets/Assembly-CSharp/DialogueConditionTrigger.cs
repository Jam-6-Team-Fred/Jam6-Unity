using UnityEngine;

public class DialogueConditionTrigger : MonoBehaviour
{
	[SerializeField]
	private string _conditionID = string.Empty;

	[SerializeField]
	private bool _player = true;

	[SerializeField]
	private bool _probe;

	[SerializeField]
	private bool _persistentCondition;

	private bool _initialized;

	private OWTriggerVolume _trigger;

	private void Start()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		if (_trigger != null)
		{
			_trigger.OnEntry += OnEntry;
			_initialized = true;
		}
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
		if ((!_player || hitObj.CompareTag("PlayerDetector")) && (!_probe || hitObj.CompareTag("ProbeDetector")))
		{
			if (_persistentCondition)
			{
				PlayerData.SetPersistentCondition(_conditionID, state: true);
			}
			else
			{
				DialogueConditionManager.SharedInstance.SetConditionState(_conditionID, conditionState: true);
			}
			_trigger.OnEntry -= OnEntry;
		}
	}
}
