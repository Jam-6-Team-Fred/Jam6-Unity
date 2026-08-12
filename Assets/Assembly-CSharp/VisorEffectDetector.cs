using UnityEngine;

public class VisorEffectDetector : PriorityDetector
{
	private float _netRainDropletRate;

	private float _netRainStreakRate;

	private float _netDirtRate;

	private float _frostRate;

	private float _maxFrost;

	public float netRainDropletRate => _netRainDropletRate;

	public float netRainStreakRate => _netRainStreakRate;

	public float netDirtRate => _netDirtRate;

	public float frostRate => _frostRate;

	public float maxFrost => _maxFrost;

	public override void AddVolume(EffectVolume eVol)
	{
		if (eVol is VisorEffectVolume)
		{
			base.AddVolume(eVol);
		}
	}

	public override void RemoveVolume(EffectVolume eVol)
	{
		if (eVol is VisorEffectVolume)
		{
			base.RemoveVolume(eVol);
		}
	}

	private void Update()
	{
		_netRainDropletRate = 0f;
		_netRainStreakRate = 0f;
		_netDirtRate = 0f;
		_frostRate = 0f;
		_maxFrost = 0f;
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			if (_activeVolumes[i] is VisorRainEffectVolume)
			{
				(_activeVolumes[i] as VisorRainEffectVolume).CalcRainRate(base.transform, out var rainRate, out var streakRate);
				_netRainDropletRate += rainRate;
				_netRainStreakRate += streakRate;
			}
			else if (_activeVolumes[i] is VisorDirtEffectVolume)
			{
				_netDirtRate += (_activeVolumes[i] as VisorDirtEffectVolume).dirtAccumulationRate;
			}
			else if (_activeVolumes[i] is VisorFrostEffectVolume)
			{
				VisorFrostEffectVolume visorFrostEffectVolume = _activeVolumes[i] as VisorFrostEffectVolume;
				_frostRate = Mathf.Max(_frostRate, visorFrostEffectVolume.frostRate);
				_maxFrost = Mathf.Max(_maxFrost, visorFrostEffectVolume.maxFrost);
			}
		}
	}
}
