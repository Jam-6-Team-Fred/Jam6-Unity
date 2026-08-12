using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class PerSectorShadowCastingState : MonoBehaviour
{
	private Renderer _renderer;

	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private ShadowCastingMode _shadowCastingMode = ShadowCastingMode.On;

	private bool _overridden;

	private ShadowCastingMode _prevShadowCastingMode;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
		_overridden = false;
		if ((bool)_sector && _renderer != null)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
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
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			if (!_overridden)
			{
				_prevShadowCastingMode = _renderer.shadowCastingMode;
				_renderer.shadowCastingMode = _shadowCastingMode;
				_overridden = true;
			}
		}
		else if (_overridden)
		{
			_renderer.shadowCastingMode = _prevShadowCastingMode;
			_overridden = false;
		}
	}

	public void SetSector(Sector sector)
	{
		if (_sector != null)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_sector = sector;
		if (_sector != null && _renderer != null)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
			OnSectorOccupantsUpdated();
		}
		else if (_overridden)
		{
			_renderer.shadowCastingMode = _prevShadowCastingMode;
			_overridden = false;
		}
	}

	public void SetShadowCastingMode(ShadowCastingMode mode)
	{
		_shadowCastingMode = mode;
		if (_overridden)
		{
			_renderer.shadowCastingMode = _shadowCastingMode;
		}
	}
}
