using UnityEngine;

public class GeyserWaterVillageHack : MonoBehaviour
{
	[SerializeField]
	private Sector _disableSector;

	[SerializeField]
	private Material _targetMaterial;

	private Renderer[] _renderers;

	private bool[] _rendererEnabledFlags;

	private bool _subscribed;

	private void Awake()
	{
		_renderers = GetComponentsInChildren<Renderer>();
		_rendererEnabledFlags = new bool[_renderers.Length];
		if (_targetMaterial != null)
		{
			for (int i = 0; i < _renderers.Length; i++)
			{
				if (_renderers[i].sharedMaterial != _targetMaterial)
				{
					_renderers[i] = null;
				}
			}
		}
		_disableSector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_disableSector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
	}

	private void OnDestroy()
	{
		_disableSector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_disableSector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
	}

	private void OnOccupantEnterSector(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player && !_subscribed)
		{
			Locator.GetPlayerCamera().onThisPreCull += new OWEvent<OWCamera>.OWCallback(OnPlayerCameraPreCull);
			Locator.GetPlayerCamera().onThisPostRender += new OWEvent<OWCamera>.OWCallback(OnPlayerCameraPostRender);
			_subscribed = true;
		}
	}

	private void OnOccupantExitSector(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player && _subscribed && Locator.GetPlayerCamera() != null)
		{
			Locator.GetPlayerCamera().onThisPreCull -= new OWEvent<OWCamera>.OWCallback(OnPlayerCameraPreCull);
			Locator.GetPlayerCamera().onThisPostRender -= new OWEvent<OWCamera>.OWCallback(OnPlayerCameraPostRender);
			_subscribed = false;
		}
	}

	private void OnPlayerCameraPreCull(OWCamera owCamera)
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			if (_renderers[i] != null)
			{
				_rendererEnabledFlags[i] = _renderers[i].enabled;
				_renderers[i].enabled = false;
			}
		}
	}

	private void OnPlayerCameraPostRender(OWCamera owCamera)
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			if (_renderers[i] != null)
			{
				_renderers[i].enabled = _rendererEnabledFlags[i];
			}
		}
	}
}
