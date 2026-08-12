using UnityEngine;

namespace Tessellation
{
	public class Sphere : Cube
	{
		private const float kInvSqr2 = 0.70710677f;

		protected override Patch.RenderMode patchRenderMode => Patch.RenderMode.Spherical;

		public override void Tessellate(int maxLevel, Vector3 localPos, float localRadius)
		{
			Vector3 normalized = localPos.normalized;
			Vector3 vector = SphericalToCubic(normalized);
			int num = ((!OWMath.ApproxEquals(vector.y, 1f)) ? (OWMath.ApproxEquals(vector.y, -1f) ? 1 : (OWMath.ApproxEquals(vector.z, 1f) ? 2 : (OWMath.ApproxEquals(vector.x, 1f) ? 3 : ((!OWMath.ApproxEquals(vector.z, -1f)) ? 5 : 4)))) : 0);
			Vector3 vector2 = Quaternion.Inverse(Cube.s_faceRotations[num]) * vector;
			faces[num].Tessellate(maxLevel, vector2, localRadius);
			for (int i = 0; i < 4; i++)
			{
				Vector3 vector3 = vector2 + Cube.s_neighborOffsets[i];
				vector3 = Cube.s_faceRotations[num] * (Cube.s_neighborTransforms[i] * vector3);
				Vector3 facePosition = Quaternion.Inverse(Cube.s_faceRotations[Cube.s_neighborIndices[num, i]]) * vector3;
				faces[num].GetNeighbor(i).Tessellate(maxLevel, facePosition, localRadius);
			}
		}

		private Vector3 CubicToSpherical(Vector3 p)
		{
			Vector3 result = default(Vector3);
			result.x = p.x * Mathf.Sqrt(1f - p.y * p.y * 0.5f - p.z * p.z * 0.5f + p.y * p.y * p.z * p.z / 3f);
			result.y = p.y * Mathf.Sqrt(1f - p.z * p.z * 0.5f - p.x * p.x * 0.5f + p.z * p.z * p.x * p.x / 3f);
			result.z = p.z * Mathf.Sqrt(1f - p.x * p.x * 0.5f - p.y * p.y * 0.5f + p.x * p.x * p.y * p.y / 3f);
			return result;
		}

		private Vector3 SphericalToCubic(Vector3 p)
		{
			Vector3 result = default(Vector3);
			Vector3 vector = new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z));
			if (vector.y >= vector.x && vector.y >= vector.z)
			{
				float num = p.x * p.x * 2f;
				float num2 = p.z * p.z * 2f;
				float num3 = 0f - num + num2 - 3f;
				float num4 = 0f - Mathf.Sqrt(num3 * num3 - 12f * num);
				result.x = (OWMath.ApproxEquals(p.x, 0f) ? 0f : (Mathf.Sqrt(num4 + num - num2 + 3f) * 0.70710677f * Mathf.Sign(p.x)));
				result.z = (OWMath.ApproxEquals(p.z, 0f) ? 0f : (Mathf.Sqrt(num4 - num + num2 + 3f) * 0.70710677f * Mathf.Sign(p.z)));
				result.y = ((p.y > 0f) ? 1f : (-1f));
			}
			else if (vector.x >= vector.y && vector.x >= vector.z)
			{
				float num5 = p.y * p.y * 2f;
				float num6 = p.z * p.z * 2f;
				float num7 = 0f - num5 + num6 - 3f;
				float num8 = 0f - Mathf.Sqrt(num7 * num7 - 12f * num5);
				result.y = (OWMath.ApproxEquals(p.y, 0f) ? 0f : (Mathf.Sqrt(num8 + num5 - num6 + 3f) * 0.70710677f * Mathf.Sign(p.y)));
				result.z = (OWMath.ApproxEquals(p.z, 0f) ? 0f : (Mathf.Sqrt(num8 - num5 + num6 + 3f) * 0.70710677f * Mathf.Sign(p.z)));
				result.x = ((p.x > 0f) ? 1f : (-1f));
			}
			else
			{
				float num9 = p.x * p.x * 2f;
				float num10 = p.y * p.y * 2f;
				float num11 = 0f - num9 + num10 - 3f;
				float num12 = 0f - Mathf.Sqrt(num11 * num11 - 12f * num9);
				result.x = (OWMath.ApproxEquals(p.x, 0f) ? 0f : (Mathf.Sqrt(num12 + num9 - num10 + 3f) * 0.70710677f * Mathf.Sign(p.x)));
				result.y = (OWMath.ApproxEquals(p.y, 0f) ? 0f : (Mathf.Sqrt(num12 - num9 + num10 + 3f) * 0.70710677f * Mathf.Sign(p.y)));
				result.z = ((p.z > 0f) ? 1f : (-1f));
			}
			return result;
		}
	}
}
