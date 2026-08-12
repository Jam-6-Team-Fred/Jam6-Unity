using UnityEngine;

public static class ShapeUtil
{
	public static class Sphere
	{
		public static Vector3 CalcWorldSpaceCenter(SphereShape sphereShape)
		{
			return sphereShape.transform.TransformPoint(sphereShape.center);
		}

		public static float CalcWorldSpaceRadius(SphereShape sphereShape)
		{
			float num = 0f;
			Vector3 lossyScale = sphereShape.transform.lossyScale;
			for (int i = 0; i < 3; i++)
			{
				num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
			}
			return sphereShape.radius * num;
		}

		public static Vector3 CalcLocalSpaceInertiaTensor(SphereShape sphereShape)
		{
			float num = sphereShape.radius * sphereShape.radius * 0.4f;
			return new Vector3(num, num, num);
		}

		public static Vector3 GetRandomContainedLocalPoint(SphereShape sphereShape)
		{
			return sphereShape.center + Random.insideUnitSphere * sphereShape.radius;
		}
	}

	public static class Hemisphere
	{
		public static Vector3 CalcWorldSpaceAxis(HemisphereShape hemisphereShape)
		{
			Vector3 zero = Vector3.zero;
			zero[hemisphereShape.direction] = (hemisphereShape.cap ? (-1f) : 1f);
			return hemisphereShape.transform.rotation * zero;
		}

		public static Vector3 GetRandomContainedLocalPoint(HemisphereShape hemisphereShape)
		{
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			if ((insideUnitSphere[hemisphereShape.direction] < 0f && !hemisphereShape.cap) || (insideUnitSphere[hemisphereShape.direction] > 0f && hemisphereShape.cap))
			{
				insideUnitSphere[hemisphereShape.direction] = 0f - insideUnitSphere[hemisphereShape.direction];
			}
			return hemisphereShape.center + insideUnitSphere * hemisphereShape.radius;
		}
	}

	public static class Capsule
	{
		public static Vector3 CalcWorldSpaceCenter(CapsuleShape capsuleShape)
		{
			return capsuleShape.transform.TransformPoint(capsuleShape.center);
		}

		public static float CalcWorldSpaceRadius(CapsuleShape capsuleShape)
		{
			float num = 0f;
			Vector3 lossyScale = capsuleShape.transform.lossyScale;
			int direction = capsuleShape.direction;
			for (int i = 0; i < 3; i++)
			{
				if (i != direction)
				{
					num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
				}
			}
			return capsuleShape.radius * num;
		}

		public static float CalcWorldSpaceHeight(CapsuleShape capsuleShape, float worldSpaceRadius)
		{
			return Mathf.Max(Mathf.Abs(capsuleShape.transform.lossyScale[capsuleShape.direction]) * capsuleShape.height, worldSpaceRadius * 2f);
		}

		public static void CalcWorldSpaceEndpoints(CapsuleShape capsuleShape, out float worldSpaceRadius, out Vector3 worldSpaceP1, out Vector3 worldSpaceP2)
		{
			float num = 0f;
			Vector3 lossyScale = capsuleShape.transform.lossyScale;
			int direction = capsuleShape.direction;
			for (int i = 0; i < 3; i++)
			{
				if (i != direction)
				{
					num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
				}
			}
			worldSpaceRadius = capsuleShape.radius * num;
			float num2 = Mathf.Max(Mathf.Abs(lossyScale[direction]) * capsuleShape.height - worldSpaceRadius * 2f, 0f);
			Vector3 vector = capsuleShape.transform.TransformPoint(capsuleShape.center);
			Vector3 zero = Vector3.zero;
			zero[direction] = num2 * 0.5f;
			Vector3 vector2 = capsuleShape.transform.rotation * zero;
			worldSpaceP1 = vector + vector2;
			worldSpaceP2 = vector - vector2;
		}

		public static Vector3 CalcLocalSpaceInertiaTensor(CapsuleShape capsuleShape)
		{
			float value = 0.4f * (capsuleShape.radius * capsuleShape.radius);
			float num = (3f * capsuleShape.radius + 2f * capsuleShape.height) * 0.125f * capsuleShape.height;
			Vector3 result = new Vector3(num, num, num);
			result[capsuleShape.direction] = value;
			return result;
		}
	}

	public static class Cylinder
	{
		public static Vector3 CalcWorldSpaceCenter(CylinderShape cylinderShape)
		{
			return cylinderShape.transform.TransformPoint(cylinderShape.center);
		}

