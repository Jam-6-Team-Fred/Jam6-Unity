public class GhostSensorData
{
	public bool isPlayerVisible;

	public bool isPlayerHeldLanternVisible;

	public bool isPlayerDroppedLanternVisible;

	public bool isPlayerHoldingLantern;

	public bool isIlluminated;

	public bool isIlluminatedByPlayer;

	public bool inContactWithPlayer;

	public bool isPlayerOccluded;

	public bool isPlayerIlluminated;

	public bool isPlayerIlluminatedByUs;

	public bool isPlayerInGuardVolume;

	public bool knowsPlayerVelocity
	{
		get
		{
			if (!isPlayerVisible)
			{
				return isPlayerHeldLanternVisible;
			}
			return true;
		}
	}

	public void CopyFromOther(GhostSensorData other)
	{
		isPlayerVisible = other.isPlayerVisible;
		isPlayerHeldLanternVisible = other.isPlayerHeldLanternVisible;
		isPlayerDroppedLanternVisible = other.isPlayerDroppedLanternVisible;
		isPlayerHoldingLantern = other.isPlayerHoldingLantern;
		isIlluminatedByPlayer = other.isIlluminatedByPlayer;
		inContactWithPlayer = other.inContactWithPlayer;
		isPlayerOccluded = other.isPlayerOccluded;
		isPlayerIlluminated = other.isPlayerIlluminated;
		isPlayerIlluminatedByUs = other.isPlayerIlluminatedByUs;
		isPlayerInGuardVolume = other.isPlayerInGuardVolume;
	}
}
