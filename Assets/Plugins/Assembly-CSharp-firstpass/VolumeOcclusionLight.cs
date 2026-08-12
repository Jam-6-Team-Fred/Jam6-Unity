using UnityEngine;

[ExecuteInEditMode] // CHANGED
public class VolumeOcclusionLight : MonoBehaviour
{
	private static readonly Vector3[] kMeshVertices = new Vector3[8]
	{
		new Vector3(-0.5f, -0.5f, 0f),
		new Vector3(0.5f, -0.5f, 0f),
		new Vector3(-0.5f, 0.5f, 0f),
		new Vector3(0.5f, 0.5f, 0f),
		new Vector3(-0.5f, -0.5f, 1f),
		new Vector3(0.5f, -0.5f, 1f),
		new Vector3(-0.5f, 0.5f, 1f),
		new Vector3(0.5f, 0.5f, 1f)
	};

	private static readonly int[] kMeshIndices = new int[36]
	{
		0, 2, 1, 2, 3, 1, 0, 1, 4, 1,
		5, 4, 1, 3, 5, 3, 7, 5, 3, 2,
		7, 2, 6, 7, 2, 0, 6, 0, 4, 6,
		7, 6, 5, 6, 4, 5
	};

	[SerializeField]
	private Vector2 _startSize = new Vector2(1f, 1f);

	[SerializeField]
	private Vector2 _endSize = new Vector2(1.5f, 1.5f);

	[SerializeField]
	private float _range = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _intensity = 1f;

	[SerializeField]
	private Texture2D _cookie;

	[SerializeField]
	private bool _distanceBlur = true;

	private Transform _transform;

	private Vector4 _localBounds;

	private Mesh _mesh;

	private Vector3[] _meshVertices = new Vector3[8];

	private bool _meshNeedsUpdate = true;

	public Vector2 startSize
	{
		get
		{
			return _startSize;
		}
		set
		{
			_startSize = new Vector2(Mathf.Max(value.x, 0f), Mathf.Max(value.y, 0f));
			_meshNeedsUpdate = true;
		}
	}

	public Vector2 endSize
	{
		get
		{
			return _endSize;
		}
		set
		{
			_endSize = new Vector2(Mathf.Max(value.x, 0f), Mathf.Max(value.y, 0f));
			_meshNeedsUpdate = true;
		}
	}

	public float range
	{
		get
		{
			return _range;
		}
		set
		{
			_range = Mathf.Max(value, 0f);
			_meshNeedsUpdate = true;
		}
	}

	public float intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			_intensity = Mathf.Clamp01(value);
		}
	}

	public Texture2D cookie
	{
		get
		{
			return _cookie;
		}
		set
		{
			_cookie = value;
		}
	}

	public bool distanceBlur
	{
		get
		{
			return _distanceBlur;
		}
		set
		{
			_distanceBlur = value;
		}
	}

	public Mesh mesh
	{
		get
		{
			if (_meshNeedsUpdate)
			{
				UpdateMesh();
			}
			return _mesh;
		}
	}

	public Vector3 lightDirection => _transform.forward;

	public Matrix4x4 localToWorldMatrix => Matrix4x4.TRS(_transform.position, _transform.rotation, Vector3.one);

	public Vector4 localBounds => _localBounds;

	public Vector4 CalcWorldBounds()
	{
		Vector3 vector = _transform.TransformPoint(new Vector3(_localBounds.x, _localBounds.y, _localBounds.z));
		return new Vector4(vector.x, vector.y, vector.z, _localBounds.w);
	}

	private void OnValidate()
	{
		if (_startSize.x < 0f)
		{
			_startSize.x = 0f;
		}
		if (_startSize.y < 0f)
		{
			_startSize.y = 0f;
		}
		if (_range < 0f)
		{
			_range = 0f;
		}
	}

	private void Awake()
	{
		_transform = base.transform;
		UpdateMesh();
	}

	private void OnEnable()
	{
		VolumeOcclusionManager.RegisterVolumeOcclusionLight(this);
	}

	private void OnDisable()
	{
		VolumeOcclusionManager.UnregisterVolumeOcclusionLight(this);
	}

	private void UpdateMesh()
	{
		for (int i = 0; i < 4; i++)
		{
			_meshVertices[i] = new Vector3(kMeshVertices[i].x * _startSize.x, kMeshVertices[i].y * _startSize.y, kMeshVertices[i].z * _range);
		}
		for (int j = 4; j < 8; j++)
		{
			_meshVertices[j] = new Vector3(kMeshVertices[j].x * _endSize.x, kMeshVertices[j].y * _endSize.y, kMeshVertices[j].z * _range);
		}
		if (_mesh == null)
		{
			_mesh = new Mesh();
			_mesh.MarkDynamic();
			_mesh.name = "VolumeOcclusionLightMesh";
			_mesh.vertices = _meshVertices;
			_mesh.triangles = kMeshIndices;
		}
		else
		{
			_mesh.vertices = _meshVertices;
		}
		_mesh.RecalculateBounds();
		Bounds bounds = _mesh.bounds;
		_localBounds = new Vector4(bounds.center.x, bounds.center.y, bounds.center.z, bounds.extents.magnitude);
		_meshNeedsUpdate = false;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 1f, 1f, 1f);
		Gizmos.matrix = localToWorldMatrix;
		Vector2 vector = _startSize * 0.5f;
		Vector2 vector2 = _endSize * 0.5f;
		Gizmos.DrawLine(new Vector3(0f - vector.x, 0f - vector.y, 0f), new Vector3(vector.x, 0f - vector.y, 0f));
		Gizmos.DrawLine(new Vector3(vector.x, 0f - vector.y, 0f), new Vector3(vector.x, vector.y, 0f));
		Gizmos.DrawLine(new Vector3(0f - vector.x, 0f - vector.y, 0f), new Vector3(0f - vector.x, vector.y, 0f));
		Gizmos.DrawLine(new Vector3(0f - vector.x, vector.y, 0f), new Vector3(vector.x, vector.y, 0f));
		Gizmos.DrawLine(new Vector3(0f - vector2.x, 0f - vector2.y, _range), new Vector3(vector2.x, 0f - vector2.y, _range));
		Gizmos.DrawLine(new Vector3(vector2.x, 0f - vector2.y, _range), new Vector3(vector2.x, vector2.y, _range));
		Gizmos.DrawLine(new Vector3(0f - vector2.x, 0f - vector2.y, _range), new Vector3(0f - vector2.x, vector2.y, _range));
		Gizmos.DrawLine(new Vector3(0f - vector2.x, vector2.y, _range), new Vector3(vector2.x, vector2.y, _range));
		Gizmos.DrawLine(new Vector3(0f - vector.x, 0f - vector.y, 0f), new Vector3(0f - vector2.x, 0f - vector2.y, _range));
		Gizmos.DrawLine(new Vector3(vector.x, 0f - vector.y, 0f), new Vector3(vector2.x, 0f - vector2.y, _range));
		Gizmos.DrawLine(new Vector3(0f - vector.x, vector.y, 0f), new Vector3(0f - vector2.x, vector2.y, _range));
		Gizmos.DrawLine(new Vector3(vector.x, vector.y, 0f), new Vector3(vector2.x, vector2.y, _range));
	}
}
