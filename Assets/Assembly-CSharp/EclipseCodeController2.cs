using UnityEngine;

public class EclipseCodeController2 : MonoBehaviour
{
	[SerializeField]
	private SingleLightSensor[] _codeLightSensor;

	[SerializeField]
	private RotaryDial[] _dials;

	[SerializeField]
	private AbstractDoor _frontDoor;

	[SerializeField]
	private int[] _code;

	private void Awake()
	{
		if (_codeLightSensor.Length != _dials.Length || _dials.Length != _code.Length)
		{
			Debug.LogError("No matching number of light sensors and dials.");
		}
		for (int i = 0; i < _codeLightSensor.Length; i++)
		{
			_codeLightSensor[i].OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
			_codeLightSensor[i].OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _codeLightSensor.Length; i++)
		{
			_codeLightSensor[i].OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
			_codeLightSensor[i].OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		}
	}

	private void OnDetectLight()
	{
		CheckForActivation();
	}

	private void OnDetectDarkness()
	{
		CheckForActivation();
		CheckForCode();
	}

	private void CheckForCode()
	{
		bool flag = true;
		for (int i = 0; i < _dials.Length; i++)
		{
			flag = flag && _dials[i].GetSymbolSelected() == _code[i];
		}
		if (flag && _frontDoor != null)
		{
			_frontDoor.Open();
		}
	}

	private void CheckForActivation()
	{
		int num = 0;
		for (int i = 0; i < _codeLightSensor.Length; i++)
		{
			num += (_codeLightSensor[i].IsIlluminated() ? 1 : 0);
		}
		if (num == 1)
		{
			if (_frontDoor != null && _frontDoor.IsOpen())
			{
				_frontDoor.Close();
			}
			for (int j = 0; j < _codeLightSensor.Length; j++)
			{
				if (_codeLightSensor[j].IsIlluminated())
				{
					_dials[j].StartRotation();
				}
			}
		}
		else
		{
			for (int k = 0; k < _codeLightSensor.Length; k++)
			{
				_dials[k].StopRotation();
			}
		}
	}
}
