public class ShipLandingModule : ShipModule
{
	public void DetachAllLegs()
	{
		for (int i = 0; i < _hulls.Length; i++)
		{
			if (_hulls[i] is ShipLandingGear)
			{
				ShipDetachableLeg[] legs = (_hulls[i] as ShipLandingGear).GetLegs();
				for (int j = 0; j < legs.Length; j++)
				{
					legs[j].Detach();
				}
			}
		}
	}
}
