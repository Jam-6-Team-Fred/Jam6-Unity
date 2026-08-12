using UnityEngine;

public class ShipFuelTankComponent : ShipComponent
{
	[Space(10f)]
	[SerializeField]
	private float _fuelLeakRate = 50f;

	private ShipResources _shipResources;

	protected void Start()
	{
		base.enabled = false;
		_shipResources = Locator.GetShipBody().GetComponentInChildren<ShipResources>();
	}

	protected void Update()
	{
		if (_damaged)
		{
			_shipResources.DrainFuel(_fuelLeakRate * Time.deltaTime);
		}
		else
		{
			base.enabled = false;
		}
	}

	protected override void OnComponentDamaged()
	{
		base.enabled = true;
	}

	protected override void OnComponentRepaired()
	{
		base.enabled = false;
	}
}
