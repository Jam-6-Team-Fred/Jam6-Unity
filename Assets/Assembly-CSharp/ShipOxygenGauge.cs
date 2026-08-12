using UnityEngine;

public class ShipOxygenGauge : ShipGauge
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
		UpdateVisuals(_shipResources.GetFractionalOxygen());
	}
}
