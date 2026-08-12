using System.Collections.Generic;

public class ShipLogEntryOrderComparer : IComparer<ShipLogEntry>
{
	public int Compare(ShipLogEntry x, ShipLogEntry y)
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
