using System.Collections.Generic;
using UnityEngine;

public class NoiseSensor : MonoBehaviour
{
	public delegate void NoiseEvent(NoiseMaker noiseMaker);

	public event NoiseEvent OnAudibleNoise;

	public event NoiseEvent OnClosestAudibleNoise;

	private void LateUpdate()
	{
		NoiseMaker noiseMaker = null;
		float num = float.PositiveInfinity;
		List<NoiseMaker> activeNoiseMakers = NoiseMaker.GetActiveNoiseMakers();
		for (int i = 0; i < activeNoiseMakers.Count; i++)
		{
			float noiseRadius = activeNoiseMakers[i].GetNoiseRadius();
			if (noiseRadius <= 0f)
			{
				continue;
			}
			float sqrMagnitude = (base.transform.position - activeNoiseMakers[i].GetNoiseOrigin()).sqrMagnitude;
			if (!(sqrMagnitude > noiseRadius * noiseRadius))
			{
				if (this.OnAudibleNoise != null)
				{
					this.OnAudibleNoise(activeNoiseMakers[i]);
				}
				if (sqrMagnitude < num)
				{
					noiseMaker = activeNoiseMakers[i];
					num = sqrMagnitude;
				}
			}
		}
		if (noiseMaker != null && this.OnClosestAudibleNoise != null)
		{
			this.OnClosestAudibleNoise(noiseMaker);
		}
	}
}
