using System;
using ShapeCollision;
using UnityEngine;

[AddComponentMenu("Shapes/Cone Shape", 5)]
public class ConeShape : Shape
{
	[SerializeField]
	protected Vector3 _center = Vector3.zero;

	[SerializeField]
	protected float _topRadius;

	[SerializeField]
	protected float _bottomRadius = 0.5f;

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

	public float topRadius
	{
		get
		{
			return _topRadius;
		}
		set
		{
			_topRadius = Mathf.Max(value, 0f);
			RecalculateLocalBounds();
		}
	}

	public float bottomRadius
	{
		get
		{
			return _bottomRadius;
		}
		set
		{
			_bottomRadius = Mathf.Max(value, 0f);
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

	protected override void Reset()
	{
		base.Reset();
		MeshFilter component = GetComponent<MeshFilter>();
		if (GetComponent<MeshRenderer>() != null && component != null && component.sharedMesh != null)
		{
			_center = component.sharedMesh.bounds.center;
			_topRadius = 0f;
			_bottomRadius = Mathf.Max(component.sharedMesh.bounds.extents.x, Mathf.Max(component.sharedMesh.bounds.extents.y, component.sharedMesh.bounds.extents.z));
			_height = component.sharedMesh.bounds.size.y;
			_direction = 1;
		}
		else
		{
			_center.Set(0f, 0f, 0f);
			_topRadius = 0f;
			_bottomRadius = 0.5f;
			_height = 1f;
			_direction = 1;
		}
	}

	protected virtual void OnValidate()
	{
		if (_topRadius < 0f)
		{
			_topRadius = 0f;
		}
		if (_bottomRadius < 0f)
		{
			_bottomRadius = 0f;
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
		_localBounds.Set(_center, new Vector2(Mathf.Max(_topRadius, _bottomRadius), _height * 0.5f).magnitude);
	}

	public override Vector3 GetWorldSpaceCenter()
	{
		return ShapeUtil.Cone.CalcWorldSpaceCenter(this);
	}

	public override bool PointInside(Vector3 point)
	{
		ShapeUtil.Cone.CalcWorldSpaceEndpoints(this, out var worldSpaceTopRadius, out var worldSpaceBottomRadius, out var worldSpaceP, out var worldSpaceP2);
		return ShapeCollision.PointInside.Cone(point, worldSpaceP, worldSpaceP2, worldSpaceTopRadius, worldSpaceBottomRadius);
	}

	public override Vector3 ClosestPoint(Vector3 point)
	{
		throw new NotImplementedException();
	}

	public override float PenetrationDistance(Vector3 point)
	{
		throw new NotImplementedException();
	}

	public override Vector3 GetLocalInertiaTensor()
	{
		return ShapeUtil.Cone.CalcLocalSpaceInertiaTensor(this);
	}

	// CHANGED
	// BUG: doesnt work for direction
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		var center = ShapeUtil.Cone.CalcWorldSpaceCenter(this);
		ShapeUtil.Cone.CalcWorldSpaceEndpoints(this, out var r1, out var r2, out var p1, out var p2);
		var height = Vector3.Distance(p1, p2);
		OWGizmos.DrawWireCone(center, transform.rotation, height, r1, r2);
	}
}
