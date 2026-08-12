using UnityEngine;

public class ShipFuelGauge : ShipGauge
{
	[Space]
	[SerializeField]
	protected ShipResources _shipResources;

	protected override void Awake()
	{
		base.Awake();
		UpdateVisuals(1f);
	}

	protected virtual void Update()
	{
		UpdateVisuals(_shipResources.GetFractionalFuel());
	}
}
