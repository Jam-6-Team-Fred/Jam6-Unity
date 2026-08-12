using UnityEngine;

[SelectionBase]
[DisallowMultipleComponent] // CHANGED
public class MeshBatch : MonoBehaviour
{
	public const string kBakedRendererAssetGroupName = "BatchedRenderers";

	public const string kBakedColliderAssetGroupName = "BatchedColliders";

	[SerializeField]
	private GameObject _originalGroup;

	[SerializeField]
	private GameObject _cloneGroup;

	[SerializeField]
	private GameObject _batchedGroup;

	[SerializeField]
	private bool _batchMeshRenderers;

	[SerializeField]
	private bool _spatialGrouping;

	[SerializeField]
	private float _groupingRadius = 10f;

	[SerializeField]
	private bool _batchMeshColliders;

	[SerializeField]
	private PhysicMaterial _physicMaterial;

	public GameObject originalGroup
	{
		get
		{
			return _originalGroup;
		}
		set
		{
			_originalGroup = value;
		}
	}

	public GameObject cloneGroup
	{
		get
		{
			return _cloneGroup;
		}
		set
		{
			_cloneGroup = value;
		}
	}

	public GameObject batchedGroup
	{
		get
		{
			return _batchedGroup;
		}
		set
		{
			_batchedGroup = value;
		}
	}

	public bool batchMeshRenderers
	{
		get
		{
			return _batchMeshRenderers;
		}
		set
		{
			_batchMeshRenderers = value;
		}
	}

	public bool spatialGrouping
	{
		get
		{
			return _spatialGrouping;
		}
		set
		{
			_spatialGrouping = value;
		}
	}

	public float groupingRadius
	{
		get
		{
			return _groupingRadius;
		}
		set
		{
			_groupingRadius = value;
		}
	}

	public bool batchMeshColliders
	{
		get
		{
			return _batchMeshColliders;
		}
		set
		{
			_batchMeshColliders = value;
		}
	}

	public PhysicMaterial physicMaterial
	{
		get
		{
			return _physicMaterial;
		}
		set
		{
			_physicMaterial = value;
		}
	}

	public bool isBatched => _batchedGroup != null;
}
