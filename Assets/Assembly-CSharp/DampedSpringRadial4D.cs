using System;
using UnityEngine;

[Serializable]
public class DampedSpringRadial4D
{
	public DampedSpringSettings settings = new DampedSpringSettings();

	[HideInInspector]
	public Vector4 velocity = Vector4.zero;

	public DampedSpringRadial4D()
	{
	}

	public DampedSpringRadial4D(float springConstant, float dampingCoefficient, float mass)
	{
		settings.springConstant = springConstant;
		settings.dampingCoefficient = dampingCoefficient;
		settings.mass = mass;
	}

	public DampedSpringRadial4D(float springConstant, float dampingRatio)
	{
		settings.springConstant = springConstant;
		settings.dampingRatio = dampingRatio;
	}

	public Vector4 Update(Vector4 currentValue, Vector4 targetValue, float deltaTime)
	{
		settings.Constrain();
		deltaTime = settings.ConstrainTimeStep(deltaTime);
		currentValue.x = DampedSpringRadial.WrapAngle(currentValue.x);
		currentValue.y = DampedSpringRadial.WrapAngle(currentValue.y);
		currentValue.z = DampedSpringRadial.WrapAngle(currentValue.z);
		currentValue.w = DampedSpringRadial.WrapAngle(currentValue.w);
		targetValue.x = DampedSpringRadial.GetWrappedTarget(currentValue.x, targetValue.x);
		targetValue.y = DampedSpringRadial.GetWrappedTarget(currentValue.y, targetValue.y);
		targetValue.z = DampedSpringRadial.GetWrappedTarget(currentValue.z, targetValue.z);
		targetValue.w = DampedSpringRadial.GetWrappedTarget(currentValue.w, targetValue.w);
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
