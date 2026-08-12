using UnityEngine;

public class RingworldShadowsOverride : MonoBehaviour
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private float _overrideShadowDistance = 50f;

	private ProxyShadowLight _proxyShadowLight;

	private bool _overrideApplied;

	private OWCamera _overridingCamera;

	private float _prevShadowDistance = -1f;

	private int _prevShadowCascades = -1;

	public float overrideShadowDistance => _overrideShadowDistance;

	private void Awake()
	{
		_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
		_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
	}

	private void Start()
	{
		_proxyShadowLight = Locator.GetSunTransform().GetComponentInChildren<ProxyShadowLight>();
	}

	private void OnDestroy()
	{
		_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
		_sector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
	}

	private void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			if (_proxyShadowLight != null)
			{
				_proxyShadowLight.enabled = false;
			}
			OWCamera.onAnyPreCull += new OWEvent<OWCamera>.OWCallback(OnCameraPreCull);
			OWCamera.onAnyPostRender += new OWEvent<OWCamera>.OWCallback(OnCameraPostRender);
		}
	}

	private void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			if (_proxyShadowLight != null)
			{
				_proxyShadowLight.enabled = true;
			}
			OWCamera.onAnyPreCull -= new OWEvent<OWCamera>.OWCallback(OnCameraPreCull);
			OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(OnCameraPostRender);
		}
	}

	private void OnCameraPreCull(OWCamera owCamera)
	{
		if (!_overrideApplied)
		{
			_prevShadowDistance = QualitySettings.shadowDistance;
			_prevShadowCascades = QualitySettings.shadowCascades;
			QualitySettings.shadowDistance = _overrideShadowDistance;
			QualitySettings.shadowCascades = 0;
			_overrideApplied = true;
			_overridingCamera = owCamera;
		}
	}

	private void OnCameraPostRender(OWCamera owCamera)
	{
		if (_overrideApplied && !(_overridingCamera != owCamera))
		{
			QualitySettings.shadowDistance = _prevShadowDistance;
			QualitySettings.shadowCascades = _prevShadowCascades;
			_prevShadowDistance = -1f;
			_prevShadowCascades = -1;
			_overrideApplied = false;
			_overridingCamera = null;
		}
	}
}
