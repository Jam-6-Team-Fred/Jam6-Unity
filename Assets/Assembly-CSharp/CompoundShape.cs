using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Shapes/Compound Shape", 100)]
public class CompoundShape : Shape
{
	protected struct CollisionCounter
	{
		public Shape shape;

		public int count;

		public CollisionCounter(Shape shape)
		{
			this.shape = shape;
			count = 1;
		}
	}

	protected Shape[] _childShapes;

	protected List<CollisionCounter> _collisions;

	public override int layerMask
	{
		get
		{
			return base.layerMask;
		}
		set
		{
			base.layerMask = value;
			for (int i = 0; i < _childShapes.Length; i++)
			{
				if (!(_childShapes[i] == this))
				{
					_childShapes[i].layerMask = value;
				}
			}
		}
	}

	public override bool pointChecksOnly
	{
		get
		{
			return base.pointChecksOnly;
		}
		set
		{
			base.pointChecksOnly = value;
			for (int i = 0; i < _childShapes.Length; i++)
			{
				if (!(_childShapes[i] == this))
				{
					_childShapes[i].pointChecksOnly = value;
				}
			}
		}
	}

	protected override void RecalculateLocalBounds()
	{
		if (_childShapes == null || _childShapes.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 1; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				if (!flag)
				{
					_localBounds = _childShapes[i].localBounds;
					flag = true;
				}
				else
				{
					_localBounds.Encapsulate(_childShapes[i].localBounds);
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_childShapes = GetComponentsInChildren<Shape>(includeInactive: true);
		_collisions = new List<CollisionCounter>(32);
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				_childShapes[i].SetCollisionMode(_collisionMode);
				_childShapes[i].SetLayer(_layer);
				_childShapes[i].layerMask = _layerMask;
				_childShapes[i].pointChecksOnly = _pointChecksOnly;
				_childShapes[i].OnCollisionEnter += OnChildCollisionEnter;
				_childShapes[i].OnCollisionExit += OnChildCollisionExit;
			}
		}
	}

	protected virtual void Start()
	{
		RecalculateLocalBounds();
	}

	protected virtual void OnDestroy()
	{
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				_childShapes[i].OnCollisionEnter -= OnChildCollisionEnter;
				_childShapes[i].OnCollisionExit -= OnChildCollisionExit;
			}
		}
	}

	protected override void OnEnable()
	{
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				_childShapes[i].enabled = base.enabled;
			}
		}
	}

	protected override void OnDisable()
	{
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				_childShapes[i].enabled = base.enabled;
			}
		}
	}

	public override Vector3 GetWorldSpaceCenter()
	{
		if (_childShapes.Length == 1)
		{
			return base.transform.position;
		}
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				zero += _childShapes[i].GetWorldSpaceCenter();
			}
		}
		return zero / (_childShapes.Length - 1);
	}

	public override bool PointInside(Vector3 point)
	{
		if (_childShapes.Length == 1)
		{
			return false;
		}
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this) && _childShapes[i].PointInside(point))
			{
				return true;
			}
		}
		return false;
	}

	public override Vector3 ClosestPoint(Vector3 point)
	{
		if (_childShapes.Length == 1)
		{
			return base.transform.position;
		}
		Vector3 result = Vector3.zero;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				Vector3 vector = _childShapes[i].ClosestPoint(point);
				float sqrMagnitude = (point - vector).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = vector;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public override float PenetrationDistance(Vector3 point)
	{
		if (_childShapes.Length == 1)
		{
			return 0f;
		}
		float num = float.NegativeInfinity;
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				float num2 = _childShapes[i].PenetrationDistance(point);
				if (Mathf.Abs(num2) < Mathf.Abs(num))
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public override void SetCollisionMode(CollisionMode newCollisionMode)
	{
		_collisionMode = newCollisionMode;
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				_childShapes[i].SetCollisionMode(newCollisionMode);
			}
		}
	}

	public override void SetLayer(Layer newLayer)
	{
		_layer = newLayer;
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				_childShapes[i].SetLayer(newLayer);
			}
		}
	}

	public override void SetActivation(bool newActive)
	{
		_active = newActive;
		for (int i = 0; i < _childShapes.Length; i++)
		{
			if (!(_childShapes[i] == this))
			{
				_childShapes[i].SetActivation(newActive);
			}
		}
	}

	protected virtual void OnChildCollisionEnter(Shape otherShape)
	{
		for (int i = 0; i < _collisions.Count; i++)
		{
			if (_collisions[i].shape == otherShape)
			{
				CollisionCounter value = _collisions[i];
				value.count++;
				_collisions[i] = value;
				return;
			}
		}
		_collisions.Add(new CollisionCounter(otherShape));
		FireCollisionEnterEvent(otherShape);
	}

	protected virtual void OnChildCollisionExit(Shape otherShape)
	{
		for (int i = 0; i < _collisions.Count; i++)
		{
			if (_collisions[i].shape == otherShape)
			{
				CollisionCounter value = _collisions[i];
				value.count--;
				_collisions[i] = value;
				if (value.count <= 0)
				{
					_collisions.QuickRemoveAt(i);
					FireCollisionExitEvent(otherShape);
				}
				break;
			}
		}
	}
}
