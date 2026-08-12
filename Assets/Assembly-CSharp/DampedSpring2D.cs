using System;
using UnityEngine;

[Serializable]
public class DampedSpring2D
{
	public DampedSpringSettings settings = new DampedSpringSettings();

	[HideInInspector]
	public Vector2 velocity = Vector2.zero;

	public DampedSpring2D()
	{
	}

	public DampedSpring2D(float springConstant, float dampingCoefficient, float mass)
	{
		settings.springConstant = springConstant;
		settings.dampingCoefficient = dampingCoefficient;
		settings.mass = mass;
	}

	public DampedSpring2D(float springConstant, float dampingRatio)
	{
		settings.springConstant = springConstant;
		settings.dampingRatio = dampingRatio;
	}

	public Vector2 Update(Vector2 currentValue, Vector2 targetValue, float deltaTime)
	{
		settings.Constrain();
		deltaTime = settings.ConstrainTimeStep(deltaTime);
		Vector2 vector = (0f - settings.springConstant) * (currentValue - targetValue);
		Vector2 vector2 = (0f - settings.dampingCoefficient) * velocity;
		Vector2 vector3 = (vector + vector2) / settings.mass;
		velocity += vector3 * deltaTime;
		return currentValue + velocity * deltaTime;
	}

	public void ResetVelocity()
	{
		velocity = Vector2.zero;
	}
}
