public class ShipHUDMarker : HUDDistanceMarker
{
	private bool _shipDestroyed;

	private bool _playerInShip;

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger.AddListener("EnterShip", OnEnterShip);
		GlobalMessenger.AddListener("ExitShip", OnExitShip);
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipDestroyed);
		GlobalMessenger.AddListener("ShipDestroyed", OnShipDestroyed);
	}

	protected override void Start()
	{
		base.Start();
		if (Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnShipEnter += new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetCloakFieldController().OnShipExit += new OWEvent.OWCallback(RefreshOwnVisibility);
		}
		if (Locator.GetRingWorldController() != null)
		{
			Locator.GetRingWorldController().OnPlayerEnter += new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnPlayerExit += new OWEvent.OWCallback(RefreshOwnVisibility);
		}
	}

	protected override void InitCanvasMarker()
	{
		_markerTarget = Locator.GetShipTransform();
		_markerLabel = UITextLibrary.GetString(UITextType.LocationShip_Cap);
		ReferenceFrameVolume componentInChildren = _markerTarget.GetComponentInChildren<ReferenceFrameVolume>();
		if (componentInChildren != null)
		{
			_markerRadius = componentInChildren.GetReferenceFrame().GetBracketsRadius();
		}
		_fogWarpDetector = Locator.GetShipDetector().GetRequiredComponent<FogWarpDetector>();
		base.InitCanvasMarker();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
		GlobalMessenger.RemoveListener("ExitShip", OnExitShip);
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipDestroyed);
		GlobalMessenger.RemoveListener("ShipDestroyed", OnShipDestroyed);
		if (Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnShipEnter -= new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetCloakFieldController().OnShipExit -= new OWEvent.OWCallback(RefreshOwnVisibility);
		}
		if (Locator.GetRingWorldController() != null)
		{
			Locator.GetRingWorldController().OnPlayerEnter -= new OWEvent.OWCallback(RefreshOwnVisibility);
			Locator.GetRingWorldController().OnPlayerExit -= new OWEvent.OWCallback(RefreshOwnVisibility);
		}
	}

	protected override void OnLanguageChanged()
	{
		_markerLabel = UITextLibrary.GetString(UITextType.LocationShip_Cap);
		if (_canvasMarker != null)
		{
			_canvasMarker.SetLabel(_markerLabel);
		}
	}

	private void OnEnterShip()
	{
		_playerInShip = true;
		_canvasMarker.NotifyResetPosition();
		RefreshOwnVisibility();
	}

	private void OnExitShip()
	{
		_playerInShip = false;
		_canvasMarker.NotifyResetPosition();
		RefreshOwnVisibility();
	}

	private void OnShipDestroyed()
	{
		_shipDestroyed = true;
		_canvasMarker.NotifyResetPosition();
		RefreshOwnVisibility();
	}

	protected override void RefreshOwnVisibility()
	{
		bool flag = Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye();
		bool flag2 = _quantumMoon != null && (_quantumMoon.IsPlayerInside() || _quantumMoon.IsShipInside());
		bool flag3 = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside;
		bool flag4 = true;
		if (Locator.GetCloakFieldController() != null)
		{
			flag4 = Locator.GetCloakFieldController().isPlayerInsideCloak == Locator.GetCloakFieldController().isShipInsideCloak;
		}
		_isVisible = !flag && !flag2 && !flag3 && !_translatorEquipped && !_inConversation && !_shipDestroyed && !_playerInShip && PlayerState.HasPlayerEnteredShip() && _isWearingHelmet && flag4;
		if (_canvasMarker != null)
		{
			_canvasMarker.SetVisibility(_isVisible);
		}
	}
}
