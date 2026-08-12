using System;
using UnityEngine;

[Serializable]
public class DampedSpring
{
	public DampedSpringSettings settings = new DampedSpringSettings();

	[HideInInspector]
	public float velocity;

	public DampedSpring()
	{
	}

	public DampedSpring(float springConstant, float dampingCoefficient, float mass)
	{
		settings.springConstant = springConstant;
		settings.dampingCoefficient = dampingCoefficient;
		settings.mass = mass;
	}

	public DampedSpring(float springConstant, float dampingRatio)
	{
		settings.springConstant = springConstant;
		settings.dampingRatio = dampingRatio;
	}

	public float Update(float currentValue, float targetValue, float deltaTime)
	{
		settings.Constrain();
		deltaTime = settings.ConstrainTimeStep(deltaTime);
		float num = (0f - settings.springConstant) * (currentValue - targetValue);
		float num2 = (0f - settings.dampingCoefficient) * velocity;
		float num3 = (num + num2) / settings.mass;
		velocity += num3 * deltaTime;
		return currentValue + velocity * deltaTime;
	}

	public void ResetVelocity()
	{
		velocity = 0f;
	}
}
