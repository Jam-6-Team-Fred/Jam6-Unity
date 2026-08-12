using UnityEngine;

public class SignalRelocationTrigger : MonoBehaviour
{
	[SerializeField]
	private Transform _signalTransform;

	[SerializeField]
	private Transform _targetTransform;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void Update()
	{
		_signalTransform.localPosition = Vector3.MoveTowards(_signalTransform.localPosition, Vector3.zero, Time.deltaTime * 100f);
		if (_signalTransform.localPosition.magnitude < 0.0001f)
		{
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_signalTransform.parent = _targetTransform;
			_trigger.OnEntry -= OnEntry;
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		hitObj.CompareTag("PlayerDetector");
	}
}
