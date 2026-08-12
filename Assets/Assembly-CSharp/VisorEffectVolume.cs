using UnityEngine;

public abstract class VisorEffectVolume : PriorityVolume
{
	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		VisorEffectDetector component = hitObj.GetComponent<VisorEffectDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		VisorEffectDetector component = hitObj.GetComponent<VisorEffectDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
