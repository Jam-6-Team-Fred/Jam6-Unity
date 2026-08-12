using UnityEngine;

public class SunOverrideVolume : SimpleVolume, SunLightController.ISunOverrider
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private int _priority;

	[SerializeField]
	private float _blendDistance = 1f;

	[SerializeField]
	private bool _overrideColor;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private bool _overrideIntensity;

	[SerializeField]
	private float _intensity = 1f;

	[SerializeField]
	private bool _overrideAmbientIntensity;

	[SerializeField]
	private float _ambientIntensity = 1f;

	[SerializeField]
	private bool _overrideShadowStrength;

	[SerializeField]
	private float _shadowStrength = 1f;

	private void Awake()
	{
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
		SunLightController.RegisterSunOverrider(this, _priority);
	}

	private void OnDisable()
	{
		SunLightController.UnregisterSunOverrider(this);
	}

	private void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	public SunLightController.SunOverrideSettings ApplySunOverrides(OWCamera owCamera, SunLightController.SunOverrideSettings settings)
	{
		Vector3 position = owCamera.transform.position;
		if (Contains(position))
		{
			if (_blendDistance > 0f)
			{
				float t = Mathf.Clamp01((0f - GetPenetrationDist(position)) / _blendDistance);
				if (_overrideColor)
				{
					settings.sunColor = Color.Lerp(settings.sunColor, _color, t);
				}
				if (_overrideIntensity)
				{
					settings.sunIntensity = Mathf.Lerp(settings.sunIntensity, _intensity, t);
				}
				if (_overrideAmbientIntensity)
				{
					settings.ambientIntensity = Mathf.Lerp(settings.ambientIntensity, _ambientIntensity, t);
				}
				if (_overrideShadowStrength)
				{
					settings.sunShadowStrength = Mathf.Lerp(settings.sunShadowStrength, _shadowStrength, t);
				}
			}
			else
			{
				if (_overrideColor)
				{
					settings.sunColor = _color;
				}
				if (_overrideIntensity)
				{
					settings.sunIntensity = _intensity;
				}
				if (_overrideAmbientIntensity)
				{
					settings.ambientIntensity = _ambientIntensity;
				}
				if (_overrideShadowStrength)
				{
					settings.sunShadowStrength = _shadowStrength;
				}
			}
		}
		return settings;
	}
}
