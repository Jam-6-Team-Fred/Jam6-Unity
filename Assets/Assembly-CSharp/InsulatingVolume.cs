using UnityEngine;

public class InsulatingVolume : EffectVolume
{
	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		HazardDetector component = hitObj.GetComponent<HazardDetector>();
		if (component != null && !component.CompareName(Detector.Name.Ship))
		{
			component.AddInsulatingVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		HazardDetector component = hitObj.GetComponent<HazardDetector>();
		if (component != null && !component.CompareName(Detector.Name.Ship))
		{
			component.RemoveInsulatingVolume(this);
		}
	}
}
