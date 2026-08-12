using UnityEngine;

namespace ShapeCollision
{
	public static class Intersection
	{
		private static Vector3[] sBoxVertCache = new Vector3[8];

		public static bool SphereSphere(Vector3 sphereCenter1, float sphereRadius1, Vector3 sphereCenter2, float sphereRadius2)
		{
			Vector3 vector = sphereCenter1 - sphereCenter2;
			float num = sphereRadius1 + sphereRadius2;
			return vector.sqrMagnitude <= num * num;
		}

		public static bool SphereHemisphere(Vector3 sphereCenter, float sphereRadius, Vector3 hemisphereCenter, float hemisphereRadius, Vector3 hemisphereNormal)
		{
			Vector3 lhs = sphereCenter - hemisphereCenter;
			float num = Vector3.Dot(lhs, -hemisphereNormal);
			if (num > sphereRadius)
			{
				return false;
			}
			if (num <= 0f)
			{
				float num2 = sphereRadius + hemisphereRadius;
				return lhs.sqrMagnitude <= num2 * num2;
			}
			Vector3 vector = sphereCenter + hemisphereNormal * num;
			float num3 = hemisphereRadius + Mathf.Sqrt(sphereRadius * sphereRadius - num * num);
			return (vector - hemisphereCenter).sqrMagnitude <= num3 * num3;
		}

		public static bool SphereCapsule(Vector3 sphereCenter, float sphereRadius, Vector3 capsuleStart, Vector3 capsuleEnd, float capsuleRadius)
		{
			Vector3 vector = capsuleEnd - capsuleStart;
			Vector3 rhs = sphereCenter - capsuleStart;
			float num = sphereRadius + capsuleRadius;
			num *= num;
			float num2 = Vector3.Dot(vector, rhs);
			if (num2 <= 0f)
			{
				return rhs.sqrMagnitude <= num;
			}
			float num3 = Vector3.Dot(vector, vector);
			if (num3 <= num2)
			{
				return (sphereCenter - capsuleEnd).sqrMagnitude <= num;
			}
			float num4 = num2 / num3;
			Vector3 vector2 = capsuleStart + num4 * vector;
			return (sphereCenter - vector2).sqrMagnitude <= num;
		}

		public static bool SphereHemicapsule(Vector3 sphereCenter, float sphereRadius, Vector3 hemicapsuleStart, Vector3 hemicapsuleEnd, float hemicapsuleRadius, bool capSide)
		{
			Vector3 vector = hemicapsuleEnd - hemicapsuleStart;
			Vector3 vector2 = sphereCenter - hemicapsuleStart;
			Vector3 lhs = sphereCenter - hemicapsuleEnd;
			float num = sphereRadius + hemicapsuleRadius;
			num *= num;
			Vector3 normalized = vector.normalized;
			float num2 = (capSide ? Vector3.Dot(vector2, -normalized) : Vector3.Dot(lhs, normalized));
			if (num2 > sphereRadius)
			{
				return false;
			}
			float num3 = Vector3.Dot(vector, vector2);
			if (num3 <= 0f)
			{
				if (capSide)
				{
					Vector3 vector3 = sphereCenter + normalized * num2;
					float num4 = hemicapsuleRadius + Mathf.Sqrt(sphereRadius * sphereRadius - num2 * num2);
					return (vector3 - hemicapsuleStart).sqrMagnitude <= num4 * num4;
				}
				return vector2.sqrMagnitude <= num;
			}
			float num5 = Vector3.Dot(vector, vector);
			if (num5 <= num3)
			{
				if (!capSide)
				{
					Vector3 vector4 = sphereCenter - normalized * num2;
					float num6 = hemicapsuleRadius + Mathf.Sqrt(sphereRadius * sphereRadius - num2 * num2);
					return (vector4 - hemicapsuleEnd).sqrMagnitude <= num6 * num6;
				}
				return lhs.sqrMagnitude <= num;
			}
			float num7 = num3 / num5;
			Vector3 vector5 = hemicapsuleStart + num7 * vector;
			return (sphereCenter - vector5).sqrMagnitude <= num;
		}

