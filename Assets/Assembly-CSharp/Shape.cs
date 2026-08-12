using System;
using UnityEngine;

public abstract class Shape : MonoBehaviour
{
	public enum CollisionMode
	{
		Volume = 0,
		Detector = 1,
		Manual = 2
	}

	public enum Layer
	{
		Default = 1,
		Sector = 2,
		Gravity = 4,
		Light = 8
	}

	public delegate void ShapeCollisionEvent(Shape otherShape);

	public delegate void ShapeActiveEvent(Shape thisShape);

	public const int kNumLayers = 4;

	[SerializeField]
	protected CollisionMode _collisionMode;

	[SerializeField]
	protected Layer _layer = Layer.Default;

	[SerializeField]
	protected int _layerMask = -1;

	[SerializeField]
	protected bool _pointChecksOnly;

	protected bool _active;

	protected bool _registered;

	protected Transform _transform;

	protected SphereBounds _localBounds;

	public virtual CollisionMode collisionMode => _collisionMode;

	public virtual Layer layer => _layer;

	public virtual int layerMask
	{
		get
		{
			return _layerMask;
		}
		set
		{
			_layerMask = value;
		}
	}

	public virtual bool pointChecksOnly
	{
		get
		{
			return _pointChecksOnly;
		}
		set
		{
			_pointChecksOnly = value;
		}
	}

	public virtual bool active => _active;

	public virtual SphereBounds localBounds => _localBounds;

	public event ShapeCollisionEvent OnCollisionEnter;

	public event ShapeCollisionEvent OnCollisionExit;

	public event ShapeActiveEvent OnShapeActivated;

	public event ShapeActiveEvent OnShapeDeactivated;

	protected virtual void Reset()
	{
		_collisionMode = CollisionMode.Volume;
		_layer = Layer.Default;
		_layerMask = -1;
	}

	public static bool CheckLayerMask(int layerMask, Layer layer)
	{
		return (int)((uint)layerMask & (uint)layer) > 0;
	}

	protected abstract void RecalculateLocalBounds();

	public SphereBounds CalcWorldBounds()
	{
		Vector3 sphereCenter = _transform.TransformPoint(_localBounds.center);
		Vector3 lossyScale = _transform.lossyScale;
		if (lossyScale.x < 0f)
		{
			lossyScale.x = 0f - lossyScale.x;
		}
		if (lossyScale.y < 0f)
		{
			lossyScale.y = 0f - lossyScale.y;
		}
		if (lossyScale.z < 0f)
		{
			lossyScale.z = 0f - lossyScale.z;
		}
		float num = lossyScale.x;
		if (num < lossyScale.y)
		{
			num = lossyScale.y;
		}
		if (num < lossyScale.z)
		{
			num = lossyScale.z;
		}
		float sphereRadius = _localBounds.radius * num;
		return new SphereBounds(sphereCenter, sphereRadius);
	}

	public abstract Vector3 GetWorldSpaceCenter();

	public virtual bool PointInside(Vector3 point)
	{
		throw new NotImplementedException();
	}

	public virtual Vector3 ClosestPoint(Vector3 point)
	{
		throw new NotImplementedException();
	}

	public virtual float PenetrationDistance(Vector3 point)
	{
		throw new NotImplementedException();
	}

	public virtual Vector3 GetRandomPointInsideShape()
	{
		throw new NotImplementedException();
	}

	public virtual Vector3 GetLocalInertiaTensor()
	{
		throw new NotImplementedException();
	}

	public virtual bool IsVisible(Plane[] _frustumPlanes)
	{
		throw new NotImplementedException();
	}

	public virtual bool IsBlocked(Plane[] _frustumPlanes)
	{
		throw new NotImplementedException();
	}

	public virtual bool IsBlocked(Vector3 cameraPos, Vector3 centerLine, float sphereDist, float halfAngle)
	{
		throw new NotImplementedException();
	}

	protected virtual void Awake()
	{
		_active = true;
		_registered = false;
		_transform = base.transform;
		RecalculateLocalBounds();
	}

	protected virtual void OnEnable()
	{
		if (_active)
		{
			RegisterShape();
		}
	}

	protected virtual void OnDisable()
	{
		UnregisterShape();
	}

	protected void RegisterShape()
	{
		if (!_registered && _collisionMode != CollisionMode.Manual)
		{
			ShapeManager.RegisterShape(this);
			_registered = true;
		}
	}

	protected void UnregisterShape()
	{
		if (_registered)
		{
			ShapeManager.UnregisterShape(this);
			_registered = false;
		}
	}

	public virtual void SetCollisionMode(CollisionMode newCollisionMode)
	{
		if (_collisionMode != newCollisionMode)
		{
			if (_registered)
			{
				UnregisterShape();
				_collisionMode = newCollisionMode;
				RegisterShape();
			}
			else
			{
				_collisionMode = newCollisionMode;
			}
		}
	}

	public virtual void SetLayer(Layer newLayer)
	{
		if (_layer != newLayer)
		{
			if (_registered)
			{
				UnregisterShape();
				_layer = newLayer;
				RegisterShape();
			}
			else
			{
				_layer = newLayer;
			}
		}
	}

	public virtual void SetActivation(bool newActive)
	{
		if (_active == newActive)
		{
			return;
		}
		_active = newActive;
		if (_active && base.isActiveAndEnabled)
		{
			RegisterShape();
			if (this.OnShapeActivated != null)
			{
				this.OnShapeActivated(this);
			}
		}
		else if (!_active)
		{
			UnregisterShape();
			if (this.OnShapeDeactivated != null)
			{
				this.OnShapeDeactivated(this);
			}
		}
	}

	public void FireCollisionEnterEvent(Shape otherShape)
	{
		if (this.OnCollisionEnter != null)
		{
			try
			{
				this.OnCollisionEnter(otherShape);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void FireCollisionExitEvent(Shape otherShape)
	{
		if (this.OnCollisionExit != null)
		{
			try
			{
				this.OnCollisionExit(otherShape);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
