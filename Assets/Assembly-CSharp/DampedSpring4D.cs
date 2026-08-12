using System;
using UnityEngine;

[Serializable]
public class DampedSpring4D
{
	public DampedSpringSettings settings = new DampedSpringSettings();

	[HideInInspector]
	public Vector4 velocity = Vector4.zero;

	public DampedSpring4D()
	{
	}

	public DampedSpring4D(float springConstant, float dampingCoefficient, float mass)
	{
		settings.springConstant = springConstant;
		settings.dampingCoefficient = dampingCoefficient;
		settings.mass = mass;
	}

	public DampedSpring4D(float springConstant, float dampingRatio)
	{
		settings.springConstant = springConstant;
		settings.dampingRatio = dampingRatio;
	}

	public Vector4 Update(Vector4 currentValue, Vector4 targetValue, float deltaTime)
	{
		settings.Constrain();
		deltaTime = settings.ConstrainTimeStep(deltaTime);
		Vector4 vector = (0f - settings.springConstant) * (currentValue - targetValue);
		Vector4 vector2 = (0f - settings.dampingCoefficient) * velocity;
		Vector4 vector3 = (vector + vector2) / settings.mass;
		velocity += vector3 * deltaTime;
		return currentValue + velocity * deltaTime;
	}

	public void ResetVelocity()
	{
		velocity = Vector4.zero;
	}
}
