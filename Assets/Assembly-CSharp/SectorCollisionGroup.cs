using UnityEngine;

[AddComponentMenu("Sectors/Sector Collision Group", 200)]
public class SectorCollisionGroup : CollisionGroup, ISectorGroup
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private bool _colliderTimeSlicing = true;

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	private void Start()
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("SectorCollisionGroup has no specified Sector!", this);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		if ((bool)_sector)
		{
			UpdateColliderLOD(_sector.GetOccupantMask(), !_colliderTimeSlicing);
		}
		else
		{
			SetColliderLOD(0, !_colliderTimeSlicing);
		}
	}

	public Sector GetSector()
	{
		return _sector;
	}

	public void SetSector(Sector sector)
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_sector = sector;
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		OnSectorOccupantsUpdated();
	}
}
