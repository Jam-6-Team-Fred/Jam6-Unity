using UnityEngine;

public class ShipHeadlightComponent : ShipComponent
{
	[Space(10f)]
	[SerializeField]
	protected ElectricalSystem _electricalSystem;

	[SerializeField]
	protected ShipLight[] _headlights;

	[SerializeField]
	protected float _disruptionImpactSpeed = 30f;

	[SerializeField]
	protected float _disruptionLength = 1f;

	protected override void OnComponentDamaged()
	{
		for (int i = 0; i < _headlights.Length; i++)
		{
			_headlights[i].SetDamaged(damaged: true);
		}
	}

	protected override void OnComponentRepaired()
	{
		for (int i = 0; i < _headlights.Length; i++)
		{
			_headlights[i].SetDamaged(damaged: false);
		}
		_electricalSystem.Disrupt(_disruptionLength);
	}

	public override bool ApplyImpact(ImpactData impact)
	{
		if (impact.speed > _disruptionImpactSpeed)
		{
			_electricalSystem.Disrupt(_disruptionLength);
		}
		return base.ApplyImpact(impact);
	}
}
