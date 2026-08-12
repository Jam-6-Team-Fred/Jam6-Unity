using UnityEngine;

public class ProxyPlanet : ProxyBody
{
	[SerializeField]
	protected Renderer _atmosphere;

	[SerializeField]
	protected Renderer _fog;

	[Space]
	[Header("Mie Interpolation")]
	[SerializeField]
	protected float _mieCurveMinDistance = 45000f;

	[SerializeField]
	protected float _mieCurveMaxDistance = 750000f;

	[SerializeField]
	protected float _mieCurveMinVal;

	[SerializeField]
	protected float _mieCurveMaxVal;

	[SerializeField]
	protected AnimationCurve _mieCurve;

	[Space]
	[Header("Fog Interpolation")]
	[SerializeField]
	protected float _fogCurveMinDistance = 45000f;

	[SerializeField]
	protected float _fogCurveMaxDistance = 750000f;

	[SerializeField]
	protected float _fogCurveMinVal;

	[SerializeField]
	protected float _fogCurveMaxVal;

	[SerializeField]
	protected AnimationCurve _fogCurve;

	[Space]
	private Material _atmosphereMaterial;

	private float _baseAtmoMatShellInnerRadius;

	private float _baseAtmoMatShellOuterRadius;

	private static readonly int propID_AtmoInnerRadius = Shader.PropertyToID("_InnerRadius");

	private static readonly int propID_AtmoOuterRadius = Shader.PropertyToID("_OuterRadius");

	private static readonly int propID_MieConstant = Shader.PropertyToID("_Km");

	private static readonly int propID_FogDensity = Shader.PropertyToID("_Density");

	private bool _hasAtmosphere;

	private bool _hasFog;

	private Material _fogMaterial;

	private static readonly int propID_LODFade = Shader.PropertyToID("_LODFade");

	protected virtual AstroObject.Name astroObjectName => AstroObject.Name.None;

	public override void UpdateScale(float scaleMultiplier, float viewDistance)
	{
		base.UpdateScale(scaleMultiplier, viewDistance);
		if (_hasAtmosphere)
		{
			_atmosphereMaterial.SetFloat(propID_AtmoInnerRadius, _baseAtmoMatShellInnerRadius * scaleMultiplier);
			_atmosphereMaterial.SetFloat(propID_AtmoOuterRadius, _baseAtmoMatShellOuterRadius * scaleMultiplier);
			float t = _mieCurve.Evaluate(Mathf.InverseLerp(_mieCurveMinDistance, _mieCurveMaxDistance, viewDistance));
			_atmosphereMaterial.SetFloat(propID_MieConstant, Mathf.Lerp(_mieCurveMinVal, _mieCurveMaxVal, t));
		}
		if (_hasFog)
		{
			float t2 = _fogCurve.Evaluate(Mathf.InverseLerp(_fogCurveMinDistance, _fogCurveMaxDistance, viewDistance));
			_fogMaterial.SetFloat(propID_FogDensity, Mathf.Lerp(_fogCurveMinVal, _fogCurveMaxVal, t2));
		}
	}

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		if (_atmosphere != null)
		{
			_atmosphere.enabled = on;
		}
		if (_fog != null)
		{
			_fog.enabled = on;
		}
	}

	protected override void Initialize()
	{
		AstroObject astroObject = Locator.GetAstroObject(astroObjectName);
		_realObjectTransform = astroObject.transform;
		_hasAtmosphere = _atmosphere != null;
		if (_hasAtmosphere)
		{
			_atmosphereMaterial = new Material(_atmosphere.sharedMaterial);
			_baseAtmoMatShellInnerRadius = _atmosphereMaterial.GetFloat(propID_AtmoInnerRadius);
			_baseAtmoMatShellOuterRadius = _atmosphereMaterial.GetFloat(propID_AtmoOuterRadius);
			_atmosphere.sharedMaterial = _atmosphereMaterial;
		}
		if (_fog != null)
		{
			_hasFog = true;
			_fogMaterial = new Material(_fog.sharedMaterial);
			_fogMaterial.SetFloat(propID_LODFade, 1f);
			_fog.sharedMaterial = _fogMaterial;
		}
	}
}
