using UnityEngine;

public class SupernovaPlanetEffectController : MonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock_Atmosphere;

	private static int s_propID_SunIntensity;

	private static int s_propID_Tint;

	private static MaterialPropertyBlock s_matPropBlock_ShockLayer;

	private static int s_propID_Color;

	private static int s_propID_WorldToLocalShockMatrix;

	private static int s_propID_Dir;

	private static int s_propID_Length;

	private static int s_propID_Flare;

	private static int s_propID_TrailFade;

	private static int s_propID_GradientLerp;

	private static int s_propID_MainTex_ST;

	[SerializeField]
	private Light _ambientLight;

	[SerializeField]
	private LODGroup _atmosphere;

	[SerializeField]
	private PlanetaryFogController _fog;

	[Space]
	[SerializeField]
	private MeshRenderer _shockLayer;

	[ColorUsage(true, true)]
	[SerializeField]
	private Color _shockLayerColor = Color.white;

	[SerializeField]
	private float _shockLayerStartRadius = 10000f;

	[SerializeField]
	private float _shockLayerFullRadius = 20000f;

	[SerializeField]
	private float _shockLayerTrailLength = 100f;

	[SerializeField]
	private float _shockLayerTrailFlare = 100f;

	private SunController _sunController;

	private bool _collapseStarted;

	private bool _supernovaStarted;

	private float _ambientLightOrigIntensity;

	private LOD[] _atmosphereLODs;

	private Color _fogOrigTint;

	private Renderer _fogImpostor;

	private void Awake()
	{
		if (s_matPropBlock_Atmosphere == null)
		{
			s_matPropBlock_Atmosphere = new MaterialPropertyBlock();
			s_propID_SunIntensity = Shader.PropertyToID("_SunIntensity");
			s_propID_Tint = Shader.PropertyToID("_Tint");
			s_matPropBlock_ShockLayer = new MaterialPropertyBlock();
			s_propID_Color = Shader.PropertyToID("_Color");
			s_propID_WorldToLocalShockMatrix = Shader.PropertyToID("_WorldToShockLocalMatrix");
			s_propID_Dir = Shader.PropertyToID("_Dir");
			s_propID_Length = Shader.PropertyToID("_Length");
			s_propID_Flare = Shader.PropertyToID("_Flare");
			s_propID_TrailFade = Shader.PropertyToID("_TrailFade");
			s_propID_GradientLerp = Shader.PropertyToID("_GradientLerp");
			s_propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
		}
		if (_ambientLight != null)
		{
			_ambientLightOrigIntensity = _ambientLight.intensity;
		}
		if (_atmosphere != null)
		{
			_atmosphereLODs = _atmosphere.GetLODs();
		}
		if (_fog != null)
		{
			_fogOrigTint = _fog.fogTint;
			_fogImpostor = _fog.fogImpostor;
		}
		if (_shockLayer != null)
		{
			_shockLayer.enabled = false;
		}
	}

	private void Start()
	{
		_sunController = Locator.GetSunTransform().GetComponent<SunController>();
		_sunController.OnCollapseStart += OnSunCollapseStart;
		_sunController.OnSupernovaStart += OnSupernovaStart;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_sunController != null)
		{
			_sunController.OnCollapseStart -= OnSunCollapseStart;
			_sunController.OnSupernovaStart -= OnSupernovaStart;
		}
	}

	private void OnSunCollapseStart()
	{
		_collapseStarted = true;
		base.enabled = true;
	}

	private void OnSupernovaStart()
	{
		_supernovaStarted = true;
		base.enabled = true;
	}

	private void Update()
	{
		if (!_collapseStarted && !_supernovaStarted)
		{
			base.enabled = false;
		}
		else if (_supernovaStarted)
		{
			float supernovaRadius = _sunController.GetSupernovaRadius();
			if (_shockLayer != null)
			{
				if (!_shockLayer.enabled)
				{
					_shockLayer.enabled = true;
				}
				float value = 1f - Mathf.InverseLerp(_shockLayerStartRadius, _shockLayerFullRadius, supernovaRadius);
				Vector3 vector = Vector3.Normalize(base.transform.position - _sunController.transform.position);
				Vector3 up = Vector3.up;
				Matrix4x4 matrix4x = Matrix4x4.TRS(base.transform.position, Quaternion.LookRotation(vector, up), Vector3.one);
				Vector4 vector2 = _shockLayer.sharedMaterial.GetVector(s_propID_MainTex_ST);
				vector2.w = 0f - Time.timeSinceLevelLoad;
				s_matPropBlock_ShockLayer.SetColor(s_propID_Color, _shockLayerColor);
				s_matPropBlock_ShockLayer.SetMatrix(s_propID_WorldToLocalShockMatrix, matrix4x.inverse);
				s_matPropBlock_ShockLayer.SetVector(s_propID_Dir, vector);
				s_matPropBlock_ShockLayer.SetFloat(s_propID_Length, _shockLayerTrailLength);
				s_matPropBlock_ShockLayer.SetFloat(s_propID_Flare, _shockLayerTrailFlare);
				s_matPropBlock_ShockLayer.SetFloat(s_propID_TrailFade, value);
				s_matPropBlock_ShockLayer.SetFloat(s_propID_GradientLerp, 0f);
				s_matPropBlock_ShockLayer.SetVector(s_propID_MainTex_ST, vector2);
				_shockLayer.SetPropertyBlock(s_matPropBlock_ShockLayer);
			}
		}
		else
		{
			if (!_collapseStarted)
			{
				return;
			}
			float collapseProgress = _sunController.GetCollapseProgress();
			if (_ambientLight != null)
			{
				_ambientLight.intensity = _ambientLightOrigIntensity * (1f - collapseProgress);
			}
			if (_atmosphere != null)
			{
				s_matPropBlock_Atmosphere.SetFloat(s_propID_SunIntensity, 1f - collapseProgress);
				for (int i = 0; i < _atmosphereLODs.Length; i++)
				{
					for (int j = 0; j < _atmosphereLODs[i].renderers.Length; j++)
					{
						_atmosphereLODs[i].renderers[j].SetPropertyBlock(s_matPropBlock_Atmosphere);
					}
				}
			}
			if (_fog != null)
			{
				_fog.fogTint = Color.Lerp(_fogOrigTint, Color.black, collapseProgress);
				if ((bool)_fogImpostor)
				{
					_fogImpostor.material.SetColor(s_propID_Tint, _fog.fogTint);
				}
			}
		}
	}
}