		public static bool SphereCylinder(Vector3 sphereCenter, float sphereRadius, Vector3 cylinderStart, Vector3 cylinderEnd, float cylinderRadius)
		{
			Vector3 vector = cylinderEnd - cylinderStart;
			Vector3 vector2 = sphereCenter - cylinderStart;
			Vector3 lhs = sphereCenter - cylinderEnd;
			float num = sphereRadius + cylinderRadius;
			num *= num;
			Vector3 vector3 = -vector.normalized;
			float num2 = Vector3.Dot(vector2, vector3);
			float num3 = Vector3.Dot(lhs, -vector3);
			if (num2 > sphereRadius || num3 > sphereRadius)
			{
				return false;
			}
			float num4 = Vector3.Dot(vector, vector2);
			if (num4 <= 0f)
			{
				Vector3 vector4 = sphereCenter - vector3 * num2;
				float num5 = cylinderRadius + Mathf.Sqrt(sphereRadius * sphereRadius - num2 * num2);
				return (vector4 - cylinderStart).sqrMagnitude <= num5 * num5;
			}
			float num6 = Vector3.Dot(vector, vector);
			if (num6 <= num4)
			{
				Vector3 vector5 = sphereCenter + vector3 * num3;
				float num7 = cylinderRadius + Mathf.Sqrt(sphereRadius * sphereRadius - num3 * num3);
				return (vector5 - cylinderEnd).sqrMagnitude <= num7 * num7;
			}
			float num8 = num4 / num6;
			Vector3 vector6 = cylinderStart + num8 * vector;
			return (sphereCenter - vector6).sqrMagnitude <= num;
		}

		public static bool SphereCone(Vector3 sphereCenter, float sphereRadius, Vector3 coneStart, Vector3 coneEnd, float coneRadiusStart, float coneRadiusEnd)
		{
			Vector3 vector = coneEnd - coneStart;
			Vector3 vector2 = sphereCenter - coneStart;
			Vector3 lhs = sphereCenter - coneEnd;
			Vector3 vector3 = -vector.normalized;
			float num = Vector3.Dot(vector2, vector3);
			float num2 = Vector3.Dot(lhs, -vector3);
			if (num > sphereRadius || num2 > sphereRadius)
			{
				return false;
			}
			float num3 = Vector3.Dot(vector, vector2);
			if (num3 <= 0f)
			{
				Vector3 vector4 = sphereCenter - vector3 * num;
				float num4 = coneRadiusStart + Mathf.Sqrt(sphereRadius * sphereRadius - num * num);
				return (vector4 - coneStart).sqrMagnitude <= num4 * num4;
			}
			float num5 = Vector3.Dot(vector, vector);
			if (num5 <= num3)
			{
				Vector3 vector5 = sphereCenter + vector3 * num2;
				float num6 = coneRadiusEnd + Mathf.Sqrt(sphereRadius * sphereRadius - num2 * num2);
				return (vector5 - coneEnd).sqrMagnitude <= num6 * num6;
			}
			float num7 = num3 / num5;
			Vector3 vector6 = coneStart + num7 * vector;
			float num8 = (1f - num7) * coneRadiusStart + num7 * coneRadiusEnd + sphereRadius;
			return (sphereCenter - vector6).sqrMagnitude <= num8 * num8;
		}

		public static bool SphereBox(Vector3 sphereCenter, float sphereRadius, Vector3 boxCenter, Vector3 boxSize, Vector3[] boxAxes)
		{
			Vector3 vector = boxSize * 0.5f;
			Vector3 lhs = sphereCenter - boxCenter;
			Vector3 vector2 = boxCenter;
			bool flag = true;
			for (int i = 0; i < 3; i++)
			{
				float num = Vector3.Dot(lhs, boxAxes[i]);
				flag &= num <= vector[i] && num >= 0f - vector[i];
				vector2 += Mathf.Clamp(num, 0f - vector[i], vector[i]) * boxAxes[i];
			}
			if (flag)
			{
				return true;
			}
			return (sphereCenter - vector2).sqrMagnitude <= sphereRadius * sphereRadius;
		}

