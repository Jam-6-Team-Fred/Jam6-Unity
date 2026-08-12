public class RiverHeatHazardVolume : HazardVolume
{
	public override HazardType GetHazardType()
	{
		return HazardType.HEAT;
	}

	public override float GetDamagePerSecond(HazardDetector detector)
	{
		if (detector.CompareName(Detector.Name.Player) && PlayerState.IsRidingRaft())
		{
			return 0f;
		}
		return _damagePerSecond;
	}
}
