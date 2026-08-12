using UnityEngine;

public class ProbeHUDMarker : HUDDistanceMarker
{
	[SerializeField]
	private Transform _markerPointerTransform;

	private HazardDetector _hazardDetector;

	private bool _launched;

	private bool _duplicatedInTLE;

	private bool _duplicatedInTLC;

	private bool _isTLCDuplicate;

	private const float c_probeMarkerSizeRadius = 0.3f;

	protected override void Awake()
	{
		base.Awake();
		_hazardDetector = GetComponentInChildren<HazardDetector>();
		GlobalMessenger.AddListener("ProbeEnterQuantumMoon", OnProbeEnterQuantumMoon);
		GlobalMessenger.AddListener("ProbeExitQuantumMoon", OnProbeExitQuantumMoon);
	}

	protected override void Start()
	{
		base.Start();
		if (Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnProbeEnter += new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetCloakFieldController().OnProbeExit += new OWEvent.OWCallback(RefreshOwnVisibility);
		}
		if (Locator.GetRingWorldController() != null)
		{
			Locator.GetRingWorldController().OnPlayerEnter += new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnPlayerExit += new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnProbeEnter += new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnProbeExit += new OWEvent.OWCallback(RefreshOwnVisibility);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_canvasMarker != null)
		{
			DestroyMarker();
		}
		GlobalMessenger.RemoveListener("ProbeEnterQuantumMoon", OnProbeEnterQuantumMoon);
		GlobalMessenger.RemoveListener("ProbeExitQuantumMoon", OnProbeExitQuantumMoon);
		if (Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnProbeEnter -= new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetCloakFieldController().OnProbeExit -= new OWEvent.OWCallback(RefreshOwnVisibility);
		}
		if (Locator.GetRingWorldController() != null)
		{
			Locator.GetRingWorldController().OnPlayerEnter -= new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnPlayerExit -= new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnProbeEnter -= new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnProbeExit -= new OWEvent.OWCallback(RefreshOwnVisibility);
		}
	}

	protected override void InitCanvasMarker()
	{
		_markerTarget = _markerPointerTransform;
		_markerLabel = UITextLibrary.GetString(UITextType.LocationProbe_Cap);
		_markerRadius = 0.3f;
		base.InitCanvasMarker();
		_canvasMarker.OnMarkerSecondaryLabelUpdate += OnUpdateMarkerSecondaryLabel;
	}

	private void DestroyMarker()
	{
		_canvasMarker.OnMarkerSecondaryLabelUpdate -= OnUpdateMarkerSecondaryLabel;
		_canvasMarker.DestroyMarker();
	}

	protected override void OnLanguageChanged()
	{
		_markerLabel = UITextLibrary.GetString(UITextType.LocationProbe_Cap);
		if (_canvasMarker != null)
		{
			_canvasMarker.SetLabel(_markerLabel);
		}
	}

	public void DestroyFogMarkersOnRetrieve()
	{
		_canvasMarker.NotifyResetPosition();
	}

	public void MarkAsRetrieved()
	{
		_launched = false;
		RefreshOwnVisibility();
	}

	public void MarkAsLaunched()
	{
		_launched = true;
		RefreshOwnVisibility();
	}

	public void MarkTLEDuplicatedState(bool value)
	{
		_duplicatedInTLE = value;
	}

	public void SetAsTLCoreDuplicate(bool value)
	{
		_isTLCDuplicate = value;
		if (_isTLCDuplicate)
		{
			MarkAsLaunched();
		}
	}

	public void MarkTLCoreDuplicatedState(bool value)
	{
		_duplicatedInTLC = value;
	}

	private CanvasMarker.SecondaryLabelType OnUpdateMarkerSecondaryLabel(CanvasMarker.SecondaryLabelType labelType)
	{
		if (_duplicatedInTLE || _duplicatedInTLC)
		{
			labelType = CanvasMarker.SecondaryLabelType.DUPLICATE;
		}
		else if (_hazardDetector.GetDisplayDangerMarker())
		{
			labelType = CanvasMarker.SecondaryLabelType.DANGER;
		}
		else if (labelType == CanvasMarker.SecondaryLabelType.DANGER)
		{
			labelType = CanvasMarker.SecondaryLabelType.NONE;
		}
		return labelType;
	}

	protected override void RefreshOwnVisibility()
	{
		bool flag = Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye();
		bool flag2 = _quantumMoon != null && (_quantumMoon.IsPlayerInside() || _quantumMoon.IsProbeInside());
		bool flag3 = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside;
		bool flag4 = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isProbeInside;
		bool flag5 = true;
		if (Locator.GetCloakFieldController() != null)
		{
			flag5 = Locator.GetCloakFieldController().isPlayerInsideCloak == Locator.GetCloakFieldController().isProbeInsideCloak;
		}
		bool flag6 = base.gameObject.activeInHierarchy;
		if (_isTLCDuplicate)
		{
			flag6 = true;
		}
		_isVisible = flag6 && !flag && !flag2 && !_translatorEquipped && !_inConversation && _launched && (_isWearingHelmet || _atFlightConsole) && flag3 == flag4 && flag5;
		if (_canvasMarker != null)
		{
			_canvasMarker.SetVisibility(_isVisible);
		}
	}

	private void OnProbeEnterQuantumMoon()
	{
		RefreshOwnVisibility();
	}

	private void OnProbeExitQuantumMoon()
	{
		RefreshOwnVisibility();
	}
}
