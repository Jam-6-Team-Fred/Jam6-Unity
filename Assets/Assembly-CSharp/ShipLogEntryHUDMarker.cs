using UnityEngine;

public class ShipLogEntryHUDMarker : HUDDistanceMarker
{
	private static ShipLogEntryLocation s_entryLocation;

	private static string s_entryLocationID;

	[SerializeField]
	private MapMarker _mapMarker;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.QuantumMoon);
		if (astroObject != null)
		{
			_quantumMoon = astroObject.GetComponent<QuantumMoon>();
		}
		if (LoadManager.GetPreviousScene() == OWScene.TitleScreen)
		{
			s_entryLocationID = "";
		}
		if (s_entryLocationID != null && s_entryLocationID != "")
		{
			ShipLogEntryLocation entryLocation = Locator.GetEntryLocation(s_entryLocationID);
			if (entryLocation != null)
			{
				SetEntryLocation(entryLocation);
			}
		}
		if (Locator.GetRingWorldController() != null)
		{
			Locator.GetRingWorldController().OnPlayerEnter += new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnPlayerExit += new OWEvent.OWCallback(RefreshOwnVisibility);
		}
		if (Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnPlayerEnter += new OWEvent.OWCallback(base.OnPlayerEnterCloakField);
			Locator.GetCloakFieldController().OnPlayerExit += new OWEvent.OWCallback(base.OnPlayerExitCloakField);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (s_entryLocation != null)
		{
			s_entryLocation.OnLocationDestroyed -= new OWEvent.OWCallback(OnSelectedLocationDestroyed);
		}
		if (Locator.GetRingWorldController() != null)
		{
			Locator.GetRingWorldController().OnPlayerEnter -= new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnPlayerExit -= new OWEvent.OWCallback(RefreshOwnVisibility);
		}
	}

	protected override void InitCanvasMarker()
	{
		base.InitCanvasMarker();
	}

	protected override void OnLanguageChanged()
	{
		if (s_entryLocation != null)
		{
			_markerLabel = s_entryLocation.GetEntryName(withLineBreaks: true);
			if (_canvasMarker != null)
			{
				_canvasMarker.SetLabel(_markerLabel);
				_mapMarker.ModifyMarker(s_entryLocation.GetTransform(), _markerLabel);
			}
		}
	}

	private void SetEntryLocationByID(string entryID)
	{
		if (entryID != null && entryID != "")
		{
			if (Locator.GetShipLogManager() != null)
			{
				ShipLogEntry entry = Locator.GetShipLogManager().GetEntry(entryID);
				ShipLogEntryLocation entryLocation = Locator.GetEntryLocation(entryID);
				if (entry != null && entry.GetState() == ShipLogEntry.State.Explored && entryLocation != null)
				{
					SetEntryLocation(entryLocation);
				}
			}
		}
		else
		{
			SetEntryLocation(null);
		}
	}

	public void SetEntryLocation(ShipLogEntryLocation entryLocation)
	{
		if (s_entryLocation != null)
		{
			s_entryLocation.OnDeselectLocation();
			s_entryLocation.OnLocationDestroyed -= new OWEvent.OWCallback(OnSelectedLocationDestroyed);
		}
		s_entryLocation = entryLocation;
		if (_canvasMarker != null)
		{
			_canvasMarker.DestroyMarker();
		}
		if (_mapMarker != null)
		{
			_mapMarker.DisableMarker();
		}
		if (s_entryLocation == null)
		{
			s_entryLocationID = "";
			return;
		}
		s_entryLocation.OnSelectLocation();
		s_entryLocation.OnLocationDestroyed += new OWEvent.OWCallback(OnSelectedLocationDestroyed);
		_markerLabel = s_entryLocation.GetEntryName(withLineBreaks: true);
		_markerTarget = s_entryLocation.GetTransform();
		_markerRadius = 0f;
		InitCanvasMarker();
		_mapMarker.InitMarker();
		_mapMarker.ModifyMarker(_markerTarget, _markerLabel);
		_mapMarker.EnableMarker();
		OuterFogWarpVolume outerFogWarpVolume = s_entryLocation.GetOuterFogWarpVolume();
		_canvasMarker.SetOuterFogWarpVolume(outerFogWarpVolume);
		_mapMarker.SetOuterFogWarpVolume(outerFogWarpVolume);
		RefreshOwnVisibility();
		if (outerFogWarpVolume != null)
		{
			Locator.GetMarkerManager().RequestFogMarkerUpdate();
		}
		s_entryLocationID = s_entryLocation.GetEntryID();
	}

	protected override void RefreshOwnVisibility()
	{
		bool flag = Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye();
		bool flag2 = _quantumMoon != null && _quantumMoon.IsPlayerInside();
		bool flag3 = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside && s_entryLocationID == "IP_RING_WORLD";
		bool flag4 = true;
		if (s_entryLocation != null && Locator.GetCloakFieldController() != null)
		{
			flag4 = s_entryLocation.IsWithinCloakField() || !Locator.GetCloakFieldController().isPlayerInsideCloak;
		}
		_isVisible = !flag && !flag2 && !_translatorEquipped && !_inConversation && s_entryLocation != null && (_isWearingHelmet || _atFlightConsole) && flag4 && !flag3;
		if (_canvasMarker != null)
		{
			_canvasMarker.SetVisibility(_isVisible);
		}
	}

	public string GetMarkedEntryID()
	{
		if (!(s_entryLocation != null))
		{
			return string.Empty;
		}
		return s_entryLocation.GetEntryID();
	}

	protected override Vector3 GetMarkerPosition()
	{
		return s_entryLocation.GetPosition();
	}

	private void OnSelectedLocationDestroyed()
	{
		SetEntryLocation(null);
	}
}
