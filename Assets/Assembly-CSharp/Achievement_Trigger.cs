using UnityEngine;

public class Achievement_Trigger : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	[SerializeField]
	private Achievements.Type _achievement;

	[SerializeField]
	private bool _scoutTriggered;

	private void Start()
	{
		_triggerVolume.OnEntry += OnEntry;
	}

	protected virtual void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") || (_scoutTriggered && hitObj.CompareTag("ProbeDetector")))
		{
			Achievements.Earn(_achievement);
			base.gameObject.SetActive(value: false);
		}
	}
}
