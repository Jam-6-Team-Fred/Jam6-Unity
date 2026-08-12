using UnityEngine;

public class ProbeSafetyVolume : EffectVolume
{
	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		ProbeDestructionDetector component = hitObj.GetComponent<ProbeDestructionDetector>();
		if (component != null)
		{
			component.AddSafetyVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		ProbeDestructionDetector component = hitObj.GetComponent<ProbeDestructionDetector>();
		if (component != null)
		{
			component.RemoveSafetyVolume(this);
		}
	}
}
