using UnityEngine;

public class RingRiverUndertowVolume : MonoBehaviour
{
	[SerializeField]
	private float _depth = 7.5f;

	[SerializeField]
	private float _speed = 30f;

	private OWTriggerVolume _triggerVolume;

	private bool _playerInside;

	private bool _probeInside;

	public float depth => _depth;

	public float speed => _speed;

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
			GlobalMessenger.FireEvent("PlayerEnterUndertowVolume");
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
			GlobalMessenger.FireEvent("PlayerExitUndertowVolume");
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInside = false;
		}
	}
}
