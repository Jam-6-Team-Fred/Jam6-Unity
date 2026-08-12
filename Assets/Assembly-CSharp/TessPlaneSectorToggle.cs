using UnityEngine;

[RequireComponent(typeof(TessellatedPlaneRenderer))]
public class TessPlaneSectorToggle : MonoBehaviour
{
	private TessellatedPlaneRenderer _renderer;

	[SerializeField]
	private Sector _sector;

	private void Awake()
	{
		_renderer = GetComponent<TessellatedPlaneRenderer>();
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("TessPlaneSectorToggle has no specified Sector!", this);
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
		_renderer.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
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
