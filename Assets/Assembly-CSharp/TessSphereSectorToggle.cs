using UnityEngine;

[RequireComponent(typeof(TessellatedSphereRenderer))]
public class TessSphereSectorToggle : MonoBehaviour
{
	private TessellatedSphereRenderer _renderer;

	[SerializeField]
	private Sector _sector;

	private bool _inMapView;

	private void Awake()
	{
		_renderer = GetComponent<TessellatedSphereRenderer>();
	}

	private void Start()
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("TessSphereSectorToggle has no specified Sector!", this);
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	private void OnDestroy()
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void OnSectorOccupantsUpdated()
	{
		if (_inMapView)
		{
			return;
		}
		if ((bool)_sector)
		{
			if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe) && !_renderer.enabled)
			{
				_renderer.enabled = true;
			}
			else if (!_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe) && _renderer.enabled)
			{
				_renderer.enabled = false;
			}
		}
		else
		{
			_renderer.enabled = true;
		}
	}

	private void OnEnterMapView()
	{
		_inMapView = true;
		if (_renderer.enabled)
		{
			_renderer.enabled = false;
		}
	}

	private void OnExitMapView()
	{
		_inMapView = false;
		OnSectorOccupantsUpdated();
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
