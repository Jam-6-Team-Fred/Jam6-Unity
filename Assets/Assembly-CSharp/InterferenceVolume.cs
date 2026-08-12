using UnityEngine;

public class InterferenceVolume : EffectVolume
{
	[SerializeField]
	private float _interferenceStrength = 1f;

	public float GetInterferenceAtPoint(Vector3 worldPosition)
	{
		return _interferenceStrength;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		InterferenceDetector component = hitObj.GetComponent<InterferenceDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		InterferenceDetector component = hitObj.GetComponent<InterferenceDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
