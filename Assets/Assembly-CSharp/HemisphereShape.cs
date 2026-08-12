using System;
using ShapeCollision;
using UnityEngine;

[AddComponentMenu("Shapes/Hemisphere Shape", 6)]
public class HemisphereShape : SphereShape
{
	[SerializeField]
	protected int _direction = 1;

	[SerializeField]
	protected bool _cap;

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

	public bool cap
	{
		get
		{
			return _cap;
		}
		set
		{
			_cap = value;
		}
	}

	protected override void Reset()
	{
		base.Reset();
		_direction = 1;
		_cap = false;
	}

	protected override void OnValidate()
	{
		if (_direction < 0)
		{
			_direction = 0;
		}
		if (_direction > 2)
		{
			_direction = 2;
		}
	}

	public override bool PointInside(Vector3 point)
	{
		Vector3 sphereCenter = ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
		float sphereRadius = ShapeUtil.Sphere.CalcWorldSpaceRadius(this);
		Vector3 sphereNormal = ShapeUtil.Hemisphere.CalcWorldSpaceAxis(this);
		return ShapeCollision.PointInside.Hemisphere(point, sphereCenter, sphereRadius, sphereNormal);
	}

	public override Vector3 ClosestPoint(Vector3 point)
	{
		throw new NotImplementedException();
	}

	public override float PenetrationDistance(Vector3 point)
	{
		throw new NotImplementedException();
	}

	public override Vector3 GetRandomPointInsideShape()
	{
		return base.transform.TransformPoint(ShapeUtil.Hemisphere.GetRandomContainedLocalPoint(this));
	}
	
	// CHANGED
	// BUG: doesnt work for direction or cap
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		var center = ShapeUtil.Sphere.CalcWorldSpaceCenter(this);
		var radius = ShapeUtil.Sphere.CalcWorldSpaceRadius(this);
		OWGizmos.DrawWireHemisphere(center, transform.rotation, radius);
	}
}
