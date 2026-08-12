using System.Collections.Generic;
using UnityEngine;

public class TrackedLayer<T> where T : PriorityVolume
{
	public bool isActive;

	public List<T> volumes = new List<T>(8);

	public void AddVolume(T volumeToAdd)
	{
		for (int i = 0; i < volumes.Count; i++)
		{
			if (volumeToAdd.GetPriority() >= volumes[i].GetPriority())
			{
				volumes.Insert(i, volumeToAdd);
				return;
			}
		}
		volumes.Add(volumeToAdd);
	}

	public bool RemoveVolume(T volumeToRemove)
	{
		return volumes.Remove(volumeToRemove);
	}

	public int GetHighestPriority()
	{
		if (volumes.Count > 0)
		{
			return volumes[0].GetPriority();
		}
		return -1;
	}

	public void Sort()
	{
		volumes.Sort(CompareByPriority);
	}

	private static int CompareByPriority(T x, T y)
	{
		if ((Object)x == (Object)null)
		{
			if ((Object)y == (Object)null)
			{
				return 0;
			}
			return -1;
		}
		if ((Object)y == (Object)null)
		{
			return 1;
		}
		return y.GetPriority().CompareTo(x.GetPriority());
	}
}
