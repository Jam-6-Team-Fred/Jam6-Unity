using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Sectors/Sector Manager", 0)]
public class SectorManager : MonoBehaviour
{
	private static List<Sector> s_sectors = new List<Sector>(1024);

	private static List<SectorDetector> s_sectorDetectors = new List<SectorDetector>(64);

	public static void RegisterSector(Sector sector)
	{
		s_sectors.SafeAdd(sector);
	}

	public static void UnregisterSector(Sector sector)
	{
		s_sectors.Remove(sector);
	}

	public static void RegisterSectorDetector(SectorDetector sectorDetector)
	{
		if (!s_sectorDetectors.SafeAdd(sectorDetector))
		{
			return;
		}
		for (int i = 0; i < s_sectors.Count; i++)
		{
			if (s_sectors[i].IsRootSector())
			{
				s_sectors[i].TrackDetector(sectorDetector);
			}
		}
	}

	public static void UnregisterSectorDetector(SectorDetector sectorDetector)
	{
		if (!s_sectorDetectors.Remove(sectorDetector))
		{
			return;
		}
		for (int i = 0; i < s_sectors.Count; i++)
		{
			if (s_sectors[i].IsRootSector())
			{
				s_sectors[i].RemoveOccupant(sectorDetector);
				s_sectors[i].UntrackDetector(sectorDetector);
			}
		}
	}

	public static List<Sector> GetRegisteredSectors()
	{
		return s_sectors;
	}

	public static List<SectorDetector> GetRegisteredSectorDetectors()
	{
		return s_sectorDetectors;
	}
}
