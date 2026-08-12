using UnityEngine;

public class ToolModeUI : MonoBehaviour, ILateInitializer
{
	[SerializeField]
	private ToolModeSwapper _toolSwapper;

	private static bool s_mapPromptReminder;

	private bool _screenPromptsInitialized;

	private ScreenPrompt _probePrompt;

	private ScreenPrompt _signalscopePrompt;

	private ScreenPrompt _flashlightPrompt;

	private ScreenPrompt _centerFlashlightPrompt;

	private ScreenPrompt _centerTranslatePrompt;

	private ScreenPrompt _centerProbePrompt;

	private ScreenPrompt _centerSignalscopePrompt;

	private ScreenPrompt _swapItemPrompt;

	private ScreenPrompt _mapPrompt;

	private ScreenPrompt _exitShipPrompt;

	private LandingPadManager _landingManager;

	private bool _inProbePromptTrigger;

	private bool _inSignalscopePromptTrigger;

	private bool _inFlashlightPromptTrigger;

	private bool _playingHideAndSeek;

	private bool _hasUsedMap_Suit;

	private bool _hasUsedMap_Ship;

	private ScreenPrompt _focusPrompt;

	private ScreenPrompt _concealPrompt;

	private ScreenPrompt _projectPrompt;

	private ScreenPrompt _centerFocusPrompt;

	private ScreenPrompt _centerConcealPrompt;

	private bool _hasFocusedInDreamEver;

	private bool _hasConcealedInDreamEver;

	private bool _hasConcealedInDreamThisLoop;

	private bool _hasConcealedFromGhostsEver;

	private bool _hasConcealedFromAlarmThisLoop;

	private bool _alarmTotemTriggered;

	private bool _ghostsAwoken;

	private bool _focusPromptCentered;

	private bool _concealPromptCentered;

	private void Awake()
	{
		_screenPromptsInitialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		GlobalMessenger.AddListener("EnterProbePromptTrigger", OnEnterProbePromptTrigger);
		GlobalMessenger.AddListener("ExitProbePromptTrigger", OnExitProbePromptTrigger);
		GlobalMessenger<bool>.AddListener("EnterSignalscopePromptTrigger", OnEnterSignalscopePromptTrigger);
		GlobalMessenger.AddListener("ExitSignalscopePromptTrigger", OnExitSignalscopePromptTrigger);
		GlobalMessenger.AddListener("EnterFlashlightPromptTrigger", OnEnterFlashlightPromptTrigger);
		GlobalMessenger.AddListener("ExitFlashlightPromptTrigger", OnExitFlashlightPromptTrigger);
		GlobalMessenger.AddListener("StartHideAndSeek", OnStartHideAndSeek);
		GlobalMessenger.AddListener("EndHideAndSeek", OnEndHideAndSeek);
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.AddListener("AlarmTotemTriggered", OnAlarmTotemTriggered);
		GlobalMessenger.AddListener("ConcealFromAlarmTotem", OnConcealFromAlarmTotem);
		GlobalMessenger.AddListener("GhostsAwoken", OnGhostsAwoken);
		GlobalMessenger<bool>.AddListener("TriggerMapPromptReminder", OnTriggerMapPromptReminder);
	}

