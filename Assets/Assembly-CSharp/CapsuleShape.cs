using ShapeCollision;
using UnityEngine;

[AddComponentMenu("Shapes/Capsule Shape", 2)]
public class CapsuleShape : Shape
{
	[SerializeField]
	protected Vector3 _center = Vector3.zero;

	[SerializeField]
	protected float _radius = 0.5f;

	[SerializeField]
	protected float _height = 1f;

	[SerializeField]
	protected int _direction = 1;

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
			_radius = Mathf.Max(value, 0f);
			RecalculateLocalBounds();
		}
	}

	public float height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = Mathf.Max(value, 0f);
			RecalculateLocalBounds();
		}
	}

	public int direction
	{
		get
		{
			return _direction;
		}
		set
		{
			_direction = Mathf.Clamp(value, 0, 2);
		}
	}

	[ContextMenu("Copy from Capsule Collider", true)]
	private bool ValidateCopySettingsFromCollider()
	{
		return GetComponent<CapsuleCollider>() != null;
	}

	[ContextMenu("Copy from Capsule Collider", false)]
	private void CopySettingsFromCollider()
	{
		CapsuleCollider component = GetComponent<CapsuleCollider>();
		_center = component.center;
		_radius = component.radius;
		_height = component.height;
		_direction = component.direction;
	}

	[ContextMenu("Copy from Capsule Proximity Trigger", true)]
	private bool ValidateCopySettingsFromCapsuleProximityTrigger()
	{
		return GetComponent<CapsuleProximityTrigger>() != null;
	}

	[ContextMenu("Copy from Capsule Proximity Trigger", false)]
	private void CopySettingsFromCapsuleProximityTrigger()
	{
		CapsuleProximityTrigger component = GetComponent<CapsuleProximityTrigger>();
		_center = Vector3.zero;
		_radius = component.radius;
		_height = component.length + component.radius * 2f;
		_direction = 1;
	}

	protected override void Reset()
	{
		base.Reset();
		MeshFilter component = GetComponent<MeshFilter>();
		if (GetComponent<MeshRenderer>() != null && component != null && component.sharedMesh != null)
		{
			_center = component.sharedMesh.bounds.center;
			_radius = Mathf.Max(component.sharedMesh.bounds.extents.x, Mathf.Max(component.sharedMesh.bounds.extents.y, component.sharedMesh.bounds.extents.z));
			_height = component.sharedMesh.bounds.size.y;
			_direction = 1;
		}
		else
		{
			_center.Set(0f, 0f, 0f);
			_radius = 0.5f;
			_height = 1f;
			_direction = 1;
		}
	}

	protected virtual void OnValidate()
	{
		if (_radius < 0f)
		{
			_radius = 0f;
		}
		if (_height < 0f)
		{
			_height = 0f;
		}
		if (_direction < 0)
		{
			_direction = 0;
		}
		if (_direction > 2)
		{
			_direction = 2;
		}
	}

	protected override void RecalculateLocalBounds()
	{
		_localBounds.Set(_center, Mathf.Max(_radius, _height * 0.5f));
	}

	public override Vector3 GetWorldSpaceCenter()
	{
		return ShapeUtil.Capsule.CalcWorldSpaceCenter(this);
	}

	public override bool PointInside(Vector3 point)
	{
		ShapeUtil.Capsule.CalcWorldSpaceEndpoints(this, out var worldSpaceRadius, out var worldSpaceP, out var worldSpaceP2);
		return ShapeCollision.PointInside.Capsule(point, worldSpaceP, worldSpaceP2, worldSpaceRadius);
	}

	public override Vector3 ClosestPoint(Vector3 point)
	{
		ShapeUtil.Capsule.CalcWorldSpaceEndpoints(this, out var worldSpaceRadius, out var worldSpaceP, out var worldSpaceP2);
		return ShapeCollision.ClosestPoint.Capsule(point, worldSpaceP, worldSpaceP2, worldSpaceRadius);
	}

	public override float PenetrationDistance(Vector3 point)
	{
		ShapeUtil.Capsule.CalcWorldSpaceEndpoints(this, out var worldSpaceRadius, out var worldSpaceP, out var worldSpaceP2);
		return Penetration.Capsule(point, worldSpaceP, worldSpaceP2, worldSpaceRadius);
	}

	public override Vector3 GetLocalInertiaTensor()
	{
		return ShapeUtil.Capsule.CalcLocalSpaceInertiaTensor(this);
	}

	public override bool IsVisible(Plane[] frustumPlanes)
	{
		float num = ShapeUtil.Capsule.CalcWorldSpaceRadius(this);
		Vector3 vector = Matrix4x4.identity.GetColumn(_direction);
		vector = vector * 0.5f * _height;
		Vector3 vector2 = base.transform.TransformPoint(_center - vector);
		Vector3 vector3 = base.transform.TransformPoint(_center + vector);
		vector = vector3 - vector2;
		for (int i = 0; i < 6; i++)
		{
			Plane plane = frustumPlanes[i];
			if (Vector3.Dot((Vector3.Dot(vector, plane.normal) < 0f) ? vector2 : vector3, plane.normal) + plane.distance < 0f - num)
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsBlocked(Plane[] frustumPlanes)
	{
		float num = ShapeUtil.Capsule.CalcWorldSpaceRadius(this);
		Vector3 vector = Matrix4x4.identity.GetColumn(_direction);
		vector = vector * 0.5f * _height;
		Vector3 vector2 = base.transform.TransformPoint(_center - vector);
		Vector3 vector3 = base.transform.TransformPoint(_center + vector);
		vector = vector3 - vector2;
		for (int i = 0; i < frustumPlanes.Length; i++)
		{
			Plane plane = frustumPlanes[i];
			if (Vector3.Dot((Vector3.Dot(vector, plane.normal) < 0f) ? vector3 : vector2, plane.normal) + plane.distance < num)
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsBlocked(Vector3 cameraPos, Vector3 centerLine, float sphereDist, float halfAngle)
	{
		float num = ShapeUtil.Capsule.CalcWorldSpaceRadius(this);
		Vector3 vector = Matrix4x4.identity.GetColumn(_direction);
		vector = vector * 0.5f * _height;
		Vector3 vector2 = base.transform.TransformPoint(_center - vector);
		Vector3 vector3 = base.transform.TransformPoint(_center + vector);
		float num2 = Mathf.Cos(halfAngle);
		Vector3 vector4 = num * Mathf.Sin(halfAngle) * centerLine;
		Vector3 vector5 = vector2 - cameraPos;
		float num3 = Vector3.Dot(centerLine, vector5);
		if (num3 <= sphereDist)
		{
			return false;
		}
		Vector3 vector6 = centerLine * num3;
		Vector3 vector7 = num * num2 * (vector5 - vector6).normalized;
		if (Vector3.Dot(centerLine, (vector2 + vector7 - vector4 - cameraPos).normalized) < num2)
		{
			return false;
		}
		Vector3 vector8 = vector3 - cameraPos;
		float num4 = Vector3.Dot(centerLine, vector8);
		if (num4 <= sphereDist)
		{
			return false;
		}
		Vector3 vector9 = centerLine * num4;
		Vector3 vector10 = num * num2 * (vector8 - vector9).normalized;
		if (Vector3.Dot(centerLine, (vector2 + vector10 - vector4 - cameraPos).normalized) < num2)
		{
			return false;
		}
		return true;
	}

	// CHANGED
	// BUG: doesnt work for direction
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		var center = ShapeUtil.Capsule.CalcWorldSpaceCenter(this);
		ShapeUtil.Capsule.CalcWorldSpaceEndpoints(this, out var radius, out var p1, out var p2);
		var height = Vector3.Distance(p1, p2);
		OWGizmos.DrawWireCapsule(center, transform.rotation, height, radius);
	}
}
