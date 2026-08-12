using UnityEngine;

public class ShockLayerRuleset : RulesetVolume
{
	public enum ShockType
	{
		None = 0,
		Radial = 1,
		Atmospheric = 2
	}

	[SerializeField]
	private ShockType _type;

	[SerializeField]
	private Transform _radialCenter;

	[SerializeField]
	private float _innerRadius = 100f;

	[SerializeField]
	private float _outerRadius = 300f;

	[SerializeField]
	[ColorUsage(true, true)]
	private Color _color = new Color(2f, 2f, 2f, 1f);

	[Header("Atmospheric Settings")]
	[SerializeField]
	private float _minShockSpeed = 100f;

	[SerializeField]
	private float _maxShockSpeed = 300f;

	[Header("Radius Settings")]
	[SerializeField]
	private float _trailLength = 20f;

	[SerializeField]
	private float _trailFlare = 10f;

	private float _radiusScale = 1f;

	public bool UsesShockLayer()
	{
		return _type != ShockType.None;
	}

	public ShockType GetShockLayerType()
	{
		return _type;
	}

	public Transform GetRadialCenter()
	{
		return _radialCenter;
	}

	public float GetInnerRadius()
	{
		return _innerRadius * _radiusScale;
	}

	public float GetOuterRadius()
	{
		return _outerRadius * _radiusScale;
	}

	public float GetRadiusScale()
	{
		return _radiusScale;
	}

	public void SetRadiusScale(float scale)
	{
		_radiusScale = scale;
	}

	public Color GetColor()
	{
		return _color;
	}

	public float GetMinShockSpeed()
	{
		return _minShockSpeed;
	}

	public float GetMaxShockSpeed()
	{
		return _maxShockSpeed;
	}

	public float GetTrailLength()
	{
		return _trailLength;
	}

	public float GetTrailFlare()
	{
		return _trailFlare;
	}
}
