using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DetailPatch : MonoBehaviour
{
	public enum Shape
	{
		Box = 0,
		Cylinder = 1
	}

	public enum Mode
	{
		PhysicalLayer = 0,
		SingleMesh = 1,
		Hierarchy = 2
	}

	public const string kBakedAssetGroupName = "DetailPatches";

	[SerializeField]
	private DetailPalette _palette;

	[SerializeField]
	private Mode _mode;

	[SerializeField]
	private GameObject[] _targets = new GameObject[0];

	[SerializeField]
	private bool _blockable = true;

	[SerializeField]
	private Shape _shape;

	[SerializeField]
	private Vector2 _size = Vector2.one;

	[SerializeField]
	private float _distance = 1f;

	[SerializeField]
	private int _seed;

	[SerializeField]
	private int _count = 10;

	[SerializeField]
	private bool _separation = true;

	[SerializeField]
	private AnimationCurve _distribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private DetailInstance[] _instances = new DetailInstance[0];

	[SerializeField]
	private Mesh _mesh;

	[SerializeField]
	private Material[] _materials = new Material[0];

	public DetailPalette palette
	{
		get
		{
			return _palette;
		}
		set
		{
			_palette = value;
		}
	}

	public Mode mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
		}
	}

	public GameObject[] targets
	{
		get
		{
			return _targets;
		}
		set
		{
			_targets = value;
		}
	}

	public bool blockable
	{
		get
		{
			return _blockable;
		}
		set
		{
			_blockable = value;
		}
	}

	public Shape shape
	{
		get
		{
			return _shape;
		}
		set
		{
			_shape = value;
		}
	}

	public Vector2 size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = Vector2.Max(value, Vector2.zero);
		}
	}

	public float distance
	{
		get
		{
			return _distance;
		}
		set
		{
			_distance = Mathf.Max(value, 0f);
		}
	}

	public int seed
	{
		get
		{
			return _seed;
		}
		set
		{
			_seed = value;
		}
	}

	public int count
	{
		get
		{
			return _count;
		}
		set
		{
			_count = Mathf.Max(value, 0);
		}
	}

	public bool separation
	{
		get
		{
			return _separation;
		}
		set
		{
			_separation = value;
		}
	}

	public DetailInstance[] instances
	{
		get
		{
			return _instances;
		}
		set
		{
			_instances = value;
		}
	}

	public Mesh mesh
	{
		get
		{
			return _mesh;
		}
		set
		{
			_mesh = value;
			GetComponent<MeshFilter>().sharedMesh = _mesh;
		}
	}

	public Material[] materials
	{
		get
		{
			return _materials;
		}
		set
		{
			_materials = value;
			GetComponent<MeshRenderer>().sharedMaterials = _materials;
		}
	}

	public float GetDistributionBias(float f)
	{
		return _distribution.Evaluate(f);
	}

	public bool IsBaked()
	{
		return mesh != null;
	}

	private void OnValidate()
	{
		if (_size.x < 0f || _size.y < 0f)
		{
			_size = Vector2.Max(_size, Vector2.zero);
		}
		if (_distance < 0f)
		{
			_distance = 0f;
		}
		if (_count < 0)
		{
			_count = 0;
		}
	}

	public void AddInstance(DetailInstance instance)
	{
		DetailInstance[] array = new DetailInstance[_instances.Length + 1];
		_instances.CopyTo(array, 0);
		array[array.Length - 1] = instance;
		_instances = array;
	}

	public bool DeleteInstanceAtIndex(int index)
	{
		if (index < 0 || index >= _instances.Length)
		{
			return false;
		}
		DetailInstance[] array = new DetailInstance[_instances.Length - 1];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (i == index)
			{
				num = 1;
			}
			array[i] = _instances[i + num];
		}
		_instances = array;
		return true;
	}
}
