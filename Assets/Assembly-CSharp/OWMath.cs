using System;
using UnityEngine;

public static class OWMath
{
	public static float RoundToNearestMultiple(float value, float multiple)
	{
		return Mathf.Round(value / multiple) * multiple;
	}

	public static float Frac(float v)
	{
		return v - Mathf.Floor(v);
	}

	public static float CalculateSphereVolume(float radius)
	{
		return (float)Math.PI * Mathf.Pow(radius, 3f);
	}

	public static bool RaySphereIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 spherePos, float sphereRadius, out Vector3 intersectPoint)
	{
		intersectPoint = Vector3.zero;
		Vector3 normalized = rayDirection.normalized;
		Vector3 vector = spherePos - rayOrigin;
		if (Vector3.Dot(vector, normalized) <= 0f)
		{
			return false;
		}
		Vector3 vector2 = Vector3.Project(vector, normalized);
		Vector3 vector3 = vector2 - vector;
		float magnitude = vector3.magnitude;
		if (magnitude > sphereRadius)
		{
			return false;
		}
		if (magnitude == sphereRadius)
		{
			intersectPoint = rayOrigin + vector3;
			return true;
		}
		float num = Mathf.Sqrt(sphereRadius * sphereRadius - magnitude * magnitude);
		intersectPoint = rayOrigin + normalized * (vector2.magnitude - num);
		return true;
	}

	public static Vector3 ClosestPointOnRay(Vector3 point, Ray ray)
	{
		float num = Vector3.Dot(point - ray.origin, ray.direction);
		float num2 = Vector3.Dot(ray.direction, ray.direction);
		float num3 = num / num2;
		return ray.origin + num3 * ray.direction;
	}

	public static float PointRayDistance(Vector3 point, Ray ray)
	{
		return Vector3.Distance(point, ClosestPointOnRay(point, ray));
	}

	public static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
	{
		Vector3 vector = segmentEnd - segmentStart;
		float num = Vector3.Dot(point - segmentStart, vector);
		if (num <= 0f)
		{
			return segmentStart;
		}
		float num2 = Vector3.Dot(vector, vector);
		if (num2 <= num)
		{
			return segmentEnd;
		}
		float num3 = num / num2;
		return segmentStart + num3 * vector;
	}

	public static float PointSegmentDistance(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
	{
		return Vector3.Distance(point, ClosestPointOnSegment(point, segmentStart, segmentEnd));
	}

	public static Vector3 ClosestPointOnBox(Vector3 point, Vector3 center, Vector3 extents)
	{
		Vector3 vector = point - center;
		Vector3 vector2 = extents * 2f;
		Vector3 vector3 = vector;
		if (vector.x < extents.x && vector.x > 0f - extents.x && vector.y < extents.y && vector.y > 0f - extents.y && vector.z < extents.z && vector.z > 0f - extents.z)
		{
			float num = float.PositiveInfinity;
			for (int i = 0; i < 3; i++)
			{
				float num2 = Mathf.Abs(vector[i] - (0f - extents[i]));
				if (num2 < num)
				{
					vector3 = vector;
					vector3[i] = 0f - extents[i];
					num = num2;
				}
				float num3 = vector2[i] - num2;
				if (num3 < num)
				{
					vector3 = vector;
					vector3[i] = extents[i];
					num = num3;
				}
			}
		}
		else
		{
			for (int j = 0; j < 3; j++)
			{
				if (vector[j] < 0f - extents[j])
				{
					vector3[j] = 0f - extents[j];
				}
				else if (vector[j] > extents[j])
				{
					vector3[j] = extents[j];
				}
			}
		}
		return center + vector3;
	}

	public static Vector3 ClosestPointOnBox(Vector3 point, Vector3 center, Vector3 extents, Quaternion rotation)
	{
		Vector3 vector = ClosestPointOnBox(Quaternion.Inverse(rotation) * (point - center), Vector3.zero, extents);
		return rotation * vector + center;
	}

	public static bool PointInBox(Vector3 point, Vector3 center, Vector3 extents)
	{
		Vector3 vector = point - center;
		if (vector.x < extents.x && vector.x > 0f - extents.x && vector.y < extents.y && vector.y > 0f - extents.y && vector.z < extents.z)
		{
			return vector.z > 0f - extents.z;
		}
		return false;
	}

	public static bool PointInBox(Vector3 point, Vector3 center, Vector3 extents, Quaternion rotation)
	{
		return PointInBox(Quaternion.Inverse(rotation) * (point - center), Vector3.zero, extents);
	}

	public static float PointBoxDistance(Vector3 point, Vector3 center, Vector3 extents)
	{
		return Vector3.Distance(point, ClosestPointOnBox(point, center, extents)) * (PointInBox(point, center, extents) ? (-1f) : 1f);
	}

	public static float PointBoxDistance(Vector3 point, Vector3 center, Vector3 extents, Quaternion rotation)
	{
		return PointBoxDistance(Quaternion.Inverse(rotation) * (point - center), Vector3.zero, extents);
	}

	public static float LerpUnclamped(float from, float to, float t)
	{
		return from + (to - from) * t;
	}

	public static float InverseLerpUnclamped(float from, float to, float value)
	{
		if (from == to)
		{
			return float.PositiveInfinity;
		}
		if (from < to)
		{
			value -= from;
			value /= to - from;
			return value;
		}
		return 1f - (value - to) / (from - to);
	}

	public static float SetAnglePositive(float angle)
	{
		while (angle < -360f)
		{
			angle += 360f;
		}
		while (angle >= 360f)
		{
			angle -= 360f;
		}
		if (angle < 0f)
		{
			angle += 360f;
		}
		return angle;
	}

	public static float WrapAngle(float angle)
	{
		angle %= 360f;
		if (angle <= -180f)
		{
			angle += 360f;
		}
		else if (angle > 180f)
		{
			angle -= 360f;
		}
		return angle;
	}

	public static bool ApproxEquals(float f1, float f2, float epsilon = 0.001f)
	{
		if (Mathf.Abs(f1 - f2) < epsilon)
		{
			return true;
		}
		return false;
	}

	public static float Angle(Vector3 from, Vector3 to, Vector3 axis)
	{
		return Vector3.Angle(from, to) * Mathf.Sign(Vector3.Dot(Vector3.Cross(from, to), axis));
	}

	public static float LinearToDecibel(float linearValue)
	{
		if (!(linearValue > 0f))
		{
			return -80f;
		}
		return 20f * Mathf.Log10(linearValue);
	}

	public static float DecibelToLinear(float decibel)
	{
		if (!(decibel <= -80f))
		{
			return Mathf.Pow(10f, decibel / 20f);
		}
		return 0f;
	}
}
