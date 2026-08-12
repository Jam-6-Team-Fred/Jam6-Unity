using UnityEngine;

[AddComponentMenu("Sectors/Multi-Sector Cull Group", 200)]
public class MultiSectorCullGroup : SectorCullGroup
{
	[SerializeField]
	private Sector _secondSector;

	protected override void Awake()
	{
		base.Awake();
		if ((bool)_secondSector)
		{
			_secondSector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("SectorCullGroup has no specified Sector!", this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)_secondSector)
		{
			_secondSector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
	}

	protected override bool ShouldBeVisible()
	{
		if (_inMapView || _isFastForwarding)
		{
			return false;
		}
		if ((bool)_sector && (bool)_secondSector)
		{
			if (!_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
			{
				return _secondSector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
			}
			return true;
		}
		return true;
	}
}
