using UnityEngine;

public class TimedHologram : Hologram
{
	[SerializeField]
	private float _hologramDuration = 10f;

	private float _activationTime;

	protected override void OnActivation()
	{
		_activationTime = Time.time;
	}

	protected override void UpdateHologram()
	{
		if (Time.time > _activationTime + _hologramDuration)
		{
			CompleteHologram();
		}
	}
}
