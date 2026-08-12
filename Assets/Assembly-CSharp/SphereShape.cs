using ShapeCollision;
using UnityEngine;

[AddComponentMenu("Shapes/Sphere Shape", 1)]
public class SphereShape : Shape
{
	[SerializeField]
	protected Vector3 _center = Vector3.zero;

	[SerializeField]
	protected float _radius = 0.5f;

	public Vector3 center
	{
		get
		{
			return _center;
		}
		set
		{
			_center = value;
			RecalculateLocalBounds();
		}
	}

	public float radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = Mathf.Max(value, 1E-05f);
			RecalculateLocalBounds();
		}
	}

	[ContextMenu("Copy from Sphere Collider", true)]
	private bool ValidateCopySettingsFromCollider()
	{
		return GetComponent<SphereCollider>() != null;
	}

	[ContextMenu("Copy from Sphere Collider", false)]
	private void CopySettingsFromCollider()
	{
		SphereCollider component = GetComponent<SphereCollider>();
		_center = component.center;
		_radius = component.radius;
	}

	[ContextMenu("Copy from Sphere Proximity Trigger", true)]
	private bool ValidateCopySettingsFromSphereProximityTrigger()
	{
		return GetComponent<SphereProximityTrigger>() != null;
	}

	[ContextMenu("Copy from Sphere Proximity Trigger", false)]
	private void CopySettingsFromSphereProximityTrigger()
	{
		SphereProximityTrigger component = GetComponent<SphereProximityTrigger>();
		_center = Vector3.zero;
		_radius = component.radius;
	}

	protected override void Reset()
	{
		base.Reset();
		MeshFilter component = GetComponent<MeshFilter>();
		if (GetComponent<MeshRenderer>() != null && component != null && component.sharedMesh != null)
		{
			_center = component.sharedMesh.bounds.center;
			_radius = Mathf.Max(component.sharedMesh.bounds.extents.x, Mathf.Max(component.sharedMesh.bounds.extents.y, component.sharedMesh.bounds.extents.z));
		}
		else
		{
			_center.Set(0f, 0f, 0f);
			_radius = 0.5f;
		}
	}

	protected virtual void OnValidate()
	{
		if (_radius < 1E-05f)
		{
			_radius = 1E-05f;
		}
	}

	protected override void RecalculateLocalBounds()
	{
		_localBounds.Set(_center, _radius);
	}

	public override Vector3 GetWorldSpaceCenter()
	{
		return ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
	}

	public override bool PointInside(Vector3 point)
	{
		Vector3 sphereCenter = ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
		float sphereRadius = ShapeUtil.Sphere.CalcWorldSpaceRadius(this);
		return ShapeCollision.PointInside.Sphere(point, sphereCenter, sphereRadius);
	}

	public override Vector3 ClosestPoint(Vector3 point)
	{
		Vector3 sphereCenter = ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
		float sphereRadius = ShapeUtil.Sphere.CalcWorldSpaceRadius(this);
		return ShapeCollision.ClosestPoint.Sphere(point, sphereCenter, sphereRadius);
	}

	public override float PenetrationDistance(Vector3 point)
	{
		Vector3 sphereCenter = ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
		float sphereRadius = ShapeUtil.Sphere.CalcWorldSpaceRadius(this);
		return Penetration.Sphere(point, sphereCenter, sphereRadius);
	}

	public override Vector3 GetRandomPointInsideShape()
	{
		return base.transform.TransformPoint(ShapeUtil.Sphere.GetRandomContainedLocalPoint(this));
	}

	public override Vector3 GetLocalInertiaTensor()
	{
		return ShapeUtil.Sphere.CalcLocalSpaceInertiaTensor(this);
	}

	public override bool IsVisible(Plane[] frustumPlanes)
	{
		Vector3 lhs = ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
		float num = ShapeUtil.Sphere.CalcWorldSpaceRadius(this);
		for (int i = 0; i < 6; i++)
		{
			Plane plane = frustumPlanes[i];
			if (Vector3.Dot(lhs, plane.normal) + plane.distance < 0f - num)
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsBlocked(Plane[] frustumPlanes)
	{
		Vector3 lhs = ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
		float num = ShapeUtil.Sphere.CalcWorldSpaceRadius(this);
		for (int i = 0; i < frustumPlanes.Length; i++)
		{
			Plane plane = frustumPlanes[i];
			if (Vector3.Dot(lhs, plane.normal) + plane.distance < num)
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsBlocked(Vector3 cameraPos, Vector3 centerLine, float sphereDist, float halfAngle)
	{
		Vector3 vector = ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
		float num = ShapeUtil.Sphere.CalcWorldSpaceRadius(this);
		Vector3 vector2 = vector - cameraPos;
		float num2 = Vector3.Dot(centerLine, vector2);
		if (num2 <= sphereDist)
		{
			return false;
		}
		Vector3 vector3 = centerLine * num2;
		Vector3 vector4 = vector2 - vector3;
		float num3 = Mathf.Cos(halfAngle);
		Vector3 vector5 = num * num3 * vector4.normalized;
		Vector3 vector6 = num * Mathf.Sin(halfAngle) * centerLine;
		return Vector3.Dot(centerLine, (vector + vector5 - vector6 - cameraPos).normalized) >= num3;
	}

	// CHANGED
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		float scale = 0f;
		Vector3 lossyScale = transform.lossyScale;
		for (int i = 0; i < 3; i++)
		{
			scale = Mathf.Max(scale, Mathf.Abs(lossyScale[i]));
		}
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one * scale);
		Gizmos.DrawWireSphere(_center, _radius);
	}
}
