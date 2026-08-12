using UnityEngine;

public class IPExteriorVisualsManager : SectoredMonoBehaviour
{
	[SerializeField]
	private Renderer[] _exteriorVisualsRenderers = new Renderer[0];

	[SerializeField]
	private Renderer[] _cockpitExteriorVisuals = new Renderer[0];

	[SerializeField]
	private Renderer[] _observationDeckHiddenVisuals = new Renderer[0];

	[SerializeField]
	private Sector _cockpitSector;

	[SerializeField]
	private Sector _observationDeckSector;

	private bool _renderersHidden;

	private bool _cockpitRenderersHidden;

	private bool _observationDeckRenderersHidden;

	private bool[] _exteriorVisualsRenderersState;

	private bool[] _cockpitExteriorRenderersState;

	private bool[] _observationDeckRenderersState;

	private bool _playerInSector;

	private bool _probeInSector;

	private bool _playerInCockpit;

	private bool _probeInCockpit;

	private bool _playerInObservationDeck;

	private bool _probeInObservationDeck;

	protected override void Awake()
	{
		base.Awake();
		_exteriorVisualsRenderersState = new bool[_exteriorVisualsRenderers.Length];
		_cockpitExteriorRenderersState = new bool[_cockpitExteriorVisuals.Length];
		_observationDeckRenderersState = new bool[_observationDeckHiddenVisuals.Length];
		_cockpitSector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnCockpitEnter);
		_cockpitSector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnCockpitExit);
		_observationDeckSector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnObservationDeckEnter);
		_observationDeckSector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnObservationDeckExit);
		OWCamera.onAnyPreCull += new OWEvent<OWCamera>.OWCallback(OnOWCameraPreCull);
		OWCamera.onAnyPostRender += new OWEvent<OWCamera>.OWCallback(OnOWCameraPostRender);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_cockpitSector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnCockpitEnter);
		_cockpitSector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnCockpitExit);
		_observationDeckSector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnObservationDeckEnter);
		_observationDeckSector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnObservationDeckExit);
		OWCamera.onAnyPreCull -= new OWEvent<OWCamera>.OWCallback(OnOWCameraPreCull);
		OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(OnOWCameraPostRender);
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInSector = true;
		}
		else if (sectorDetector.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeInSector = true;
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInSector = false;
		}
		else if (sectorDetector.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeInSector = false;
		}
	}

	private void OnCockpitEnter(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInCockpit = true;
		}
		else if (detector.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeInCockpit = true;
		}
	}

	private void OnCockpitExit(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInCockpit = false;
		}
		else if (detector.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeInCockpit = false;
		}
	}

	private void OnObservationDeckEnter(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInObservationDeck = true;
		}
		else if (detector.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeInObservationDeck = true;
		}
	}

	private void OnObservationDeckExit(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInObservationDeck = false;
		}
		else if (detector.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeInObservationDeck = false;
		}
	}

	private void OnOWCameraPreCull(OWCamera owCamera)
	{
		if (!_playerInSector && !_probeInSector && !_playerInCockpit && !_probeInCockpit && !_playerInObservationDeck && !_probeInObservationDeck)
		{
			return;
		}
		if (owCamera.CompareTag("MainCamera"))
		{
			if (_playerInSector)
			{
				HideRenderers();
				HideCockpitRenderers();
				if (_playerInObservationDeck)
				{
					HideObservationDeckRenderers();
				}
			}
			else if (_playerInCockpit)
			{
				HideCockpitRenderers();
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
				if (_playerInSector)
				{
					HideRenderers();
					HideCockpitRenderers();
					if (_probeInObservationDeck)
					{
						HideObservationDeckRenderers();
					}
				}
			}
			else if (_probeInSector)
			{
				HideRenderers();
				HideCockpitRenderers();
				if (_probeInObservationDeck)
				{
					HideObservationDeckRenderers();
				}
			}
			else if (_probeInCockpit)
			{
				HideCockpitRenderers();
			}
		}
	}

	private void OnOWCameraPostRender(OWCamera owCamera)
	{
		RestoreRenderers();
		RestoreCockpitRenderers();
		RestoreObservationDeckRenderers();
	}

	private void HideRenderers()
	{
		if (!_renderersHidden)
		{
			_renderersHidden = true;
			for (int i = 0; i < _exteriorVisualsRenderers.Length; i++)
			{
				_exteriorVisualsRenderersState[i] = _exteriorVisualsRenderers[i].enabled;
				_exteriorVisualsRenderers[i].enabled = false;
			}
		}
	}

	private void RestoreRenderers()
	{
		if (_renderersHidden)
		{
			for (int i = 0; i < _exteriorVisualsRenderers.Length; i++)
			{
				_exteriorVisualsRenderers[i].enabled = _exteriorVisualsRenderersState[i];
			}
			_renderersHidden = false;
		}
	}

	private void HideCockpitRenderers()
	{
		if (!_cockpitRenderersHidden)
		{
			_cockpitRenderersHidden = true;
			for (int i = 0; i < _cockpitExteriorVisuals.Length; i++)
			{
				_cockpitExteriorRenderersState[i] = _cockpitExteriorVisuals[i].enabled;
				_cockpitExteriorVisuals[i].enabled = false;
			}
		}
	}

	private void RestoreCockpitRenderers()
	{
		if (_cockpitRenderersHidden)
		{
			for (int i = 0; i < _cockpitExteriorVisuals.Length; i++)
			{
				_cockpitExteriorVisuals[i].enabled = _cockpitExteriorRenderersState[i];
			}
			_cockpitRenderersHidden = false;
		}
	}

	private void HideObservationDeckRenderers()
	{
		if (!_observationDeckRenderersHidden)
		{
			_observationDeckRenderersHidden = true;
			for (int i = 0; i < _observationDeckHiddenVisuals.Length; i++)
			{
				_observationDeckRenderersState[i] = _observationDeckHiddenVisuals[i].enabled;
				_observationDeckHiddenVisuals[i].enabled = false;
			}
		}
	}

	private void RestoreObservationDeckRenderers()
	{
		if (_observationDeckRenderersHidden)
		{
			for (int i = 0; i < _observationDeckHiddenVisuals.Length; i++)
			{
				_observationDeckHiddenVisuals[i].enabled = _observationDeckRenderersState[i];
			}
			_observationDeckRenderersHidden = false;
		}
	}
}
