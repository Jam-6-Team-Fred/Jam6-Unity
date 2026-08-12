using System.Collections.Generic;
using UnityEngine;

public class HeightmapAmbientLight : SectoredMonoBehaviour
{
	private static List<HeightmapAmbientLight> s_activeLights = new List<HeightmapAmbientLight>(8);

	[Space]
	[SerializeField]
	private Texture2D _heightmap;

	[SerializeField]
	private Vector3 _size = new Vector3(100f, 100f, 100f);

	[SerializeField]
	private float _falloff = 10f;

	[Space]
	[SerializeField]
	[Range(0f, 8f)]
	private float _intensity = 1f;

	[SerializeField]
	private Color _color = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Texture2D _gradient;

	public Texture2D heightmap
	{
		get
		{
			return _heightmap;
		}
		set
		{
			_heightmap = value;
		}
	}

	public Vector3 size
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

	public float falloff
	{
		get
		{
			return _falloff;
		}
		set
		{
			_falloff = value;
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
			_intensity = Mathf.Clamp(value, 0f, 8f);
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

	public Texture2D gradient
	{
		get
		{
			return _gradient;
		}
		set
		{
			_gradient = value;
		}
	}

	public static HeightmapAmbientLight GetActiveLight()
	{
		if (s_activeLights.Count == 0)
		{
			return null;
		}
		return s_activeLights[s_activeLights.Count - 1];
	}

	public Matrix4x4 CalcWorldToLightMatrix()
	{
		return Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(_size.x * 0.5f, _size.y * 0.5f, _size.z)).inverse;
	}

	protected override void Awake()
	{
		base.Awake();
		if (_sector != null)
		{
			base.enabled = false;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
	}

	private void OnEnable()
	{
		s_activeLights.Add(this);
	}

	private void OnDisable()
	{
		s_activeLights.Remove(this);
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, _size);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(new Vector3(0f, 0f, 0.5f), Vector3.one);
		}
	}
}
