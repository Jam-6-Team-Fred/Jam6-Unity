using UnityEngine;

public class ProbeDestructionVolume : EffectVolume
{
	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		ProbeDestructionDetector component = hitObj.GetComponent<ProbeDestructionDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		ProbeDestructionDetector component = hitObj.GetComponent<ProbeDestructionDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
