using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightLOD : MonoBehaviour
{
	private Light _light;

	[SerializeField]
	private bool _fadeShadows;

	[SerializeField]
	private float _shadowFadeStart = 50f;

	[SerializeField]
	private float _shadowFadeEnd = 75f;

	[Space]
	[SerializeField]
	private bool _disableOnQualitySetting;

	[SerializeField]
	private ShadowQuality _highestQualitySetting = ShadowQuality.HIGH;

	private bool _listeningToCameraEvents;

	private bool _appliedOverride;

	private LightShadows _baseShadows;

	private LightShadows _oldShadows;

	private float _oldShadowStrength;

	private void Awake()
	{
		_light = GetComponent<Light>();
		if (_disableOnQualitySetting)
		{
			_baseShadows = _light.shadows;
			if (PlayerData.GetGraphicSettings().shadowQuality < _highestQualitySetting)
			{
				_light.shadows = LightShadows.None;
			}
			GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		}
	}

	private void OnEnable()
	{
		if (_fadeShadows && _light.shadows != 0)
		{
			_listeningToCameraEvents = true;
			OWCamera.onAnyPreCull += new OWEvent<OWCamera>.OWCallback(ApplyLODSettings);
			OWCamera.onAnyPostRender += new OWEvent<OWCamera>.OWCallback(RevertLODSettings);
		}
	}

	private void OnDisable()
	{
		if (_listeningToCameraEvents)
		{
			OWCamera.onAnyPreCull -= new OWEvent<OWCamera>.OWCallback(ApplyLODSettings);
			OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(RevertLODSettings);
			_listeningToCameraEvents = false;
		}
	}

	private void OnDestroy()
	{
		if (_disableOnQualitySetting)
		{
			GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		}
	}

	private void ApplyLODSettings(OWCamera owCamera)
	{
		if (!_light.enabled || !_light.gameObject.activeInHierarchy || !_fadeShadows)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(owCamera.transform.position - _light.transform.position);
		if (num > _shadowFadeStart * _shadowFadeStart)
		{
			float num2 = ((!(num >= _shadowFadeEnd * _shadowFadeEnd)) ? Mathf.Clamp01((Mathf.Sqrt(num) - _shadowFadeStart) / (_shadowFadeEnd - _shadowFadeStart)) : 1f);
			_oldShadows = _light.shadows;
			_oldShadowStrength = _light.shadowStrength;
			if (num2 >= 1f)
			{
				_light.shadows = LightShadows.None;
			}
			else
			{
				_light.shadowStrength = _oldShadowStrength * (1f - num2);
			}
			_appliedOverride = true;
		}
	}

	private void RevertLODSettings(OWCamera owCamera)
	{
		if (_appliedOverride)
		{
			if (_fadeShadows)
			{
				_light.shadows = _oldShadows;
				_light.shadowStrength = _oldShadowStrength;
			}
			_appliedOverride = false;
		}
	}

	private void OnGraphicSettingsUpdated(GraphicSettings graphicsSettings)
	{
		if (graphicsSettings.shadowQuality < _highestQualitySetting)
		{
			_light.shadows = LightShadows.None;
			if (_listeningToCameraEvents)
			{
				OWCamera.onAnyPreCull -= new OWEvent<OWCamera>.OWCallback(ApplyLODSettings);
				OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(RevertLODSettings);
				_listeningToCameraEvents = false;
			}
		}
		else
		{
			_light.shadows = _baseShadows;
			if (_fadeShadows && _light.shadows != 0 && base.enabled && base.gameObject.activeInHierarchy && !_listeningToCameraEvents)
			{
				_listeningToCameraEvents = true;
				OWCamera.onAnyPreCull += new OWEvent<OWCamera>.OWCallback(ApplyLODSettings);
				OWCamera.onAnyPostRender += new OWEvent<OWCamera>.OWCallback(RevertLODSettings);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_fadeShadows)
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
			Gizmos.color = new Color(0.2f, 0.2f, 1f, 0.5f);
			Gizmos.DrawWireSphere(Vector3.zero, _shadowFadeStart);
			Gizmos.color = new Color(0.2f, 0.2f, 0.5f, 0.5f);
			Gizmos.DrawWireSphere(Vector3.zero, _shadowFadeEnd);
		}
	}
}
