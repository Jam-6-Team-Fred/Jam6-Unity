using System.Collections.Generic;
using UnityEngine;

public class BHPathNode
{
	[SerializeField]
	public List<BHPathNode> reachableNodes;

	private float _damageMultiplier = 1f;

	public float GetDamageMultiplier()
	{
		return _damageMultiplier;
	}
}
