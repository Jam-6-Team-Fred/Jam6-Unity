using System;
using UnityEngine;

public class ElectricalArc : MonoBehaviour
{
	public enum Preset
	{
		None = 0,
		Auto = 1,
		Small = 2,
		Medium = 3,
		Large = 4
	}

	public enum Mode
	{
		Looping = 0,
		Jumping = 1,
		OneShot = 2
	}

	public enum Dimension
	{
		_1D = 1,
		_2D = 2,
		_3D = 3
	}

	public delegate void ElectricalArcEvent(ElectricalArc arc);

	private static Material s_materialCache;

	private LineRenderer _lineRenderer;

	[Header("Targets")]
	[SerializeField]
	private Transform _startTransform;

	[SerializeField]
	private Vector3 _startLocalPosition = Vector3.zero;

	[SerializeField]
	private Transform _endTransform;

	[SerializeField]
	private Vector3 _endLocalPosition = Vector3.zero;

	[Header("Line Parameters")]
	[SerializeField]
	private Material _material;

	[SerializeField]
	private int _resolution = 16;

	[SerializeField]
	private float _width = 0.1f;

	[Header("Arc Parameters")]
	[SerializeField]
	private Mode _mode;

	[SerializeField]
	private float _frequency = 1f;

	[SerializeField]
	private float _intensity = 1f;

	[SerializeField]
	private float _speed = 1f;

	[SerializeField]
	private float _scrollSpeed = 1f;

	[SerializeField]
	private float _jumpTime = 1f;

	[SerializeField]
	private Vector3 _jumpDirection = Vector3.zero;

	[Header("Noise Parameters")]
	[SerializeField]
	private Dimension _dimension = Dimension._2D;

	[SerializeField]
	private Vector3 _dimensionScale = Vector3.one;

	[SerializeField]
	private int _octaves = 1;

	private Vector3[] _verts;

	private float _randomTimeOffset;

	private float _startTime;

	private float _jumpStartTime;

	public Transform startTransform
	{
		get
		{
			return _startTransform;
		}
		set
		{
			_startTransform = value;
		}
	}

	public Vector3 startLocalPosition
	{
		get
		{
			return _startLocalPosition;
		}
		set
		{
			_startLocalPosition = value;
		}
	}

	public Transform endTransform
	{
		get
		{
			return _endTransform;
		}
		set
		{
			_endTransform = value;
		}
	}

	public Vector3 endLocalPosition
	{
		get
		{
			return _endLocalPosition;
		}
		set
		{
			_endLocalPosition = value;
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
		}
	}

	public int resolution
	{
		get
		{
			return _resolution;
		}
		set
		{
			_resolution = value;
			_verts = new Vector3[_resolution];
			_lineRenderer.positionCount = _resolution;
			RecalculatePositions();
		}
	}

	public float width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
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

	public float frequency
	{
		get
		{
			return _frequency;
		}
		set
		{
			_frequency = value;
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
			_intensity = value;
		}
	}

