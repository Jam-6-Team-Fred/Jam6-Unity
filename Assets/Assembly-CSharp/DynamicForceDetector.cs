public class DynamicForceDetector : ForceDetector
{
	private bool _isPlayerDetector;

	private ForceVolume _inheritedVolume;

	protected override void Awake()
	{
		base.Awake();
		if (base.gameObject.CompareTag("PlayerDetector"))
		{
			_isPlayerDetector = true;
		}
	}

	protected override void OnVolumeActivated(PriorityVolume pVol)
	{
		ForceVolume forceVolume = pVol as ForceVolume;
		if (!(forceVolume == null))
		{
			if (forceVolume.IsInheritable())
			{
				_activeInheritedDetector = forceVolume.GetAttachedForceDetector();
				_inheritedVolume = forceVolume;
			}
			if (_isPlayerDetector && forceVolume.GetPlayGravityCrystalAudio())
			{
				Locator.GetPlayerAudioController().OnEnterGravityAudioVolume();
			}
		}
	}

	protected override void OnVolumeDeactivated(PriorityVolume pVol)
	{
		ForceVolume forceVolume = pVol as ForceVolume;
		if (!(forceVolume == null))
		{
			if (forceVolume.IsInheritable() && forceVolume.Equals(_inheritedVolume))
			{
				_activeInheritedDetector = null;
				_inheritedVolume = null;
			}
			if (_isPlayerDetector && forceVolume.GetPlayGravityCrystalAudio())
			{
				Locator.GetPlayerAudioController().OnExitGravityAudioVolume();
			}
		}
	}
}
