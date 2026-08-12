using UnityEngine;

public class DestroyOnEnterTrigger : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _trigger;

	private void Awake()
	{
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
			Object.Destroy(base.gameObject);
		}
	}
}
