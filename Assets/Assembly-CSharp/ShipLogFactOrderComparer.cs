using System.Collections.Generic;

public class ShipLogFactOrderComparer : IComparer<ShipLogFact>
{
	public int Compare(ShipLogFact x, ShipLogFact y)
	{
		if (x.GetRevealOrder() == y.GetRevealOrder())
		{
			return 0;
		}
		if (x.GetRevealOrder() > y.GetRevealOrder())
		{
			return 1;
		}
		return -1;
	}
}
