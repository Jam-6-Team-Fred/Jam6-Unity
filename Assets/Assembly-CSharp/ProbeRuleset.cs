using UnityEngine;

public class ProbeRuleset : RulesetVolume
{
	[SerializeField]
	private bool _overrideProbeSpeed = true;

	[SerializeField]
	private float _probeSpeedOverride;

	[Space]
	[SerializeField]
	private bool _overrideLanternRange;

	[SerializeField]
	private float _lanternRange;

	[Space]
	[SerializeField]
	private bool _ignoreAnchor;

	public bool GetUseProbeSpeedOverride()
	{
		return _overrideProbeSpeed;
	}

	public float GetProbeSpeedOverride()
	{
		return _probeSpeedOverride;
	}

	public bool GetOverrideLanternRange()
	{
		return _overrideLanternRange;
	}

	public float GetLanternRange()
	{
		return _lanternRange;
	}

	public bool GetIgnoreAnchor()
	{
		return _ignoreAnchor;
	}
}
