using System;
using UnityEngine;

public static class OWPhysics
{
	public static Vector3 FromToAngularVelocity(Vector3 fromDirection, Vector3 toDirection)
	{
		Vector3 vector = Vector3.Cross(fromDirection.normalized, toDirection.normalized);
		float num = Mathf.Asin(vector.magnitude);
		return vector.normalized * num / Time.fixedDeltaTime;
	}

	public static Vector3 FromToAngularVelocity(Quaternion fromRotation, Quaternion toRotation)
	{
		Quaternion quaternion = toRotation * Quaternion.Inverse(fromRotation);
		return new Vector3(Mathf.DeltaAngle(0f, quaternion.eulerAngles.x), Mathf.DeltaAngle(0f, quaternion.eulerAngles.y), Mathf.DeltaAngle(0f, quaternion.eulerAngles.z)) * ((float)Math.PI / 180f) / Time.fixedDeltaTime;
	}

	public static Vector3 FromToAngularImpulse(Vector3 fromDirection, Vector3 toDirection, Rigidbody rigidbody)
	{
		Vector3 vector = Vector3.Cross(fromDirection.normalized, toDirection.normalized);
		float num = Mathf.Asin(vector.magnitude);
		Vector3 vector2 = vector.normalized * num / Time.fixedDeltaTime;
		Quaternion quaternion = rigidbody.transform.rotation * rigidbody.inertiaTensorRotation;
		return quaternion * Vector3.Scale(rigidbody.inertiaTensor, Quaternion.Inverse(quaternion) * vector2);
	}

	public static Vector3 PointTangentialVelocity(Vector3 point, Vector3 centerOfRotation, Vector3 angularVelocity)
	{
		Vector3 rhs = point - centerOfRotation;
		return Vector3.Cross(angularVelocity, rhs);
	}

	public static Vector3 CalculateOrbitVelocity(OWRigidbody primaryBody, OWRigidbody satelliteBody, float orbitAngle = 0f)
	{
		GravityVolume attachedGravityVolume = primaryBody.GetAttachedGravityVolume();
		if (attachedGravityVolume == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = satelliteBody.GetWorldCenterOfMass() - primaryBody.GetWorldCenterOfMass();
		Vector3 normalized = Vector3.Cross(vector, Vector3.up).normalized;
		normalized = Quaternion.AngleAxis(orbitAngle, vector) * normalized;
		float num = Mathf.Sqrt(attachedGravityVolume.CalculateForceAccelerationOnBody(satelliteBody).magnitude * vector.magnitude);
		return normalized * num;
	}

	public static float GetSphereRadius(SphereCollider sphereCollider)
	{
		float num = 0f;
		Vector3 lossyScale = sphereCollider.transform.lossyScale;
		for (int i = 0; i < 3; i++)
		{
			num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
		}
		return sphereCollider.radius * num;
	}

	public static void GetCapsuleEndpoints(CapsuleCollider capsuleCollider, out Vector3 p1, out Vector3 p2)
	{
		Vector3 zero = Vector3.zero;
		zero[capsuleCollider.direction] = 1f;
		Vector3 vector = zero * (capsuleCollider.height * 0.5f - capsuleCollider.radius);
		p1 = capsuleCollider.transform.TransformPoint(capsuleCollider.center + vector);
		p2 = capsuleCollider.transform.TransformPoint(capsuleCollider.center - vector);
	}

	public static float GetCapsuleRadius(CapsuleCollider capsuleCollider)
	{
		float num = 0f;
		Vector3 lossyScale = capsuleCollider.transform.lossyScale;
		int direction = capsuleCollider.direction;
		for (int i = 0; i < 3; i++)
		{
			if (i != direction)
			{
				num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
			}
		}
		return capsuleCollider.radius * num;
	}

	public static Vector3 ClosestPointOnSurface(SphereCollider sphereCollider, Vector3 worldPosition)
	{
		Vector3 vector = sphereCollider.transform.TransformPoint(sphereCollider.center);
		Vector3 vector2 = worldPosition - vector;
		if (vector2.sqrMagnitude < 1.0000001E-06f)
		{
			vector2 = Vector3.up;
		}
		return vector2.normalized * sphereCollider.radius;
	}

	public static Vector3 ClosestPointOnSurface(CapsuleCollider capsuleCollider, Vector3 worldPosition)
	{
		GetCapsuleEndpoints(capsuleCollider, out var p, out var p2);
		Vector3 vector = OWMath.ClosestPointOnSegment(worldPosition, p, p2);
		Vector3 vector2 = worldPosition - vector;
		if (vector2.sqrMagnitude < 1.0000001E-06f)
		{
			vector2 = Vector3.up;
		}
		return vector2.normalized * capsuleCollider.radius;
	}

	public static Vector3 ClosestPointOnSurface(BoxCollider boxCollider, Vector3 worldPosition)
	{
		Vector3 position = OWMath.ClosestPointOnBox(boxCollider.transform.InverseTransformPoint(worldPosition), extents: boxCollider.size * 0.5f, center: Vector3.zero);
		return boxCollider.transform.TransformPoint(position);
	}

	public static float GetDistToCenter(SphereCollider sphereCollider, Vector3 worldPosition)
	{
		Vector3 b = sphereCollider.transform.TransformPoint(sphereCollider.center);
		return Vector3.Distance(worldPosition, b);
	}

	public static float GetDistToCenter(CapsuleCollider capsuleCollider, Vector3 worldPosition)
	{
		GetCapsuleEndpoints(capsuleCollider, out var p, out var p2);
		return OWMath.PointSegmentDistance(worldPosition, p, p2);
	}

	public static bool IsPointContained(SphereCollider sphereCollider, Vector3 worldPosition)
	{
		Vector3 vector = sphereCollider.transform.TransformPoint(sphereCollider.center);
		float num = sphereCollider.radius * Mathf.Max(Mathf.Max(sphereCollider.transform.lossyScale.x, sphereCollider.transform.lossyScale.y), sphereCollider.transform.lossyScale.z);
		return (worldPosition - vector).sqrMagnitude < num * num;
	}

	public static bool IsPointContained(CapsuleCollider capsuleCollider, Vector3 worldPosition)
	{
		GetCapsuleEndpoints(capsuleCollider, out var p, out var p2);
		Vector3 vector = OWMath.ClosestPointOnSegment(worldPosition, p, p2);
		float num = capsuleCollider.radius * Mathf.Max(capsuleCollider.transform.lossyScale.x, capsuleCollider.transform.lossyScale.y, capsuleCollider.transform.lossyScale.z);
		return (worldPosition - vector).sqrMagnitude < num * num;
	}

	public static bool IsPointContained(BoxCollider boxCollider, Vector3 worldPosition)
	{
		return OWMath.PointInBox(boxCollider.transform.InverseTransformPoint(worldPosition), extents: boxCollider.size * 0.5f, center: Vector3.zero);
	}

	public static float GetDistToSurface(SphereCollider sphereCollider, Vector3 worldPosition)
	{
		Vector3 b = sphereCollider.transform.TransformPoint(sphereCollider.center);
		float num = Vector3.Distance(worldPosition, b);
		return GetSphereRadius(sphereCollider) - num;
	}

	public static float GetDistToSurface(CapsuleCollider capsuleCollider, Vector3 worldPosition)
	{
		GetCapsuleEndpoints(capsuleCollider, out var p, out var p2);
		float num = OWMath.PointSegmentDistance(worldPosition, p, p2);
		return GetCapsuleRadius(capsuleCollider) - num;
	}
}
