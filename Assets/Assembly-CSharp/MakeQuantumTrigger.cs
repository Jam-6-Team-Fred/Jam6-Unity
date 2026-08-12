using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class MakeQuantumTrigger : MonoBehaviour
{
	[SerializeField]
	private QuantumObject[] _quantumObjects;

	[SerializeField]
	private bool _undoOnExit;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		if (_undoOnExit)
		{
			_trigger.OnExit += OnExit;
		}
	}

	private void Start()
	{
		for (int i = 0; i < _quantumObjects.Length; i++)
		{
			_quantumObjects[i].SetIsQuantum(isQuantum: false);
		}
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		if (_undoOnExit)
		{
			_trigger.OnExit -= OnExit;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _quantumObjects.Length; i++)
			{
				_quantumObjects[i].SetIsQuantum(isQuantum: true);
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _quantumObjects.Length; i++)
			{
				_quantumObjects[i].SetIsQuantum(isQuantum: false);
			}
		}
	}
}
