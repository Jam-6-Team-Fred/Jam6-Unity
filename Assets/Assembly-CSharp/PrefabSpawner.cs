using UnityEngine;

[RequireComponent(typeof(Shape))]
public class PrefabSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject _prefab;

	[SerializeField]
	private int _spawnCount;

	[SerializeField]
	private bool _spawnOnStart;

	private Shape _shape;

	private bool _gizmosDirty;

	private Vector3[] _gizmosPositions;

	private void OnValidate()
	{
		if (_spawnCount < 0)
		{
			_spawnCount = 0;
		}
		Shape component = GetComponent<Shape>();
		if (component != null && component.collisionMode != Shape.CollisionMode.Manual)
		{
			component.SetCollisionMode(Shape.CollisionMode.Manual);
		}
		if (_gizmosPositions == null || _spawnCount != _gizmosPositions.Length)
		{
			_gizmosDirty = true;
			_gizmosPositions = new Vector3[_spawnCount];
		}
	}

	private void Awake()
	{
		_shape = base.gameObject.GetRequiredComponent<Shape>();
	}

	private void Start()
	{
		if (_spawnOnStart)
		{
			Spawn();
		}
	}

	public void Spawn()
	{
		if (_prefab != null)
		{
			for (int i = 0; i < _spawnCount; i++)
			{
				Object.Instantiate(_prefab, _shape.GetRandomPointInsideShape(), base.transform.rotation).transform.parent = base.transform;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_gizmosDirty)
		{
			Shape component = GetComponent<Shape>();
			if (component != null)
			{
				for (int i = 0; i < _gizmosPositions.Length; i++)
				{
					_gizmosPositions[i] = component.GetRandomPointInsideShape();
				}
			}
			_gizmosDirty = false;
		}
		if (_gizmosPositions != null)
		{
			for (int j = 0; j < _gizmosPositions.Length; j++)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(_gizmosPositions[j], 0.5f);
			}
		}
	}
}
