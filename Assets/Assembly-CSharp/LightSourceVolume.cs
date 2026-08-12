using UnityEngine;

public class LightSourceVolume : EffectVolume
{
	private ILightSource _linkedLightSource;

	public Shape GetShape()
	{
		return _triggerVolume.GetShape();
	}

	public void LinkLightSource(ILightSource lightSource)
	{
		_linkedLightSource = lightSource;
	}

	public ILightSource GetLightSource()
	{
		return _linkedLightSource;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		LightSourceDetector component = hitObj.GetComponent<LightSourceDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		LightSourceDetector component = hitObj.GetComponent<LightSourceDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
