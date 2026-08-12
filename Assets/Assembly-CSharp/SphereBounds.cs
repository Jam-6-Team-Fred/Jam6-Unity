using System;
using UnityEngine;

[Serializable]
public struct SphereBounds
{
	public Vector3 center;

	public float radius;

	public SphereBounds(Vector3 sphereCenter, float sphereRadius)
	{
		center = sphereCenter;
		radius = sphereRadius;
	}

	public SphereBounds(Bounds aabbBounds)
	{
		center = aabbBounds.center;
		radius = aabbBounds.extents.magnitude;
	}

	public void Set(Vector3 sphereCenter, float sphereRadius)
	{
		center = sphereCenter;
		radius = sphereRadius;
	}

	public void Encapsulate(Vector3 point)
	{
		Vector3 vector = point - center;
		float sqrMagnitude = vector.sqrMagnitude;
		if (!(radius * radius >= sqrMagnitude))
		{
			float num = Mathf.Sqrt(sqrMagnitude);
			center += (num - radius) / num * vector * 0.5f;
			radius = (num + radius) * 0.5f;
		}
	}

	public void Encapsulate(SphereBounds other)
	{
		Vector3 vector = other.center - center;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = radius - other.radius;
		if (num * num >= sqrMagnitude)
		{
			if (!(radius >= other.radius))
			{
				center = other.center;
				radius = other.radius;
			}
		}
		else
		{
			float num2 = Mathf.Sqrt(sqrMagnitude);
			center += (other.radius + num2 - radius) / num2 * vector * 0.5f;
			radius = (num2 + radius + other.radius) * 0.5f;
		}
	}

	public void Encapsulate(Bounds aabbBounds)
	{
		Encapsulate(new SphereBounds(aabbBounds));
	}

	public bool Overlaps(SphereBounds other, Vector3 fromPoint)
	{
		Vector3 vector = center - fromPoint;
		Vector3 rhs = other.center - fromPoint;
		float num = 1f / vector.magnitude;
		float num2 = 1f / rhs.magnitude;
		float num3 = Vector3.Dot(vector, rhs) * num * num2;
		float num4 = radius * num;
		float num5 = other.radius * num2;
		float num6 = num3 * num3 + num4 * num4 + num5 * num5;
		float num7 = 2f * num3 * num4 * num5;
		bool num8 = num6 + num7 - 1f >= 0f;
		float distanceToPoint = new Plane(vector, center).GetDistanceToPoint(other.center);
		return num8 & (distanceToPoint > other.radius);
	}

	public bool Occludes(SphereBounds other, Vector3 fromPoint)
	{
		Vector3 vector = center - fromPoint;
		Vector3 rhs = other.center - fromPoint;
		float num = 1f / vector.magnitude;
		float num2 = 1f / rhs.magnitude;
		float num3 = Vector3.Dot(vector, rhs) * num * num2;
		float num4 = radius * num;
		float num5 = other.radius * num2;
		float num6 = num3 * num3 + num4 * num4 + num5 * num5;
		float num7 = 2f * num3 * num4 * num5;
		bool num8 = num6 + num7 - 1f >= 0f && num6 - num7 - 1f >= 0f;
		float distanceToPoint = new Plane(vector, center).GetDistanceToPoint(other.center);
		return num8 & (distanceToPoint > other.radius);
	}

	public override int GetHashCode()
	{
		return center.GetHashCode() ^ (radius.GetHashCode() << 2);
	}

	public override bool Equals(object other)
	{
		if (!(other is SphereBounds sphereBounds))
		{
			return false;
		}
		if (center.Equals(sphereBounds.center))
		{
			return radius.Equals(sphereBounds.radius);
		}
		return false;
	}

	public override string ToString()
	{
		return string.Concat("Center: ", center, ", Radius: ", radius);
	}

	public string ToString(string format)
	{
		return "Center: " + center.ToString(format) + ", Radius: " + radius.ToString(format);
	}

	public static bool operator ==(SphereBounds lhs, SphereBounds rhs)
	{
		if (lhs.center == rhs.center)
		{
			return lhs.radius == rhs.radius;
		}
		return false;
	}

	public static bool operator !=(SphereBounds lhs, SphereBounds rhs)
	{
		return !(lhs == rhs);
	}
}
