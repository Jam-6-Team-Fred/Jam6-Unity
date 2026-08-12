using System.Collections.Generic;
using UnityEngine;

public abstract class CollisionGroup : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private List<OWCollider> _colliders;

	[SerializeField]
	[HideInInspector]
	private List<Shape> _shapes;

	private void Awake()
	{
		if (!_prebuilt)
		{
			BuildCollisionGroup();
		}
	}

	public OWCollider[] GetColliders()
	{
		return _colliders.ToArray();
	}

	public int GetColliderCount()
	{
		return _colliders.Count;
	}

	public Shape[] GetShapes()
	{
		return _shapes.ToArray();
	}

	public int GetShapeCount()
	{
		return _shapes.Count;
	}

	public List<Collider> FindCollidersInHierarchy()
	{
		return RecursivelyFindCollidersInHierarchy(base.transform, new List<Collider>());
	}

	public List<Shape> FindShapesInHierarchy()
	{
		return RecursivelyFindShapesInHierarchy(base.transform, new List<Shape>());
	}

	public void FindNumCollidersAndShapesInHierarchy(out int numColliders, out int numShapes)
	{
		numColliders = (numShapes = 0);
		RecursivelyCountCollidersAndShapesInHierarchy(base.transform, ref numColliders, ref numShapes);
	}

	public void RemoveCollider(OWCollider colliderToRemove)
	{
		if (_colliders.Remove(colliderToRemove))
		{
			colliderToRemove.SetLODLevel(0);
		}
	}

	public void BeginScalingGroup()
	{
		for (int i = 0; i < _colliders.Count; i++)
		{
			if (_colliders[i] != null)
			{
				_colliders[i].BeginScaling();
			}
		}
	}

	public void EndScalingGroup()
	{
		for (int i = 0; i < _colliders.Count; i++)
		{
			if (_colliders[i] != null)
			{
				_colliders[i].EndScaling();
			}
		}
	}

	protected void BuildCollisionGroup()
	{
		_colliders = new List<OWCollider>();
		_shapes = new List<Shape>();
		RecursivelyAddCollidersAndShapes(base.transform);
	}

	protected void RecursivelyAddCollidersAndShapes(Transform parent)
	{
		if (!ShouldIncludeObject(parent))
		{
			return;
		}
		Collider component = parent.GetComponent<Collider>();
		if (component != null)
		{
			component.enabled = false;
			OWCollider addComponent = component.gameObject.GetAddComponent<OWCollider>();
			_colliders.Add(addComponent);
		}
		Shape component2 = parent.GetComponent<Shape>();
		if (component2 != null)
		{
			component2.enabled = false;
			_shapes.Add(component2);
		}
		foreach (Transform item in parent)
		{
			RecursivelyAddCollidersAndShapes(item);
		}
	}

	protected void UpdateColliderLOD(DynamicOccupant occupantMask, bool ignoreDelay = false)
	{
		for (int i = 0; i < _colliders.Count; i++)
		{
			if (_colliders[i] != null)
			{
				float delayFraction = (ignoreDelay ? 0f : ((float)i / (float)_colliders.Count));
				_colliders[i].CheckLODActivation(occupantMask, delayFraction);
			}
		}
		bool flag = occupantMask != DynamicOccupant.Undefined;
		for (int j = 0; j < _shapes.Count; j++)
		{
			if (_shapes[j] != null)
			{
				_shapes[j].enabled = flag;
			}
		}
	}

	protected void SetColliderLOD(int lod, bool ignoreDelay = false)
	{
		for (int i = 0; i < _colliders.Count; i++)
		{
			if (_colliders[i] != null)
			{
				float delay = (ignoreDelay ? 0f : ((float)i / (float)_colliders.Count));
				_colliders[i].SetLODLevel(lod, delay);
			}
		}
		bool flag = lod == 0;
		for (int j = 0; j < _shapes.Count; j++)
		{
			if (_shapes[j] != null)
			{
				_shapes[j].enabled = flag;
			}
		}
	}

	private List<Collider> RecursivelyFindCollidersInHierarchy(Transform parent, List<Collider> colliderList)
	{
		if (ShouldIncludeObject(parent))
		{
			Collider component = parent.GetComponent<Collider>();
			if (component != null)
			{
				colliderList.Add(component);
			}
			foreach (Transform item in parent)
			{
				RecursivelyFindCollidersInHierarchy(item, colliderList);
			}
		}
		return colliderList;
	}

	private List<Shape> RecursivelyFindShapesInHierarchy(Transform parent, List<Shape> shapeList)
	{
		if (ShouldIncludeObject(parent))
		{
			Shape component = parent.GetComponent<Shape>();
			if (component != null)
			{
				shapeList.Add(component);
			}
			if (component is CompoundShape)
			{
				return shapeList;
			}
			foreach (Transform item in parent)
			{
				RecursivelyFindShapesInHierarchy(item, shapeList);
			}
		}
		return shapeList;
	}

	private void RecursivelyCountCollidersAndShapesInHierarchy(Transform parent, ref int numColliders, ref int numShapes)
	{
		if ((parent.TryGetComponent<CollisionGroupExcluder>(out var component) && component.gameObject != base.gameObject) || (parent.TryGetComponent<CollisionGroup>(out var component2) && component2.gameObject != base.gameObject) || parent.TryGetComponent<OWItem>(out var _) || parent.TryGetComponent<LightSourceDetector>(out var _))
		{
			return;
		}
		if (parent.TryGetComponent<Collider>(out var _))
		{
			numColliders++;
		}
		if (parent.TryGetComponent<Shape>(out var component6))
		{
			numShapes++;
			if (component6 is CompoundShape)
			{
				return;
			}
		}
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			RecursivelyCountCollidersAndShapesInHierarchy(parent.GetChild(i), ref numColliders, ref numShapes);
		}
	}

	protected virtual bool ShouldIncludeObject(Transform transform)
	{
		if (transform == null)
		{
			return false;
		}
		CollisionGroupExcluder component = transform.GetComponent<CollisionGroupExcluder>();
		if (component != null && component.gameObject != base.gameObject)
		{
			return false;
		}
		CollisionGroup component2 = transform.GetComponent<CollisionGroup>();
		if (component2 != null && component2.gameObject != base.gameObject)
		{
			return false;
		}
		if (transform.GetComponent<OWItem>() != null)
		{
			return false;
		}
		if (transform.GetComponent<LightSourceDetector>() != null)
		{
			return false;
		}
		return true;
	}
}
