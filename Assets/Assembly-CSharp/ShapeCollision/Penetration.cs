using UnityEngine;

namespace ShapeCollision
{
	public static class Penetration
	{
		public static float Sphere(Vector3 point, Vector3 sphereCenter, float sphereRadius)
		{
			return sphereRadius - Vector3.Distance(point, sphereCenter);
		}

		public static float Capsule(Vector3 point, Vector3 capsuleStart, Vector3 capsuleEnd, float capsuleRadius)
		{
			Vector3 vector = capsuleEnd - capsuleStart;
			Vector3 rhs = point - capsuleStart;
			float num = Vector3.Dot(vector, rhs);
			if (num <= 0f)
			{
				return capsuleRadius - Vector3.Distance(point, capsuleStart);
			}
			float num2 = Vector3.Dot(vector, vector);
			if (num2 <= num)
			{
				return capsuleRadius - Vector3.Distance(point, capsuleEnd);
			}
			float num3 = num / num2;
			Vector3 b = capsuleStart + num3 * vector;
			return capsuleRadius - Vector3.Distance(point, b);
		}

		public static Vector3 Box(Vector3 point, Vector3 boxCenter, Vector3 boxSize, Vector3[] boxAxes)
		{
			Vector3 vector = boxSize * 0.5f;
			Vector3 lhs = point - boxCenter;
			Vector3 result = vector;
			for (int i = 0; i < 3; i++)
			{
				float f = Vector3.Dot(lhs, boxAxes[i]);
				result[i] -= Mathf.Abs(f);
			}
			return result;
		}
	}
}