		public static bool CapsuleCapsule(Vector3 capsuleStart1, Vector3 capsuleEnd1, float capsuleRadius1, Vector3 capsuleStart2, Vector3 capsuleEnd2, float capsuleRadius2)
		{
			Vector3 vector = capsuleEnd1 - capsuleStart1;
			Vector3 vector2 = capsuleEnd2 - capsuleStart2;
			Vector3 rhs = capsuleStart1 - capsuleStart2;
			float sqrMagnitude = vector.sqrMagnitude;
			float sqrMagnitude2 = vector2.sqrMagnitude;
			float num = Vector3.Dot(vector2, rhs);
			float num2 = capsuleRadius1 + capsuleRadius2;
			num2 *= num2;
			if (sqrMagnitude < Mathf.Epsilon && sqrMagnitude2 < Mathf.Epsilon)
			{
				return rhs.sqrMagnitude <= num2;
			}
			if (sqrMagnitude < Mathf.Epsilon)
			{
				float num3 = Mathf.Clamp01(num / sqrMagnitude2);
				Vector3 vector3 = capsuleStart2 + vector2 * num3;
				return (capsuleStart1 - vector3).sqrMagnitude <= num2;
			}
			if (sqrMagnitude2 < Mathf.Epsilon)
			{
				float num4 = Mathf.Clamp01((0f - Vector3.Dot(vector, rhs)) / sqrMagnitude);
				return (capsuleStart1 + vector * num4 - capsuleStart2).sqrMagnitude <= num2;
			}
			float num5 = Vector3.Dot(vector, rhs);
			float num6 = Vector3.Dot(vector, vector2);
			float num7 = sqrMagnitude * sqrMagnitude2 - num6 * num6;
			float num8 = ((num7 != 0f) ? Mathf.Clamp01((num6 * num - num5 * sqrMagnitude2) / num7) : 0f);
			float num9 = (num6 * num8 + num) / sqrMagnitude2;
			if (num9 < 0f)
			{
				num8 = Mathf.Clamp01((0f - num5) / sqrMagnitude);
				num9 = 0f;
			}
			else if (num9 > 1f)
			{
				num8 = Mathf.Clamp01((num6 - num5) / sqrMagnitude);
				num9 = 1f;
			}
			Vector3 vector4 = capsuleStart1 + vector * num8;
			Vector3 vector5 = capsuleStart2 + vector2 * num9;
			return (vector4 - vector5).sqrMagnitude <= num2;
		}

		public static bool CapsuleBox(Vector3 capsuleStart, Vector3 capsuleEnd, float capsuleRadius, Vector3 boxCenter, Vector3 boxSize, Vector3[] boxAxes, Vector3[] boxVerts)
		{
			Vector3 vector = boxSize * 0.5f;
			Vector3 rhs = capsuleStart - boxCenter;
			Vector3 rhs2 = capsuleEnd - boxCenter;
			Vector3 vector2 = capsuleEnd - capsuleStart;
			Vector3 vector3 = boxCenter;
			Vector3 vector4 = boxCenter;
			bool flag = true;
			bool flag2 = true;
			for (int i = 0; i < 3; i++)
			{
				float num = Vector3.Dot(boxAxes[i], rhs);
				float num2 = Vector3.Dot(boxAxes[i], rhs2);
				flag &= num <= vector[i] && num >= 0f - vector[i];
				flag2 &= num2 <= vector[i] && num2 >= 0f - vector[i];
				vector3 += boxAxes[i] * Mathf.Clamp(num, 0f - vector[i], vector[i]);
				vector4 += boxAxes[i] * Mathf.Clamp(num2, 0f - vector[i], vector[i]);
				float num3 = vector[i] + capsuleRadius;
				if ((num > num3 && num2 > num3) || (num < 0f - num3 && num2 < 0f - num3))
				{
					return false;
				}
			}
			if (flag || flag2)
			{
				return true;
			}
			float num4 = capsuleRadius * capsuleRadius;
			if ((capsuleStart - vector3).sqrMagnitude <= num4 || (capsuleEnd - vector4).sqrMagnitude <= num4)
			{
				return true;
			}
			for (int j = 0; j < 8; j++)
			{
				sBoxVertCache[j] = boxVerts[j] - capsuleStart;
			}
			for (int k = 0; k < 3; k++)
			{
				Vector3 normalized = Vector3.Cross(boxAxes[k], vector2).normalized;
				float num5 = float.PositiveInfinity;
				float num6 = float.NegativeInfinity;
				for (int l = 0; l < 8; l++)
				{
					float b = Vector3.Dot(normalized, sBoxVertCache[l]);
					num5 = Mathf.Min(num5, b);
					num6 = Mathf.Max(num6, b);
				}
				if (num5 > capsuleRadius || num6 < 0f - capsuleRadius)
				{
					return false;
				}
			}
			Vector3 normalized2 = (vector3 - capsuleStart).normalized;
			float b2 = Vector3.Dot(normalized2, vector2);
			float num7 = Mathf.Min(0f, b2) - capsuleRadius;
			float num8 = Mathf.Max(0f, b2) + capsuleRadius;
			float num9 = float.PositiveInfinity;
			float num10 = float.NegativeInfinity;
			for (int m = 0; m < 8; m++)
			{
				float b3 = Vector3.Dot(normalized2, sBoxVertCache[m]);
				num9 = Mathf.Min(num9, b3);
				num10 = Mathf.Max(num10, b3);
			}
			if (num10 < num7 || num9 > num8)
			{
				return false;
			}
			Vector3 normalized3 = (vector4 - capsuleEnd).normalized;
			float b4 = Vector3.Dot(normalized3, -vector2);
			float num11 = Mathf.Min(0f, b4) - capsuleRadius;
			float num12 = Mathf.Max(0f, b4) + capsuleRadius;
			float num13 = float.PositiveInfinity;
			float num14 = float.NegativeInfinity;
			for (int n = 0; n < 8; n++)
			{
				float b5 = Vector3.Dot(normalized3, boxVerts[n] - capsuleEnd);
				num13 = Mathf.Min(num13, b5);
				num14 = Mathf.Max(num14, b5);
			}
			if (num14 < num11 || num13 > num12)
			{
				return false;
			}
			int num15 = -1;
			Vector3 vector5 = capsuleStart;
			float num16 = float.PositiveInfinity;
			for (int num17 = 0; num17 < 8; num17++)
			{
				float sqrMagnitude = vector2.sqrMagnitude;
				float num18 = Vector3.Dot(sBoxVertCache[num17], vector2) / sqrMagnitude;
				Vector3 vector6 = capsuleStart + vector2 * num18;
				float sqrMagnitude2 = (boxVerts[num17] - vector6).sqrMagnitude;
				if (sqrMagnitude2 < num16)
				{
					num15 = num17;
					vector5 = vector6;
					num16 = sqrMagnitude2;
				}
			}
			Vector3 normalized4 = (boxVerts[num15] - vector5).normalized;
			float num19 = float.PositiveInfinity;
			float num20 = float.NegativeInfinity;
			for (int num21 = 0; num21 < 8; num21++)
			{
				float b6 = Vector3.Dot(normalized4, boxVerts[num21] - vector5);
				num19 = Mathf.Min(num19, b6);
				num20 = Mathf.Max(num20, b6);
			}
			if (num19 > capsuleRadius || num20 < 0f - capsuleRadius)
			{
				return false;
			}
			return true;
		}

