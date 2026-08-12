using UnityEngine;

public abstract class VanishVolumeCustomHandler : MonoBehaviour
{
	protected RelativeLocationData _relativeLocationData;

	public virtual bool ShouldHandleVanish(VanishVolume hitVanishVolume)
	{
		return false;
	}

	public virtual void CacheVanishData(VanishVolume hitVanishVolume, RelativeLocationData relativeLocationData)
	{
		_relativeLocationData = relativeLocationData;
	}

	public virtual void HandleVanish(VanishVolume hitVanishVolume)
	{
	}
}
