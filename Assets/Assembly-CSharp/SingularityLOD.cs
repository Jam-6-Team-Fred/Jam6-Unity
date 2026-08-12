using UnityEngine;

public class SingularityLOD : SectoredMonoBehaviour
{
	[SerializeField]
	private Material _lodMaterial;

	private Renderer _renderer;

	private Material _defaultMaterial;

	private bool _materialSwapped;

	protected override void Awake()
	{
		base.Awake();
		_renderer = GetComponent<Renderer>();
		_defaultMaterial = _renderer.sharedMaterial;
		_renderer.sharedMaterial = _lodMaterial;
		_materialSwapped = true;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			if (_materialSwapped)
			{
				_renderer.sharedMaterial = _defaultMaterial;
				_materialSwapped = false;
			}
		}
		else if (!_materialSwapped)
		{
			_renderer.sharedMaterial = _lodMaterial;
			_materialSwapped = true;
		}
	}
}
