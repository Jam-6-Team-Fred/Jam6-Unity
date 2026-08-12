using UnityEngine;

[RequireComponent(typeof(Shape))]
public class ShapeVisibilityTracker : VisibilityTracker
{
	private Shape[] _shapes;

	private void Reset()
	{
		Shape component = GetComponent<Shape>();
		if (component != null)
		{
			component.SetCollisionMode(Shape.CollisionMode.Manual);
		}
	}

	private void Awake()
	{
		_shapes = GetComponents<Shape>();
	}

	public bool IsBlocked(Plane[] frustumPlanes)
	{
		for (int i = 0; i < _shapes.Length; i++)
		{
			if (_shapes[i].enabled && !_shapes[i].IsBlocked(frustumPlanes))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsBlocked(Vector3 cameraPos, Vector3 centerLine, float dist, float halfAngle)
	{
		for (int i = 0; i < _shapes.Length; i++)
		{
			if (_shapes[i].enabled && !_shapes[i].IsBlocked(cameraPos, centerLine, dist, halfAngle))
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsPointInside(Vector3 worldPos)
	{
		for (int i = 0; i < _shapes.Length; i++)
		{
			if (_shapes[i].PointInside(worldPos))
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsVisibleUsingCameraFrustum()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		return IsInFrustum(Locator.GetActiveCamera().GetFrustumPlanes());
	}

	public override bool IsVisible()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (IsVisibleUsingCameraFrustum())
		{
			return VisibilityOccluder.CanYouSee(this, Locator.GetActiveCamera().mainCamera.transform.position);
		}
		return false;
	}

	public override bool IsVisibleToProbe(OWCamera owCamera)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (IsInFrustum(owCamera.GetFrustumPlanes()))
		{
			return VisibilityOccluder.CanYouSee(this, owCamera.mainCamera.transform.position);
		}
		return false;
	}

	private bool IsInFrustum(Plane[] frustumPlanes)
	{
		for (int i = 0; i < _shapes.Length; i++)
		{
			if (_shapes[i].enabled && _shapes[i].IsVisible(frustumPlanes))
			{
				return true;
			}
		}
		return false;
	}
}
