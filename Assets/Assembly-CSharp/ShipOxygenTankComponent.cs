using UnityEngine;

public class ShipOxygenTankComponent : ShipComponent
{
	[Space(10f)]
	[SerializeField]
	private float _oxygenLeakRate = 5f;

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
			_shipResources.DrainOxygen(_oxygenLeakRate * Time.deltaTime);
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
