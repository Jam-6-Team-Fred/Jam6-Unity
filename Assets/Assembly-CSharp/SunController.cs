using UnityEngine;

public class SunController : MonoBehaviour
{
	public delegate void SunEvent();

	[Header("References")]
	[SerializeField]
	private ReferenceFrameVolume _rfVolume;

	[SerializeField]
	private Light _ambientLight;

	[SerializeField]
	private LODGroup _atmosphere;

	[SerializeField]
	private PlanetaryFogController _fog;

	[SerializeField]
	private ShockLayerRuleset _shockLayerRuleset;

	[SerializeField]
	private TessellatedSphereRenderer _surface;

	[SerializeField]
	private Transform _scaledVolumesRoot;

	[SerializeField]
	private SolarFlareEmitter _solarFlareEmitter;

	[SerializeField]
	private SunLightController _sunLight;

	[SerializeField]
	private SupernovaEffectController _supernova;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private Transform _lightShaftRoot;

	[Header("Progression ")]
	[SerializeField]
	private float _progressionStartTime = 1f;

	[SerializeField]
	private float _progressionEndTime = 19f;

	[Space]
	[SerializeField]
	private float _endScale = 2f;

	[SerializeField]
	private float _fogEndRadius = 4250f;

	[SerializeField]
	private float _fogEndFadeDistance = 250f;

	[SerializeField]
	private float _scaleStartTime = 10f;

	[SerializeField]
	private float _scaleEndTime = 19f;

	[Space]
	[SerializeField]
	private Gradient _atmosphereColor = new Gradient();

	[Space]
	[SerializeField]
	private Material _startSurfaceMaterial;

	[SerializeField]
	private Material _endSurfaceMaterial;

	[Space]
	[SerializeField]
	private Gradient _solarFlareTint = new Gradient();

	[Space]
	[SerializeField]
	private float _endLightIntensity = 0.5f;

	[SerializeField]
	private Gradient _lightColor = new Gradient();

	[Header("Collapse")]
	[SerializeField]
	private float _collapseLength = 10f;

	[SerializeField]
	private float _collapsedScale = 0.1f;

	[Space]
	[SerializeField]
	private ParticleSystem[] _collapseParticles = new ParticleSystem[0];

	[Space]
	[SerializeField]
	private Gradient _collapseAtmosphereColor = new Gradient();

	[Space]
	[SerializeField]
	private Material _collapseTransitionMaterial;

	[SerializeField]
	private Material _collapseStartSurfaceMaterial;

	[SerializeField]
	private Material _collapseEndSurfaceMaterial;

	[Header("Supernova")]
	[SerializeField]
	private Color _lightBlastColor = Color.white;

	[SerializeField]
	private AnimationCurve _lightFlareCurve;

	private float _rfVolumeBracketsRadius;

	private float _rfVolumeMinColliderRadius;

	private float _rfVolumeMaxColliderRadius;

	private float _ambientLightOuterRadius;

	private Vector3 _atmosphereScale;

	private Material _atmosphereMaterial;

	private int _propID_SkyColor;

	private int _propID_OuterRadius;

	private int _propID_InnerRadius;

	private float _atmosphereOuterRadius;

	private float _atmosphereInnerRadius;

	private Renderer[] _atmosphereRenderers;

	private Material _fogMaterial;

	private int _propID_Tint;

	private int _propID_Radius;

	private float _fogRadius;

	private float _fogLODFadeDistance;

	private Vector3 _origSurfaceScale;

	private Material _surfaceMaterial;

	private Vector3 _solarFlareEmitterScale;

	private float _sunLightIntensity;

	private bool _collapseStarted;

	private float _collapseStartTime;

	private float _collapseT;

	private bool _supernovaStarted;

	private float _supernovaStartTime;

	private Material _lightShaftMaterial;

	private float _lightShaftOrigScale;

	private float _scale = 1f;

	private SunProxyEffectController _sunProxyEffects;

	public event SunEvent OnCollapseStart;

	public event SunEvent OnSupernovaStart;

