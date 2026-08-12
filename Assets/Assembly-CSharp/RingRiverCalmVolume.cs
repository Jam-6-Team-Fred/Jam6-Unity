using UnityEngine;

public class RingRiverCalmVolume : MonoBehaviour
{
	private OWTriggerVolume _triggerVolume;

	private bool _playerInside;

	private bool _probeInside;

	public bool ContainsDetector(Detector.Name name)
	{
		if (!_playerInside || name != Detector.Name.Player)
		{
			if (_probeInside)
			{
				return name == Detector.Name.Probe;
			}
			return false;
		}
		return true;
	}

	private void Awake()
	{
		_triggerVolume = GetComponent<OWTriggerVolume>();
		if (_triggerVolume != null)
		{
			_triggerVolume.OnEntry += OnEntry;
			_triggerVolume.OnExit += OnExit;
		}
	}

	private void Start()
	{
		Locator.GetRingRiverFluidVolume().RegisterCalmVolume(this);
	}

	private void OnDestroy()
	{
		if (_triggerVolume != null)
		{
			_triggerVolume.OnEntry -= OnEntry;
			_triggerVolume.OnExit -= OnExit;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInside = true;
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInside = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInside = false;
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInside = false;
		}
	}
}
