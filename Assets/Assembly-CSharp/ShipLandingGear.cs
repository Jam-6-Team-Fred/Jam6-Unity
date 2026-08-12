using UnityEngine;

public class ShipLandingGear : ShipHull
{
	[SerializeField]
	private ShipDetachableLeg[] _legs = new ShipDetachableLeg[0];

	private bool _legDetached;

	public ShipDetachableLeg[] GetLegs()
	{
		return _legs;
	}

	public override void ApplyImpact(ImpactData impact)
	{
		float t = Mathf.Clamp01(Vector3.Dot(impact.velocity.normalized, base.transform.up));
		float velocityScale = Mathf.Lerp(1f, 0.5f, t);
		ImpactData impact2 = new ImpactData(impact, velocityScale);
		base.ApplyImpact(impact2);
		if (!(_integrity <= 0f) || _legDetached)
		{
			return;
		}
		for (int i = 0; i < _legs.Length; i++)
		{
			if (_legs[i].ContainsCollider(impact.thisCollider))
			{
				_legs[i].Detach();
				_legDetached = true;
			}
		}
	}
}
