using UnityEngine;

public class FloodToggle : MonoBehaviour
{
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private GameObject[] _targets = new GameObject[0];

	[SerializeField]
	private OWTriggerVolume[] _volumes = new OWTriggerVolume[0];

	[SerializeField]
	private bool _deactivateOnFlood = true;

	[SerializeField]
	private bool _toggleActiveOnAwake;

	private void Awake()
	{
		if (_toggleActiveOnAwake)
		{
			for (int i = 0; i < _targets.Length; i++)
			{
				_targets[i].SetActive(_deactivateOnFlood);
			}
		}
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
			return;
		}
		Debug.LogError("Flood toggle does not have a flood sensor!", this);
		Debug.Break();
	}

	private void Start()
	{
		if (_toggleActiveOnAwake)
		{
			for (int i = 0; i < _volumes.Length; i++)
			{
				_volumes[i].SetTriggerActivation(_deactivateOnFlood);
			}
		}
	}

	private void OnDestroy()
	{
		_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
	}

	private void OnFloodImpact()
	{
		for (int i = 0; i < _targets.Length; i++)
		{
			_targets[i].SetActive(!_deactivateOnFlood);
		}
		for (int j = 0; j < _volumes.Length; j++)
		{
			_volumes[j].SetTriggerActivation(!_deactivateOnFlood);
		}
	}
}
