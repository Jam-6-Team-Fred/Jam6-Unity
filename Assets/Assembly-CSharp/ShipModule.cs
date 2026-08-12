using UnityEngine;

public class ShipModule : MonoBehaviour
{
	protected ShipHull[] _hulls;

	protected virtual void Awake()
	{
		_hulls = GetComponents<ShipHull>();
	}

	public virtual void ApplyImpact(ImpactData impact)
	{
		for (int i = 0; i < _hulls.Length; i++)
		{
			if (_hulls[i].ContainsCollider(impact.thisCollider))
			{
				_hulls[i].ApplyImpact(impact);
			}
		}
	}
}
