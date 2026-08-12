using UnityEngine;

public class LightDarkDoorController : MonoBehaviour
{
	[SerializeField]
	private bool _openInDarkness;

	[SerializeField]
	private bool _stayOpen;

	[Space]
	[SerializeField]
	private SlidingDoor _door;

	[SerializeField]
	private LightSensor _lightSensor;

	private void Awake()
	{
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void OnDetectLight()
	{
		if (!_openInDarkness)
		{
			_door.Open();
		}
		else if (!_stayOpen)
		{
			_door.Close();
		}
	}

	private void OnDetectDarkness()
	{
		if (_openInDarkness)
		{
			_door.Open();
		}
		else if (!_stayOpen)
		{
			_door.Close();
		}
	}
}