		public static bool BoxBox(Vector3 boxCenter1, Vector3 boxSize1, Vector3[] boxAxes1, Vector3[] boxVerts1, Vector3 boxCenter2, Vector3 boxSize2, Vector3[] boxAxes2, Vector3[] boxVerts2)
		{
			Vector3 vector = boxSize1 * 0.5f;
			Vector3 vector2 = boxSize2 * 0.5f;
			for (int i = 0; i < 3; i++)
			{
				float num = float.PositiveInfinity;
				float num2 = float.NegativeInfinity;
				for (int j = 0; j < 8; j++)
				{
					float b = Vector3.Dot(boxAxes1[i], boxVerts2[j] - boxCenter1);
					num = Mathf.Min(num, b);
					num2 = Mathf.Max(num2, b);
				}
				if ((num > vector[i] && num2 > vector[i]) || (num < 0f - vector[i] && num2 < 0f - vector[i]))
				{
					return false;
				}
			}
			for (int k = 0; k < 3; k++)
			{
				float num3 = float.PositiveInfinity;
				float num4 = float.NegativeInfinity;
				for (int l = 0; l < 8; l++)
				{
					float b2 = Vector3.Dot(boxAxes2[k], boxVerts1[l] - boxCenter2);
					num3 = Mathf.Min(num3, b2);
					num4 = Mathf.Max(num4, b2);
				}
				if ((num3 > vector2[k] && num4 > vector2[k]) || (num3 < 0f - vector2[k] && num4 < 0f - vector2[k]))
				{
					return false;
				}
			}
			for (int m = 0; m < 3; m++)
			{
				for (int n = 0; n < 3; n++)
				{
					Vector3 lhs = Vector3.Cross(boxAxes1[m], boxAxes2[n]);
					float num5 = float.PositiveInfinity;
					float num6 = float.PositiveInfinity;
					float num7 = float.NegativeInfinity;
					float num8 = float.NegativeInfinity;
					for (int num9 = 0; num9 < 8; num9++)
					{
						float b3 = Vector3.Dot(lhs, boxVerts1[num9] - boxCenter1);
						float b4 = Vector3.Dot(lhs, boxVerts2[num9] - boxCenter1);
						num5 = Mathf.Min(num5, b3);
						num6 = Mathf.Min(num6, b4);
						num7 = Mathf.Max(num7, b3);
						num8 = Mathf.Max(num8, b4);
					}
					if (num5 > num8 || num7 < num6)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
