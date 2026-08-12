using UnityEngine;

public class RingworldSunController : SectoredMonoBehaviour, SunLightController.ISunOverrider
{
	[SerializeField]
	private Light _ringworldSunLight;

	[SerializeField]
	private Light _ringworldAmbientLight;

	private bool _playerInsideRingworld;

	private bool _probeInsideRingworld;

	private bool _overrideActive;

	private bool _ringworldLightOverrideApplied;

	private bool _ringworldSunLightWasEnabled;

	private bool _ringworldAmbientLightWasEnabled;

	protected override void OnSectorOccupantsUpdated()
	{
		_playerInsideRingworld = _sector.ContainsOccupant(DynamicOccupant.Player);
		_probeInsideRingworld = _sector.ContainsOccupant(DynamicOccupant.Probe);
		if ((_playerInsideRingworld || _probeInsideRingworld) && !_overrideActive)
		{
			_overrideActive = true;
			OWCamera.onAnyPreCull += new OWEvent<OWCamera>.OWCallback(OnOWCameraPreCull);
			OWCamera.onAnyPostRender += new OWEvent<OWCamera>.OWCallback(OnOWCameraPostRender);
			SunLightController.RegisterSunOverrider(this, 500);
		}
		else if (!_playerInsideRingworld && !_probeInsideRingworld && _overrideActive)
		{
			OWCamera.onAnyPreCull -= new OWEvent<OWCamera>.OWCallback(OnOWCameraPreCull);
			OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(OnOWCameraPostRender);
			SunLightController.UnregisterSunOverrider(this);
			_overrideActive = false;
		}
	}

	private void OnOWCameraPreCull(OWCamera owCamera)
	{
		if (owCamera.CompareTag("MainCamera"))
		{
			if (!_playerInsideRingworld)
			{
				ApplyRingworldLightOverride();
			}
		}
		else
		{
			if (!owCamera.CompareTag("ProbeCamera"))
			{
				return;
			}
			if (owCamera.GetComponent<ProbeCamera>().GetID() == ProbeCamera.ID.PreLaunch)
			{
				if (!_playerInsideRingworld)
				{
					ApplyRingworldLightOverride();
				}
			}
			else if (!_probeInsideRingworld)
			{
				ApplyRingworldLightOverride();
			}
		}
	}

	private void OnOWCameraPostRender(OWCamera owCamera)
	{
		RevertRingworldLightOverride();
	}

	private void ApplyRingworldLightOverride()
	{
		if (!_ringworldLightOverrideApplied)
		{
			_ringworldLightOverrideApplied = true;
			_ringworldSunLightWasEnabled = _ringworldSunLight.enabled;
			_ringworldAmbientLightWasEnabled = _ringworldAmbientLight.enabled;
			_ringworldSunLight.enabled = false;
			_ringworldAmbientLight.enabled = false;
		}
	}

	private void RevertRingworldLightOverride()
	{
		if (_ringworldLightOverrideApplied)
		{
			_ringworldSunLight.enabled = _ringworldSunLightWasEnabled;
			_ringworldAmbientLight.enabled = _ringworldAmbientLightWasEnabled;
			_ringworldLightOverrideApplied = false;
		}
	}

	public SunLightController.SunOverrideSettings ApplySunOverrides(OWCamera owCamera, SunLightController.SunOverrideSettings settings)
	{
		if (owCamera.CompareTag("MainCamera"))
		{
			if (!_playerInsideRingworld)
			{
				return settings;
			}
		}
		else if (owCamera.CompareTag("ProbeCamera"))
		{
			if (owCamera.GetComponent<ProbeCamera>().GetID() == ProbeCamera.ID.PreLaunch)
			{
				if (!_playerInsideRingworld)
				{
					return settings;
				}
			}
			else if (!_probeInsideRingworld)
			{
				return settings;
			}
		}
		settings.sunIntensity = 0f;
		settings.sunShadowStrength = 0f;
		return settings;
	}
}
