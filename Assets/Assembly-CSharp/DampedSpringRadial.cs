using System;
using UnityEngine;

[Serializable]
public class DampedSpringRadial
{
	public DampedSpringSettings settings = new DampedSpringSettings();

	[HideInInspector]
	public float velocity;

	public DampedSpringRadial()
	{
	}

	public DampedSpringRadial(float springConstant, float dampingCoefficient, float mass)
	{
		settings.springConstant = springConstant;
		settings.dampingCoefficient = dampingCoefficient;
		settings.mass = mass;
	}

	public DampedSpringRadial(float springConstant, float dampingRatio)
	{
		settings.springConstant = springConstant;
		settings.dampingRatio = dampingRatio;
	}

	public float Update(float currentValue, float targetValue, float deltaTime)
	{
		settings.Constrain();
		deltaTime = settings.ConstrainTimeStep(deltaTime);
		currentValue = WrapAngle(currentValue);
		targetValue = GetWrappedTarget(currentValue, targetValue);
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

	public static float GetWrappedTarget(float currentValue, float targetValue)
	{
		targetValue = WrapAngle(targetValue);
		float num = ((targetValue > currentValue) ? (targetValue - 360f) : (targetValue + 360f));
		if (!(Mathf.Abs(currentValue - targetValue) <= Mathf.Abs(currentValue - num)))
		{
			return num;
		}
		return targetValue;
	}

	public static float WrapAngle(float angle)
	{
		while (angle > 180f)
		{
			angle -= 360f;
		}
		while (angle <= -180f)
		{
			angle += 360f;
		}
		return angle;
	}
}
