using UnityEngine;

public class ShipAutopilotComponent : ShipComponent
{
	[Space(10f)]
	[SerializeField]
	private Autopilot _autopilot;

	protected override void OnComponentDamaged()
	{
		_autopilot.SetDamaged(damaged: true);
	}

	protected override void OnComponentRepaired()
	{
		_autopilot.SetDamaged(damaged: false);
	}
}
