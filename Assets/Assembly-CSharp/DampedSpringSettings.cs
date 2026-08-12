using System;
using UnityEngine;

[Serializable]
public class DampedSpringSettings
{
	public float springConstant = 1f;

	public float dampingCoefficient = 1f;

	public float mass = 1f;

	public static float minMass = 0.0001f;

	public static float maxTimestep = 0.05f;

	public float dampingRatio
	{
		get
		{
			return dampingCoefficient / (2f * Mathf.Sqrt(springConstant * mass));
		}
		set
		{
			dampingCoefficient = value * (2f * Mathf.Sqrt(springConstant * mass));
		}
	}

	public void Constrain()
	{
		mass = Mathf.Max(mass, minMass);
	}

	public float ConstrainTimeStep(float timestep)
	{
		return Mathf.Min(timestep, maxTimestep);
	}
}
