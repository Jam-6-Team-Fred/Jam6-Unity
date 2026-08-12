using UnityEngine;

[AddComponentMenu("Sectors/Sector Renderer LOD Group", 400)]
public class SectorRendererLODGroup : CullGroup, ISectorGroup
{
	[SerializeField]
	private Sector _LODsector;

	[SerializeField]
	private Sector _sectorWhereActive;

	protected override void Awake()
	{
		base.Awake();
		if (_LODsector != null)
		{
			_LODsector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(UpdateState);
		}
		else
		{
			Debug.LogWarning("SectorRendererLODGroup has no specified Sector!", this);
		}
		if (_sectorWhereActive != null)
		{
			_sectorWhereActive.OnSectorOccupantsUpdated += new OWEvent.OWCallback(UpdateState);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_LODsector != null)
		{
			_LODsector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(UpdateState);
		}
		if (_sectorWhereActive != null)
		{
			_sectorWhereActive.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(UpdateState);
		}
	}

	private void UpdateState()
	{
		bool flag = ((_LODsector != null && _sectorWhereActive != null) ? (!_LODsector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe) && _sectorWhereActive.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe)) : ((_LODsector != null) ? (!_LODsector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe)) : (_sectorWhereActive != null && _sectorWhereActive.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))));
		if (IsVisible() != flag)
		{
			SetVisible(flag);
		}
	}

	public Sector GetSector()
	{
		return _LODsector;
	}

	public void SetSector(Sector sectorWhereActive)
	{
		if ((bool)_sectorWhereActive)
		{
			_sectorWhereActive.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(UpdateState);
		}
		_sectorWhereActive = sectorWhereActive;
		if ((bool)_sectorWhereActive)
		{
			_sectorWhereActive.OnSectorOccupantsUpdated += new OWEvent.OWCallback(UpdateState);
		}
		UpdateState();
	}

	public void SetLODSector(Sector lodSector)
	{
		if ((bool)_LODsector)
		{
			_LODsector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(UpdateState);
		}
		_LODsector = lodSector;
		if ((bool)_LODsector)
		{
			_LODsector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(UpdateState);
		}
		UpdateState();
	}
}
