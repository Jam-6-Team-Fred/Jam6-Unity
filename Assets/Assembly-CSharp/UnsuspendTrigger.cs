using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class UnsuspendTrigger : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _unsuspendTrigger;

	[SerializeField]
	private OWTriggerVolume _suspendTrigger;

	private OWRigidbody _body;

	private void Awake()
	{
		_body = GetComponent<OWRigidbody>();
		_unsuspendTrigger.OnEntry += OnEntry;
		_suspendTrigger.OnExit += OnExit;
	}

	private void Start()
	{
		_body.Suspend(_body.GetOrigParentBody());
	}

	private void OnDestroy()
	{
		_unsuspendTrigger.OnEntry -= OnEntry;
		_suspendTrigger.OnExit -= OnExit;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_body.Unsuspend();
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_body.Suspend();
		}
	}
}
