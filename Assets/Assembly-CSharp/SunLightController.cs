using System.Collections.Generic;
using UnityEngine;

public class SunLightController : MonoBehaviour
{
	public interface ISunOverrider
	{
		SunOverrideSettings ApplySunOverrides(OWCamera camera, SunOverrideSettings settings);
	}

	public struct SunOverrideSettings
	{
		public Color sunColor;

		public float sunIntensity;

		public float sunShadowStrength;

		public float ambientIntensity;
	}

	private struct ActiveOverrider
	{
		public ISunOverrider sunOverrider;

		public int priority;

		public ActiveOverrider(ISunOverrider sunOverrider, int priority)
		{
			this.sunOverrider = sunOverrider;
			this.priority = priority;
		}
	}

	[SerializeField]
	private Light _sunLight;

	[SerializeField]
	private Light _ambientLight;

	private static SunLightController s_instance = null;

	private static List<ActiveOverrider> s_overriders = new List<ActiveOverrider>(16);

	private bool _initialized;

	private Color _sunBaseColor;

	private float _sunBaseIntensity;

	private float _sunBaseShadowStrength;

	private float _ambientBaseIntensity;

	public Color sunColor
	{
		get
		{
			Initialize();
			return _sunBaseColor;
		}
		set
		{
			Initialize();
			_sunBaseColor = value;
		}
	}

	public float sunIntensity
	{
		get
		{
			Initialize();
			return _sunBaseIntensity;
		}
		set
		{
			Initialize();
			_sunBaseIntensity = Mathf.Max(value, 0f);
		}
	}

	public float sunShadowStrength
	{
		get
		{
			Initialize();
			return _sunBaseShadowStrength;
		}
		set
		{
			Initialize();
			_sunBaseShadowStrength = Mathf.Clamp01(value);
		}
	}

	public float ambientIntensity
	{
		get
		{
			Initialize();
			return _ambientBaseIntensity;
		}
		set
		{
			Initialize();
			_ambientBaseIntensity = Mathf.Max(value, 0f);
		}
	}

	private void Initialize()
	{
		if (!_initialized)
		{
			_sunBaseColor = _sunLight.color;
			_sunBaseIntensity = _sunLight.intensity;
			_sunBaseShadowStrength = _sunLight.shadowStrength;
			_ambientBaseIntensity = _ambientLight.intensity;
			_initialized = true;
		}
	}

	private void Awake()
	{
		if (s_instance != null)
		{
			Debug.LogError("Duplicate SunLightController in Scene!", this);
		}
		s_instance = this;
		Initialize();
		OWCamera.onAnyPreCull += new OWEvent<OWCamera>.OWCallback(ApplySunOverrides);
	}

	private void OnDestroy()
	{
		s_instance = null;
		s_overriders.Clear();
		OWCamera.onAnyPreCull -= new OWEvent<OWCamera>.OWCallback(ApplySunOverrides);
	}

	private void ApplySunOverrides(OWCamera owCamera)
	{
		if (s_overriders.Count == 0)
		{
			_sunLight.color = _sunBaseColor;
			_sunLight.intensity = _sunBaseIntensity;
			_sunLight.shadowStrength = _sunBaseShadowStrength;
			_sunLight.shadows = ((_sunBaseShadowStrength > 0f) ? LightShadows.Soft : LightShadows.None);
			_ambientLight.intensity = _ambientBaseIntensity;
			return;
		}
		SunOverrideSettings settings = default(SunOverrideSettings);
		settings.sunColor = _sunBaseColor;
		settings.sunIntensity = _sunBaseIntensity;
		settings.sunShadowStrength = _sunBaseShadowStrength;
		settings.ambientIntensity = _ambientBaseIntensity;
		for (int i = 0; i < s_overriders.Count; i++)
		{
			settings = s_overriders[i].sunOverrider.ApplySunOverrides(owCamera, settings);
		}
		_sunLight.color = settings.sunColor;
		_sunLight.intensity = settings.sunIntensity;
		_sunLight.shadowStrength = settings.sunShadowStrength;
		_sunLight.shadows = ((settings.sunShadowStrength > 0f) ? LightShadows.Soft : LightShadows.None);
		_ambientLight.intensity = settings.ambientIntensity;
	}

	public static void RegisterSunOverrider(ISunOverrider overrider, int priority)
	{
		for (int i = 0; i < s_overriders.Count; i++)
		{
			if (s_overriders[i].priority > priority)
			{
				s_overriders.Insert(i, new ActiveOverrider(overrider, priority));
				return;
			}
		}
		s_overriders.Add(new ActiveOverrider(overrider, priority));
	}

	public static void UnregisterSunOverrider(ISunOverrider overrider)
	{
		for (int num = s_overriders.Count - 1; num >= 0; num--)
		{
			if (s_overriders[num].sunOverrider == overrider)
			{
				s_overriders.RemoveAt(num);
			}
		}
	}
}
