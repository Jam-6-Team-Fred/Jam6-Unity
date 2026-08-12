using UnityEngine;

public class EclipseCodeController : MonoBehaviour
{
	[SerializeField]
	private SingleLightSensor _codeLightSensor;

	[SerializeField]
	private RotaryDial _dial;

	[SerializeField]
	private AbstractDoor _frontDoor;

	private void Awake()
	{
		_codeLightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_codeLightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_codeLightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_codeLightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void FixedUpdate()
	{
	}

	private void OnDetectLight()
	{
		if (_frontDoor.IsOpen())
		{
			_frontDoor.Close();
		}
		_dial.StartRotation();
	}

	private void OnDetectDarkness()
	{
		_dial.StopRotation();
		base.enabled = false;
	}
}
