using UnityEngine;

public class ColliderData
{
	private enum ColliderType
	{
		Mesh = 0,
		Sphere = 1,
		Box = 2,
		Capsule = 3
	}

	private ColliderType _colliderType;

	private PhysicMaterial _physicMaterial;

	private bool _isTrigger;

	private bool _isConvex;

	private Mesh _mesh;

	private float _height;

	private int _direction;

	private float _radius;

	private Vector3 _size;

	private Vector3 _center;

	public ColliderData(Collider collider)
	{
		SaveData(collider);
	}

	private void SaveData(Collider savedCollider)
	{
		_physicMaterial = savedCollider.material;
		_isTrigger = savedCollider.isTrigger;
		if (savedCollider.GetType() == typeof(MeshCollider))
		{
			_colliderType = ColliderType.Mesh;
			_isConvex = ((MeshCollider)savedCollider).convex;
			_mesh = ((MeshCollider)savedCollider).sharedMesh;
		}
		else if (savedCollider.GetType() == typeof(SphereCollider))
		{
			_colliderType = ColliderType.Sphere;
			_radius = ((SphereCollider)savedCollider).radius;
			_center = ((SphereCollider)savedCollider).center;
		}
		else if (savedCollider.GetType() == typeof(BoxCollider))
		{
			_colliderType = ColliderType.Box;
			_size = ((BoxCollider)savedCollider).size;
			_center = ((BoxCollider)savedCollider).center;
		}
		else if (savedCollider.GetType() == typeof(CapsuleCollider))
		{
			_colliderType = ColliderType.Capsule;
			_radius = ((CapsuleCollider)savedCollider).radius;
			_center = ((CapsuleCollider)savedCollider).center;
			_height = ((CapsuleCollider)savedCollider).height;
			_direction = ((CapsuleCollider)savedCollider).direction;
		}
	}

	public Collider CreateColliderFromData(GameObject obj)
	{
		Collider collider = null;
		if (_colliderType == ColliderType.Mesh)
		{
			collider = obj.AddComponent<MeshCollider>();
			((MeshCollider)collider).sharedMesh = _mesh;
			((MeshCollider)collider).convex = _isConvex;
		}
		else if (_colliderType == ColliderType.Sphere)
		{
			collider = obj.AddComponent<SphereCollider>();
			((SphereCollider)collider).center = _center;
			((SphereCollider)collider).radius = _radius;
		}
		else if (_colliderType == ColliderType.Box)
		{
			collider = obj.AddComponent<BoxCollider>();
			((BoxCollider)collider).center = _center;
			((BoxCollider)collider).size = _size;
		}
		else if (_colliderType == ColliderType.Capsule)
		{
			collider = obj.AddComponent<CapsuleCollider>();
			((CapsuleCollider)collider).radius = _radius;
			((CapsuleCollider)collider).center = _center;
			((CapsuleCollider)collider).height = _height;
			((CapsuleCollider)collider).direction = _direction;
		}
		collider.material = _physicMaterial;
		collider.isTrigger = _isTrigger;
		return collider;
	}
}
