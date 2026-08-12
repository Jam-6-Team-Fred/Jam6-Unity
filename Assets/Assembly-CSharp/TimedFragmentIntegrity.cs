using UnityEngine;

public class TimedFragmentIntegrity : FragmentIntegrity
{
	[SerializeField]
	private float _earliestTime;

	[SerializeField]
	private float _latestTime;

	[SerializeField]
	protected float _damageMultiplierWhenTime = 1f;

	protected override void Awake()
	{
		Invoke("OnLatestTimeReached", _latestTime);
		base.Awake();
	}

	public void OnLatestTimeReached()
	{
		if (TimeLoop.IsTimeFlowing() && !MeteorImpactMapper.AreFragmentsLocked())
		{
			if (TimeLoop.GetSecondsElapsed() < _latestTime)
			{
				Invoke("OnLatestTimeReached", _latestTime - TimeLoop.GetSecondsElapsed());
				return;
			}
			_integrity = 0f;
			CallOnTakeDamage();
		}
	}

	protected override float DamageMultiplier()
	{
		if (TimeLoop.GetSecondsElapsed() < _earliestTime)
		{
			return _damageMultiplier;
		}
		return _damageMultiplier * _damageMultiplierWhenTime;
	}

	protected override bool CanBreak()
	{
		if (base.CanBreak())
		{
			return TimeLoop.GetSecondsElapsed() > _earliestTime;
		}
		return false;
	}
}
