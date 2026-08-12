using UnityEngine;

public abstract class SectorEventLink : MonoBehaviour
{
	[SerializeField]
	protected Sector _sector;

	protected virtual void Awake()
	{
		if (_sector == null)
		{
			_sector = GetComponent<Sector>();
			Transform transform = base.transform;
			while (_sector == null && transform.parent != null)
			{
				Sector component = transform.parent.GetComponent<Sector>();
				if (component != null)
				{
					_sector = component;
					break;
				}
				transform = transform.parent.transform;
			}
		}
		if (_sector != null)
		{
			_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OccupantEnterSector);
			_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OccupantExitSector);
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(SectorOccupantsUpdated);
		}
		else
		{
			Debug.LogError("SectorEventLink Sector not defined");
		}
	}

	protected abstract void SectorOccupantsUpdated();

	protected abstract void OccupantEnterSector(SectorDetector detector);

	protected abstract void OccupantExitSector(SectorDetector detector);
}
