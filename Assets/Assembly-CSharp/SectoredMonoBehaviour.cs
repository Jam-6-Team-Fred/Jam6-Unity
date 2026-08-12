using UnityEngine;

public abstract class SectoredMonoBehaviour : MonoBehaviour
{
	[SerializeField]
	protected Sector _sector;

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	protected virtual void Awake()
	{
		if (_sector == null)
		{
			_sector = GetComponentInParent<Sector>();
		}
		if (_sector != null)
		{
			_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
			_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
	}

	protected virtual void OnDestroy()
	{
		if (_sector != null)
		{
			_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
			_sector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
	}

	public Sector GetSector()
	{
		return _sector;
	}

	public void SetSector(Sector sector)
	{
		if (!(_sector == sector))
		{
			if (_sector != null)
			{
				_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
				_sector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
				_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
			}
			if (sector != null)
			{
				sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
				sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
				sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
			}
			Sector sector2 = _sector;
			_sector = sector;
			OnChangeSector(sector2, _sector);
		}
	}

	protected virtual void OnChangeSector(Sector oldSector, Sector newSector)
	{
	}

	protected virtual void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
	}

	protected virtual void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
	}

	protected virtual void OnSectorOccupantsUpdated()
	{
	}
}
