using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlanetaryFogController : MonoBehaviour
{
	private static bool s_callbackActive = false;

	private static List<PlanetaryFogController> s_activeFogSpheres = new List<PlanetaryFogController>(16);

	private static PlanetaryFogController s_lastFogSphere = null;

	private static PlanetaryFogController s_playerFogSphere = null;

	[SerializeField]
	private Texture3D _fogLookupTexture;

	[SerializeField]
	private float _fogRadius = 300f;

	[SerializeField]
	private float _fogDensity = 1f;

	[SerializeField]
	[Range(0f, 2f)]
	private float _fogExponent = 1f;

	[SerializeField]
	private Texture2D _fogColorRampTexture;

	[SerializeField]
	private float _fogColorRampIntensity = 1f;

	[SerializeField]
	private Color _fogTint = Color.white;

	[SerializeField]
	private float _skyboxFactor = 1f;

	[Header("Fog Impostor (Optional)")]
	[SerializeField]
	private Renderer _fogImpostor;

	[SerializeField]
	private float _lodFadeDistance = 100f;

	[Header("DLC")]
	[SerializeField]
	private bool _isRingworldFog;

	[SerializeField]
	private float _ringworldPlaneDist1 = 160.11f;

	[SerializeField]
	private float _ringworldPlaneDist2 = 161.67f;

	private static MaterialPropertyBlock s_fogImpostorMatPropBlock;

	private static int s_propID_FogLookupTex;

	private static int s_propID_FogColorRampTex;

	private static int s_propID_FogPosition;

	private static int s_propID_FogDirRight;

	private static int s_propID_FogDirForward;

	private static int s_propID_FogParams;

	private static int s_propID_FogTint;

	private static int s_propID_LODFade;

	public Texture3D fogLookupTexture
	{
		get
		{
			return _fogLookupTexture;
		}
		set
		{
			_fogLookupTexture = value;
		}
	}

	public float fogRadius
	{
		get
		{
			return _fogRadius;
		}
		set
		{
			_fogRadius = value;
		}
	}

	public float fogDensity
	{
		get
		{
			return _fogDensity;
		}
		set
		{
			_fogDensity = value;
		}
	}

	public float fogExponent
	{
		get
		{
			return _fogExponent;
		}
		set
		{
			_fogExponent = value;
		}
	}

	public Texture2D fogColorRampTexture
	{
		get
		{
			return _fogColorRampTexture;
		}
		set
		{
			_fogColorRampTexture = value;
		}
	}

	public float fogColorRampIntensity
	{
		get
		{
			return _fogColorRampIntensity;
		}
		set
		{
			_fogColorRampIntensity = value;
		}
	}

	public Color fogTint
	{
		get
		{
			return _fogTint;
		}
		set
		{
			_fogTint = value;
		}
	}

	public Renderer fogImpostor => _fogImpostor;

	public float lodFadeDistance
	{
		get
		{
			return _lodFadeDistance;
		}
		set
		{
			_lodFadeDistance = value;
		}
	}

	public bool isRingworldFog => _isRingworldFog;

	public float ringworldPlaneDist1 => _ringworldPlaneDist1;

	public float ringworldPlaneDist2 => _ringworldPlaneDist2;

	private void Awake()
	{
		if (s_fogImpostorMatPropBlock == null)
		{
			s_propID_FogLookupTex = Shader.PropertyToID("_FogLookupTex");
			s_propID_FogColorRampTex = Shader.PropertyToID("_FogColorRampTex");
			s_propID_FogPosition = Shader.PropertyToID("_FogPosition");
			s_propID_FogDirRight = Shader.PropertyToID("_FogDirRight");
			s_propID_FogDirForward = Shader.PropertyToID("_FogDirForward");
			s_propID_FogParams = Shader.PropertyToID("_FogParams");
			s_propID_FogTint = Shader.PropertyToID("_FogTint");
			s_propID_LODFade = Shader.PropertyToID("_LODFade");
			s_fogImpostorMatPropBlock = new MaterialPropertyBlock();
			s_fogImpostorMatPropBlock.SetFloat(s_propID_LODFade, 0f);
		}
	}

	private void OnEnable()
	{
		// CHANGED
#if UNITY_EDITOR
		// sets fog stuff properly i think
		Awake();
#endif

		s_activeFogSpheres.Add(this);
		if (!s_callbackActive)
		{
			OWCamera.onAnyPreCull += new OWEvent<OWCamera>.OWCallback(UpdateFogSettings);
			OWCamera.onAnyPostRender += new OWEvent<OWCamera>.OWCallback(ResetFogSettings);
			s_callbackActive = true;
		}
	}

	private void OnDisable()
	{
		s_activeFogSpheres.Remove(this);
		if (s_activeFogSpheres.Count == 0)
		{
			OWCamera.onAnyPreCull -= new OWEvent<OWCamera>.OWCallback(UpdateFogSettings);
			OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(ResetFogSettings);
			s_callbackActive = false;
		}
	}

	public static PlanetaryFogController GetActiveFogSphere()
	{
		return s_playerFogSphere;
	}

	private static void UpdateFogSettings(OWCamera owCamera)
	{
		s_fogImpostorMatPropBlock.SetFloat(s_propID_LODFade, 1f);
		for (int i = 0; i < s_activeFogSpheres.Count; i++)
		{
			if (!(s_activeFogSpheres[i]._fogImpostor == null))
			{
				s_activeFogSpheres[i]._fogImpostor.SetPropertyBlock(s_fogImpostorMatPropBlock);
			}
		}
		Vector3 position = owCamera.transform.position;
		PlanetaryFogController planetaryFogController = null;
		float f = float.PositiveInfinity;
		for (int j = 0; j < s_activeFogSpheres.Count; j++)
		{
			PlanetaryFogController planetaryFogController2 = s_activeFogSpheres[j];
			float num = Vector3.SqrMagnitude(position - planetaryFogController2.transform.position);
			float num2 = planetaryFogController2._fogRadius + planetaryFogController2._lodFadeDistance;
			if (!(num > num2 * num2) && (planetaryFogController == null || planetaryFogController2._fogRadius < planetaryFogController._fogRadius))
			{
				planetaryFogController = planetaryFogController2;
				f = num;
			}
		}
		if (planetaryFogController != null)
		{
			float num3 = Mathf.Clamp01((Mathf.Sqrt(f) - planetaryFogController._fogRadius) / planetaryFogController._lodFadeDistance);
			if (planetaryFogController != s_lastFogSphere)
			{
				Shader.SetGlobalTexture(s_propID_FogLookupTex, planetaryFogController._fogLookupTexture);
				Shader.SetGlobalTexture(s_propID_FogColorRampTex, (planetaryFogController._fogColorRampTexture != null) ? planetaryFogController._fogColorRampTexture : Texture2D.whiteTexture);
				s_lastFogSphere = planetaryFogController;
			}
			Vector3 position2 = planetaryFogController.transform.position;
			Shader.SetGlobalVector(s_propID_FogPosition, new Vector4(position2.x, position2.y, position2.z, planetaryFogController._fogRadius));
			Shader.SetGlobalVector(s_propID_FogDirRight, planetaryFogController.transform.right);
			Shader.SetGlobalVector(s_propID_FogDirForward, planetaryFogController.transform.forward);
			Shader.SetGlobalVector(s_propID_FogParams, new Vector4((planetaryFogController._fogLookupTexture != null) ? (planetaryFogController._fogDensity * (1f - num3)) : 0f, planetaryFogController._fogColorRampIntensity, planetaryFogController._fogExponent, planetaryFogController._skyboxFactor));
			Shader.SetGlobalVector(s_propID_FogTint, planetaryFogController._fogTint.linear);
			if (planetaryFogController._fogImpostor != null)
			{
				s_fogImpostorMatPropBlock.SetFloat(s_propID_LODFade, num3);
				planetaryFogController._fogImpostor.SetPropertyBlock(s_fogImpostorMatPropBlock);
			}
		}
		if (owCamera.CompareTag("MainCamera"))
		{
			s_playerFogSphere = planetaryFogController;
		}
	}

	private static void ResetFogSettings(OWCamera owCamera)
	{
		Shader.SetGlobalVector(s_propID_FogParams, new Vector4(0f, 0f, 1f, 0f));
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(base.transform.position, _fogRadius);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, _fogRadius + _lodFadeDistance);
		}
	}
}
