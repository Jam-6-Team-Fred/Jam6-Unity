using UnityEngine;

public class ShipLogFactListTriggerVolume : MonoBehaviour
{
	[SerializeField]
	private string[] _factIDs;

	[SerializeField]
	private bool _player = true;

	[SerializeField]
	private bool _probe;

	private bool _initialized;

	private OWTriggerVolume _trigger;

	private void Start()
	{
		for (int i = 0; i < _factIDs.Length; i++)
		{
			if (!Locator.GetShipLogManager().IsFactRevealed(_factIDs[i]))
			{
				_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
				_trigger.OnEntry += OnEntry;
				_initialized = true;
				break;
			}
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
			for (int i = 0; i < _factIDs.Length; i++)
			{
				Locator.GetShipLogManager().RevealFact(_factIDs[i]);
			}
			_trigger.OnEntry -= OnEntry;
		}
	}
}
