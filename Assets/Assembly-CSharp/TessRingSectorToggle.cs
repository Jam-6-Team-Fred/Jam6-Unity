using UnityEngine;

[RequireComponent(typeof(TessellatedRingRenderer))]
public class TessRingSectorToggle : MonoBehaviour
{
	private TessellatedRingRenderer _renderer;

	private int _originalRendererLayer;

	[SerializeField]
	private Sector _sector;

	private void Awake()
	{
		_renderer = GetComponent<TessellatedRingRenderer>();
		_originalRendererLayer = _renderer.gameObject.layer;
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("TessRingSectorToggle has no specified Sector!", this);
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
			bool flag = _sector.ContainsOccupant(DynamicOccupant.Player);
			bool flag2 = _sector.ContainsOccupant(DynamicOccupant.Probe);
			if ((flag || flag2) && !_renderer.enabled)
			{
				_renderer.enabled = true;
			}
			if (!flag && !flag2 && _renderer.enabled)
			{
				_renderer.enabled = false;
			}
			if (Locator.GetProbe().IsLaunched() && !Locator.GetProbe().IsRetrieving() && flag != flag2)
			{
				int num = LayerMask.NameToLayer(flag ? "VisibleToPlayer" : "VisibleToProbe");
				if (_renderer.gameObject.layer != num)
				{
					_renderer.gameObject.layer = num;
				}
			}
			else if (_renderer.gameObject.layer != _originalRendererLayer)
			{
				_renderer.gameObject.layer = _originalRendererLayer;
			}
		}
		else
		{
			_renderer.enabled = true;
			_renderer.gameObject.layer = _originalRendererLayer;
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
