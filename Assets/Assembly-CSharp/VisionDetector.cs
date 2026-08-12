using UnityEngine;

public class VisionDetector : MonoBehaviour
{
	private enum Name
	{
		Default = 0,
		Player = 1,
		Probe = 2
	}

	[SerializeField]
	private Name _name;

	[SerializeField]
	private float _illuminationRadius;

	[SerializeField]
	private Vector3 _localIlluminationOffset;

	public bool CheckIllumination(Light[] lights = null)
	{
		Vector3 point = base.transform.TransformPoint(_localIlluminationOffset);
		if (Locator.GetFlashlight().IsFlashlightOn() && (_name == Name.Player || Locator.GetFlashlight().CheckIlluminationAtPoint(point, _illuminationRadius)))
		{
			return true;
		}
		if (Locator.GetProbe() != null && Locator.GetProbe().IsLaunched() && (_name == Name.Probe || Locator.GetProbe().CheckIlluminationAtPoint(point, _illuminationRadius)))
		{
			return true;
		}
		if (Locator.GetThrusterLightTracker().GetLightRange() > 0f && (_name == Name.Player || Locator.GetThrusterLightTracker().CheckIlluminationAtPoint(point, _illuminationRadius)))
		{
			return true;
		}
		if (lights != null)
		{
			for (int i = 0; i < lights.Length; i++)
			{
				if (lights[i].intensity > 0f && lights[i].range > 0f)
				{
					return true;
				}
			}
		}
		return false;
	}
}