	private void Start()
	{
		if (Locator.GetShipBody() != null)
		{
			_landingManager = Locator.GetShipBody().GetComponent<LandingPadManager>();
		}
		_hasUsedMap_Suit = PlayerData.GetPersistentCondition("HAS_USED_MAP_SUIT");
		_hasUsedMap_Ship = PlayerData.GetPersistentCondition("HAS_USED_MAP_SHIP");
		if (LoadManager.GetPreviousScene() == OWScene.TitleScreen)
		{
			s_mapPromptReminder = true;
		}
		_flashlightPrompt = new ScreenPrompt(InputLibrary.flashlight, UITextLibrary.GetString(UITextType.FlashlightPrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.PressPrompt));
		_probePrompt = new ScreenPrompt(InputLibrary.probeLaunch, UITextLibrary.GetString(UITextType.ScoutModePrompt) + "   <CMD>");
		_mapPrompt = new ScreenPrompt(InputLibrary.map, UITextLibrary.GetString(UITextType.MapPrompt) + "   <CMD>");
		_signalscopePrompt = new ScreenPrompt(InputLibrary.signalscope, UITextLibrary.GetString(UITextType.SignalscopePrompt) + "   <CMD>");
		_exitShipPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.UnbucklePrompt) + "   <CMD>");
		_centerSignalscopePrompt = new ScreenPrompt(InputLibrary.signalscope, "<CMD>   " + UITextLibrary.GetString(UITextType.SignalscopePrompt));
		_centerTranslatePrompt = new ScreenPrompt(InputLibrary.interact, "<CMD>   " + UITextLibrary.GetString(UITextType.TranslatorPrompt));
		_centerFlashlightPrompt = new ScreenPrompt(InputLibrary.flashlight, "<CMD>" + UITextLibrary.GetString(UITextType.PressPrompt) + "   " + UITextLibrary.GetString(UITextType.FlashlightPrompt));
		_centerProbePrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, "<CMD>   " + UITextLibrary.GetString(UITextType.ScoutModePrompt));
		_focusPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.LanternFocusPrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));
		_concealPrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, UITextLibrary.GetString(UITextType.LanternConcealPrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));
		_projectPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.TorchProjectPrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));
		_centerFocusPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.LanternFocusPrompt));
		_centerConcealPrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.LanternConcealPrompt));
		_hasFocusedInDreamEver = PlayerData.GetPersistentCondition("HAS_FOCUSED_IN_DREAM");
		_hasConcealedInDreamEver = PlayerData.GetPersistentCondition("HAS_CONCEALED_IN_DREAM");
		_hasConcealedFromGhostsEver = PlayerData.GetPersistentCondition("HAS_CONCEALED_FROM_GHOSTS");
	}

	private void OnDestroy()
	{
		if (!_screenPromptsInitialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		GlobalMessenger.RemoveListener("EnterProbePromptTrigger", OnEnterProbePromptTrigger);
		GlobalMessenger.RemoveListener("ExitProbePromptTrigger", OnExitProbePromptTrigger);
		GlobalMessenger<bool>.RemoveListener("EnterSignalscopePromptTrigger", OnEnterSignalscopePromptTrigger);
		GlobalMessenger.RemoveListener("ExitSignalscopePromptTrigger", OnExitSignalscopePromptTrigger);
		GlobalMessenger.RemoveListener("EnterFlashlightPromptTrigger", OnEnterFlashlightPromptTrigger);
		GlobalMessenger.RemoveListener("ExitFlashlightPromptTrigger", OnExitFlashlightPromptTrigger);
		GlobalMessenger.RemoveListener("StartHideAndSeek", OnStartHideAndSeek);
		GlobalMessenger.RemoveListener("EndHideAndSeek", OnEndHideAndSeek);
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.RemoveListener("AlarmTotemTriggered", OnAlarmTotemTriggered);
		GlobalMessenger.RemoveListener("ConcealFromAlarmTotem", OnConcealFromAlarmTotem);
		GlobalMessenger.RemoveListener("GhostsAwoken", OnGhostsAwoken);
		GlobalMessenger<bool>.RemoveListener("TriggerMapPromptReminder", OnTriggerMapPromptReminder);
	}

	public void LateInitialize()
	{
		_screenPromptsInitialized = true;
		Locator.GetPromptManager().AddScreenPrompt(_exitShipPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_probePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_signalscopePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_flashlightPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_mapPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_centerFlashlightPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_centerTranslatePrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_centerProbePrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_centerSignalscopePrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_focusPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_concealPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_projectPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_centerFocusPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_centerConcealPrompt, PromptPosition.Center);
	}

	private void Update()
	{
		_mapPrompt.SetVisibility(isVisible: false);
		_flashlightPrompt.SetVisibility(isVisible: false);
		_centerFlashlightPrompt.SetVisibility(isVisible: false);
		_probePrompt.SetVisibility(isVisible: false);
		_signalscopePrompt.SetVisibility(isVisible: false);
		_centerTranslatePrompt.SetVisibility(isVisible: false);
		_centerSignalscopePrompt.SetVisibility(isVisible: false);
		_exitShipPrompt.SetVisibility(isVisible: false);
		_centerProbePrompt.SetVisibility(isVisible: false);
		_focusPrompt.SetVisibility(isVisible: false);
		_concealPrompt.SetVisibility(isVisible: false);
		_projectPrompt.SetVisibility(isVisible: false);
		_centerFocusPrompt.SetVisibility(isVisible: false);
		_centerConcealPrompt.SetVisibility(isVisible: false);
		if (_inFlashlightPromptTrigger && !PlayerState.IsFlashlightOn())
		{
			_centerFlashlightPrompt.SetVisibility(isVisible: true);
		}
		if (_toolSwapper.IsTranslatorEquipPromptAllowed())
		{
			_centerTranslatePrompt.SetVisibility(isVisible: true);
		}
		if (_toolSwapper.GetToolMode() == ToolMode.None)
		{
			bool flag = OWInput.IsInputMode(InputMode.Character) && !PlayerState.IsInsideShip() && !PlayerState.IsAttached() && Locator.GetPlayerSuit().IsWearingHelmet() && !Locator.GetPlayerSuit().IsTrainingSuit() && !PlayerState.InZeroG() && PlayerData.GetPersistentCondition("HAS_USED_JETPACK");
			bool flag2 = OWInput.IsInputMode(InputMode.ShipCockpit) && _landingManager != null && !_landingManager.IsLanded();
			if (_playingHideAndSeek && OWInput.IsInputMode(InputMode.Character) && !Locator.GetPlayerSuit().IsWearingSuit() && !PlayerState.IsInsideShip() && !PlayerState.IsAttached())
			{
				_signalscopePrompt.SetVisibility(isVisible: true);
			}
			if (flag || flag2)
			{
				_signalscopePrompt.SetVisibility(isVisible: true);
				_probePrompt.SetVisibility(isVisible: true);
			}
			bool num = (s_mapPromptReminder || !_hasUsedMap_Suit) && flag;
			bool flag3 = (s_mapPromptReminder || !_hasUsedMap_Ship) && OWInput.IsInputMode(InputMode.ShipCockpit);
			bool flag4 = PlayerState.IsInsideShip() && OWInput.IsInputMode(InputMode.Character) && !PlayerState.IsAttached() && Locator.GetPlayerSuit().IsWearingHelmet() && !Locator.GetPlayerSuit().IsTrainingSuit() && !PlayerState.InZeroG();
			if (num || flag3 || flag4)
			{
				_mapPrompt.SetVisibility(isVisible: true);
			}
			if (OWInput.IsInputMode(InputMode.ShipCockpit))
			{
				_exitShipPrompt.SetVisibility(isVisible: true);
				if (_landingManager != null && _landingManager.IsLanded())
				{
					_mapPrompt.SetVisibility(isVisible: true);
				}
			}
			if (!_inFlashlightPromptTrigger && !PlayerState.IsFlashlightOn() && OWInput.IsInputMode(InputMode.Character) && !PlayerState.IsAttached() && !PlayerState.IsInsideShip() && !PlayerState.InZeroG() && (PlayerState.InDarkZone() || Locator.GetPlayerSuit().IsWearingHelmet()))
			{
				_flashlightPrompt.SetVisibility(isVisible: true);
			}
			if (_inSignalscopePromptTrigger)
			{
				_centerSignalscopePrompt.SetVisibility(isVisible: true);
				_signalscopePrompt.SetVisibility(isVisible: false);
			}
			if (_inProbePromptTrigger && _toolSwapper.GetProbeLauncher().GetActiveProbe() == null)
			{
				_centerProbePrompt.SetVisibility(isVisible: true);
			}
		}
		if (!OWInput.IsInputMode(InputMode.Character) || _toolSwapper.GetToolMode() != ToolMode.Item)
		{
			return;
		}
		if (_toolSwapper.GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern)
		{
			DreamLanternItem dreamLanternItem = (DreamLanternItem)_toolSwapper.GetItemCarryTool().GetHeldItem();
			if (dreamLanternItem.GetLanternType() != DreamLanternType.Functioning)
			{
				return;
			}
			bool flag5 = PlayerState.InDreamWorld();
			_focusPrompt.SetVisibility(!_focusPromptCentered || !flag5);
			_concealPrompt.SetVisibility(!_concealPromptCentered || !flag5);
			_centerFocusPrompt.SetVisibility(_focusPromptCentered && flag5);
			_centerConcealPrompt.SetVisibility(_concealPromptCentered && flag5);
			if (!flag5)
			{
				return;
			}
			if (dreamLanternItem.GetLanternController().IsFocused(0.5f))
			{
				_focusPromptCentered = false;
				if (!_hasFocusedInDreamEver)
				{
					_hasFocusedInDreamEver = true;
					PlayerData.SetPersistentCondition("HAS_FOCUSED_IN_DREAM", state: true);
				}
			}
			if (dreamLanternItem.GetLanternController().IsConcealed(0.5f))
			{
				_hasConcealedInDreamThisLoop = true;
				_concealPromptCentered = false;
				if (!_hasConcealedInDreamEver)
				{
					_hasConcealedInDreamEver = true;
					PlayerData.SetPersistentCondition("HAS_CONCEALED_IN_DREAM", state: true);
				}
				if (!_hasConcealedFromGhostsEver && _ghostsAwoken)
				{
					_hasConcealedFromGhostsEver = true;
					PlayerData.SetPersistentCondition("HAS_CONCEALED_FROM_GHOSTS", state: true);
				}
			}
		}
		else if (_toolSwapper.GetItemCarryTool().GetHeldItemType() == ItemType.VisionTorch)
		{
			_projectPrompt.SetVisibility(isVisible: true);
		}
	}

	private void OnEnterMapView()
	{
		if (!Locator.GetMapController().IsObservatoryMap())
		{
			s_mapPromptReminder = false;
			_mapPrompt.SetDisplayState(ScreenPrompt.DisplayState.Normal);
			if (!_hasUsedMap_Suit && OWInput.IsInputMode(InputMode.Character))
			{
				PlayerData.SetPersistentCondition("HAS_USED_MAP_SUIT", state: true);
			}
			else if (!_hasUsedMap_Ship && OWInput.IsInputMode(InputMode.ShipCockpit))
			{
				PlayerData.SetPersistentCondition("HAS_USED_MAP_SHIP", state: true);
			}
		}
	}

	private void OnEnterProbePromptTrigger()
	{
		_inProbePromptTrigger = true;
	}

	private void OnExitProbePromptTrigger()
	{
		_inProbePromptTrigger = false;
	}

	private void OnEnterSignalscopePromptTrigger(bool centerZoomPrompt)
	{
		_inSignalscopePromptTrigger = true;
	}

	private void OnExitSignalscopePromptTrigger()
	{
		_inSignalscopePromptTrigger = false;
	}

	private void OnEnterFlashlightPromptTrigger()
	{
		_inFlashlightPromptTrigger = true;
	}

	private void OnExitFlashlightPromptTrigger()
	{
		_inFlashlightPromptTrigger = false;
	}

	private void OnStartHideAndSeek()
	{
		_playingHideAndSeek = true;
	}

	private void OnEndHideAndSeek()
	{
		_playingHideAndSeek = false;
	}

	private void OnExitDreamWorld()
	{
		if (!_hasFocusedInDreamEver || !_hasConcealedInDreamEver)
		{
			_focusPromptCentered = true;
			_concealPromptCentered = true;
		}
		DreamWakeType lastDreamWakeType = Locator.GetDreamWorldController().GetLastDreamWakeType();
		if (_ghostsAwoken && !_hasConcealedFromGhostsEver && lastDreamWakeType == DreamWakeType.LanternBlownOut)
		{
			_focusPromptCentered = true;
			_concealPromptCentered = true;
		}
		if (_alarmTotemTriggered && !_hasConcealedFromAlarmThisLoop && !Locator.GetShipLogManager().IsFactRevealed("IP_DREAM_ZONE_2_X3") && !Locator.GetShipLogManager().IsFactRevealed("IP_DREAM_LIBRARY_2_X1"))
		{
			_focusPromptCentered = true;
			_concealPromptCentered = true;
		}
		_alarmTotemTriggered = false;
	}

	private void OnAlarmTotemTriggered()
	{
		_alarmTotemTriggered = true;
	}

	private void OnConcealFromAlarmTotem()
	{
		_hasConcealedFromAlarmThisLoop = true;
	}

	private void OnGhostsAwoken()
	{
		_ghostsAwoken = true;
	}

	private void OnTriggerMapPromptReminder(bool attentionState)
	{
		s_mapPromptReminder = true;
		if (attentionState)
		{
			_mapPrompt.SetDisplayState(ScreenPrompt.DisplayState.Attention);
		}
	}
}
