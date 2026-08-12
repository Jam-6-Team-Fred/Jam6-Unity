public class LightSourceDetector : Detector
{
	public delegate void LightVolumeEnterEvent(LightSourceVolume volume);

	public delegate void LightVolumeExitEvent(LightSourceVolume volume);

	public event LightVolumeEnterEvent OnLightVolumeEnter;

	public event LightVolumeExitEvent OnLightVolumeExit;

	protected override void OnVolumeAdded(EffectVolume volume)
	{
		LightSourceVolume lightSourceVolume = volume as LightSourceVolume;
		if (lightSourceVolume != null && this.OnLightVolumeEnter != null)
		{
			this.OnLightVolumeEnter(lightSourceVolume);
		}
	}

	protected override void OnVolumeRemoved(EffectVolume volume)
	{
		LightSourceVolume lightSourceVolume = volume as LightSourceVolume;
		if (lightSourceVolume != null && this.OnLightVolumeExit != null)
		{
			this.OnLightVolumeExit(lightSourceVolume);
		}
	}

	public override void AddVolume(EffectVolume eVol)
	{
		if (eVol as LightSourceVolume != null)
		{
			base.AddVolume(eVol);
		}
	}

	public override void RemoveVolume(EffectVolume eVol)
	{
		if (eVol as LightSourceVolume != null)
		{
			base.RemoveVolume(eVol);
		}
	}
}
