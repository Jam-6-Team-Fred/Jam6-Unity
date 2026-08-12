public class PlayerSectorDetector : SectorDetector
{
	private int _brambleDimensionCount;

	private bool _inBrambleDimension;

	private bool _inVesselDimension;

	private Sector _lastExitedBrambleDimension;

	public bool InVesselDimension()
	{
		return _inVesselDimension;
	}

	public bool InBrambleDimension()
	{
		return _brambleDimensionCount > 0;
	}

	public Sector GetLastExitedBrambleDimension()
	{
		return _lastExitedBrambleDimension;
	}

	protected override void OnAddSector(Sector sector)
	{
		bool num = sector.GetIDString() == "GDInterior";
		if (sector.IsBrambleDimension())
		{
			if (!_inBrambleDimension)
			{
				_inBrambleDimension = true;
				GlobalMessenger.FireEvent("PlayerEnterBrambleDimension");
			}
			_brambleDimensionCount++;
		}
		if (sector.GetName() == Sector.Name.VesselDimension)
		{
			_inVesselDimension = true;
		}
		if (num)
		{
			GlobalMessenger.FireEvent("PlayerEnterGiantsDeep");
		}
	}

	protected override void OnRemoveSector(Sector sector)
	{
		bool num = sector.GetIDString() == "GDInterior";
		if (sector.IsBrambleDimension())
		{
			_lastExitedBrambleDimension = sector;
			_brambleDimensionCount--;
			base.enabled = true;
		}
		if (sector.GetName() == Sector.Name.VesselDimension)
		{
			_inVesselDimension = false;
		}
		if (num)
		{
			GlobalMessenger.FireEvent("PlayerExitGiantsDeep");
		}
	}

	private void Update()
	{
		if (_inBrambleDimension && _brambleDimensionCount == 0)
		{
			_inBrambleDimension = false;
			GlobalMessenger.FireEvent("PlayerExitBrambleDimension");
		}
		base.enabled = false;
	}

	public void RemoveFromAllSectors()
	{
		while (_sectorList.Count > 0)
		{
			_sectorList[0].GetTriggerVolume().RemoveObjectFromVolume(base.gameObject);
		}
	}
}
