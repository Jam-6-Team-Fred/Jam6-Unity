public class RingRiverCollisionChecker : CustomCollisionChecker
{
	protected override OWCustomCollider FindCustomCollider()
	{
		return Locator.GetRingRiverFluidVolume().GetCollider();
	}
}
