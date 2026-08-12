using UnityEngine;

namespace ShapeCollision
{
	public static class ClosestPoint
	{
		public static Vector3 Sphere(Vector3 point, Vector3 sphereCenter, float sphereRadius)
		{
			Vector3 vector = point - sphereCenter;
			float magnitude = vector.magnitude;
			vector = ((magnitude > Mathf.Epsilon) ? (vector / magnitude) : Vector3.up);
			return sphereCenter + vector * sphereRadius;
		}

		public static Vector3 Capsule(Vector3 point, Vector3 capsuleStart, Vector3 capsuleEnd, float capsuleRadius)
		{
			Vector3 vector = capsuleEnd - capsuleStart;
			Vector3 rhs = point - capsuleStart;
			float num = Vector3.Dot(vector, rhs);
			float magnitude;
			Vector3 vector2;
			if (num <= 0f)
			{
				vector2 = point - capsuleStart;
				magnitude = vector2.magnitude;
				if (magnitude > Mathf.Epsilon)
				{
					return capsuleStart + vector2 / magnitude * capsuleRadius;
				}
				vector2 = capsuleStart - capsuleEnd;
				magnitude = vector2.magnitude;
				if (magnitude > Mathf.Epsilon)
				{
					return capsuleStart + vector2 / magnitude * capsuleRadius;
				}
				return capsuleStart + Vector3.up * capsuleRadius;
			}
			float num2 = Vector3.Dot(vector, vector);
			if (num2 <= num)
			{
				vector2 = point - capsuleEnd;
				magnitude = vector2.magnitude;
				if (magnitude > Mathf.Epsilon)
				{
					return capsuleEnd + vector2 / magnitude * capsuleRadius;
				}
				vector2 = capsuleEnd - capsuleStart;
				magnitude = vector2.magnitude;
				if (magnitude > Mathf.Epsilon)
				{
					return capsuleEnd + vector2 / magnitude * capsuleRadius;
				}
				return capsuleEnd + Vector3.up * capsuleRadius;
			}
			float num3 = num / num2;
			Vector3 vector3 = capsuleStart + num3 * vector;
			vector2 = point - vector3;
			magnitude = vector2.magnitude;
			if (magnitude > Mathf.Epsilon)
			{
				return vector3 + vector2 / magnitude * capsuleRadius;
			}
			vector2 = capsuleStart - capsuleEnd;
			magnitude = vector2.magnitude;
			if (magnitude > Mathf.Epsilon)
			{
				vector2 /= magnitude;
				vector2 = ((!(1f - Mathf.Abs(Vector3.Dot(vector2, Vector3.up)) > Mathf.Epsilon)) ? Vector3.Cross(vector2, Vector3.right).normalized : Vector3.Cross(vector2, Vector3.up).normalized);
				return vector3 + vector2 * capsuleRadius;
			}
			return vector3 + Vector3.up * capsuleRadius;
		}

		public static Vector3 Box(Vector3 point, Vector3 boxCenter, Vector3 boxSize, Vector3[] boxAxes)
		{
			Vector3 vector = boxSize * 0.5f;
			Vector3 rhs = point - boxCenter;
			Vector3 result = boxCenter;
			for (int i = 0; i < 3; i++)
			{
				float value = Vector3.Dot(boxAxes[i], rhs);
				result += boxAxes[i] * Mathf.Clamp(value, 0f - vector[i], vector[i]);
			}
			return result;
		}
	}
}
