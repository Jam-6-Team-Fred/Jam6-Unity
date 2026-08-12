using UnityEngine;

[AddComponentMenu("Audio/Audio Detector", 400)]
public class AudioDetector : PriorityDetector
{
	protected override void OnVolumeAdded(EffectVolume volume)
	{
	}

	protected override void OnVolumeRemoved(EffectVolume volume)
	{
	}

	protected override void OnVolumeActivated(PriorityVolume volume)
	{
		AudioVolume audioVolume = volume as AudioVolume;
		if (audioVolume != null)
		{
			audioVolume.Activate();
		}
	}

	protected override void OnVolumeDeactivated(PriorityVolume volume)
	{
		AudioVolume audioVolume = volume as AudioVolume;
		if (audioVolume != null)
		{
			audioVolume.Deactivate();
		}
	}

	public override void AddVolume(EffectVolume eVol)
	{
		if (eVol as AudioVolume != null)
		{
			base.AddVolume(eVol);
		}
	}

	public override void RemoveVolume(EffectVolume eVol)
	{
		if (eVol as AudioVolume != null)
		{
			base.RemoveVolume(eVol);
		}
	}

	public void DeactivateAllVolumes(float fadeSeconds)
	{
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			((AudioVolume)_activeVolumes[i]).Deactivate(fadeSeconds);
		}
	}
}
