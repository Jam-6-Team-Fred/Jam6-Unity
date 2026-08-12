using UnityEngine;

public class OxygenVolume : EffectVolume
{
	[SerializeField]
	private bool _treeVolume = true;

	[SerializeField]
	private bool _playRefillAudio = true;

	public bool IsTreeVolume()
	{
		return _treeVolume;
	}

	public bool PlaysRefillAudio()
	{
		return _playRefillAudio;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		OxygenDetector component = hitObj.GetComponent<OxygenDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		OxygenDetector component = hitObj.GetComponent<OxygenDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