		public static float CalcWorldSpaceRadius(CylinderShape cylinderShape)
		{
			float num = 0f;
			Vector3 lossyScale = cylinderShape.transform.lossyScale;
			int direction = cylinderShape.direction;
			for (int i = 0; i < 3; i++)
			{
				if (i != direction)
				{
					num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
				}
			}
			return cylinderShape.radius * num;
		}

		public static float CalcWorldSpaceHeight(CylinderShape cylinderShape)
		{
			return Mathf.Abs(cylinderShape.transform.lossyScale[cylinderShape.direction]) * cylinderShape.height;
		}

		public static void CalcWorldSpaceEndpoints(CylinderShape cylinderShape, out float worldSpaceRadius, out Vector3 worldSpaceP1, out Vector3 worldSpaceP2)
		{
			float num = 0f;
			Vector3 lossyScale = cylinderShape.transform.lossyScale;
			int direction = cylinderShape.direction;
			for (int i = 0; i < 3; i++)
			{
				if (i != direction)
				{
					num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
				}
			}
			worldSpaceRadius = cylinderShape.radius * num;
			float num2 = Mathf.Abs(lossyScale[direction]) * cylinderShape.height;
			Vector3 vector = cylinderShape.transform.TransformPoint(cylinderShape.center);
			Vector3 zero = Vector3.zero;
			zero[direction] = num2 * 0.5f;
			Vector3 vector2 = cylinderShape.transform.rotation * zero;
			worldSpaceP1 = vector + vector2;
			worldSpaceP2 = vector - vector2;
		}

		public static Vector3 CalcLocalSpaceInertiaTensor(CylinderShape cylinderShape)
		{
			float num = cylinderShape.radius * cylinderShape.radius;
			float num2 = cylinderShape.height * cylinderShape.height;
			float value = num * 0.5f;
			float num3 = 0.083333f * (3f * num + num2);
			Vector3 result = new Vector3(num3, num3, num3);
			result[cylinderShape.direction] = value;
			return result;
		}

		public static Vector3 GetRandomContainedLocalPoint(CylinderShape cylinderShape)
		{
			float num = Random.Range(0f - cylinderShape.height, cylinderShape.height) * 0.5f;
			Vector2 vector = Random.insideUnitCircle * cylinderShape.radius;
			if (cylinderShape.direction == 0)
			{
				return cylinderShape.center + new Vector3(num, vector.x, vector.y);
			}
			if (cylinderShape.direction == 1)
			{
				return cylinderShape.center + new Vector3(vector.x, num, vector.y);
			}
			return cylinderShape.center + new Vector3(vector.x, vector.y, num);
		}
	}

	public static class Cone
	{
		public static Vector3 CalcWorldSpaceCenter(ConeShape coneShape)
		{
			return coneShape.transform.TransformPoint(coneShape.center);
		}

		public static void CalcWorldSpaceRadius(ConeShape coneShape, out float worldSpaceTopRadius, out float worldSpaceBottomRadius)
		{
			float num = 0f;
			Vector3 lossyScale = coneShape.transform.lossyScale;
			int direction = coneShape.direction;
			for (int i = 0; i < 3; i++)
			{
				if (i != direction)
				{
					num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
				}
			}
			worldSpaceTopRadius = coneShape.topRadius * num;
			worldSpaceBottomRadius = coneShape.bottomRadius * num;
		}

		public static float CalcWorldSpaceHeight(ConeShape coneShape)
		{
			return Mathf.Abs(coneShape.transform.lossyScale[coneShape.direction]) * coneShape.height;
		}

		public static void CalcWorldSpaceEndpoints(ConeShape coneShape, out float worldSpaceTopRadius, out float worldSpaceBottomRadius, out Vector3 worldSpaceP1, out Vector3 worldSpaceP2)
		{
			float num = 0f;
			Vector3 lossyScale = coneShape.transform.lossyScale;
			int direction = coneShape.direction;
			for (int i = 0; i < 3; i++)
			{
				if (i != direction)
				{
					num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
				}
			}
			worldSpaceTopRadius = coneShape.topRadius * num;
			worldSpaceBottomRadius = coneShape.bottomRadius * num;
			float num2 = Mathf.Abs(lossyScale[direction]) * coneShape.height;
			Vector3 vector = coneShape.transform.TransformPoint(coneShape.center);
			Vector3 zero = Vector3.zero;
			zero[direction] = num2 * 0.5f;
			Vector3 vector2 = coneShape.transform.rotation * zero;
			worldSpaceP1 = vector + vector2;
			worldSpaceP2 = vector - vector2;
		}

