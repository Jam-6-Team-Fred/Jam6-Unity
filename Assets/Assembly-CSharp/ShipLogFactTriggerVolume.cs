using UnityEngine;
using UnityEngine.Serialization;

public class ShipLogFactTriggerVolume : MonoBehaviour
{
	[FormerlySerializedAs("_dataPointID")]
	[SerializeField]
	private string _factID = string.Empty;

	[SerializeField]
	private bool _player = true;

	[SerializeField]
	private bool _probe;

	private bool _initialized;

	private OWTriggerVolume _trigger;

	private void Start()
	{
		if (Locator.GetShipLogManager().GetFact(_factID) != null && !Locator.GetShipLogManager().IsFactRevealed(_factID))
		{
			_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
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
		if ((!_player || hitObj.CompareTag("PlayerDetector")) && (!_probe || hitObj.CompareTag("ProbeDetector")) && Locator.GetShipLogManager().GetFact(_factID) != null)
		{
			Locator.GetShipLogManager().RevealFact(_factID);
			_trigger.OnEntry -= OnEntry;
		}
	}
}
