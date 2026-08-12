using System.Collections.Generic;

public class FragmentDataComparer : IComparer<FragmentData>
{
	public int Compare(FragmentData x, FragmentData y)
	{
		if (x.totalHitCount == y.totalHitCount)
		{
			return 0;
		}
		if (x.totalHitCount > y.totalHitCount)
		{
			return 1;
		}
		return -1;
	}
}
