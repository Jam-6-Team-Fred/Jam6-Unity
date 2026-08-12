using UnityEngine;

public class DarkZone : MonoBehaviour
{
	[SerializeField]
	private Light _ambientLight;

	[SerializeField]
	private PlanetaryFogController _planetaryFog;

	[SerializeField]
	private FogOverrideVolume _fogOverride;

	[SerializeField]
	private SandFunnelController _sandFunnel;

	[SerializeField]
	private OWRenderer _entryFogPlane;

	[SerializeField]
	private OWRenderer _exitFogPlane;

	[SerializeField]
	private float _transitionTime = 5f;

	private float _origAmbientIntensity;

	private float _origFogDensity;

	private float _origFogOverrideDensity;

	private int _propID_Fade;

	private float _darkTransitionFraction;

	private bool _playerInDarkZone;

	private OWTriggerVolume _triggerVolume;

	private void Awake()
	{
		base.enabled = false;
		_origAmbientIntensity = ((_ambientLight != null) ? _ambientLight.intensity : 0f);
		_origFogDensity = ((_planetaryFog != null) ? _planetaryFog.fogDensity : 0f);
		_origFogOverrideDensity = ((_fogOverride != null) ? _fogOverride.density : 0f);
		if (_fogOverride != null)
		{
			_fogOverride.density = 0f;
		}
		if (_entryFogPlane != null || _exitFogPlane != null)
		{
			_propID_Fade = Shader.PropertyToID("_Fade");
		}
		_triggerVolume = base.gameObject.GetAddComponent<OWTriggerVolume>();
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
	}

	private void OnDestroy()
	{
		if (_playerInDarkZone)
		{
			RemovePlayerFromZone();
		}
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
	}

	private void Update()
	{
		float num = (_playerInDarkZone ? 1f : 0f);
		_darkTransitionFraction = Mathf.MoveTowards(_darkTransitionFraction, num, Time.deltaTime / _transitionTime);
		if (_darkTransitionFraction == num)
		{
			base.enabled = false;
		}
		if (_planetaryFog != null)
		{
			_planetaryFog.fogDensity = _origFogDensity * (1f - _darkTransitionFraction);
		}
		if (_fogOverride != null)
		{
			_fogOverride.density = _origFogOverrideDensity * _darkTransitionFraction;
		}
		if (_ambientLight != null)
		{
			_ambientLight.intensity = _origAmbientIntensity * (1f - _darkTransitionFraction);
		}
		if (_entryFogPlane != null)
		{
			_entryFogPlane.SetMaterialProperty(_propID_Fade, _darkTransitionFraction);
		}
		if (_exitFogPlane != null)
		{
			_exitFogPlane.SetMaterialProperty(_propID_Fade, 1f - _darkTransitionFraction);
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			AddPlayerToZone();
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			RemovePlayerFromZone();
		}
	}

	public void AddPlayerToZone(bool instant = false)
	{
		_playerInDarkZone = true;
		if (instant)
		{
			_darkTransitionFraction = 1f;
		}
		base.enabled = true;
		if (_sandFunnel != null)
		{
			_sandFunnel.SetPlayerInDarkZone(playerInDarkZone: true);
		}
		GlobalMessenger.FireEvent("EnterDarkZone");
	}

	public void RemovePlayerFromZone(bool instant = false)
	{
		_playerInDarkZone = false;
		if (instant)
		{
			_darkTransitionFraction = 0f;
		}
		base.enabled = true;
		if (_sandFunnel != null)
		{
			_sandFunnel.SetPlayerInDarkZone(playerInDarkZone: false);
		}
		GlobalMessenger.FireEvent("ExitDarkZone");
	}
}
