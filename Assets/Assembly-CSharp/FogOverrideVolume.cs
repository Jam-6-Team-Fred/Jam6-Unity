using UnityEngine;

[ExecuteInEditMode]
public class FogOverrideVolume : SimpleVolume
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private float _blendDistance = 1f;

	[SerializeField]
	private bool _overrideDensity;

	[SerializeField]
	private float _density = 1f;

	[SerializeField]
	private bool _overrideColorRampIntensity;

	[SerializeField]
	private float _colorRampIntensity = 1f;

	[SerializeField]
	private bool _overrideTint;

	[SerializeField]
	private Color _tint = Color.white;

	private int _propID_FogParams;

	private int _propID_FogTint;

	public float blendDistance
	{
		get
		{
			return _blendDistance;
		}
		set
		{
			_blendDistance = value;
		}
	}

	public bool overrideDensity
	{
		get
		{
			return _overrideDensity;
		}
		set
		{
			_overrideDensity = value;
		}
	}

	public float density
	{
		get
		{
			return _density;
		}
		set
		{
			_density = value;
		}
	}

	public bool overrideColorRampIntensity
	{
		get
		{
			return _overrideColorRampIntensity;
		}
		set
		{
			_overrideColorRampIntensity = value;
		}
	}

	public float colorRampIntensity
	{
		get
		{
			return _colorRampIntensity;
		}
		set
		{
			_colorRampIntensity = value;
		}
	}

	public bool overrideTint
	{
		get
		{
			return _overrideTint;
		}
		set
		{
			_overrideTint = value;
		}
	}

	public Color tint
	{
		get
		{
			return _tint;
		}
		set
		{
			_tint = value;
		}
	}

	private void Awake()
	{
		_propID_FogParams = Shader.PropertyToID("_FogParams");
		_propID_FogTint = Shader.PropertyToID("_FogTint");
		if (_sector != null)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
	}

	private void OnDestroy()
	{
		if (_sector != null)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
	}

	private void OnEnable()
	{
		OWCamera.onAnyPreRender += new OWEvent<OWCamera>.OWCallback(OverrideFogSettings);
	}

	private void OnDisable()
	{
		OWCamera.onAnyPreRender -= new OWEvent<OWCamera>.OWCallback(OverrideFogSettings);
	}

	private void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	private void OverrideFogSettings(OWCamera owCamera)
	{
		if (!Contains(owCamera.transform.position))
		{
			return;
		}
		float t = Mathf.Clamp01((0f - GetPenetrationDist(owCamera.transform.position)) / _blendDistance);
		if (_overrideDensity || _overrideColorRampIntensity)
		{
			Vector4 globalVector = Shader.GetGlobalVector(_propID_FogParams);
			if (_overrideDensity)
			{
				globalVector.x = Mathf.Lerp(globalVector.x, _density, t);
			}
			if (_overrideColorRampIntensity)
			{
				globalVector.y = Mathf.Lerp(globalVector.y, _colorRampIntensity, t);
			}
			Shader.SetGlobalVector(_propID_FogParams, globalVector);
		}
		if (_overrideTint)
		{
			Color globalColor = Shader.GetGlobalColor(_propID_FogTint);
			Shader.SetGlobalColor(_propID_FogTint, Color.Lerp(globalColor, _tint.linear, t));
		}
	}
}
