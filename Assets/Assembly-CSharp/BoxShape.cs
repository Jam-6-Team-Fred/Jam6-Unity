using ShapeCollision;
using UnityEngine;

[AddComponentMenu("Shapes/Box Shape", 3)]
public class BoxShape : Shape
{
	[SerializeField]
	protected Vector3 _center = Vector3.zero;

	[SerializeField]
	protected Vector3 _size = Vector3.one;

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

	public Vector3 size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
			RecalculateLocalBounds();
		}
	}

	public Vector3 extents
	{
		get
		{
			return _size * 0.5f;
		}
		set
		{
			_size = value * 2f;
			RecalculateLocalBounds();
		}
	}

	[ContextMenu("Copy from Box Collider", true)]
	private bool ValidateCopySettingsFromCollider()
	{
		return GetComponent<BoxCollider>() != null;
	}

	[ContextMenu("Copy from Box Collider", false)]
	private void CopySettingsFromCollider()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		_center = component.center;
		_size = component.size;
	}

	[ContextMenu("Copy from Box Proximity Trigger", true)]
	private bool ValidateCopySettingsFromBoxProximityTrigger()
	{
		return GetComponent<BoxProximityTrigger>() != null;
	}

	[ContextMenu("Copy from Box Proximity Trigger", false)]
	private void CopySettingsFromBoxProximityTrigger()
	{
		BoxProximityTrigger component = GetComponent<BoxProximityTrigger>();
		_center = Vector3.zero;
		_size = component.size;
	}

	protected override void Reset()
	{
		base.Reset();
		MeshFilter component = GetComponent<MeshFilter>();
		if (GetComponent<MeshRenderer>() != null && component != null && component.sharedMesh != null)
		{
			_center = component.sharedMesh.bounds.center;
			_size = component.sharedMesh.bounds.size;
		}
		else
		{
			_center.Set(0f, 0f, 0f);
			_size.Set(1f, 1f, 1f);
		}
	}

	protected override void RecalculateLocalBounds()
	{
		_localBounds.Set(_center, _size.magnitude * 0.5f);
	}

	public override Vector3 GetWorldSpaceCenter()
	{
		return ShapeUtil.Box.CalcWorldSpaceCenter(this);
	}

	public override bool PointInside(Vector3 point)
	{
		Vector3 boxCenter = ShapeUtil.Box.CalcWorldSpaceCenter(this);
		Vector3 boxSize = ShapeUtil.Box.CalcWorldSpaceSize(this);
		Vector3[] axes = new Vector3[3];
		ShapeUtil.Box.CalcWorldSpaceAxes(this, ref axes);
		return ShapeCollision.PointInside.Box(point, boxCenter, boxSize, axes);
	}

	public override Vector3 ClosestPoint(Vector3 point)
	{
		Vector3 boxCenter = ShapeUtil.Box.CalcWorldSpaceCenter(this);
		Vector3 boxSize = ShapeUtil.Box.CalcWorldSpaceSize(this);
		Vector3[] axes = new Vector3[3];
		ShapeUtil.Box.CalcWorldSpaceAxes(this, ref axes);
		return ShapeCollision.ClosestPoint.Box(point, boxCenter, boxSize, axes);
	}

	public override float PenetrationDistance(Vector3 point)
	{
		Vector3 boxCenter = ShapeUtil.Box.CalcWorldSpaceCenter(this);
		Vector3 boxSize = ShapeUtil.Box.CalcWorldSpaceSize(this);
		Vector3[] axes = new Vector3[3];
		ShapeUtil.Box.CalcWorldSpaceAxes(this, ref axes);
		Vector3 vector = Penetration.Box(point, boxCenter, boxSize, axes);
		int index = 0;
		float num = float.PositiveInfinity;
		for (int i = 0; i < 3; i++)
		{
			float num2 = Mathf.Abs(vector[i]);
			if (num2 < num)
			{
				index = i;
				num = num2;
			}
		}
		return vector[index];
	}

	public override Vector3 GetRandomPointInsideShape()
	{
		return base.transform.TransformPoint(ShapeUtil.Box.GetRandomContainedLocalPoint(this));
	}

	public override Vector3 GetLocalInertiaTensor()
	{
		return ShapeUtil.Box.CalcLocalSpaceInertiaTensor(this);
	}

	public override bool IsVisible(Plane[] frustumPlanes)
	{
		Vector3 lhs = base.transform.TransformDirection(Vector3.right);
		Vector3 lhs2 = base.transform.TransformDirection(Vector3.up);
		Vector3 lhs3 = base.transform.TransformDirection(Vector3.forward);
		Vector3 one = Vector3.one;
		for (int i = 0; i < 6; i++)
		{
			Plane plane = frustumPlanes[i];
			float f = Vector3.Dot(lhs, plane.normal);
			one.x = Mathf.Sign(f) * 0.5f * _size.x;
			f = Vector3.Dot(lhs2, plane.normal);
			one.y = Mathf.Sign(f) * 0.5f * _size.y;
			f = Vector3.Dot(lhs3, plane.normal);
			one.z = Mathf.Sign(f) * 0.5f * _size.z;
			one += _center;
			if (Vector3.Dot(base.transform.TransformPoint(one), plane.normal) + plane.distance < 0f)
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsBlocked(Plane[] frustumPlanes)
	{
		Vector3 lhs = base.transform.TransformDirection(Vector3.right);
		Vector3 lhs2 = base.transform.TransformDirection(Vector3.up);
		Vector3 lhs3 = base.transform.TransformDirection(Vector3.forward);
		Vector3 one = Vector3.one;
		for (int i = 0; i < frustumPlanes.Length; i++)
		{
			Plane plane = frustumPlanes[i];
			float f = Vector3.Dot(lhs, plane.normal);
			one.x = (0f - Mathf.Sign(f)) * 0.5f * _size.x;
			f = Vector3.Dot(lhs2, plane.normal);
			one.y = (0f - Mathf.Sign(f)) * 0.5f * _size.y;
			f = Vector3.Dot(lhs3, plane.normal);
			one.z = (0f - Mathf.Sign(f)) * 0.5f * _size.z;
			one += _center;
			if (Vector3.Dot(base.transform.TransformPoint(one), plane.normal) + plane.distance < 0f)
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsBlocked(Vector3 cameraPos, Vector3 centerLine, float sphereDist, float halfAngle)
	{
		Vector3[] array = new Vector3[8]
		{
			0.5f * new Vector3(0f - _size.x, 0f - _size.y, 0f - _size.z),
			0.5f * new Vector3(_size.x, 0f - _size.y, 0f - _size.z),
			0.5f * new Vector3(_size.x, _size.y, 0f - _size.z),
			0.5f * new Vector3(0f - _size.x, _size.y, 0f - _size.z),
			0.5f * new Vector3(0f - _size.x, 0f - _size.y, _size.z),
			0.5f * new Vector3(_size.x, 0f - _size.y, _size.z),
			0.5f * new Vector3(_size.x, _size.y, _size.z),
			0.5f * new Vector3(0f - _size.x, _size.y, _size.z)
		};
		float num = Mathf.Cos(halfAngle);
		for (int i = 0; i < 8; i++)
		{
			Vector3 vector = base.transform.TransformPoint(array[i] + _center) - cameraPos;
			float magnitude = vector.magnitude;
			if (magnitude <= sphereDist)
			{
				return false;
			}
			if (Vector3.Dot(centerLine, (vector / magnitude).normalized) < num)
			{
				return false;
			}
		}
		return true;
	}

	// CHANGED
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
		Gizmos.DrawWireCube(_center, _size);
	}
}
