using UnityEngine;

public static class ProxyShadowSettings
{
	public delegate void SettingsChanged();

	private static float _shadowDistance = 1200f;

	private static float _bias = 1f;

	private static int _shadowTextureSquareSize = 1024;

	private static ProxyShadowCascade.Division[] _cascadeDivisions = new ProxyShadowCascade.Division[4]
	{
		new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Near, 0.05f),
		new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Mid, 0.2f),
		new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Far, 0.65f),
		new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Final, 1f)
	};

	public static float shadowDistance
	{
		get
		{
			return _shadowDistance;
		}
		set
		{
			_shadowDistance = Mathf.Max(value, 0.001f);
			if (ProxyShadowSettings.OnShadowDistanceChanged != null)
			{
				ProxyShadowSettings.OnShadowDistanceChanged();
			}
		}
	}

	public static float bias
	{
		get
		{
			return _bias;
		}
		set
		{
			_bias = Mathf.Clamp(value, 0f, 2f);
			if (ProxyShadowSettings.OnBiasChanged != null)
			{
				ProxyShadowSettings.OnBiasChanged();
			}
		}
	}

	public static int shadowTextureSquareSize
	{
		get
		{
			return _shadowTextureSquareSize;
		}
		set
		{
			_shadowTextureSquareSize = Mathf.Max(value, 0);
			if (ProxyShadowSettings.OnShadowTextureSizeChanged != null)
			{
				ProxyShadowSettings.OnShadowTextureSizeChanged();
			}
		}
	}

	public static ProxyShadowCascade.Division[] cascadeDivisions
	{
		get
		{
			return _cascadeDivisions;
		}
		set
		{
			_cascadeDivisions = value;
			if (_cascadeDivisions.Length != 0)
			{
				_cascadeDivisions[_cascadeDivisions.Length - 1].fraction = 1f;
			}
			if (ProxyShadowSettings.OnCascadeDivisionsChanged != null)
			{
				ProxyShadowSettings.OnCascadeDivisionsChanged();
			}
		}
	}

	public static int numCascades => _cascadeDivisions.Length;

	public static event SettingsChanged OnCascadeDivisionsChanged;

	public static event SettingsChanged OnShadowTextureSizeChanged;

	public static event SettingsChanged OnBiasChanged;

	public static event SettingsChanged OnShadowDistanceChanged;
}
