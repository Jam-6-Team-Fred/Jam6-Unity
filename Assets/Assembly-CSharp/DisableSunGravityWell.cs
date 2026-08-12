using UnityEngine;

public class DisableSunGravityWell : MonoBehaviour
{
	private void Start()
	{
		Locator.GetSunTransform().GetComponentInChildren<GravityVolume>().SetVolumeActivation(active: false);
	}
}
