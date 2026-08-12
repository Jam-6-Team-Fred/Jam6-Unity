using System;
using UnityEngine;

[Serializable]
public class DampedSpringQuat
{
	public DampedSpringSettings settings = new DampedSpringSettings();

	[HideInInspector]
	public Vector4 velocity = Vector4.zero;

	public DampedSpringQuat()
	{
	}

	public DampedSpringQuat(float springConstant, float dampingCoefficient, float mass)
	{
		settings.springConstant = springConstant;
		settings.dampingCoefficient = dampingCoefficient;
		settings.mass = mass;
	}

	public DampedSpringQuat(float springConstant, float dampingRatio)
	{
		settings.springConstant = springConstant;
		settings.dampingRatio = dampingRatio;
	}

	public Quaternion Update(Quaternion currentValue, Quaternion targetValue, float deltaTime)
	{
		settings.Constrain();
		deltaTime = settings.ConstrainTimeStep(deltaTime);
		if (Quaternion.Dot(currentValue, targetValue) < 0f)
		{
			targetValue.x *= -1f;
			targetValue.y *= -1f;
			targetValue.z *= -1f;
			targetValue.w *= -1f;
		}
		Vector4 vector = (0f - settings.springConstant) * new Vector4(currentValue.x - targetValue.x, currentValue.y - targetValue.y, currentValue.z - targetValue.z, currentValue.w - targetValue.w);
		Vector4 vector2 = (0f - settings.dampingCoefficient) * velocity;
		Vector4 vector3 = (vector + vector2) / settings.mass;
		velocity += vector3 * deltaTime;
		currentValue.x += velocity.x * deltaTime;
		currentValue.y += velocity.y * deltaTime;
		currentValue.z += velocity.z * deltaTime;
		currentValue.w += velocity.w * deltaTime;
		return currentValue;
	}

	public void ResetVelocity()
	{
		velocity = Vector4.zero;
	}
}
