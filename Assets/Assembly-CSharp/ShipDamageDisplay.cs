using UnityEngine;

public class ShipDamageDisplay : MonoBehaviour
{
	private MeshRenderer _meshRenderer;

	[SerializeField]
	private ShipHull _shipHull;

	[SerializeField]
	private ShipComponent _shipComponent;

	[SerializeField]
	private Material _repairedMaterial;

	[SerializeField]
	private Material _damagedMaterial;

	private void Awake()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
		if (_shipHull != null)
		{
			_shipHull.OnDamaged += OnHullDamaged;
			_shipHull.OnRepaired += OnHullRepaired;
		}
		else if (_shipComponent != null)
		{
			_shipComponent.OnDamaged += OnComponentDamaged;
			_shipComponent.OnRepaired += OnComponentRepaired;
		}
	}

	private void OnDestroy()
	{
		if (_shipHull != null)
		{
			_shipHull.OnDamaged -= OnHullDamaged;
			_shipHull.OnRepaired -= OnHullRepaired;
		}
		else if (_shipComponent != null)
		{
			_shipComponent.OnDamaged -= OnComponentDamaged;
			_shipComponent.OnRepaired -= OnComponentRepaired;
		}
	}

	private void OnHullDamaged(ShipHull shipHull)
	{
		_meshRenderer.sharedMaterial = _damagedMaterial;
	}

	private void OnHullRepaired(ShipHull shipHull)
	{
		_meshRenderer.sharedMaterial = _repairedMaterial;
	}

	private void OnComponentDamaged(ShipComponent shipComponent)
	{
		_meshRenderer.sharedMaterial = _damagedMaterial;
	}

	private void OnComponentRepaired(ShipComponent shipComponent)
	{
		_meshRenderer.sharedMaterial = _repairedMaterial;
	}
}