	public float speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
		}
	}

	public float scrollSpeed
	{
		get
		{
			return _scrollSpeed;
		}
		set
		{
			_scrollSpeed = value;
		}
	}

	public float jumpTime
	{
		get
		{
			return _jumpTime;
		}
		set
		{
			_jumpTime = value;
		}
	}

	public Vector3 jumpDirection
	{
		get
		{
			return _jumpDirection;
		}
		set
		{
			_jumpDirection = value;
		}
	}

	public Dimension dimension
	{
		get
		{
			return _dimension;
		}
		set
		{
			_dimension = value;
		}
	}

	public Vector3 dimensionScale
	{
		get
		{
			return _dimensionScale;
		}
		set
		{
			_dimensionScale = value;
		}
	}

	public int octaves
	{
		get
		{
			return _octaves;
		}
		set
		{
			_octaves = value;
		}
	}

	public event ElectricalArcEvent OnJump;

	public event ElectricalArcEvent OnFinish;

	private static Material GetMaterial()
	{
		if (s_materialCache == null)
		{
			s_materialCache = Resources.Load<Material>("Materials/Effects_ElectricalArc_mat");
		}
		return s_materialCache;
	}

	private void OnValidate()
	{
		if (_resolution < 2 || _resolution > 1024)
		{
			_resolution = Mathf.Clamp(_resolution, 2, 1024);
		}
		if (_width < 0f)
		{
			_width = 0f;
		}
		if (_jumpTime < 0.001f)
		{
			_jumpTime = 0.001f;
		}
		if (_octaves < 1 || _octaves > 8)
		{
			_octaves = Mathf.Clamp(_octaves, 1, 8);
		}
	}

	private void Awake()
	{
		_lineRenderer = base.gameObject.GetAddComponent<LineRenderer>();
		_lineRenderer.sharedMaterial = _material;
		_verts = new Vector3[_resolution];
		_lineRenderer.positionCount = _resolution;
		if (_startTransform == null)
		{
			_startTransform = base.transform;
		}
		if (_endTransform == null)
		{
			_endTransform = base.transform;
		}
		_randomTimeOffset = UnityEngine.Random.Range(-1000f, 1000f);
		_startTime = Time.time;
		_jumpStartTime = _startTime;
		RecalculatePositions();
	}

	private void OnEnable()
	{
		_lineRenderer.enabled = true;
		if (_mode == Mode.Jumping)
		{
			_jumpStartTime = Time.time;
		}
	}

	private void OnDisable()
	{
		_lineRenderer.enabled = false;
	}

	private void Update()
	{
		RecalculatePositions();
	}

	private void RecalculatePositions()
	{
		float num = Time.time - _startTime;
		float num2 = 0f;
		if (_mode == Mode.Jumping)
		{
			num2 = Mathf.Clamp01((Time.time - _jumpStartTime) / _jumpTime);
			num2 = (num2 - 2f) * (0f - num2);
			if (num2 >= 1f)
			{
				_jumpStartTime = Time.time;
				_randomTimeOffset = UnityEngine.Random.Range(-1000f, 1000f);
				if (this.OnJump != null)
				{
					this.OnJump(this);
				}
			}
		}
		else if (_mode == Mode.OneShot)
		{
			num2 = Mathf.Clamp01((Time.time - _jumpStartTime) / _jumpTime);
			num2 = (num2 - 2f) * (0f - num2);
			if (num2 >= 1f)
			{
				if (this.OnFinish != null)
				{
					this.OnFinish(this);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		Vector3 vector = _startTransform.TransformPoint(_startLocalPosition);
		Vector3 vector2 = _endTransform.TransformPoint(_endLocalPosition);
		Vector3 vector3 = vector2 - vector;
		float magnitude = vector3.magnitude;
		vector3 /= magnitude;
		float seedScale = _frequency * magnitude;
		Vector3 vector4 = vector3;
		Vector3 vector5 = _startTransform.up;
		if (Vector3.Dot(vector3, vector5) > 0.99999f)
		{
			vector5 = _startTransform.right;
		}
		Vector3 normalized = Vector3.Cross(vector5, vector3).normalized;
		Vector3 normalized2 = Vector3.Cross(vector4, normalized).normalized;
		for (int i = 0; i < _verts.Length; i++)
		{
			float num3 = (float)i / (float)(_verts.Length - 1);
			float num4 = 1f - Mathf.Cos(num3 * (float)Math.PI * 2f);
			Vector3 vector6 = Vector3.Lerp(vector, vector2, num3);
			Vector3 zero = Vector3.zero;
			for (int j = 0; j < _octaves; j++)
			{
				zero += CalcNoise(num3 - num * _scrollSpeed, seedScale, num * _speed + _randomTimeOffset, j);
			}
			zero.Scale(_dimensionScale);
			zero *= _intensity;
			zero += _jumpDirection * num2;
			zero = normalized * zero.x + normalized2 * zero.y + vector4 * zero.z;
			_verts[i] = vector6 + zero * num4;
		}
		_lineRenderer.widthMultiplier = (1f - num2) * _width;
		_lineRenderer.SetPositions(_verts);
	}

	private Vector3 CalcNoise(float baseSeed, float seedScale, float time, int octave)
	{
		float num = 1 << octave;
		Vector3 zero = Vector3.zero;
		zero.y = Mathf.PerlinNoise(baseSeed * num * seedScale, time) * 2f - 1f;
		if (_dimension > Dimension._1D)
		{
			zero.x = Mathf.PerlinNoise((baseSeed + 1f) * num * seedScale, time) * 2f - 1f;
		}
		if (_dimension > Dimension._2D)
		{
			zero.z = Mathf.PerlinNoise((baseSeed + 2f) * num * seedScale, time) * 2f - 1f;
		}
		return zero / num;
	}

	public static ElectricalArc PlayOneShot(Transform start, Vector3 startOffset, Transform end, Vector3 endOffset, Preset preset = Preset.Auto)
	{
		GameObject gameObject = new GameObject("ElectricalArc");
		gameObject.transform.SetParent(start);
		ElectricalArc electricalArc = gameObject.AddComponent<ElectricalArc>();
		electricalArc._startTransform = ((start != null) ? start : gameObject.transform);
		electricalArc._startLocalPosition = startOffset;
		electricalArc._endTransform = ((end != null) ? end : gameObject.transform);
		electricalArc._endLocalPosition = endOffset;
		electricalArc.ApplyPreset(preset);
		electricalArc._material = GetMaterial();
		electricalArc._lineRenderer.sharedMaterial = electricalArc._material;
		electricalArc._dimension = Dimension._2D;
		electricalArc._mode = Mode.OneShot;
		if (preset == Preset.Auto)
		{
			electricalArc._jumpTime *= 0.5f;
			electricalArc._jumpDirection *= 0.5f;
		}
		return electricalArc;
	}

	private void ApplyPreset(Preset preset)
	{
		switch (preset)
		{
		default:
			return;
		case Preset.Auto:
		{
			Vector3 a = _startTransform.TransformPoint(_startLocalPosition);
			Vector3 b = _endTransform.TransformPoint(_endLocalPosition);
			float num = Vector3.Distance(a, b);
			_resolution = (int)(32f * Mathf.Sqrt(num));
			_width = num * 0.05f;
			_frequency = 3f * Mathf.Pow(num, -0.5f);
			_intensity = num * 0.1f;
			_speed = 1f;
			_scrollSpeed = 2f * Mathf.Pow(num, -0.5f);
			_jumpTime = 0.5f * Mathf.Pow(num, 0.5f);
			_jumpDirection = new Vector3(0f, num * 0.3f, 0f);
			_octaves = ((!(num > 1f)) ? 1 : 2);
			break;
		}
		case Preset.Small:
			_resolution = 16;
			_width = 0.02f;
			_frequency = 10f;
			_intensity = 0.05f;
			_speed = 1f;
			_scrollSpeed = 5f;
			_jumpTime = 0.15f;
			_jumpDirection = new Vector3(0f, 0.1f, 0f);
			_octaves = 1;
			break;
		case Preset.Medium:
			_resolution = 32;
			_width = 0.1f;
			_frequency = 2f;
			_intensity = 0.25f;
			_speed = 1f;
			_scrollSpeed = 2f;
			_jumpTime = 0.5f;
			_jumpDirection = new Vector3(0f, 0.25f, 0f);
			_octaves = 2;
			break;
		case Preset.Large:
			_resolution = 128;
			_width = 0.5f;
			_frequency = 1f;
			_intensity = 1f;
			_speed = 1f;
			_scrollSpeed = 0.5f;
			_jumpTime = 1f;
			_jumpDirection = new Vector3(0f, 3f, 0f);
			_octaves = 2;
			break;
		}
		_verts = new Vector3[_resolution];
		_lineRenderer.positionCount = _resolution;
		RecalculatePositions();
	}
}
