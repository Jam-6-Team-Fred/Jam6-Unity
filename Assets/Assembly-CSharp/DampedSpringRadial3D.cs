using System;
using UnityEngine;

[Serializable]
public class DampedSpringRadial3D
{
	public DampedSpringSettings settings = new DampedSpringSettings();

	[HideInInspector]
	public Vector3 velocity = Vector3.zero;

	public DampedSpringRadial3D()
	{
	}

	public DampedSpringRadial3D(float springConstant, float dampingCoefficient, float mass)
	{
		settings.springConstant = springConstant;
		settings.dampingCoefficient = dampingCoefficient;
		settings.mass = mass;
	}

	public DampedSpringRadial3D(float springConstant, float dampingRatio)
	{
		settings.springConstant = springConstant;
		settings.dampingRatio = dampingRatio;
	}

	public Vector3 Update(Vector3 currentValue, Vector3 targetValue, float deltaTime)
	{
		settings.Constrain();
		deltaTime = settings.ConstrainTimeStep(deltaTime);
		currentValue.x = DampedSpringRadial.WrapAngle(currentValue.x);
		currentValue.y = DampedSpringRadial.WrapAngle(currentValue.y);
		currentValue.z = DampedSpringRadial.WrapAngle(currentValue.z);
		targetValue.x = DampedSpringRadial.GetWrappedTarget(currentValue.x, targetValue.x);
		targetValue.y = DampedSpringRadial.GetWrappedTarget(currentValue.y, targetValue.y);
		targetValue.z = DampedSpringRadial.GetWrappedTarget(currentValue.z, targetValue.z);
		Vector3 vector = (0f - settings.springConstant) * (currentValue - targetValue);
		Vector3 vector2 = (0f - settings.dampingCoefficient) * velocity;
		Vector3 vector3 = (vector + vector2) / settings.mass;
		velocity += vector3 * deltaTime;
		return currentValue + velocity * deltaTime;
	}

	public void ResetVelocity()
	{
		velocity = Vector3.zero;
	}
}
