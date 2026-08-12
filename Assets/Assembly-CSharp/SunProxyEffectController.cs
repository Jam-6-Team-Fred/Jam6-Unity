using UnityEngine;

public class SunProxyEffectController : MonoBehaviour
{
	[SerializeField]
	private Renderer _atmosphere;

	[SerializeField]
	private Renderer _fog;

	[SerializeField]
	private TessellatedSphereRenderer _surface;

	[SerializeField]
	private SolarFlareEmitter _solarFlareEmitter;

	[SerializeField]
	private Transform _lightShaftRoot;

	[SerializeField]
	private SupernovaEffectController _proxySupernova;

	[SerializeField]
	protected float _rayleighCurveMinDistance = 45000f;

	[SerializeField]
	protected float _rayleighCurveMaxDistance = 1250000f;

	[SerializeField]
	protected float _rayleighCurveMinVal;

	[SerializeField]
	protected float _rayleighCurveMaxVal;

	[SerializeField]
	protected AnimationCurve _rayleighCurve;

	private Material _atmosphereMaterial;

	private int _propID_SkyColor;

	private int _propID_OuterRadius;

	private int _propID_InnerRadius;

	private Material _fogMaterial;

	private int _propID_Radius;

	private int _propID_LODFade;

	private static readonly int propID_RayleighConstant = Shader.PropertyToID("_Kr");

	private bool _supernovaStarted;

	private bool _shouldBeVisible;

	private void Awake()
	{
		_proxySupernova.SetIsProxy(isProxy: true);
		_propID_SkyColor = Shader.PropertyToID("_SkyColor");
		_propID_OuterRadius = Shader.PropertyToID("_OuterRadius");
		_propID_InnerRadius = Shader.PropertyToID("_InnerRadius");
		_propID_Radius = Shader.PropertyToID("_Radius");
		_propID_LODFade = Shader.PropertyToID("_LODFade");
	}

	public void SetMaterials(Material surfaceMaterial, Material atmMaterial, Material fogMaterial, Material lightShaftMaterial)
	{
		_surface.sharedMaterial = surfaceMaterial;
		_atmosphereMaterial = new Material(atmMaterial);
		_atmosphere.sharedMaterial = _atmosphereMaterial;
		_fogMaterial = new Material(fogMaterial);
		_fogMaterial.SetFloat(_propID_LODFade, 1f);
		_fog.sharedMaterial = fogMaterial;
		MeshRenderer[] componentsInChildren = _lightShaftRoot.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterial = lightShaftMaterial;
		}
	}

	public void UpdateScales(Vector3 surfaceScale, float fogRadius, Vector3 solarFlareEmitterScale, Vector3 atmosphereScale)
	{
		_surface.transform.localScale = surfaceScale;
		_fog.transform.localScale = Vector3.one * fogRadius;
		_fogMaterial.SetFloat(_propID_Radius, fogRadius);
		_solarFlareEmitter.transform.localScale = solarFlareEmitterScale;
		_atmosphere.transform.localScale = atmosphereScale * 2f;
	}

	public void BeginSupernova()
	{
		_supernovaStarted = true;
		_atmosphere.enabled = false;
		_fog.enabled = false;
		_solarFlareEmitter.enabled = false;
		_proxySupernova.enabled = true;
		_proxySupernova.SetParticlesVisibility(_shouldBeVisible);
	}

	public void UpdateAtmosphereColor(Color atmColor)
	{
		_atmosphereMaterial.SetColor(_propID_SkyColor, atmColor);
	}

	public void UpdateAtmosphereRadii(float realSunInnerRadius, float realSunOuterRadius)
	{
		float x = base.transform.parent.localScale.x;
		_atmosphereMaterial.SetFloat(_propID_InnerRadius, realSunInnerRadius * x);
		_atmosphereMaterial.SetFloat(_propID_OuterRadius, realSunOuterRadius * x);
	}

	public void UpdateRayleighConstant(float viewDistance)
	{
		float t = _rayleighCurve.Evaluate(Mathf.InverseLerp(_rayleighCurveMinDistance, _rayleighCurveMaxDistance, viewDistance));
		_atmosphereMaterial.SetFloat(propID_RayleighConstant, Mathf.Lerp(_rayleighCurveMinVal, _rayleighCurveMaxVal, t));
	}

	public void UpdateLightShaftScale(Vector3 lightShaftScale)
	{
		_lightShaftRoot.localScale = lightShaftScale;
	}

	public void SetLightShaftsActive(bool active)
	{
		if (_lightShaftRoot.gameObject.activeSelf != active)
		{
			_lightShaftRoot.gameObject.SetActive(active);
		}
	}

	public void SetRenderingEnabled(bool renderingEnabled)
	{
		_shouldBeVisible = renderingEnabled;
		_surface.enabled = renderingEnabled;
		if (!_supernovaStarted)
		{
			_atmosphere.enabled = renderingEnabled;
			_fog.enabled = renderingEnabled;
			_solarFlareEmitter.SetRenderingEnabled(renderingEnabled);
		}
		_proxySupernova.SetRenderingEnabled(renderingEnabled);
	}
}