	private void Awake()
	{
		_rfVolumeBracketsRadius = _rfVolume.GetReferenceFrame().GetBracketsRadius();
		_rfVolumeMinColliderRadius = _rfVolume.MinColliderRadius;
		_rfVolumeMaxColliderRadius = _rfVolume.MaxColliderRadius;
		_ambientLightOuterRadius = _ambientLight.range;
		_atmosphereScale = _atmosphere.transform.localScale;
		_atmosphereRenderers = _atmosphere.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < _atmosphereRenderers.Length; i++)
		{
			if (_atmosphereMaterial == null)
			{
				_atmosphereMaterial = new Material(_atmosphereRenderers[i].material);
			}
			_atmosphereRenderers[i].material = _atmosphereMaterial;
		}
		_propID_SkyColor = Shader.PropertyToID("_SkyColor");
		_propID_OuterRadius = Shader.PropertyToID("_OuterRadius");
		_propID_InnerRadius = Shader.PropertyToID("_InnerRadius");
		_atmosphereOuterRadius = _atmosphereMaterial.GetFloat(_propID_OuterRadius);
		_atmosphereInnerRadius = _atmosphereMaterial.GetFloat(_propID_InnerRadius);
		_fogMaterial = new Material(_fog.fogImpostor.material);
		_fog.fogImpostor.material = _fogMaterial;
		_propID_Tint = Shader.PropertyToID("_Tint");
		_propID_Radius = Shader.PropertyToID("_Radius");
		_fogRadius = _fog.fogRadius;
		_fogLODFadeDistance = _fog.lodFadeDistance;
		_origSurfaceScale = _surface.transform.localScale;
		_surfaceMaterial = new Material(_startSurfaceMaterial);
		_surface.sharedMaterial = _surfaceMaterial;
		_solarFlareEmitterScale = _solarFlareEmitter.transform.localScale;
		_sunLightIntensity = _sunLight.sunIntensity;
		_sunLight.sunColor = _lightColor.Evaluate(0f);
		_collapseStarted = false;
		_collapseT = 0f;
		_supernovaStarted = false;
		MeshRenderer[] componentsInChildren = _lightShaftRoot.GetComponentsInChildren<MeshRenderer>();
		_lightShaftMaterial = new Material(componentsInChildren[0].sharedMaterial);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].sharedMaterial = _lightShaftMaterial;
		}
		_lightShaftMaterial.SetAlpha(0f);
		_lightShaftRoot.gameObject.SetActive(value: false);
		_lightShaftOrigScale = _lightShaftRoot.localScale.x;
		_supernova.SetIsProxy(isProxy: false);
		GlobalMessenger.AddListener("TriggerSupernova", OnTriggerSupernova);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("TriggerSupernova", OnTriggerSupernova);
	}

	public float GetSurfaceRadius()
	{
		return _origSurfaceScale.x * _scale;
	}

	private void OnTriggerSupernova()
	{
		_collapseStarted = true;
		_collapseStartTime = Time.timeSinceLevelLoad;
		if (!PlayerState.IsSleepingAtCampfire() && !PlayerState.InDreamWorld())
		{
			_oneShotSource.PlayOneShot(AudioType.Sun_Collapse);
		}
		for (int i = 0; i < _collapseParticles.Length; i++)
		{
			if (_collapseParticles[i] != null)
			{
				_collapseParticles[i].Play(withChildren: true);
			}
		}
		_lightShaftRoot.gameObject.SetActive(value: true);
		_sunProxyEffects.SetLightShaftsActive(active: true);
		if (this.OnCollapseStart != null)
		{
			this.OnCollapseStart();
		}
	}

	private void Update()
	{
		if (_supernovaStarted)
		{
			float num = Time.timeSinceLevelLoad - _supernovaStartTime;
			float value = (base.transform.position - Locator.GetActiveCamera().transform.position).magnitude - _supernova.GetSupernovaRadius();
			float num2 = Mathf.InverseLerp(0f, 5f, num);
			float num3 = Mathf.InverseLerp(10000f, 0f, value);
			float num4 = num2 * num3 * num3 * 4f;
			float num5 = 5f;
			if (num < num5)
			{
				float time = Mathf.InverseLerp(0f, num5, num);
				time = _lightFlareCurve.Evaluate(time);
				_sunLight.sunIntensity = time * 2f + num4;
			}
			else
			{
				_sunLight.sunIntensity = num4;
			}
		}
		else if (_collapseStarted)
		{
			_collapseT = Mathf.Clamp01((Time.timeSinceLevelLoad - _collapseStartTime) / _collapseLength);
			float scale = Mathf.Lerp(_endScale, _collapsedScale, _collapseT * _collapseT);
			UpdateScale(scale);
			if (_collapseT <= 0.1f)
			{
				_surfaceMaterial.Lerp(_endSurfaceMaterial, _collapseTransitionMaterial, _collapseT / 0.1f);
			}
			else
			{
				float t = (_collapseT - 0.1f) / 0.9f;
				_surfaceMaterial.CopyPropertiesFromMaterial(_collapseStartSurfaceMaterial);
				_surfaceMaterial.Lerp(_collapseStartSurfaceMaterial, _collapseEndSurfaceMaterial, t);
			}
			Color linear = _collapseAtmosphereColor.Evaluate(_collapseT).linear;
			_atmosphereMaterial.SetColor(_propID_SkyColor, linear);
			_sunProxyEffects.UpdateAtmosphereColor(linear);
			_fogMaterial.SetColor(_propID_Tint, _collapseAtmosphereColor.Evaluate(_collapseT));
			_fog.fogTint = _collapseAtmosphereColor.Evaluate(_collapseT);
			_sunLight.sunIntensity = Mathf.Lerp(_endLightIntensity, 0f, _collapseT);
			float num6 = 0.9f;
			float num7 = ((_collapseT < num6) ? Mathf.InverseLerp(0f, num6, _collapseT) : (1f - Mathf.InverseLerp(num6, 1f, _collapseT)));
			_lightShaftMaterial.SetAlpha(num7 * num7);
			float num8 = Mathf.InverseLerp(0f, 1f, _collapseT);
			num8 *= num8;
			Vector3 vector = Mathf.Lerp(1f, 0.5f, num8) * _lightShaftOrigScale * Vector3.one;
			_lightShaftRoot.localScale = vector;
			_sunProxyEffects.UpdateLightShaftScale(vector);
			if (_collapseT == 1f)
			{
				_supernovaStarted = true;
				_supernovaStartTime = Time.timeSinceLevelLoad;
				_ambientLight.enabled = false;
				_atmosphere.enabled = false;
				for (int i = 0; i < _atmosphereRenderers.Length; i++)
				{
					_atmosphereRenderers[i].enabled = false;
				}
				_fog.enabled = false;
				_fog.fogImpostor.enabled = false;
				_solarFlareEmitter.enabled = false;
				_solarFlareEmitter.Clear();
				_rfVolume.GetComponent<Collider>().enabled = false;
				_sunLight.sunColor = _lightBlastColor;
				_supernova.enabled = true;
				_sunProxyEffects.BeginSupernova();
				if (!PlayerState.IsSleepingAtCampfire() && !PlayerState.InDreamWorld())
				{
					_oneShotSource.PlayOneShot(AudioType.Sun_Explosion);
					CheckPlayerSawSunExplode();
				}
				GlobalMessenger.FireEvent("SunExploded");
				if (this.OnSupernovaStart != null)
				{
					this.OnSupernovaStart();
				}
			}
		}
		else
		{
			float num9 = Mathf.InverseLerp(_progressionStartTime, _progressionEndTime, TimeLoop.GetMinutesElapsed());
			float t2 = Mathf.InverseLerp(_scaleStartTime, _scaleEndTime, TimeLoop.GetMinutesElapsed());
			float scale2 = Mathf.Lerp(1f, _endScale, Mathf.SmoothStep(0f, 1f, t2));
			UpdateScale(scale2);
			Color linear2 = _atmosphereColor.Evaluate(num9).linear;
			_atmosphereMaterial.SetColor(_propID_SkyColor, linear2);
			_sunProxyEffects.UpdateAtmosphereColor(linear2);
			_fogMaterial.SetColor(_propID_Tint, _atmosphereColor.Evaluate(num9));
			_fog.fogTint = _atmosphereColor.Evaluate(num9);
			_surfaceMaterial.Lerp(_startSurfaceMaterial, _endSurfaceMaterial, num9);
			_solarFlareEmitter.tint = _solarFlareTint.Evaluate(num9);
			_sunLight.sunIntensity = Mathf.Lerp(_sunLightIntensity, _endLightIntensity, num9);
			_sunLight.sunColor = _lightColor.Evaluate(num9);
		}
	}

	private void UpdateScale(float scale)
	{
		_scale = scale;
		_rfVolume.GetReferenceFrame().SetBracketsRadius(_rfVolumeBracketsRadius * scale);
		_rfVolume.MinColliderRadius = _rfVolumeMinColliderRadius * scale;
		_rfVolume.MaxColliderRadius = _rfVolumeMaxColliderRadius * scale;
		_ambientLight.range = _ambientLightOuterRadius * scale;
		Vector3 vector = _atmosphereScale * scale;
		_atmosphere.transform.localScale = vector;
		float num = _atmosphereInnerRadius * scale;
		float num2 = _atmosphereOuterRadius * scale;
		_atmosphereMaterial.SetFloat(_propID_OuterRadius, num2);
		_atmosphereMaterial.SetFloat(_propID_InnerRadius, num);
		float t = Mathf.InverseLerp(1f, _endScale, scale);
		float num3 = ((scale < 1f) ? (_fogRadius * scale) : Mathf.Lerp(_fogRadius, _fogEndRadius, t));
		_fog.transform.localScale = new Vector3(num3, num3, num3);
		_fogMaterial.SetFloat(_propID_Radius, num3);
		_fog.fogRadius = num3;
		_fog.lodFadeDistance = Mathf.Lerp(_fogLODFadeDistance, _fogEndFadeDistance, t);
		Vector3 vector2 = _origSurfaceScale * scale;
		_surface.transform.localScale = vector2;
		_scaledVolumesRoot.transform.localScale = Vector3.one * scale;
		Vector3 vector3 = _solarFlareEmitterScale * scale;
		_solarFlareEmitter.transform.localScale = vector3;
		_shockLayerRuleset.SetRadiusScale(scale);
		_sunProxyEffects.UpdateScales(vector2, num3, vector3, vector);
		_sunProxyEffects.UpdateAtmosphereRadii(num, num2);
	}

	public float GetCollapseProgress()
	{
		return _collapseT;
	}

	public float GetSupernovaRadius()
	{
		return _supernova.GetSupernovaRadius();
	}

	public void SetRenderingEnabled(bool renderingEnabled)
	{
		_surface.enabled = renderingEnabled;
		Renderer[] atmosphereRenderers = _atmosphereRenderers;
		for (int i = 0; i < atmosphereRenderers.Length; i++)
		{
			atmosphereRenderers[i].enabled = renderingEnabled;
		}
		_atmosphere.enabled = renderingEnabled;
		for (int j = 0; j < _atmosphereRenderers.Length; j++)
		{
			_atmosphereRenderers[j].enabled = renderingEnabled;
		}
		_solarFlareEmitter.SetRenderingEnabled(renderingEnabled);
		_supernova.SetRenderingEnabled(renderingEnabled);
	}

	public void SetProxyEffectController(SunProxyEffectController sunProxyEffectController)
	{
		_sunProxyEffects = sunProxyEffectController;
		_sunProxyEffects.SetLightShaftsActive(active: false);
		_sunProxyEffects.SetMaterials(_surfaceMaterial, _atmosphereMaterial, _fogMaterial, _lightShaftMaterial);
	}

	public bool HasSupernovaStarted()
	{
		return _supernovaStarted;
	}

	public bool IsPointInsideSupernova(Vector3 worldPosition)
	{
		if (_supernovaStarted)
		{
			return (worldPosition - base.transform.position).sqrMagnitude < _supernova.GetSupernovaRadius() * _supernova.GetSupernovaRadius();
		}
		return false;
	}

	public float GetDistanceToSupernova(Vector3 worldPosition)
	{
		return Vector3.Distance(worldPosition, base.transform.position) - _supernova.GetSupernovaRadius();
	}

	private void CheckPlayerSawSunExplode()
	{
		if (PlayerData.GetPersistentCondition("HAS_SEEN_SUN_EXPLODE") || !(Locator.GetActiveCamera() == Locator.GetPlayerCamera()) || PlayerState.InGiantsDeep() || PlayerState.InBrambleDimension() || PlayerState.OnQuantumMoon() || PlayerState.InDarkZone() || PlayerState.IsCameraUnderwater() || PlayerState.InConversation() || (PlayerState.IsInsideShip() && !PlayerState.AtFlightConsole()))
		{
			return;
		}
		Transform transform = Locator.GetPlayerCamera().transform;
		Vector3 vector = Locator.GetSunTransform().position - transform.position;
		if (Vector3.Angle(vector, transform.forward) < Locator.GetPlayerCamera().fieldOfView * 0.5f && vector.sqrMagnitude < 900000000f)
		{
			if (!Physics.Raycast(transform.position, vector, out var hitInfo, 1000f, OWLayerMask.physicalMask))
			{
				PlayerData.SetPersistentCondition("HAS_SEEN_SUN_EXPLODE", state: true);
				MonoBehaviour.print("HAS_SEEN_SUN_EXPLODE");
			}
			else
			{
				MonoBehaviour.print("view of supernova obstructed by " + hitInfo.collider.gameObject);
			}
		}
	}
}
