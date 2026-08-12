using UnityEngine;

public class ThrustRuleset : RulesetVolume
{
	[SerializeField]
	private float _thrustLimit = float.PositiveInfinity;

	[SerializeField]
	private bool _nerfJetpackBooster;

	[SerializeField]
	private float _nerfDuration = 0.5f;

	public float GetThrustLimit()
	{
		return _thrustLimit;
	}

	public bool IsJetpackBoosterNerfed()
	{
		return _nerfJetpackBooster;
	}

	public float GetJetpackBoosterNerfDuration()
	{
		return _nerfDuration;
	}
}
