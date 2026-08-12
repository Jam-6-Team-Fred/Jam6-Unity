using UnityEngine;

namespace ShapeCollision
{
	public static class PointInside
	{
		public static bool Sphere(Vector3 point, Vector3 sphereCenter, float sphereRadius)
		{
			return (point - sphereCenter).sqrMagnitude <= sphereRadius * sphereRadius;
		}

		public static bool Hemisphere(Vector3 point, Vector3 sphereCenter, float sphereRadius, Vector3 sphereNormal)
		{
			Vector3 lhs = point - sphereCenter;
			if (Vector3.Dot(lhs, sphereNormal) < 0f)
			{
				return false;
			}
			return lhs.sqrMagnitude <= sphereRadius * sphereRadius;
		}

		public static bool Capsule(Vector3 point, Vector3 capsuleStart, Vector3 capsuleEnd, float capsuleRadius)
		{
			Vector3 vector = capsuleEnd - capsuleStart;
			Vector3 rhs = point - capsuleStart;
			float num = capsuleRadius * capsuleRadius;
			float num2 = Vector3.Dot(vector, rhs);
			if (num2 <= 0f)
			{
				return rhs.sqrMagnitude <= num;
			}
			float num3 = Vector3.Dot(vector, vector);
			if (num3 <= num2)
			{
				return (point - capsuleEnd).sqrMagnitude <= num;
			}
			float num4 = num2 / num3;
			Vector3 vector2 = capsuleStart + num4 * vector;
			return (point - vector2).sqrMagnitude <= num;
		}

		public static bool Hemicapsule(Vector3 point, Vector3 capsuleStart, Vector3 capsuleEnd, float capsuleRadius, bool capSide)
		{
			Vector3 vector = capsuleEnd - capsuleStart;
			Vector3 rhs = point - capsuleStart;
			float num = capsuleRadius * capsuleRadius;
			float num2 = Vector3.Dot(vector, rhs);
			if (num2 <= 0f)
			{
				if (capSide)
				{
					return false;
				}
				return rhs.sqrMagnitude <= num;
			}
			float num3 = Vector3.Dot(vector, vector);
			if (num3 <= num2)
			{
				if (capSide)
				{
					return (point - capsuleEnd).sqrMagnitude <= num;
				}
				return false;
			}
			float num4 = num2 / num3;
			Vector3 vector2 = capsuleStart + num4 * vector;
			return (point - vector2).sqrMagnitude <= num;
		}

		public static bool Cylinder(Vector3 point, Vector3 cylinderStart, Vector3 cylinderEnd, float cylinderRadius)
		{
			Vector3 vector = cylinderEnd - cylinderStart;
			Vector3 rhs = point - cylinderStart;
			float num = Vector3.Dot(vector, rhs);
			if (num <= 0f)
			{
				return false;
			}
			float num2 = Vector3.Dot(vector, vector);
			if (num2 <= num)
			{
				return false;
			}
			float num3 = num / num2;
			Vector3 vector2 = cylinderStart + num3 * vector;
			return (point - vector2).sqrMagnitude <= cylinderRadius * cylinderRadius;
		}

		public static bool Cone(Vector3 point, Vector3 coneStart, Vector3 coneEnd, float coneRadiusStart, float coneRadiusEnd)
		{
			Vector3 vector = coneEnd - coneStart;
			Vector3 rhs = point - coneStart;
			float num = Vector3.Dot(vector, rhs);
			if (num <= 0f)
			{
				return false;
			}
			float num2 = Vector3.Dot(vector, vector);
			if (num2 <= num)
			{
				return false;
			}
			float num3 = num / num2;
			Vector3 vector2 = coneStart + num3 * vector;
			float num4 = (1f - num3) * coneRadiusStart + num3 * coneRadiusEnd;
			return (point - vector2).sqrMagnitude <= num4 * num4;
		}

		public static bool Box(Vector3 point, Vector3 boxCenter, Vector3 boxSize, Vector3[] boxAxes)
		{
			Vector3 vector = boxSize * 0.5f;
			Vector3 lhs = point - boxCenter;
			for (int i = 0; i < 3; i++)
			{
				if (Mathf.Abs(Vector3.Dot(lhs, boxAxes[i])) > vector[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
