using System;
using ShapeCollision;
using UnityEngine;

[AddComponentMenu("Shapes/Hemicapsule Shape", 7)]
public class HemicapsuleShape : CapsuleShape
{
	[SerializeField]
	private bool _cap;

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
		_cap = false;
	}

	public override bool PointInside(Vector3 point)
	{
		ShapeUtil.Capsule.CalcWorldSpaceEndpoints(this, out var worldSpaceRadius, out var worldSpaceP, out var worldSpaceP2);
		return ShapeCollision.PointInside.Hemicapsule(point, worldSpaceP, worldSpaceP2, worldSpaceRadius, _cap);
	}

	public override Vector3 ClosestPoint(Vector3 point)
	{
		throw new NotImplementedException();
	}

	public override float PenetrationDistance(Vector3 point)
	{
		throw new NotImplementedException();
	}

	// TODO gizmos
}
