using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshProjector : MonoBehaviour
{
	public enum Mode
	{
		PhysicalLayer = 0,
		SingleMesh = 1,
		Hierarchy = 2
	}

	public const string kBakedAssetGroupName = "Decals";

	[SerializeField]
	private Mesh _mesh;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private Mode _mode;

	[SerializeField]
	private GameObject[] _targets = new GameObject[0];

	[SerializeField]
	private Vector2 _size = Vector2.one;

	[SerializeField]
	private float _distance = 1f;

	[SerializeField]
	private Vector2 _uvMin = Vector2.zero;

	[SerializeField]
	private Vector2 _uvMax = Vector2.one;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private Vector2 _angleFade = new Vector2(45f, 90f);

	[SerializeField]
	private bool _inheritVertexColors;

	[SerializeField]
	private bool _inheritUV1;

	[SerializeField]
	private bool _inheritUV2;

	[SerializeField]
	private bool _inheritUV3;

	[SerializeField]
	private bool _inheritUV4;

	[SerializeField]
	private bool _offsetFromSurface;

	[SerializeField]
	private float _surfaceOffsetDistance;

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

	public Material material
	{
		get
		{
			return _material;
		}
		set
		{
			_material = value;
			GetComponent<MeshRenderer>().sharedMaterial = _material;
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

	public Vector2 size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
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
			_distance = value;
		}
	}

	public Vector2 uvMin
	{
		get
		{
			return _uvMin;
		}
		set
		{
			_uvMin = value;
		}
	}

	public Vector2 uvMax
	{
		get
		{
			return _uvMax;
		}
		set
		{
			_uvMax = value;
		}
	}

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
		}
	}

	public Vector2 angleFade
	{
		get
		{
			return _angleFade;
		}
		set
		{
			_angleFade = value;
		}
	}

	public Vector3 center => base.transform.position - base.transform.up * _distance * 0.5f;

	public Vector3 extents => new Vector3(_size.x, _distance, _size.y) * 0.5f;

	public bool inheritVertexColors
	{
		get
		{
			return _inheritVertexColors;
		}
		set
		{
			_inheritVertexColors = value;
		}
	}

	public bool inheritUV1
	{
		get
		{
			return _inheritUV1;
		}
		set
		{
			_inheritUV1 = value;
		}
	}

	public bool inheritUV2
	{
		get
		{
			return _inheritUV2;
		}
		set
		{
			_inheritUV2 = value;
		}
	}

	public bool inheritUV3
	{
		get
		{
			return _inheritUV3;
		}
		set
		{
			_inheritUV3 = value;
		}
	}

	public bool inheritUV4
	{
		get
		{
			return _inheritUV4;
		}
		set
		{
			_inheritUV4 = value;
		}
	}

	public bool offsetFromSurface
	{
		get
		{
			return _offsetFromSurface;
		}
		set
		{
			_offsetFromSurface = value;
		}
	}

	public float surfaceOffsetDistance
	{
		get
		{
			return _surfaceOffsetDistance;
		}
		set
		{
			_surfaceOffsetDistance = value;
		}
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
		if (_angleFade.x < 0f || _angleFade.x > 180f)
		{
			_angleFade.x = Mathf.Clamp(_angleFade.x, 0f, 180f);
		}
		if (_angleFade.y < 0f || _angleFade.y > 180f)
		{
			_angleFade.y = Mathf.Clamp(_angleFade.y, 0f, 180f);
		}
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component != null && component.sharedMaterial != _material)
		{
			component.sharedMaterial = _material;
		}
	}
}
