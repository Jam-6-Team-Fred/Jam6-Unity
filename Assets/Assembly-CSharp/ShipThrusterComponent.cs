using UnityEngine;

public class ShipThrusterComponent : ShipComponent
{
	[Space(10f)]
	[SerializeField]
	private ThrusterBank _thrusterBank;

	private ShipThrusterModel _thrusterModel;

	protected void Start()
	{
		_thrusterModel = Locator.GetShipBody().GetComponent<ShipThrusterModel>();
	}

	protected override void OnComponentDamaged()
	{
		_thrusterModel.SetThrusterBankEnabled(_thrusterBank, enabled: false);
	}

	protected override void OnComponentRepaired()
	{
		_thrusterModel.SetThrusterBankEnabled(_thrusterBank, enabled: true);
	}
}