		public static Vector3 CalcLocalSpaceInertiaTensor(ConeShape coneShape)
		{
			float num = (coneShape.topRadius + coneShape.bottomRadius) * 0.5f;
			float num2 = num * num;
			float num3 = coneShape.height * coneShape.height;
			float value = num2 * 0.5f;
			float num4 = 0.083333f * (3f * num2 + num3);
			Vector3 result = new Vector3(num4, num4, num4);
			result[coneShape.direction] = value;
			return result;
		}
	}

	public static class Box
	{
		public static Vector3 CalcWorldSpaceCenter(BoxShape boxShape)
		{
			return boxShape.transform.TransformPoint(boxShape.center);
		}

		public static Vector3 CalcWorldSpaceSize(BoxShape boxShape)
		{
			return Vector3.Scale(boxShape.size, boxShape.transform.lossyScale);
		}

		public static void CalcWorldSpaceAxes(BoxShape boxShape, out Vector3 right, out Vector3 up, out Vector3 forward)
		{
			Quaternion rotation = boxShape.transform.rotation;
			right = rotation * Vector3.right;
			up = rotation * Vector3.up;
			forward = rotation * Vector3.forward;
		}

		public static void CalcWorldSpaceAxes(BoxShape boxShape, ref Vector3[] axes)
		{
			Quaternion rotation = boxShape.transform.rotation;
			axes[0] = rotation * Vector3.right;
			axes[1] = rotation * Vector3.up;
			axes[2] = rotation * Vector3.forward;
		}

		public static void CalcWorldSpaceData(BoxShape boxShape, out Vector3 center, out Vector3 size, ref Vector3[] axes, ref Vector3[] verts)
		{
			center = boxShape.transform.TransformPoint(boxShape.center);
			size = Vector3.Scale(boxShape.size, boxShape.transform.lossyScale);
			Quaternion rotation = boxShape.transform.rotation;
			axes[0] = rotation * Vector3.right;
			axes[1] = rotation * Vector3.up;
			axes[2] = rotation * Vector3.forward;
			Vector3 vector = size * 0.5f;
			Vector3 vector2 = axes[0] * vector.x;
			Vector3 vector3 = axes[1] * vector.y;
			Vector3 vector4 = axes[2] * vector.z;
			verts[0] = center + vector2 + vector3 + vector4;
			verts[1] = center - vector2 + vector3 + vector4;
			verts[2] = center + vector2 - vector3 + vector4;
			verts[3] = center - vector2 - vector3 + vector4;
			verts[4] = center + vector2 + vector3 - vector4;
			verts[5] = center - vector2 + vector3 - vector4;
			verts[6] = center + vector2 - vector3 - vector4;
			verts[7] = center - vector2 - vector3 - vector4;
		}

		public static Vector3 CalcLocalSpaceInertiaTensor(BoxShape boxShape)
		{
			Vector3 size = boxShape.size;
			Vector3 vector = new Vector3(size.x * size.x, size.y * size.y, size.z * size.z);
			float x = 0.083333f * (vector.y + vector.z);
			float y = 0.083333f * (vector.x + vector.z);
			float z = 0.083333f * (vector.x + vector.y);
			return new Vector3(x, y, z);
		}

		public static Vector3 GetRandomContainedLocalPoint(BoxShape boxShape)
		{
			Vector3 extents = boxShape.extents;
			float x = Random.Range(0f - extents.x, extents.x);
			float y = Random.Range(0f - extents.y, extents.y);
			float z = Random.Range(0f - extents.z, extents.z);
			return boxShape.center + new Vector3(x, y, z);
		}
	}

	private static readonly int[] bitPosition = new int[32]
	{
		0, 9, 1, 10, 13, 21, 2, 29, 11, 14,
		16, 18, 22, 25, 3, 30, 8, 12, 20, 28,
		15, 17, 24, 7, 19, 27, 23, 6, 26, 5,
		4, 31
	};

	public static int LayerToIndex(Shape.Layer layer)
	{
		int num = (int)layer;
		num |= num >> 1;
		num |= num >> 2;
		num |= num >> 4;
		num |= num >> 8;
		num |= num >> 16;
		return bitPosition[(long)num * 130329821L >> 27];
	}
}
