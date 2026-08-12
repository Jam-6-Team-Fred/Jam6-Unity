using UnityEngine;

public class SandLevelQuantumSocket : QuantumSocket
{
	[SerializeField]
	private SandLevelController _sandLevelController;

	[SerializeField]
	private float _sandLevelOffset;

	public override bool IsOccupied()
	{
		if (base.IsOccupied())
		{
			return true;
		}
		return _sandLevelController.GetRadius() > Vector3.Distance(base.transform.position, _sandLevelController.transform.position) + _sandLevelOffset;
	}
}
