using System;
using UnityEngine;

public class UnitSphere
{
	public static Vector3 GetPointOnCap(float spotAngle)
	{
		float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		float f2 = UnityEngine.Random.Range(0f, spotAngle * ((float)Math.PI / 180f));
		Vector3 result = new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f);
		result *= Mathf.Sin(f2);
		result.z = Mathf.Cos(f2);
		return result;
	}

	public static Vector3 GetPointOnCap(float spotAngle, Quaternion orientation)
	{
		return orientation * GetPointOnCap(spotAngle);
	}

	public static Vector3 GetPointOnCap(float spotAngle, Transform relativeTo, float radius)
	{
		return relativeTo.TransformPoint(GetPointOnCap(spotAngle) * radius);
	}

	public static Vector3 GetPointOnRing(float innerSpotAngle, float outerSpotAngle)
	{
		float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		float f2 = UnityEngine.Random.Range(innerSpotAngle, outerSpotAngle) * ((float)Math.PI / 180f);
		Vector3 result = new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f);
		result *= Mathf.Sin(f2);
		result.z = Mathf.Cos(f2);
		return result;
	}

	public static Vector3 GetPointOnRing(float innerSpotAngle, float outerSpotAngle, Quaternion orientation)
	{
		return orientation * GetPointOnRing(innerSpotAngle, outerSpotAngle);
	}

	public static Vector3 GetPointOnRing(float innerSpotAngle, float outerSpotAngle, Transform relativeTo, float radius)
	{
		return relativeTo.TransformPoint(GetPointOnRing(innerSpotAngle, outerSpotAngle) * radius);
	}
}
