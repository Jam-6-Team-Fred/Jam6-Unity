using UnityEngine;

[AddComponentMenu("Sectors/Sector Volume Occlusion Group", 200)]
public class SectorVolumeOcclusionGroup : VolumeOcclusionGroup
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private float _fadeLength;

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	protected override void Awake()
	{
		base.Awake();
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("SectorVolumeOcclusionGroup has no specified Sector!", this);
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
		bool flag = _sector == null || _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (IsVisible() != flag)
		{
			SetVisible(flag, _fadeLength);
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
