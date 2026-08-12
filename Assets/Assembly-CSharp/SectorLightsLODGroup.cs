using UnityEngine;

[AddComponentMenu("Sectors/Sector Lights LOD Group", 400)]
public class SectorLightsLODGroup : LightsCullGroup, ISectorGroup
{
	[SerializeField]
	private Sector _LODsector;

	[SerializeField]
	private Sector _sectorWhereActive;

	[SerializeField]
	private float _fadeLength = 1f;

	protected override void Awake()
	{
		base.Awake();
		if (_LODsector != null)
		{
			_LODsector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(UpdateState);
		}
		else
		{
			Debug.LogWarning("SectorLightsCullGroup has no specified Sector!", this);
		}
		if (_sectorWhereActive != null)
		{
			_sectorWhereActive.OnSectorOccupantsUpdated += new OWEvent.OWCallback(UpdateState);
		}
	}

	private void OnDestroy()
	{
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
		if (IsShining() != flag)
		{
			SetShining(flag, _fadeLength);
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
