using System.Collections.Generic;
using UnityEngine;

public class LightCode
{
	public const float MAX_SHORT_INTERVAL = 0.7f;

	public const float MAX_LONG_INTERVAL = 3f;

	private static float AVG_SHORT_INTERVAL;

	private static float AVG_LONG_INTERVAL;

	private const bool DARK = false;

	private const bool LIGHT = true;

	private const bool SHORT = false;

	private const bool LONG = true;

	private static List<LightCode> _lightCodes;

	public LightCodeName name;

	private LightPulse[] pulses;

	public static LightCode GetLightCode(LightCodeName name)
	{
		if (_lightCodes == null)
		{
			PopulateList();
		}
		for (int i = 0; i < _lightCodes.Count; i++)
		{
			if (name == _lightCodes[i].name)
			{
				return _lightCodes[i];
			}
		}
		Debug.LogError(string.Concat("Light code ", name, " has not been defined."));
		return null;
	}

	public static List<LightCode> GetAllLightCodes()
	{
		if (_lightCodes == null)
		{
			PopulateList();
		}
		return _lightCodes;
	}

	private static void PopulateList()
	{
		LightCode item = new LightCode(LightCodeName.WAKE, new LightPulse[5]
		{
			new LightPulse(illuminated: true, isLong: true),
			new LightPulse(illuminated: false, isLong: false),
			new LightPulse(illuminated: true, isLong: true),
			new LightPulse(illuminated: false, isLong: false),
			new LightPulse(illuminated: true, isLong: false)
		});
		LightCode item2 = new LightCode(LightCodeName.DAY, new LightPulse[5]
		{
			new LightPulse(illuminated: true, isLong: false),
			new LightPulse(illuminated: false, isLong: false),
			new LightPulse(illuminated: true, isLong: false),
			new LightPulse(illuminated: false, isLong: false),
			new LightPulse(illuminated: true, isLong: true)
		});
		LightCode item3 = new LightCode(LightCodeName.FAST, new LightPulse[4]
		{
			new LightPulse(illuminated: true, isLong: false),
			new LightPulse(illuminated: false, isLong: false),
			new LightPulse(illuminated: true, isLong: false),
			new LightPulse(illuminated: false, isLong: false)
		});
		_lightCodes = new List<LightCode>();
		_lightCodes.Add(item);
		_lightCodes.Add(item2);
		_lightCodes.Add(item3);
		AVG_SHORT_INTERVAL = 0.35f;
		AVG_LONG_INTERVAL = 1.8499999f;
	}

	public LightCode(LightCodeName name, LightPulse[] pulses)
	{
		this.name = name;
		this.pulses = pulses;
	}

	public float PulseLength(int index)
	{
		if (!pulses[index].isLong)
		{
			return AVG_SHORT_INTERVAL;
		}
		return AVG_LONG_INTERVAL;
	}

	public bool isLight(int index)
	{
		return pulses[index].illuminated;
	}

	public int Count()
	{
		return pulses.Length;
	}

	public LightCodeName ReverseName()
	{
		return name + 1;
	}

	public bool CheckForMatch(LightPulse pulse, int index)
	{
		return pulse.Equals(pulses[index]);
	}
}
