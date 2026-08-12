using System.Collections.Generic;
using UnityEngine;

public class LightCodeInterpreter : MonoBehaviour
{
	public delegate void CodeEvent(LightCodeName code);

	private LightSensor _lightSensor;

	private List<LightPulse> _lightPulses;

	private List<LightCode> _lightCodes;

	private List<int> index;

	private List<int> reverseIndex;

	private float _lastLightTime;

	private float _lastDarkTime;

	public event CodeEvent OnEnterCode;

	private void Awake()
	{
		_lightSensor = GetComponent<LightSensor>();
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		_lightPulses = new List<LightPulse>();
		_lastDarkTime = Time.time;
		_lightCodes = LightCode.GetAllLightCodes();
		index = new List<int>(_lightCodes.Count);
		reverseIndex = new List<int>(_lightCodes.Count);
		for (int i = 0; i < _lightCodes.Count; i++)
		{
			index.Add(0);
			reverseIndex.Add(_lightCodes[i].Count() - 1);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void OnDetectLight()
	{
		_lastLightTime = Time.time;
		float num = Time.time - _lastDarkTime;
		if (num > 3f)
		{
			_lightPulses.Clear();
			return;
		}
		_lightPulses.Add(new LightPulse(illuminated: false, num));
		CheckForCodeMatches();
	}

	private void OnDetectDarkness()
	{
		_lastDarkTime = Time.time;
		float num = Time.time - _lastLightTime;
		if (num > 3f)
		{
			_lightPulses.Clear();
			return;
		}
		_lightPulses.Add(new LightPulse(illuminated: true, num));
		CheckForCodeMatches();
	}

	private void CheckForCodeMatches()
	{
		for (int i = 0; i < _lightCodes.Count; i++)
		{
			index[i] = 0;
			reverseIndex[i] = _lightCodes[i].Count() - 1;
		}
		for (int j = 0; j < _lightPulses.Count; j++)
		{
			for (int k = 0; k < _lightCodes.Count; k++)
			{
				if (_lightCodes[k].CheckForMatch(_lightPulses[j], index[k]))
				{
					index[k]++;
					if (index[k] >= _lightCodes[k].Count())
					{
						_lightPulses.Clear();
						MonoBehaviour.print("Entered Code: " + _lightCodes[k].name);
						if (this.OnEnterCode != null)
						{
							this.OnEnterCode(_lightCodes[k].name);
						}
						return;
					}
				}
				else
				{
					index[k] = 0;
				}
				if (_lightCodes[k].CheckForMatch(_lightPulses[j], reverseIndex[k]))
				{
					reverseIndex[k]--;
					if (reverseIndex[k] < 0)
					{
						_lightPulses.Clear();
						MonoBehaviour.print("Entered Code: " + _lightCodes[k].ReverseName());
						if (this.OnEnterCode != null)
						{
							this.OnEnterCode(_lightCodes[k].ReverseName());
						}
						return;
					}
				}
				else
				{
					reverseIndex[k] = _lightCodes[k].Count() - 1;
				}
			}
		}
	}
}
