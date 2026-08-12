using UnityEngine;

public abstract class RulesetVolume : EffectVolume
{
	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		RulesetDetector component = hitObj.GetComponent<RulesetDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		RulesetDetector component = hitObj.GetComponent<RulesetDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
