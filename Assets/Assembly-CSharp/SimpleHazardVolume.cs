using UnityEngine;

public class SimpleHazardVolume : HazardVolume
{
	[SerializeField]
	private HazardType _type;

	public override HazardType GetHazardType()
	{
		return _type;
	}
}
