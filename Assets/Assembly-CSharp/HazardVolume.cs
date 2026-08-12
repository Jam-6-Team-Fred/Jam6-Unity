using UnityEngine;

public class HazardVolume : EffectVolume
{
	public enum HazardType
	{
		NONE = 0,
		GENERAL = 1,
		DARKMATTER = 2,
		HEAT = 4,
		FIRE = 8,
		SANDFALL = 0x10,
		ELECTRICITY = 0x20,
		RAPIDS = 0x40
	}

	[SerializeField]
	protected float _firstContactDamage;

	[SerializeField]
	protected InstantDamageType _firstContactDamageType;

	[SerializeField]
	protected float _damagePerSecond = 10f;

	public float GetFirstContactDamage()
	{
		return _firstContactDamage;
	}

	public virtual float GetDamagePerSecond(HazardDetector detector)
	{
		return _damagePerSecond;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		HazardDetector component = hitObj.GetComponent<HazardDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		HazardDetector component = hitObj.GetComponent<HazardDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}

	public virtual HazardType GetHazardType()
	{
		return HazardType.GENERAL;
	}

	public virtual InstantDamageType GetFirstContactDamageType()
	{
		return _firstContactDamageType;
	}
}
