using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class CowerAnimTriggerVolume : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

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
		if (hitObj.CompareTag("ModelShipDetector"))
		{
			_animator.SetBool("Cower", value: true);
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_animator.SetTrigger("ProbeDodge");
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("ModelShipDetector"))
		{
			_animator.SetBool("Cower", value: false);
		}
	}
}
