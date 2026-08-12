using System;
using ShapeCollision;
using UnityEngine;

[AddComponentMenu("Shapes/Cylinder Shape", 4)]
public class CylinderShape : CapsuleShape
{
	protected override void RecalculateLocalBounds()
	{
		_localBounds.Set(_center, Mathf.Sqrt(_radius * _radius + _height * 0.5f * (_height * 0.5f)));
	}

	public override bool PointInside(Vector3 point)
	{
		ShapeUtil.Cylinder.CalcWorldSpaceEndpoints(this, out var worldSpaceRadius, out var worldSpaceP, out var worldSpaceP2);
		return ShapeCollision.PointInside.Cylinder(point, worldSpaceP, worldSpaceP2, worldSpaceRadius);
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
		return base.transform.TransformPoint(ShapeUtil.Cylinder.GetRandomContainedLocalPoint(this));
	}

	public override Vector3 GetLocalInertiaTensor()
	{
		return ShapeUtil.Cylinder.CalcLocalSpaceInertiaTensor(this);
	}
	
	// CHANGED
	// BUG: doesnt work for direction
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		var center = ShapeUtil.Cylinder.CalcWorldSpaceCenter(this);
		ShapeUtil.Cylinder.CalcWorldSpaceEndpoints(this, out var radius, out var p1, out var p2);
		var height = Vector3.Distance(p1, p2);
		OWGizmos.DrawWireCylinder(center, transform.rotation, height, radius);
	}

}
