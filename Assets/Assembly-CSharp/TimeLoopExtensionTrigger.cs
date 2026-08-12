using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class TimeLoopExtensionTrigger : MonoBehaviour
{
	[SerializeField]
	private float _rewindToSecondsRemaining = 385f;

	[SerializeField]
	private float _extraSecondsInLighthouse = 100f;

	[SerializeField]
	private bool _ignoreEarlyEntries;

	[SerializeField]
	private bool _ignoreAfterLighthouseCollapse;

	[Space]
	[SerializeField]
	private TimeLoopExtensionTrigger[] _linkedTriggersToDisable;

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

	public void DisableTrigger()
	{
		_trigger.OnEntry -= OnEntry;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (!hitObj.CompareTag("PlayerDetector"))
		{
			return;
		}
		if (_ignoreEarlyEntries && TimeLoop.GetSecondsRemaining() >= _rewindToSecondsRemaining)
		{
			Debug.Log("Early time loop extension trigger entry ignored");
			return;
		}
		DisableTrigger();
		for (int i = 0; i < _linkedTriggersToDisable.Length; i++)
		{
			_linkedTriggersToDisable[i].DisableTrigger();
		}
		bool flag = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().hasLighthouseCollapsed;
		bool flag2 = false;
		DreamWorldController dreamWorldController = Locator.GetDreamWorldController();
		if (dreamWorldController != null && dreamWorldController.IsInDream())
		{
			flag2 = dreamWorldController.IsPlayerSleepingAtLocation(DreamArrivalPoint.Location.Zone2);
		}
		if (!Locator.GetGlobalMusicController().IsEndTimesPlaying() && !(flag2 && flag) && !(_ignoreAfterLighthouseCollapse && flag))
		{
			if (flag2)
			{
				_rewindToSecondsRemaining += _extraSecondsInLighthouse;
			}
			if (TimeLoop.GetSecondsRemaining() < _rewindToSecondsRemaining)
			{
				Debug.Log("Rewinding Time Loop to " + _rewindToSecondsRemaining + " seconds remaining.");
				TimeLoop.SetSecondsRemaining(_rewindToSecondsRemaining);
			}
		}
	}
}
