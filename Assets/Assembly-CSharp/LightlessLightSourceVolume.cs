using UnityEngine;

public class LightlessLightSourceVolume : LightSourceVolume, ILightSource
{
	protected override void Awake()
	{
		base.Awake();
		LinkLightSource(this);
	}

	public LightSourceType GetLightSourceType()
	{
		return LightSourceType.VOLUME_ONLY;
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer, float maxDistance)
	{
		return true;
	}

	public OWLight2[] GetLights()
	{
		return null;
	}
}
