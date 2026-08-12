using UnityEngine;

public class VisorRainEffectVolume : VisorEffectVolume
{
	public enum RainDirection
	{
		Linear = 0,
		Radial = 1
	}

	[SerializeField]
	private RainDirection _rainDirection;

	[SerializeField]
	private float _dropletRate = 10f;

	[SerializeField]
	private AnimationCurve _dropletDirScale = AnimationCurve.EaseInOut(-1f, 0f, 1f, 1f);

	[SerializeField]
	private float _streakRate = 1f;

	[SerializeField]
	private AnimationCurve _streakDirScale = AnimationCurve.EaseInOut(-1f, 1f, 1f, 0f);

	public void CalcRainRate(Transform visor, out float rainRate, out float streakRate)
	{
		if (_dropletRate <= 0f && _streakRate <= 0f)
		{
			rainRate = 0f;
			streakRate = 0f;
		}
		else
		{
			float time = Vector3.Dot((_rainDirection == RainDirection.Radial) ? Vector3.Normalize(visor.position - base.transform.position) : base.transform.up, visor.forward);
			rainRate = _dropletDirScale.Evaluate(time) * _dropletRate;
			streakRate = _streakDirScale.Evaluate(time) * _streakRate;
		}
	}
}
