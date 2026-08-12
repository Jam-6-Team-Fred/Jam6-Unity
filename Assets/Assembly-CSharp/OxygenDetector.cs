public class OxygenDetector : Detector
{
	public delegate void EnterOxygenEvent();

	public delegate void ExitOxygenEvent();

	public event EnterOxygenEvent OnEnterOxygen;

	public event ExitOxygenEvent OnExitOxygen;

	public bool GetDetectOxygen()
	{
		return _activeVolumes.Count > 0;
	}

	public bool GetPlayRefillAudio()
	{
		if (GetDetectOxygen())
		{
			for (int i = 0; i < _activeVolumes.Count; i++)
			{
				if (!((OxygenVolume)_activeVolumes[i]).PlaysRefillAudio())
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool GetDetectTrees()
	{
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			if (((OxygenVolume)_activeVolumes[i]).IsTreeVolume())
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnVolumeAdded(EffectVolume effectVol)
	{
		if (!(effectVol as OxygenVolume == null) && _activeVolumes.Count == 1 && this.OnEnterOxygen != null)
		{
			this.OnEnterOxygen();
		}
	}

	protected override void OnVolumeRemoved(EffectVolume effectVol)
	{
		if (!(effectVol as OxygenVolume == null) && _activeVolumes.Count == 0 && this.OnExitOxygen != null)
		{
			this.OnExitOxygen();
		}
	}
}
